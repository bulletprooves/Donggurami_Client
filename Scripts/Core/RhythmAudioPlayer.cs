using System.Collections.Generic;
using UnityEngine;

// BATCH 구조로 전략 변경
//
//  CircleTrack
//  -> NotePlayRequest만 생성
//
//  RhythmManager
//  -> 모든 Track의 NotePlayRequest를 한 리스트에 모음
//
//  RhythmAudioPlayer
//  -> 모인 요청을 한 번에 재생
//
//1. CircleTrack이 AudioSource를 몰라도 됨
//2.오디오 재생 책임이 RhythmAudioPlayer로 모임
//3. 이후 중복 제거, 최대 재생 수 제한, 볼륨 조절을 한 곳에서 처리 가능


[System.Serializable]
public class OctaveSample
{
    public int octave;
    public AudioClip clip;
}

public class RhythmAudioPlayer : MonoBehaviour
{
    [Header("Audio Samples")]
    [SerializeField] private string resourcesRootPath = "AudioSamples";
    [SerializeField] private int minSampleOctave = 1;
    [SerializeField] private int maxSampleOctave = 7;

    [Header("Pool")]
    [SerializeField] private int audioSourcePoolSize = 32;

    [Header("Musical Settings")]
    [SerializeField] private ScaleType scaleType = ScaleType.Major;
    [SerializeField] private float volume = 0.8f;

    // Private Area
    private AudioSource[] audioSources;
    private int currentIndex;
    private readonly Dictionary<InstrumentType, Dictionary<int, OctaveSample>> _sampleCache = new();
    private int loadedSampleCount = Const.ZERO;
    private readonly HashSet<string> _missingSampleWarningKeys = new();

    #region R_Unity
    private void Awake()
    {
        audioSourcePoolSize = Mathf.Max(Const.COUNT_MIN, audioSourcePoolSize);
        audioSources = new AudioSource[audioSourcePoolSize];
        for (int i = 0; i < audioSourcePoolSize; i++)
        {
            GameObject go = new GameObject($"AudioSource_{i}");
            go.transform.SetParent(transform);
            AudioSource source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = Const.ZEROF;
            audioSources[i] = source;
        }

        // 샘플 캐시 만들기
        loadedSampleCount = Const.ZERO;
        _sampleCache.Clear();

        InstrumentType[] instrumentTypes = (InstrumentType[])System.Enum.GetValues(typeof(InstrumentType));

        for (int i = 0; i < instrumentTypes.Length; i++)
        {
            InstrumentType instrumentType = instrumentTypes[i];
            Dictionary<int, OctaveSample> octaveMap = new Dictionary<int, OctaveSample>();
            _sampleCache.Add(instrumentType, octaveMap);

            for (int octave = minSampleOctave; octave <= maxSampleOctave; octave++)
            {
                string sampleName = GetSampleName(instrumentType, octave);
                string path = $"{resourcesRootPath}/{instrumentType}/{sampleName}";

                AudioClip clip = Resources.Load<AudioClip>(path);

                if (clip == null)
                    continue;

                OctaveSample sample = new OctaveSample { octave = octave, clip = clip };
                octaveMap[octave] = sample;
                loadedSampleCount++;
            }
        }

        //GameManager.Instance.Log($"{name}: Loaded Audio Samples: {loadedSampleCount}");
        //NullReferenceException: Object reference not set to an instance of an object 이거 나중에 Awake 순서 때문에 GameManager.Instance가 null일 수 있음. 따라서 로그는 주석 처리

    }
    #endregion

    #region R_Public
    public void PlayNote(int scaleDegree, int rootOctave, NoteAccidental accidental, InstrumentType instrumentType, float volumeScale = 1f)
    {
        if (audioSources == null || audioSources.Length == Const.ZERO)
        {
            GameManager.Instance.LogWarning($"{name}: 오디오소스 풀 is not initialized");
            return;
        }

        // 어베일러브 AudioSource 찾기
        AudioSource source = null;
        bool isAllPlaying = true;
        for (int i = 0; i < audioSources.Length; i++)
        {
            int index = (currentIndex + i) % audioSources.Length;
            source = audioSources[index];

            if (!source.isPlaying)
            {
                currentIndex = (index + 1) % audioSources.Length;
                isAllPlaying = false;
                break;
            }
        }
        // FALLBACK: 전부 다 쓰고 있으면 그냥 다음 걸로 재사용
        if (isAllPlaying)
        {
            AudioSource fallback = audioSources[currentIndex];

            currentIndex++;
            if (currentIndex >= audioSources.Length)
                currentIndex = Const.ZERO;

            source = fallback;
        }

        // MIDI 노트 계산
        int targetMidiNote = ScaleDegreeToMidiNote(scaleDegree, rootOctave, accidental);
        int sampleMidiNote = 0;
        int targetOctave = Utility.MidiNoteToOctave(targetMidiNote);

        OctaveSample sample = FindSample(instrumentType, targetOctave);

        AudioClip clip = null;

        if (sample != null)
        {
            clip = sample.clip;
            sampleMidiNote = Utility.OctaveToCMidiNote(sample.octave);
        }

        if (source == null)
            return;

        // 클립 샘플 없으면 예외처리
        if (clip == null)
        {
            string warningKey = $"{instrumentType}_{targetOctave}";

            if (!_missingSampleWarningKeys.Contains(warningKey))
            {
                _missingSampleWarningKeys.Add(warningKey);
                GameManager.Instance?.LogWarning($"{name}: 해당 옥타브 샘플 없음. Instrument: {instrumentType}, TargetOctave: {targetOctave}");
            }

            return;
        }

        source.pitch = Mathf.Pow(2f, (targetMidiNote - sampleMidiNote) / (float)Const.CHROMATIC_SCALE);
        source.volume = volume * volumeScale;
        source.clip = clip;
        source.Play();

        GameManager.Instance.Log($"클립: {clip}");
    }

