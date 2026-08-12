using System.Collections.Generic;
using UnityEngine;

public class PortalLensManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PortalLensView trackLensPrefab;
    [SerializeField] private Transform lensRoot;
    [SerializeField] private TrackFocusCameraController focusCameraController;

    [Header("Shared References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private Camera worldCamera;

    [Header("Settings")]
    [SerializeField] private float defaultTrackLensDiameter = 200f;

    private readonly Dictionary<CircleTrack, PortalLensView> _lensByTrack = new();
    private readonly List<PortalLensView> _trackLenses = new();

    private float currentFocusScale = 1f;
    private bool isTrackLensDiameterLocked;

    private bool isTrackLensVisible = true;

    public IReadOnlyList<PortalLensView> TrackLenses => _trackLenses;
    public float CurrentTrackLensDiameter => defaultTrackLensDiameter * currentFocusScale;
    public bool IsTrackLensVisible => isTrackLensVisible;

    private void Awake()
    {
        if (canvas == null)
            canvas = FindFirstObjectByType<Canvas>();

        if (canvasRect == null && canvas != null)
            canvasRect = canvas.GetComponent<RectTransform>();

        if (worldCamera == null)
            worldCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (focusCameraController != null)
            focusCameraController.OnFocusScaleChanged += HandleFocusScaleChanged;
    }

    private void OnDisable()
    {
        if (focusCameraController != null)
            focusCameraController.OnFocusScaleChanged -= HandleFocusScaleChanged;
    }

    public bool CanShowLens(PortalLensView lens)
    {
        if (lens == null)
            return false;

        foreach (KeyValuePair<CircleTrack, PortalLensView> pair in _lensByTrack)
        {
            CircleTrack track = pair.Key;
            PortalLensView targetLens = pair.Value;

            if (targetLens != lens)
                continue;

            if (track == null)
                return false;

            return isTrackLensVisible && !track.IsSleeping;
        }

        return false;
    }

    public void SetTrackLensDiameterLocked(bool isLocked)
    {
        isTrackLensDiameterLocked = isLocked;

        if (!isTrackLensDiameterLocked)
            SetAllTrackLensDiameters(CurrentTrackLensDiameter);
    }

    public void CreateLensesForExistingTracks(IReadOnlyList<CircleTrack> tracks)
    {
        if (tracks == null)
            return;

        for (int i = 0; i < tracks.Count; i++)
        {
            CreateLensForTrack(tracks[i]);
        }
    }

    public void SetLensVisibleForTrack(CircleTrack track, bool visible)
    {
        if (track == null)
            return;

        if (!_lensByTrack.TryGetValue(track, out PortalLensView lens))
            return;

        if (lens == null)
            return;

        bool finalVisible = visible && isTrackLensVisible;

        if (finalVisible)
        {
            lens.SetDiameter(CurrentTrackLensDiameter);
            lens.gameObject.SetActive(true);
        }
        else
        {
            lens.SetDiameter(0f);
            lens.gameObject.SetActive(false);
        }
    }

    public PortalLensView CreateLensForTrack(CircleTrack track)
    {
        if (track == null)
            return null;

        if (_lensByTrack.TryGetValue(track, out PortalLensView existingLens))
            return existingLens;

        if (track.Arrow == null)
        {
            GameManager.Instance.LogWarning($"{track.name}: Arrow가 없어서 PortalLens를 만들 수 없습니다.");
            return null;
        }

        if (trackLensPrefab == null)
        {
            GameManager.Instance.LogWarning($"{name}: Track Lens Prefab이 없습니다.");
            return null;
        }

        Transform parent = lensRoot != null ? lensRoot : transform;

        PortalLensView lens = Instantiate(trackLensPrefab, parent);
        lens.name = $"PortalLens_{track.name}";

        lens.Initialize(canvas, canvasRect, worldCamera, track.Arrow);

        bool shouldShowLens = isTrackLensVisible && !track.IsSleeping;

        if (shouldShowLens)
        {
            lens.SetDiameter(CurrentTrackLensDiameter);
            lens.gameObject.SetActive(true);
        }
        else
        {
            lens.SetDiameter(Const.ZEROF);
            lens.gameObject.SetActive(false);
        }

        _lensByTrack.Add(track, lens);
        _trackLenses.Add(lens);

        return lens;
    }

    public bool RemoveLensForTrack(CircleTrack track)
    {
        if (track == null)
            return false;

        if (!_lensByTrack.TryGetValue(track, out PortalLensView lens))
            return false;

        _lensByTrack.Remove(track);
        _trackLenses.Remove(lens);

        if (lens != null)
            Destroy(lens.gameObject);

        return true;
    }

    public void RefreshLensTarget(CircleTrack track)
    {
        if (track == null)
            return;

        if (!_lensByTrack.TryGetValue(track, out PortalLensView lens))
            return;

        if (lens == null)
            return;

        lens.SetFollowTarget(track.Arrow);
    }

    public void SetAllTrackLensesVisible(bool visible)
    {
        for (int i = 0; i < _trackLenses.Count; i++)
        {
            PortalLensView lens = _trackLenses[i];

            if (lens == null)
                continue;

            lens.gameObject.SetActive(visible);
        }
    }

    public void SetAllTrackLensDiameters(float diameter)
    {
        for (int i = 0; i < _trackLenses.Count; i++)
        {
            PortalLensView lens = _trackLenses[i];

            if (lens == null)
                continue;

            lens.SetDiameter(diameter);
        }
    }

    public void SetTrackLensesVisible(bool visible)
    {
        isTrackLensVisible = visible;

        foreach (KeyValuePair<CircleTrack, PortalLensView> pair in _lensByTrack)
        {
            CircleTrack track = pair.Key;
            PortalLensView lens = pair.Value;

            if (track == null || lens == null)
                continue;

            bool shouldShowLens = visible && !track.IsSleeping;

            if (shouldShowLens)
            {
                lens.SetDiameter(CurrentTrackLensDiameter);
                lens.gameObject.SetActive(true);
            }
            else
            {
                lens.SetDiameter(0f);
                lens.gameObject.SetActive(false);
            }
        }
    }

    #region R_Callback
    private void HandleFocusScaleChanged(float focusScale)
    {
        currentFocusScale = focusScale;

        if (isTrackLensDiameterLocked)
            return;

        SetAllTrackLensDiameters(CurrentTrackLensDiameter);
    }
    #endregion
}