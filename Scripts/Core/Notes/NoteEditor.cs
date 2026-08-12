using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NoteEditor : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private Config config;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private RhythmManager rhythmManager;
    [SerializeField] private NoteEditArea noteEditArea;

    [Header("Default Note Settings")]
    [SerializeField] private float minOutwardLength = 0.5f;
    [SerializeField] private float maxOutwardLength = 4f;
    [SerializeField] private float maxDrumOutwardLength = 3.5f;

    [Header("Pitch Mapping")]
    [SerializeField] private float pitchUnitLength = 0.5f;
    [SerializeField] private int minPitch = 0;
    [SerializeField] private int maxPitch = 14;

    [Header("Click Settings")]
    [SerializeField] private float maxDistanceFromCircle = 2.0f;

    [Header("Snap Settings")]
    [SerializeField] private bool useAngleSnap = true;

    [Header("Remove Settings")]
    [SerializeField] private float removeMaxDistance = 3.5f;

    [Header("Preview")]
    [SerializeField] private Transform previewPrefab;
    [SerializeField] private Transform previewRoot;
    [SerializeField] private bool showPreview = true;
    [SerializeField] private float previewOutwardLength = 1f;
    [SerializeField] private float previewWidth = 0.15f;

    [Header("Audio Info")]
    [SerializeField] private RhythmAudioPlayer audioPlayer;

    [Header("Note Name UI")]
    [SerializeField] private TextMeshProUGUI noteNameText;
    [SerializeField] private Vector2 noteNameScreenOffset = new Vector2(24f, 24f);

    [Header("Track Move")]
    [SerializeField] private float centerDragStartDistance = 0.15f;
    [SerializeField] private float moveSmoothTime = 0.04f;

    [Header("Octave Hover UI")]
    [SerializeField] private TextMeshProUGUI octaveHoverText;
    [SerializeField] private Vector2 octaveHoverScreenOffset = new Vector2(24f, -24f);

    [SerializeField] private TextMeshProUGUI previewIndexText;
    [SerializeField] private Vector2 previewIndexScreenOffset = new Vector2(24f, -48f);
    [SerializeField] private bool useOneBasedPreviewIndex = false;

    [Header("Save")]
    [SerializeField] private MusicSaveManager songSaveManager;

    // Private Area
    private bool _isDragging;
    private NoteBlock _editingNote;
    private CircleTrack _editingTrack;
    private float _editingAngleDeg;

    private Transform _previewView;
    private CircleTrack _previewTrack;
    private float _previewAngleDeg;

    private bool _isCenterPressed;
    private bool _isMovingTrack;
    private CircleTrack _movingTrack;
    private Vector3 _centerPressMouseWorldPosition;
    private Vector3 _centerPressTrackWorldPosition;
    private Vector3 _moveVelocity;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (config == null)
            return;

        minOutwardLength = config.MinOutwardLength;
        maxOutwardLength = config.MaxOutwardLength;
        maxDrumOutwardLength = config.MaxDrumOutwardLength;
        pitchUnitLength = config.PitchUnitLength;

        maxDistanceFromCircle = config.MaxDistanceFromCircle;
        removeMaxDistance = config.RemoveMaxDistance;

        centerDragStartDistance = config.CenterDragStartDistance;
        moveSmoothTime = config.MoveSmoothTime;

        previewOutwardLength = config.PreviewOutwardLength;
        previewWidth = config.PreviewWidth;

        if (previewPrefab == null)
            return;

        _previewView = Instantiate(previewPrefab, previewRoot);
        _previewView.name = "NotePreview";

        _previewView.gameObject.SetActive(false);

        if (noteEditArea == null)
        {
            Debug.LogWarning("NoteEditAreaÍ∞Ä ?ÜÏùå!");
        }
    }

    private void Update()
    {
        // ?∏Ìä∏ ?∏Ïßë Ï§ëÏù∏ÏßÄ ?ïÏù∏
        bool isEditingNow = _isDragging || _isCenterPressed || _isMovingTrack;

        // ?ùÏóÖ???¥Î†§ ?àÏúºÎ©??ÖÎ†• Ï∞®Îã®
        if (UIInputBlocker.Instance.IsBlocked)
        {
            HidePreview();
            HideNoteNameText();
            return;
        }

        // ÎßàÏö∞???ÑÏπò?????ÑÎ†à???àÏóê?úÎäî Í∞ôÏúºÎØÄÎ°???Î≤àÎßå Í≤Ä??
        bool isMouseInsideEditArea = noteEditArea != null && noteEditArea.ContainsScreenPoint(Input.mousePosition);

        // ?∏Ìä∏ ÎØ∏Î¶¨Î≥¥Í∏∞ ?ÖÎç∞?¥Ìä∏
        bool canShowPreview = !isEditingNow && isMouseInsideEditArea && showPreview && _previewView != null && targetCamera != null;

        if (canShowPreview)
        {
            Vector3 mouseWorldPosition = GetMouseWorldPosition();
            CircleTrack nearestTrack = FindNearestTrack(mouseWorldPosition);

            if (nearestTrack != null)
            {
                float rawAngleDeg = nearestTrack.WorldPositionToAngleDeg(mouseWorldPosition);

                float snappedAngleDeg = SnapAngle(rawAngleDeg, nearestTrack.SnapDivision);

                _previewTrack = nearestTrack;
                _previewAngleDeg = snappedAngleDeg;

                float rad = snappedAngleDeg * Const.DEG2RAD;

                Vector3 outwardDirection = new Vector3(
                    Mathf.Cos(rad),
                    Mathf.Sin(rad),
                    Const.ZEROF
                );

                Vector3 localPosition = outwardDirection * (nearestTrack.Radius + previewOutwardLength * Const.HALF);

                Vector3 worldPosition = nearestTrack.transform.TransformPoint(localPosition);

                Quaternion worldRotation =
                    nearestTrack.transform.rotation *
                    Quaternion.Euler(
                        Const.ZEROF,
                        Const.ZEROF,
                        snappedAngleDeg - 90f
                    );

                _previewView.SetParent(previewRoot);
                _previewView.position = worldPosition;
                _previewView.rotation = worldRotation;
                _previewView.localScale = Vector3.one;

                NoteView noteView = _previewView.GetComponent<NoteView>();

                if (noteView != null)
                {
                    noteView.SetSize(previewWidth, previewOutwardLength);
                    noteView.SetAccidental(NoteAccidental.None);
                }

                _previewView.gameObject.SetActive(true);
                UpdatePreviewIndexText(snappedAngleDeg, nearestTrack.SnapDivision);
            }
            else
            {
                HidePreview();
            }
        }
        else
        {
            HidePreview();
        }

        // ?•Ì?Î∏??∏Î≤Ñ ?çÏä§??
        if (!isEditingNow)
            UpdateOctaveHoverText();
        else
            HideOctaveHoverText();

        // ÎßàÏö∞???∞ÌÅ¥Î¶? ?∏Ìä∏ ?úÍ±∞
        if (Input.GetMouseButtonDown(Const.MOUSE_RIGHT))
        {
            if (targetCamera == null || rhythmManager == null)
                return;

            Vector3 mouseWorldPosition = GetMouseWorldPosition();
            CircleTrack centerTrack = FindCenterClickedTrack(mouseWorldPosition);

            if (centerTrack != null)
            {
                centerTrack.CyclePlayMode();
                HidePreview();
                HideNoteNameText();
                HideOctaveHoverText();
                return;
            }

            if (isMouseInsideEditArea && _previewTrack != null)
            {
                NoteBlock targetNote = _previewTrack.FindNoteByAngle(_previewAngleDeg);

                if (targetNote != null && _previewTrack.RemoveNote(targetNote))
                {
                    GameManager.Instance.Log($"?∏Ìä∏ ÏßÄ?†Ï™Ñ??/ " + $"Track: {_previewTrack.name}, " + $"Angle: {_previewAngleDeg}");
                    HidePreview();
                }
            }
        }

        // ÎßàÏö∞??Ï¢åÌÅ¥Î¶??úÏûë
        if (Input.GetMouseButtonDown(Const.MOUSE_LEFT))
        {
            if (targetCamera == null || rhythmManager == null)
                return;

            Vector3 mouseWorldPosition = GetMouseWorldPosition();
            CircleTrack centerTrack = FindCenterClickedTrack(mouseWorldPosition);

            // ?∏Îûô Ï§ëÏã¨ ?¥Î¶≠
            if (centerTrack != null)
            {
                _isCenterPressed = true;
                _isMovingTrack = false;
                _movingTrack = centerTrack;

                _centerPressMouseWorldPosition = mouseWorldPosition;
                _centerPressTrackWorldPosition = centerTrack.transform.position;
                _moveVelocity = Vector3.zero;

                HidePreview();
                return;
            }

            // ?∏Ìä∏ ?∏Ïßë ?ÅÏó≠ Î∞îÍπ•?¥Î©¥ ?ùÏÑ±?òÏ? ?äÏùå
            if (!isMouseInsideEditArea)
                return;

            // ?∏Ìä∏ ?ùÏÑ± ?úÏûë
            CircleTrack nearestTrack = _previewTrack != null ? _previewTrack : FindNearestTrack(mouseWorldPosition);

            if (nearestTrack != null)
            {
                _editingTrack = nearestTrack;

                if (_previewTrack == _editingTrack)
                {
                    _editingAngleDeg = _previewAngleDeg;
                }
                else
                {
                    float rawAngleDeg = _editingTrack.WorldPositionToAngleDeg(mouseWorldPosition);
                    _editingAngleDeg = SnapAngle(rawAngleDeg, _editingTrack.SnapDivision);
                }

                NoteBlock existingNote = _editingTrack.FindNoteByAngle(_editingAngleDeg);

                // ?¥Î? ?∏Ìä∏Í∞Ä ?àÏúºÎ©??ÑÏãú??Î≥ÄÍ≤?
                if (existingNote != null)
                {
                    existingNote.CycleAccidental();
                    _editingTrack.UpdateNoteView(existingNote);

                    GameManager.Instance.Log(
                        $"Cycle Accidental / " +
                        $"Track: {_editingTrack.name}, " +
                        $"Angle: {_editingAngleDeg}, " +
                        $"Accidental: {existingNote.accidental}"
                    );

                    _editingNote = null;
                    _editingTrack = null;
                    _isDragging = false;

                    HidePreview();
                    HideNoteNameText();
                }
                else
                {
                    // ???∏Ìä∏ ?ùÏÑ±
                    _editingNote = _editingTrack.AddNote(_editingAngleDeg, minOutwardLength, minPitch);

                    GameManager.Instance.Log($"Create New Note / " + $"Track: {_editingTrack.name}, " + $"SnapAngle: {_editingAngleDeg}");
                    _isDragging = true;

                    HidePreview();
                    UpdateNoteNameText(_editingNote.pitch, _editingTrack.CurrentOctave, _editingNote.accidental);
                }
            }
        }

        // ÎßàÏö∞??Ï¢åÌÅ¥Î¶??†Ï?
        if (Input.GetMouseButton(Const.MOUSE_LEFT))
        {
            // ?∏Îûô ?¥Îèô
            if (_isCenterPressed && _movingTrack != null)
            {
                Vector3 mouseDelta = GetMouseWorldPosition() - _centerPressMouseWorldPosition;

                if (!_isMovingTrack && mouseDelta.magnitude >= centerDragStartDistance)
                    _isMovingTrack = true;

                if (_isMovingTrack)
                {
                    Vector3 targetPosition = _centerPressTrackWorldPosition + mouseDelta;

                    _movingTrack.transform.position = Vector3.SmoothDamp(
                            _movingTrack.transform.position,
                            targetPosition,
                            ref _moveVelocity,
                            moveSmoothTime
                        );
                }
            }
            // ?∏Ìä∏ ?ºÏπò ?∏Ïßë
            else if (!_isCenterPressed && !_isMovingTrack && _isDragging && _editingNote != null && _editingTrack != null)
            {
                Vector3 localMousePosition = _editingTrack.transform.InverseTransformPoint(GetMouseWorldPosition());

                // ?úÎüº???åÎ? Íµ¨Î∂Ñ!!! NOTE DAV: ?¨Í∏∞ ?òÏ§ë??Íº?'?úÎüºÎ•òÎßå'?ºÎ°ú maxOutwardLengthÎ•??òÏ†ï?òÎèÑÎ°?!!
                float outwardLength = Mathf.Clamp(localMousePosition.magnitude - _editingTrack.Radius, minOutwardLength,
                    (_editingTrack.InstrumentType == InstrumentType.Drum_Blues) ? maxDrumOutwardLength : maxOutwardLength);
                int pitch = Mathf.FloorToInt((outwardLength - minOutwardLength) / pitchUnitLength);
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
                float snappedOutwardLength = minOutwardLength + pitch * pitchUnitLength;
                _editingNote.outwardLength = snappedOutwardLength;
                _editingNote.pitch = pitch;
                _editingTrack.UpdateNoteView(_editingNote);

                UpdateNoteNameText(pitch, _editingTrack.CurrentOctave, _editingNote.accidental);
            }
        }

        // ÎßàÏö∞??Ï¢åÌÅ¥Î¶??¥Ï†ú
        if (Input.GetMouseButtonUp(Const.MOUSE_LEFT))
        {
            // ?∏Îûô Ï§ëÏã¨ ?¥Î¶≠ ?êÎäî ?¥Îèô Ï¢ÖÎ£å
            if (_isCenterPressed)
            {
                if (_movingTrack != null)
                {
                    if (_isMovingTrack)
                    {
                        GameManager.Instance.Log($"Move Track End / " + $"Track: {_movingTrack.name}, " + $"Position: {_movingTrack.transform.position}");
                    }
                    else
                    {
                        _movingTrack.IncreaseOctave();
                        UpdateOctaveHoverText();
                    }
                }

                _isCenterPressed = false;
                _isMovingTrack = false;
                _movingTrack = null;
                _moveVelocity = Vector3.zero;

                return;
            }

            // ?∏Ìä∏ ?ùÏÑ± Ï¢ÖÎ£å
            if (_isDragging)
            {
                _isDragging = false;

                if (_editingNote != null &&
                    _editingTrack != null)
                {
                    GameManager.Instance.Log(
                        $"End Note / " +
                        $"Track: {_editingTrack.name}, " +
                        $"Angle: {_editingNote.angleDeg}, " +
                        $"Length: {_editingNote.outwardLength}, " +
                        $"Pitch: {_editingNote.pitch}"
                    );
                }

                _editingNote = null;
                _editingTrack = null;

                HideNoteNameText();
            }
        }
    }

    private void UpdateOctaveHoverText()
    {
        if (octaveHoverText == null || targetCamera == null)
            return;

        if (UIInputBlocker.Instance != null && UIInputBlocker.Instance.IsBlocked)
        {
            HideOctaveHoverText();
            return;
        }

        Vector3 mouseWorldPosition = GetMouseWorldPosition();

        CircleTrack centerTrack = FindCenterClickedTrack(mouseWorldPosition);

        if (centerTrack == null)
        {
            HideOctaveHoverText();
            return;
        }

        // ?úÎüºÎ•òÏùº ?åÎßå, ?§Î•¥Í≤??úÏãú
        if (centerTrack.InstrumentType == InstrumentType.Drum_Blues)
            octaveHoverText.text = Utility.GetDrumNameByOctave(centerTrack.CurrentOctave);
        else
            octaveHoverText.text = $"Octave {centerTrack.CurrentOctave}";

        octaveHoverText.gameObject.SetActive(true);

        Vector2 screenPosition = Input.mousePosition;
        octaveHoverText.rectTransform.position = screenPosition + octaveHoverScreenOffset;
    }

    private void HideOctaveHoverText()
    {
        if (octaveHoverText != null)
            octaveHoverText.gameObject.SetActive(false);
    }

    private void HidePreview()
    {
        if (_previewView != null && _previewView.gameObject.activeSelf)
            _previewView.gameObject.SetActive(false);

        HidePreviewIndexText();

        _previewTrack = null;
    }

    private void UpdatePreviewIndexText(float snappedAngleDeg, int snapDivision)
    {
        if (previewIndexText == null)
            return;

        int index = GetSnapIndex(snappedAngleDeg, snapDivision);

        if (useOneBasedPreviewIndex)
            index++;

        previewIndexText.text = index.ToString();
        previewIndexText.gameObject.SetActive(true);

        Vector2 screenPosition = Input.mousePosition;
        previewIndexText.rectTransform.position = screenPosition + previewIndexScreenOffset;
    }

    private void HidePreviewIndexText()
    {
        if (previewIndexText != null)
            previewIndexText.gameObject.SetActive(false);
    }

    private int GetSnapIndex(float snappedAngleDeg, int snapDivision)
    {
        int division = Mathf.Max(Const.COUNT_MIN, snapDivision);
        float step = Const.PI_DEG * 2f / division;

        /*
        int index = Mathf.RoundToInt(snappedAngleDeg / step);
        index %= division;
        if (index < 0)
            index += division;
        return index;
        */

        // NOTE DAV: ?∏Îç±?§Îäî ?êÎûò Î∞òÏãúÍ≥ÑÎ∞©?•ÏúºÎ°?(ref: Í∑πÏ¢å?? Ï¶ùÍ??? ?òÏ?Îß??†Ï?Í∞Ä Î≥¥Í∏∞ ?∏ÌïòÍ≤??úÍ≥ÑÎ∞©Ìñ•?ºÎ°ú ?∏Îç±???úÏãú??
        int cntrClockwiseIdx = Mathf.RoundToInt(snappedAngleDeg / step);
        cntrClockwiseIdx %= division;

        if (cntrClockwiseIdx < Const.ZERO)
            cntrClockwiseIdx += division;

        int clockwiseIndex = (division - cntrClockwiseIdx) % division;

        return clockwiseIndex;
    }

    private void UpdateNoteNameText(int pitch, int octave, NoteAccidental accidental)
    {
        if (noteNameText == null || audioPlayer == null)
            return;

        string noteName = audioPlayer.GetNoteName(pitch, octave, accidental);

        noteNameText.text = noteName;
        noteNameText.gameObject.SetActive(true);

        Vector2 screenPosition = Input.mousePosition;
        noteNameText.rectTransform.position = screenPosition + noteNameScreenOffset;
    }

    private void HideNoteNameText()
    {
        if (noteNameText != null)
        {
            noteNameText.gameObject.SetActive(false);
        }
    }

    private float SnapAngle(float angleDeg, int division)
    {
        if (!useAngleSnap)
            return angleDeg;

        float step = Const.PI_DEG * 2 / Mathf.Max(Const.COUNT_MIN, division);
        float snappedAngle = Mathf.Round(angleDeg / step) * step;

        return Utility.NormalizeAngle(snappedAngle);
    }

    private IReadOnlyList<CircleTrack> GetTracks()
    {
        if (rhythmManager == null)
            return null;

        return rhythmManager.Tracks;
    }

    private CircleTrack FindNearestTrack(Vector3 worldPosition)
    {
        IReadOnlyList<CircleTrack> tracks = GetTracks();

        if (tracks == null || tracks.Count == Const.ZERO)
            return null;

        CircleTrack nearestTrack = null;
        float nearestDistanceFromCircle = float.MaxValue;

        foreach (CircleTrack track in tracks)
        {
            if (track == null)
                continue;

            Vector3 localPosition =
                track.transform.InverseTransformPoint(worldPosition);

            float distanceFromCenter = localPosition.magnitude;

            float distanceFromCircle = Mathf.Abs(distanceFromCenter - track.Radius);

            if (distanceFromCircle < nearestDistanceFromCircle)
            {
                nearestDistanceFromCircle = distanceFromCircle;
                nearestTrack = track;
            }
        }

        if (nearestTrack == null)
            return null;

        if (nearestDistanceFromCircle > maxDistanceFromCircle)
            return null;

        return nearestTrack;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        float distanceFromCamera = Mathf.Abs(targetCamera.transform.position.z);
        mouseScreenPosition.z = distanceFromCamera;

        return targetCamera.ScreenToWorldPoint(mouseScreenPosition);
    }

    private CircleTrack FindCenterClickedTrack(Vector3 worldPosition)
    {
        CircleTrack nearestTrack = null;
        float nearestDistance = float.MaxValue;

        IReadOnlyList<CircleTrack> tracks = GetTracks();

        if (tracks == null)
            return null;

        foreach (CircleTrack track in tracks)
        {
            if (track == null)
                continue;

            Vector3 localPosition = track.transform.InverseTransformPoint(worldPosition);
            float distanceFromCenter = localPosition.magnitude;

            if (distanceFromCenter <= track.CenterClickRadius &&
                distanceFromCenter < nearestDistance)
            {
                nearestDistance = distanceFromCenter;
                nearestTrack = track;
            }
        }

        return nearestTrack;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        minOutwardLength = Mathf.Max(0.01f, minOutwardLength);
        maxOutwardLength = Mathf.Max(minOutwardLength, maxOutwardLength);
        pitchUnitLength = Mathf.Max(0.01f, pitchUnitLength);

        minPitch = Mathf.Max(0, minPitch);
        maxPitch = Mathf.Max(minPitch, maxPitch);

        maxDistanceFromCircle = Mathf.Max(0.01f, maxDistanceFromCircle);
        removeMaxDistance = Mathf.Max(0.01f, removeMaxDistance);

        previewOutwardLength = Mathf.Max(minOutwardLength, previewOutwardLength);
        previewWidth = Mathf.Max(0.01f, previewWidth);

        centerDragStartDistance = Mathf.Max(0.01f, centerDragStartDistance);
        moveSmoothTime = Mathf.Max(0.001f, moveSmoothTime);
    }
#endif
}