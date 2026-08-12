using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrackRemoveModePopupUI : BasePopup
{
    [Header("References")]
    [SerializeField] private RhythmManager rhythmManager;
    [SerializeField] private Camera worldCamera;

    [Header("Overlay UI")]
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("Selection")]
    [SerializeField] private float maxSelectDistance = 1.2f;
    [SerializeField] private Vector3 baseScale = Vector3.one;
    [SerializeField] private float highlightedScale = 1.08f;
    [SerializeField] private float scaleDuration = 0.12f;

    // Private Area
    private bool _isRemoveMode;
    private CircleTrack _highlightedTrack;
    private Coroutine _scaleCoroutine;

    #region R_Unity
    protected override void Awake()
    {
        base.Awake();

        if (worldCamera == null)
            worldCamera = Camera.main;

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (messageText != null)
            messageText.text = "Click on a track to remove it.";
    }

    void Update()   // 야기도 오버라이드 추가하자
    {

        if (!_isRemoveMode)
            return;

        UpdateHighlightedTrack();

        if (Input.GetMouseButtonDown(0))
            TryRemoveHighlightedTrack();

        if (Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);
    }
    #endregion

    #region R_Override
    protected override void OnOpened()
    {
        base.OnOpened();

        _isRemoveMode = true;
        _highlightedTrack = null;

        ResetAllTrackScales();
    }

    protected override void OnClosed()
    {
        base.OnClosed();

        _isRemoveMode = false;
        _highlightedTrack = null;

        StopScaleCoroutine();
        ResetAllTrackScales();
    }
    #endregion

    #region R_Public
    public void EnterRemoveMode()
    {
        if (rhythmManager == null)
            return;

        if (!rhythmManager.IsStopped)
        {
            GameManager.Instance?.LogWarning("CircleTrack can only be removed while stopped.");
            return;
        }

        Open();
    }

    #endregion

    private void UpdateHighlightedTrack()
    {
        CircleTrack nearestTrack = FindNearestTrackToMouse();

        if (nearestTrack == _highlightedTrack)
            return;

        SetHighlightedTrack(nearestTrack);
    }

    private void SetHighlightedTrack(CircleTrack targetTrack)
    {
        StopScaleCoroutine();
        ResetAllTrackScales();

        _highlightedTrack = targetTrack;

        if (_highlightedTrack == null)
            return;

        _highlightedTrack.SetHighlighted(true);

        _scaleCoroutine = StartCoroutine(ScaleRoutine(_highlightedTrack.transform, baseScale, baseScale * highlightedScale));
    }

    private CircleTrack FindNearestTrackToMouse()
    {
        if (rhythmManager == null || worldCamera == null)
            return null;

        IReadOnlyList<CircleTrack> tracks = rhythmManager.Tracks;

        if (tracks == null || tracks.Count == Const.ZERO)
            return null;

        Vector3 mouseWorldPosition = GetMouseWorldPosition();

        CircleTrack nearestTrack = null;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < tracks.Count; i++)
        {
            CircleTrack track = tracks[i];

            if (track == null)
                continue;

            float distanceFromCenter = Vector3.Distance(mouseWorldPosition, track.transform.position);

            // 원의 둘레와 마우스 사이의 거리
            float distanceFromCircle = Mathf.Abs(distanceFromCenter - track.Radius);

            if (distanceFromCircle < nearestDistance)
            {
                nearestDistance = distanceFromCircle;
                nearestTrack = track;
            }
        }

        if (nearestDistance > maxSelectDistance)
            return null;

        return nearestTrack;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;

        float zDistance = Mathf.Abs(worldCamera.transform.position.z);
        mousePosition.z = zDistance;

        Vector3 worldPosition = worldCamera.ScreenToWorldPoint(mousePosition);
        worldPosition.z = Const.ZEROF;

        return worldPosition;
    }

    private void ResetAllTrackScales()
    {
        if (rhythmManager == null)
            return;

        IReadOnlyList<CircleTrack> tracks = rhythmManager.Tracks;

        if (tracks == null)
            return;

        for (int i = 0; i < tracks.Count; i++)
        {
            CircleTrack track = tracks[i];

            if (track == null)
                continue;

            track.transform.localScale = baseScale;
            track.SetHighlighted(false);
        }
    }

    private void StopScaleCoroutine()
    {
        if (_scaleCoroutine == null)
            return;

        StopCoroutine(_scaleCoroutine);
        _scaleCoroutine = null;
    }

    private IEnumerator ScaleRoutine(Transform target, Vector3 from, Vector3 to)
    {
        float elapsed = 0f;

        while (elapsed < scaleDuration)
        {
            if (target == null)
                yield break;

            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / scaleDuration);

            // ease out
            t = 1f - Mathf.Pow(1f - t, 3f);

            target.localScale = Vector3.LerpUnclamped(from, to, t);

            yield return null;
        }

        if (target != null)
            target.localScale = to;

        _scaleCoroutine = null;
    }

    private void TryRemoveHighlightedTrack()
    {
        if (_highlightedTrack == null)
            return;

        CircleTrack target = _highlightedTrack;

        StopScaleCoroutine();

        _highlightedTrack = null;

        if (target != null)
        {
            target.SetHighlighted(false);
            target.transform.localScale = baseScale;
        }

        bool removed = false;

        if (rhythmManager != null)
            removed = rhythmManager.RemoveTrack(target);

        if (removed)
        {
            Close();
        }
        else
        {
            SetHighlightedTrack(null);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        maxSelectDistance = Mathf.Max(0.01f, maxSelectDistance);
        highlightedScale = Mathf.Max(1f, highlightedScale);
        scaleDuration = Mathf.Max(0.001f, scaleDuration);
    }
#endif
}