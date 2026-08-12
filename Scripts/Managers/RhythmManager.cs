using System.Collections.Generic;
using UnityEngine;

public enum RhythmPlayState
{
    Stop = 0,
    Play = 1,
    Pause = 2
}

public class RhythmManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Config config;
    [SerializeField] private PortalLensManager portalLensManager;

    [Header("Audio")]
    [SerializeField] private RhythmAudioPlayer audioPlayer;

    [Header("Tracks")]
    [SerializeField] private List<CircleTrack> tracks = new();
    [SerializeField] private CircleTrack circleTrackPrefab;
    [SerializeField] private Transform trackRoot;

    [Header("Playback")]
    [SerializeField] private RhythmPlayState playState = RhythmPlayState.Stop;

    // Properties
    public int Bpm => _bpm;
    public int BeatsPerBar => _beatsPerBar;
    public int MinBpm => (config != null) ? config.MinBpm : 40;
    public int MaxBpm => (config != null) ? config.MaxBpm : 240;
    /// <summary>
    /// ê±??????´ë‹ˆê¹? 'ê±°ë¦¬/?œê°„ = ?ë„' (ê±°ë¦¬ = ?œë°”ê¾?ê¸¸ì´, ?œê°„ = ?œë§ˆ??
    /// ì´ˆë‹¹ ë¹„íŠ¸ ?˜ì˜ ??ˆ˜ (60f / bpm = ë¹„íŠ¸ ?œë ˆ??ë¥??¨ìœ„ Bar??ë¹„íŠ¸ ??ë§Œí¼ ê³±í•¨ (?œë§ˆ?”ì— ê±¸ë¦¬???œê°„)
    /// ê·?ê³±í•œ ê±??˜ë ˆ (2? r)ë§Œí¼ ê³±í•˜ë©? ? í˜• ?ë„ (?€ì§ì—¬?¼í•  ?¨ìœ„)ê°€ ?˜ì˜´
    /// </summary>
    public float LinearSpeed => Utility.TwoPiRadius(_playbackBaseRadius) / ((60f / _bpm) * _beatsPerBar);
    public IReadOnlyList<CircleTrack> Tracks => tracks;
    public int TrackCount => tracks != null ? tracks.Count : Const.ZERO;

    public RhythmPlayState PlayState => playState;
    public bool IsStopped => playState == RhythmPlayState.Stop;
    public bool IsPlaying => playState == RhythmPlayState.Play;
    public bool IsPaused => playState == RhythmPlayState.Pause;

    public bool CanCreateTrack
    {
        get
        {
            int maxCount = config != null ? config.MaxTrackCount : int.MaxValue;
            return IsStopped && tracks.Count < maxCount;
        }
    }

    public bool CanRemoveTrack
    {
        get
        {
            int minCount = config != null ? config.MinTrackCount : 1;
            return IsStopped && tracks.Count > minCount;
        }
    }
    public event System.Action<CircleTrack> OnTrackCreated;
    public event System.Action<CircleTrack> OnTrackRemoved;

    // Private Area
    private readonly List<NotePlayRequest> _notePlayRequests = new();
    private int _bpm;
    private float _playbackBaseRadius;
    private int _beatsPerBar;


    #region R_Unity
    private void Awake()
    {
        if (config == null)
        {
            GameManager.Instance.LogError($"{name}: No Config");
            enabled = false;
            return;
        }

        _bpm = config.DefaultBpm;
        _beatsPerBar = config.DefaultBeatsPerBar;
        _playbackBaseRadius = config.PlaybackBaseRadius;
    }

    private void Start()
    {
        if (portalLensManager != null)
        {
            portalLensManager.CreateLensesForExistingTracks(Tracks);
        }
    }

    private void Update()
    {
        if (playState != RhythmPlayState.Play)
            return;

        _notePlayRequests.Clear();

        for (int i = 0; i < tracks.Count; i++)
        {
            CircleTrack track = tracks[i];

            if (track == null)
                continue;

            track.CollectTriggeredNotes(LinearSpeed, Time.deltaTime, _notePlayRequests);
        }

        if (audioPlayer != null)
            audioPlayer.PlayNotes(_notePlayRequests);
    }
    #endregion

    #region R_Set
    public void SetBpm(int value)
    {
        _bpm = Mathf.Clamp(value, MinBpm, MaxBpm);
    }

    public void SetBeatsPerBar(int value)
    {
        _beatsPerBar = Mathf.Max(Const.COUNT_MIN, value);
    }
    #endregion

    public bool AddTrack(CircleTrack track)
    {
        if (track == null)
            return false;

        if (tracks.Contains(track))
            return false;

        tracks.Add(track);
        return true;
    }

    public bool DestroyTrack(CircleTrack track)
    {
        if (track == null || !tracks.Contains(track))
            return false;

        if (!tracks.Remove(track))
            return false;

        Destroy(track.gameObject);
        return true;
    }

    public bool RemoveTrack(CircleTrack track)
    {
        if (!IsStopped)
        {
            GameManager.Instance.LogWarning($"{name}: Stop ?íƒœ?ì„œë§?CircleTrack???œê±°?????ˆìŠµ?ˆë‹¤.");
            return false;
        }

        if (track == null)
            return false;

        int minCount = config != null ? config.MinTrackCount : 1;

        if (tracks.Count <= minCount)
        {
            GameManager.Instance.LogWarning($"{name}: ìµœì†Œ {minCount}ê°œì˜ CircleTrack?€ ?¨ì•„ ?ˆì–´???©ë‹ˆ??");
            return false;
        }

        return RemoveTrackImmediately(track);
    }

    public void ClearAllTracksForLoad()
    {
        if (!IsStopped)
            Stop();

        for (int i = tracks.Count - 1; i >= 0; i--)
        {
            CircleTrack track = tracks[i];

            if (track == null)
                continue;

            RemoveTrackImmediately(track);
        }

        tracks.Clear();
    }

    private bool RemoveTrackImmediately(CircleTrack track)
    {
        if (track == null)
            return false;

        bool removed = tracks.Remove(track);

        if (!removed)
            return false;

        if (portalLensManager != null)
            portalLensManager.RemoveLensForTrack(track);

        OnTrackRemoved?.Invoke(track);

        Destroy(track.gameObject);

        return true;
    }

    public CircleTrack CreateTrack(float radius, InstrumentType instrumentType)
    {

        if (!IsStopped)
        {
            GameManager.Instance.LogWarning($"{name}: Stop ?íƒœ?ì„œë§?CircleTrack???ì„±?????ˆìŠµ?ˆë‹¤.");
            return null;
        }

        if (config != null && tracks.Count >= config.MaxTrackCount)
        {
            GameManager.Instance.LogWarning($"{name}: ìµœë? Track ê°œìˆ˜???„ë‹¬?ˆìŠµ?ˆë‹¤.");
            return null;
        }

        if (!CanCreateTrack)
        {
            GameManager.Instance.LogWarning($"{name}: CircleTrack???ì„±?????†ëŠ” ?íƒœ?…ë‹ˆ??");
            return null;
        }

        if (circleTrackPrefab == null)
        {
            GameManager.Instance.LogWarning($"{name}: CircleTrack Prefab???°ê²°?˜ì? ?Šì•˜?µë‹ˆ??");
            return null;
        }

        Transform parent = trackRoot != null ? trackRoot : transform;

        CircleTrack track = Instantiate(circleTrackPrefab, parent);

        track.name = $"CircleTrack_{tracks.Count:00}";
        track.SetRadius(radius);
        track.SetInstrument(instrumentType);
        ThemeManager.Instance?.ApplyThemeToTrack(track);    // ?„ì¬ ?Œë§ˆ ?ìš©
        track.ResetPlayback();

        AddTrack(track);

        if (portalLensManager != null)
            portalLensManager.CreateLensForTrack(track);

        OnTrackCreated?.Invoke(track);


        return track;
    }

    public CircleTrack CreateTrackFromSave(Vector3 position, float radius, InstrumentType instrumentType)
    {
        CircleTrack track = CreateTrack(radius, instrumentType);

        if (track == null)
            return null;

        track.transform.position = position;

        return track;
    }

    public void ClearAllTracks()
    {
        for (int i = tracks.Count - 1; i >= 0; i--)
        {
            CircleTrack track = tracks[i];

            if (track == null)
                continue;

            RemoveTrack(track);
        }

        tracks.Clear();
    }

    public void Play()
    {
        if (playState == RhythmPlayState.Play)
            return;

        bool wasStopped = playState == RhythmPlayState.Stop;

        playState = RhythmPlayState.Play;

        if (wasStopped)
            PlayCurrentNotesAtStart();
    }

    public void Pause()
    {
        if (playState != RhythmPlayState.Play)
            return;

        playState = RhythmPlayState.Pause;
    }

    public void Stop()
    {
        if (playState == RhythmPlayState.Stop)
            return;

        playState = RhythmPlayState.Stop;

        ResetAllTracks();
    }

    private void PlayCurrentNotesAtStart()
    {
        if (audioPlayer == null)
            return;

        _notePlayRequests.Clear();

        for (int i = 0; i < tracks.Count; i++)
        {
            CircleTrack track = tracks[i];

            if (track == null)
                continue;

            track.CollectCurrentNote(_notePlayRequests);
        }

        audioPlayer.PlayNotes(_notePlayRequests);
    }

    private void ResetAllTracks()
    {
        for (int i = 0; i < tracks.Count; i++)
        {
            CircleTrack track = tracks[i];

            if (track == null)
                continue;

            track.ResetPlayback();
        }
    }

#if UNITY_EDITOR
    // ? íš¨??ê²€ì¦ìš©, 0?´ë‚˜ ?Œìˆ˜ ?˜ì˜¤ë©??ˆë˜?ˆê¹Œ ìµœì†Œ ê°’ì„ ?•í•´?€??
    private void OnValidate()
    {
        _bpm = Mathf.Max(Const.COUNT_MIN, _bpm);
        _beatsPerBar = Mathf.Max(Const.COUNT_MIN, _beatsPerBar);
        _playbackBaseRadius = Mathf.Max(Const.MINIMUM_HUNDREDTH, _playbackBaseRadius);
    }
#endif
}