    public string GetNoteName(int scaleDegree, int rootOctave, NoteAccidental accidental)
    {
        int midiNote = ScaleDegreeToMidiNote(scaleDegree, rootOctave, accidental);
        string[] noteNames = Utility.Solfeges();
        int noteIndex = Utility.Mod(midiNote, Const.CHROMATIC_SCALE);
        int octave = Utility.MidiNoteToOctave(midiNote);

        return $"{noteNames[noteIndex]}{octave}";
    }

    public void PlayNotes(List<NotePlayRequest> requests)
    {
        if (requests == null || requests.Count == Const.ZERO)
            return;

        float batchVolumeScale = 1f / Mathf.Sqrt(requests.Count);

        for (int i = 0; i < requests.Count; i++)
        {
            NotePlayRequest request = requests[i];

            PlayNote(
                request.pitch,
                request.octave,
                request.accidental,
                request.instrumentType,
                request.volumeScale * batchVolumeScale
            );
        }
    }
    #endregion

    private string GetSampleName(InstrumentType instrumentType, int octave)
    {
        if (instrumentType == InstrumentType.Drum_Blues)
            return GetDrumSampleName(octave);

        return $"Octave{octave}";
    }

    private string GetDrumSampleName(int octave)
    {
        DrumSampleType sampleType = (DrumSampleType)Mathf.Clamp(octave, 1, 7);

        return sampleType.ToString();
    }

    private int ScaleDegreeToMidiNote(int scaleDegree, int rootOctave, NoteAccidental accidental)
    {
        int[] scale = Utility.GetScaleIntervals(scaleType);

        int octaveOffset = Mathf.FloorToInt(scaleDegree / (float)scale.Length);
        int degreeIndex = Utility.Mod(scaleDegree, scale.Length);

        int semitoneOffset = scale[degreeIndex] + (int)accidental + octaveOffset * Const.CHROMATIC_SCALE;

        int rootMidiNote = Utility.OctaveToCMidiNote(rootOctave);

        return rootMidiNote + semitoneOffset;
    }

    private void BuildSampleCache()
    {
        _sampleCache.Clear();

        InstrumentType[] instrumentTypes = (InstrumentType[])System.Enum.GetValues(typeof(InstrumentType));

        for (int i = 0; i < instrumentTypes.Length; i++)
        {
            InstrumentType instrumentType = instrumentTypes[i];
            Dictionary<int, OctaveSample> octaveMap = new Dictionary<int, OctaveSample>();
            _sampleCache.Add(instrumentType, octaveMap);

            for (int octave = minSampleOctave; octave <= maxSampleOctave; octave++)
            {
                string path = $"{resourcesRootPath}/{instrumentType}/Octave{octave}";
                AudioClip clip = Resources.Load<AudioClip>(path);

                if (clip == null)
                    continue;

                OctaveSample sample = new OctaveSample
                {
                    octave = octave,
                    clip = clip
                };

                octaveMap[octave] = sample;
            }
        }
    }



    private OctaveSample FindSample(InstrumentType instrumentType, int targetOctave)
    {
        if (!_sampleCache.TryGetValue(instrumentType, out Dictionary<int, OctaveSample> octaveMap))
            return null;

        if (octaveMap.TryGetValue(targetOctave, out OctaveSample exactSample))
            return exactSample;

        OctaveSample nearestSample = null;
        int nearestDistance = int.MaxValue;

        foreach (KeyValuePair<int, OctaveSample> pair in octaveMap)
        {
            OctaveSample sample = pair.Value;

            if (sample == null || sample.clip == null)
                continue;

            int distance = Mathf.Abs(pair.Key - targetOctave);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestSample = sample;
            }
        }

        return nearestSample;
    }
}