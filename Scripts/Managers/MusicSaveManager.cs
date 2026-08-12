using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MusicSaveManager : MonoBehaviour
{
    // 로컬 저장 (비밀이야 <3)
    private const string PLAYER_PREFS_SAVE_KEY = "Donggeurami_SaveCode";

    [Header("References")]
    [SerializeField] private RhythmManager rhythmManager;
    [SerializeField] private ThemeManager themeManager;
    [SerializeField] private TMP_InputField saveCodeInputField;

    [Header("Remote Share Codes")]
    [SerializeField] private string shareCodeFolderPath = "ShareCodes";

    #region R_Public
    public string ExportCode()
    {
        MusicSaveData data = CaptureSaveData();

        return MusicSaveCodeUtility.ExportCode(data);
    }

    public bool ImportCode(string code)
    {
        if (!MusicSaveCodeUtility.TryImportCode(code, out MusicSaveData data))
        {
            GameManager.Instance?.LogWarning("Save code import failed.");
            return false;
        }

        ApplySaveData(data);

        return true;
    }

    public void SaveToLocal()
    {
        string code = ExportCode();

        if (string.IsNullOrEmpty(code))
            return;

        PlayerPrefs.SetString(PLAYER_PREFS_SAVE_KEY, code);
        PlayerPrefs.Save();

        GameManager.Instance?.Log("data saved to local.");
    }

    public bool LoadFromLocal()
    {
        string code = PlayerPrefs.GetString(PLAYER_PREFS_SAVE_KEY, string.Empty);

        if (string.IsNullOrEmpty(code))
        {
            GameManager.Instance?.LogWarning("No saved data found.");
            return false;
        }

        bool result = ImportCode(code);

        if (result)
            GameManager.Instance?.Log("data loaded from local.");
        else
            GameManager.Instance?.LogWarning("data load failed.");

        return result;
    }

    public void ExportCodeToInputField()
    {
        if (saveCodeInputField == null)
        {
            GameManager.Instance?.LogWarning($"{name}: SaveCodeInputField가 없습니다.");
            return;
        }

        string code = ExportCode();

        if (string.IsNullOrEmpty(code))
        {
            GameManager.Instance?.LogWarning("Export code failed.");
            return;
        }

        saveCodeInputField.text = code;

        GameManager.Instance?.Log("Export code created.");
    }

    public void ImportCodeFromInputField()
    {
        if (saveCodeInputField == null)
        {
            GameManager.Instance?.LogWarning($"{name}: SaveCodeInputField가 NRE.");
            return;
        }

        string code = saveCodeInputField.text;

        if (string.IsNullOrWhiteSpace(code))
        {
            GameManager.Instance?.LogWarning("Save code is empty.");
            return;
        }

        code = code.Trim();

        // 프리셋 접두어: ShareCodes 하위 폴더 참조 준비
        if (code.StartsWith("@"))
        {
            StartCoroutine(ImportPresetShareCodeRoutine(code));
            return;
        }

        // 프리셋 없을 경우 DG 접두어 ㄱㄱ (데이터로 읽을 준비)
        if (ImportCode(code))
            GameManager.Instance?.Log("Import code success.");
        else
            GameManager.Instance?.LogWarning("Import code failed.");
    }
    #endregion

    private MusicSaveData CaptureSaveData()
    {
        // #1 전체 음악 데이터 ---------------------------------------------------------------------------------------------
        MusicSaveData data = new MusicSaveData();

        if (rhythmManager != null)
            data.bpm = rhythmManager.Bpm;

        if (themeManager != null && themeManager.CurrentTheme != null)  // NOTE DAV: 테마 커스텀 고민 필요
            data.themeType = themeManager.CurrentTheme.themeType;

        // #2 트랙 데이터 ---------------------------------------------------------------------------------------------
        IReadOnlyList<CircleTrack> tracks = (rhythmManager != null) ? rhythmManager.Tracks : null;

        if (tracks == null)
            return data;

        for (int i = 0; i < tracks.Count; i++)
        {
            CircleTrack track = tracks[i];

            if (track == null)
                continue;

            TrackSaveData trackData = new TrackSaveData();

            Vector3 position = track.transform.position;
            trackData.x = position.x;
            trackData.y = position.y;
            trackData.z = position.z;

            trackData.radius = track.Radius;
            trackData.octave = track.CurrentOctave;
            trackData.instrumentType = track.InstrumentType;
            //trackData.isSleeping = track.IsSleeping; // 구버전 호환용
            trackData.playMode = track.PlayMode;     // 신버전 저장용

            // #3 노트 데이터 ---------------------------------------------------------------------------------------------
            IReadOnlyList<NoteBlock> notes = track.Notes;

            if (notes != null)
            {
                for (int j = 0; j < notes.Count; j++)
                {
                    NoteBlock note = notes[j];

                    if (note == null)
                        continue;

                    NoteSaveData noteData = new NoteSaveData();

                    noteData.angleDeg = note.angleDeg;          // 일관성 땜에 빼놓음
                    noteData.outwardLength = note.outwardLength;
                    noteData.pitch = note.pitch;
                    noteData.accidental = note.accidental;

                    trackData.notes.Add(noteData);
                }
            }

            data.tracks.Add(trackData);
        }

        return data;
    }

    private void ApplySaveData(MusicSaveData data)
    {
        if (data == null || rhythmManager == null)
            return;

        // NOTE DAV: 런타임용, 개별 RemoveTrackImmediately() 의 존재 이유
        rhythmManager.ClearAllTracksForLoad();

        if (rhythmManager != null)
            rhythmManager.SetBpm(data.bpm);

        if (themeManager != null)
            themeManager.SetTheme(data.themeType);

        for (int i = 0; i < data.tracks.Count; i++)
        {
            TrackSaveData trackData = data.tracks[i];

            if (trackData == null)
                continue;

            CircleTrack track = rhythmManager.CreateTrackFromSave(
                new Vector3(trackData.x, trackData.y, trackData.z),
                trackData.radius,
                trackData.instrumentType
            );

            // 트랙 존재 여부 확인
            if (track == null)
                continue;

            track.SetOctave(trackData.octave);

            // NOTE DAV: 구버전 호환용(이었는데 없앴음)
            //if (data.version <= 1)
            //    track.SetPlayMode(trackData.isSleeping ? TrackPlayMode.Sleep : TrackPlayMode.Normal);
            //else
            //    track.SetPlayMode(trackData.playMode);
            track.SetPlayMode(trackData.playMode);

            for (int j = 0; j < trackData.notes.Count; j++)
            {
                NoteSaveData noteData = trackData.notes[j];

                if (noteData == null)
                    continue;

                NoteBlock note = track.AddNote(
                    noteData.angleDeg,
                    noteData.outwardLength,
                    noteData.pitch
                );

                // 노트 존재 여부 확인
                if (note == null)
                    continue;

                note.accidental = noteData.accidental;
                track.UpdateNoteView(note);
            }
        }
    }

    private IEnumerator ImportPresetShareCodeRoutine(string presetKey)
    {
        string fileName = BuildShareCodeFileName(presetKey);

        if (string.IsNullOrEmpty(fileName))
        {
            GameManager.Instance?.LogWarning("Invalid preset key.");
            yield break;
        }

        string url = BuildShareCodeUrl(fileName);

        using UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            GameManager.Instance?.LogWarning($"Share code load failed. Url: {url}, Error: {request.error}");
            yield break;
        }

        string loadedCode = request.downloadHandler.text;

        if (string.IsNullOrWhiteSpace(loadedCode))
        {
            GameManager.Instance?.LogWarning("Loaded share code is empty.");
            yield break;
        }

        bool result = ImportCode(loadedCode.Trim());

        if (result)
            GameManager.Instance?.Log($"Preset share code imported: {presetKey}");
        else
            GameManager.Instance?.LogWarning($"Preset share code import failed: {presetKey}");
    }

    private string BuildShareCodeFileName(string presetKey)
    {
        if (string.IsNullOrWhiteSpace(presetKey))
            return string.Empty;

        string key = presetKey.Trim();

        if (key.StartsWith("@"))
            key = key.Substring(1);

        if (string.IsNullOrWhiteSpace(key))
            return string.Empty;

        return $"{key}.txt";
    }

    private string BuildShareCodeUrl(string fileName)
    {
        string baseUrl = Application.absoluteURL;

        if (string.IsNullOrEmpty(baseUrl))
            return $"{shareCodeFolderPath}/{fileName}";

        int queryIndex = baseUrl.IndexOf('?');

        if (queryIndex >= 0)
            baseUrl = baseUrl.Substring(0, queryIndex);

        int slashIndex = baseUrl.LastIndexOf('/');

        if (slashIndex >= 0)
            baseUrl = baseUrl.Substring(0, slashIndex + 1);

        return $"{baseUrl}{shareCodeFolderPath}/{fileName}";
    }

    #region R_Event
    public void OnClickSaveButton()
    {
        SaveToLocal();
    }

    public void OnClickLoadButton()
    {
        LoadFromLocal();
    }

    public void OnClickExportCodeButton()
    {
        ExportCodeToInputField();
    }

    public void OnClickImportCodeButton()
    {
        ImportCodeFromInputField();
    }
    #endregion
}