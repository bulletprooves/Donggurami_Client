using System.Collections.Generic;
using UnityEngine;



public class CircleTrack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Config config;
    [SerializeField] private Transform arrow;
    [SerializeField] private LineRenderer highlightRing;
    [SerializeField] private List<NoteBlock> notes = new();
    [SerializeField] private Transform notePrefab;
    [SerializeField] private Transform noteRoot;
    [SerializeField] private Transform gridTickPrefab;
    [SerializeField] private Transform gridRoot;
    [SerializeField] private Transform centerRoot;
    [SerializeField] private Transform arrowRoot;

    [Header("Track")]
    [SerializeField] private float radius = 3f;
    [SerializeField] private float highlightRingOffset = 0.15f;

    [Header("Grid")]
    [SerializeField] private bool showSnapGrid = true;
    [SerializeField] private int snapDivision = 32;
    [SerializeField] private float gridTickLength = 0.25f;
    [SerializeField] private bool buildGridOnStart = true;
    [SerializeField] private bool showRuntimeGrid = true;

    [Header("Octave")]
    [SerializeField] private int currentOctave = 3;
    [SerializeField] private float centerClickRadius = 0.6f;

    [Header("Note Shape")]
    [SerializeField] private float noteWidth = 0.75f;

    [SerializeField] private float angleDeg;

    [SerializeField] private int minOctave = 1;
    [SerializeField] private int maxOctave = 7;

    [SerializeField] private float normalGridTickWidth = 0.05f;
    [SerializeField] private float normalGridTickLength = 0.25f;
    [SerializeField] private float majorGridTickLength = 0.45f;

    [Header("Sleep")]
    [Header("Track Play Mode")]
    [SerializeField] private TrackPlayMode playMode = TrackPlayMode.Normal;
    [SerializeField] private GameObject sleepSpriteObject;
    [SerializeField] private GameObject sleepDimSpriteObject;
    [SerializeField] private GameObject halfVolumeSpriteObject;
    [SerializeField] private PortalLensManager portalLensManager;

    public TrackPlayMode PlayMode => playMode;
    public bool IsSleeping => playMode == TrackPlayMode.Sleep;
    public float VolumeMultiplier => playMode == TrackPlayMode.HalfVolume ? 0.4375f : 1f;


    [Header("Theme Targets")]

    [SerializeField] private InstrumentType instrumentType = InstrumentType.GrandPiano;

    public float Radius => radius;
    public float AngleDeg => angleDeg;
    public int SnapDivision => Mathf.Max(Const.COUNT_MIN, snapDivision);
    public int CurrentOctave => currentOctave;
    public float CenterClickRadius => centerClickRadius;
    public bool IsGridVisible => showRuntimeGrid;
    public Transform Arrow => arrow;
    public InstrumentType InstrumentType => instrumentType;
    public IReadOnlyList<NoteBlock> Notes
    {
        get
        {
            EnsureNoteHelpers();
            return _noteStore.Notes;
        }
    }


    private readonly List<Transform> _gridTicks = new List<Transform>();
    private PortalLensManager _portalLensManager;
    private CircleTrackNoteStore _noteStore;
    private CircleTrackNoteViewController _noteViewController;
    private CircleTrackPlaybackCursor _playbackCursor;
    private CircleTrackThemeController _themeController;

    #region R_ContextMenu
    [ContextMenu("Refresh Note Views")]
    private void RefreshNoteViews()
    {
        EnsureNoteHelpers();
        _noteViewController.RefreshViews(_noteStore.Notes);
    }

    [ContextMenu("Rebuild Grid View")]
    public void RebuildGridView()
    {
        ClearGridView();

        if (gridTickPrefab == null)
        {
            GameManager.Instance.LogWarning($"{name}: gridTickPrefab 없음");
            return;
        }

        Transform parent = gridRoot != null ? gridRoot : transform;

        int division = SnapDivision;
        float stepDeg = Const.PI_DEG * 2f / division;

        for (int i = 0; i < division; i++)
        {
            float tickAngleDeg = i * stepDeg;

            Transform tick = Instantiate(gridTickPrefab, parent);
            tick.name = $"GridTick_{i:00}_{tickAngleDeg:0.##}deg";
            _gridTicks.Add(tick);

            if (tick == null)
                return;

            float rad = tickAngleDeg * Const.DEG2RAD;
            Vector3 outwardDir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), Const.ZEROF);

            tick.localPosition = outwardDir * radius;
            tick.localRotation = Quaternion.Euler(Const.ZEROF, Const.ZEROF, tickAngleDeg - Const.PI_DEG * Const.HALF);

            float tickLength = normalGridTickLength;

            if (config != null)
            {
                int majorGridSnapCount = SnapDivision < config.MajorGridSnapThreshold ? config.MinGridSnapCount : config.MaxGridSnapCount;

                if (i % majorGridSnapCount == Const.ZERO)
                    tickLength = majorGridTickLength;
            }

            tick.localScale = new Vector3(normalGridTickWidth, tickLength, Const.IDENTITY);
            tick.gameObject.SetActive(showRuntimeGrid);
        }
    }
    #endregion

    #region R_Unity
    private void Awake()
    {
        if (arrowRoot != null)
            arrow = arrowRoot;

        if (_portalLensManager == null)
            _portalLensManager = portalLensManager;

        if (_portalLensManager == null)
            _portalLensManager = FindFirstObjectByType<PortalLensManager>();

        EnsureNoteHelpers();
        EnsureThemeController();

        if (config != null)
        {
            noteWidth = config.NoteWidth;
            currentOctave = config.DefaultOctave;
            minOctave = config.MinOctave;
            maxOctave = config.MaxOctave;
            centerClickRadius = config.CenterClickRadius;

            normalGridTickWidth = config.NormalGridTickWidth;
            normalGridTickLength = config.NormalGridTickLength;
            majorGridTickLength = config.MajorGridTickLength;
        }

        _noteViewController?.Configure(notePrefab, noteRoot, transform, radius, noteWidth);
    }

    private void Start()
    {
        EnsureNoteHelpers();
        _noteStore.RebuildLookup(SnapDivision);
        _playbackCursor.Reset(_noteStore, angleDeg);
        _noteViewController.CreateViews(_noteStore.Notes);

        ApplyNoteThemes();

        if (buildGridOnStart)
            RebuildGridView();

        SetupHighlightRing();
        SetHighlighted(false);
        SetPlayMode(playMode);
    }

    private void OnDestroy()
    {
        ClearGridView();
        _noteViewController?.Clear();
    }
    #endregion

    public void ApplyTheme(WorldTheme theme)
    {
        EnsureNoteHelpers();
        EnsureThemeController();

        _themeController.Apply(theme, _noteStore.Notes, _noteViewController);
    }

    private void EnsureThemeController()
    {
        if (_themeController == null)
        {
            _themeController = new CircleTrackThemeController(
                value => arrow = value,
                UpdateArrowTransform
            );
        }

        _themeController.Configure(centerRoot, arrowRoot);
    }

    private void ApplyNoteThemes()
    {
        EnsureNoteHelpers();
        EnsureThemeController();

        _themeController.ApplyNoteThemes(_noteStore.Notes, _noteViewController);
    }
    #region R_Grid
    public void SetGridVisible(bool isVisible)
    {
        showRuntimeGrid = isVisible;

        foreach (Transform tick in _gridTicks)
        {
            if (tick == null)
                continue;

            tick.gameObject.SetActive(showRuntimeGrid);
        }
    }

    private void SetupHighlightRing()
    {
        if (highlightRing == null)
            return;

        highlightRing.useWorldSpace = false;
        highlightRing.loop = true;

        int segmentCount = 64;
        float ringRadius = radius + highlightRingOffset;
        highlightRing.positionCount = segmentCount;

        for (int i = 0; i < segmentCount; i++)
        {
            float t = i / (float)segmentCount;
            float angleRad = t * Mathf.PI * 2f;
            Vector3 localPosition = new Vector3(
                Mathf.Cos(angleRad) * ringRadius,
                Mathf.Sin(angleRad) * ringRadius,
                Const.ZEROF
            );

            highlightRing.SetPosition(i, localPosition);
        }
    }

    public void SetHighlighted(bool isHighlighted)
    {
        if (highlightRing == null)
            return;

        if (highlightRing.gameObject.activeSelf != isHighlighted)
            highlightRing.gameObject.SetActive(isHighlighted);
    }

    private void ClearGridView()
    {
        for (int i = _gridTicks.Count - 1; i >= 0; i--)
        {
            Transform tick = _gridTicks[i];

            if (tick == null)
                continue;

            if (Application.isPlaying)
                Destroy(tick.gameObject);
            else
                DestroyImmediate(tick.gameObject);
        }

        _gridTicks.Clear();
    }
    #endregion

    #region R_Playback
    public void CollectTriggeredNotes(float linearSpeed, float deltaTime, List<NotePlayRequest> notePlayRequests)
    {
        if (arrow == null)
            return;

        EnsureNoteHelpers();

        float angularSpeedRad = linearSpeed / radius;
        float angularSpeedDeg = angularSpeedRad * Mathf.Rad2Deg;

        angleDeg -= angularSpeedDeg * deltaTime;
        angleDeg = Utility.NormalizeAngle(angleDeg);

        //if (angleDeg > _previousAngleDeg)
        //    lapCount++;

        if (!IsSleeping)
            if (!IsSleeping)
            {
                _playbackCursor.CollectPassedNotes(
    _noteStore,
    angleDeg,
    currentOctave,
    instrumentType,
    VolumeMultiplier,
    notePlayRequests
);
            }

        UpdateArrowTransform();
    }

    public void ResetPlayback()
    {
        EnsureNoteHelpers();

        angleDeg = Const.ZEROF;

        _noteStore.ResetTriggerStates();
        _noteStore.RebuildLookup(SnapDivision);
        _playbackCursor.Reset(_noteStore, angleDeg);

        UpdateArrowTransform();
    }

    public void CollectCurrentNote(List<NotePlayRequest> notePlayRequests)
    {
        if (notePlayRequests == null)
            return;

        // 잠든 상태의 트랙은 재생 안 할 거얌
        if (IsSleeping)
            return;

        EnsureNoteHelpers();
        _noteStore.RebuildLookup(SnapDivision);

        NoteBlock note = _noteStore.FindByAngle(angleDeg);

        if (note == null)
            return;

        notePlayRequests.Add(new NotePlayRequest(
    note.pitch,
    currentOctave,
    note.accidental,
    instrumentType,
    VolumeMultiplier
));
    }

    public void SetPlayMode(TrackPlayMode mode)
    {
        playMode = mode;

        bool isSleep = playMode == TrackPlayMode.Sleep;
        bool isHalfVolume = playMode == TrackPlayMode.HalfVolume;

        if (arrowRoot != null)
            arrowRoot.gameObject.SetActive(!isSleep);
        else if (arrow != null)
            arrow.gameObject.SetActive(!isSleep);

        if (sleepSpriteObject != null)
            sleepSpriteObject.SetActive(isSleep);

        if (sleepDimSpriteObject != null)
        {
            sleepDimSpriteObject.SetActive(isSleep);

            if (isSleep)
                sleepDimSpriteObject.transform.localScale = new Vector3(radius, radius, Const.IDENTITY);
        }

        if (halfVolumeSpriteObject != null)
            halfVolumeSpriteObject.SetActive(isHalfVolume);

        if (_portalLensManager != null)
            _portalLensManager.SetLensVisibleForTrack(this, !isSleep);

        GameManager.Instance?.Log($"{name} PlayMode: {playMode}");
    }

    public void CyclePlayMode()
    {
        switch (playMode)
        {
            case TrackPlayMode.Normal:
                SetPlayMode(TrackPlayMode.Sleep);
                break;

            case TrackPlayMode.Sleep:
                SetPlayMode(TrackPlayMode.HalfVolume);
                break;

            case TrackPlayMode.HalfVolume:
                SetPlayMode(TrackPlayMode.Normal);
                break;
        }
    }
    #endregion

    #region R_Note
    public NoteBlock FindNoteByAngle(float targetAngleDeg)
    {
        EnsureNoteHelpers();
        _noteStore.RebuildLookup(SnapDivision);

        return _noteStore.FindByAngle(targetAngleDeg);
    }

    public NoteBlock AddNote(float targetAngleDeg, float outwardLength, int pitch)
    {
        EnsureNoteHelpers();
        _noteStore.RebuildLookup(SnapDivision);

        NoteBlock note = _noteStore.Add(targetAngleDeg, outwardLength, pitch);
        _noteViewController.Create(note);

        ApplyNoteThemes();

        return note;
    }

    public bool RemoveNote(NoteBlock note)
    {
        EnsureNoteHelpers();

        if (!_noteStore.Remove(note))
            return false;

        _noteViewController.Destroy(note);
        return true;
    }

    public bool RemoveNearestNote(Vector3 worldPosition, float maxDistance)
    {
        EnsureNoteHelpers();

        NoteBlock nearestNote = null;
        float nearestDistance = float.MaxValue;
        IReadOnlyList<NoteBlock> noteList = _noteStore.Notes;

        for (int i = 0; i < noteList.Count; i++)
        {
            NoteBlock note = noteList[i];
            Transform noteView = _noteViewController.Get(note);

            if (noteView == null)
                continue;

            float distance = Vector3.Distance(worldPosition, noteView.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestNote = note;
            }
        }

        if (nearestNote == null || nearestDistance > maxDistance)
            return false;

        _noteViewController.Destroy(nearestNote);
        _noteStore.Remove(nearestNote);

        return true;
    }
    #endregion

    #region R_NoteView
    public void UpdateNoteView(NoteBlock note)
    {
        EnsureNoteHelpers();
        _noteViewController.Update(note);
    }

    private void EnsureNoteHelpers()
    {
        if (_noteStore == null)
            _noteStore = new CircleTrackNoteStore(notes, () => name);

        if (_noteViewController == null)
            _noteViewController = new CircleTrackNoteViewController(this);

        if (_playbackCursor == null)
            _playbackCursor = new CircleTrackPlaybackCursor(name);

        _noteViewController?.Configure(notePrefab, noteRoot, transform, radius, noteWidth);
    }

    #endregion

    #region R_Set
    public void SetInstrument(InstrumentType value)
    {
        instrumentType = value;
    }

    public void SetOctave(int value)
    {
        currentOctave = Mathf.Clamp(value, minOctave, maxOctave);
    }

    public void IncreaseOctave()
    {
        currentOctave++;

        if (currentOctave > maxOctave)
            currentOctave = minOctave;

        GameManager.Instance.Log($"{name} Octave Changed: {currentOctave}");
    }

    public void SetRadius(float value)
    {
        EnsureNoteHelpers();

        radius = Mathf.Max(0.01f, value);
        snapDivision = CalculateSnapDivisionByRadius(radius);

        _noteStore.RebuildLookup(SnapDivision);

        if (arrow != null)
            UpdateArrowTransform();

        _noteViewController?.Configure(notePrefab, noteRoot, transform, radius, noteWidth);
        _noteViewController.RefreshViews(_noteStore.Notes);

        if (Application.isPlaying && buildGridOnStart)
            RebuildGridView();

        SetupHighlightRing();
    }

    public void ToggleSleeping()
    {
        CyclePlayMode();
    }

    public void SetSleeping(bool value)
    {
        SetPlayMode(value ? TrackPlayMode.Sleep : TrackPlayMode.Normal);
    }
    #endregion

    #region R_Utility
    public float WorldPositionToAngleDeg(Vector3 worldPosition)
    {
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);

        float angleRad = Mathf.Atan2(localPosition.y, localPosition.x);
        float worldAngleDeg = angleRad * Mathf.Rad2Deg;

        return Utility.NormalizeAngle(worldAngleDeg);
    }

    private int CalculateSnapDivisionByRadius(float targetRadius)
    {
        if (config == null)
            return snapDivision;

        int calculatedSnapDivision = Mathf.RoundToInt(targetRadius * config.FactorSnapDivision);

        return Mathf.Clamp(calculatedSnapDivision, config.MinSnapDivision, config.MaxSnapDivision);
    }

    private void UpdateArrowTransform()
    {
        if (arrow == null)
            return;

        float rad = angleDeg * Mathf.Deg2Rad;

        arrow.localPosition = new Vector3(
            Mathf.Cos(rad) * radius,
            Mathf.Sin(rad) * radius,
            Const.ZEROF
        );

        arrow.localRotation = Quaternion.Euler(
            Const.ZEROF,
            Const.ZEROF,
            angleDeg - Const.PI_DEG * Const.HALF
        );
    }
    #endregion

#if UNITY_EDITOR
    private void OnValidate()
    {
        radius = Mathf.Max(0.01f, radius);
        snapDivision = Mathf.Max(1, snapDivision);

        if (config != null)
        {
            gridTickLength = Mathf.Max(0.01f, config.NormalGridTickLength);
            noteWidth = Mathf.Max(0.01f, config.NoteWidth);
        }
        else
        {
            gridTickLength = Mathf.Max(0.01f, gridTickLength);
            noteWidth = Mathf.Max(0.01f, noteWidth);
        }

        if (arrow != null)
            UpdateArrowTransform();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        const int segmentCount = 128;

        Vector3 prevPoint = transform.position + new Vector3(radius, Const.ZEROF, Const.ZEROF);

        for (int i = 1; i <= segmentCount; i++)
        {
            float t = i / (float)segmentCount;
            float angle = Utility.TwoPiRadius(t);
            Vector3 nextPoint = transform.position + new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                Const.ZEROF
            );

            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }

        if (!showSnapGrid)
            return;

        Gizmos.color = Color.gray;

        int division = Mathf.Max(1, snapDivision);
        float stepDeg = Const.PI_DEG * 2 / division;

        for (int i = 0; i < division; i++)
        {
            float tickAngleDeg = i * stepDeg;
            float rad = tickAngleDeg * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), Const.ZEROF);

            Vector3 inner = transform.position + dir * (radius - gridTickLength * Const.HALF);
            Vector3 outer = transform.position + dir * (radius + gridTickLength * Const.HALF);

            Gizmos.DrawLine(inner, outer);
        }
    }
#endif
}
