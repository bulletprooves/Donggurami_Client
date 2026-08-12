using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortalWorldToggleController : MonoBehaviour
{
    private enum PortalViewMode
    {
        Default = 0,
        AlternateWorld = 1,
        HideTrackLens = 2
    }

    [Header("References")]
    [SerializeField] private Button worldButton;
    [SerializeField] private Image worldButtonImage;
    [SerializeField] private PortalLensView transitionLens;
    [SerializeField] private PortalLensManager portalLensManager;

    [Header("Mode Images")]
    [SerializeField] private Sprite defaultModeSprite;
    [SerializeField] private Sprite alternateWorldModeSprite;
    [SerializeField] private Sprite hideTrackLensModeSprite;

    [Header("Animation")]
    [SerializeField] private float expandDuration = 0.25f;
    [SerializeField] private float shrinkDuration = 0.2f;
    [SerializeField] private float closedDiameter = 0f;

    [SerializeField] private float lensAppearInterval = 0.05f;
    [SerializeField] private float lensAppearDuration = 0.15f;

    [Header("Track Lens Bounce")]
    [SerializeField]
    private AnimationCurve lensBounceCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.45f, 1.2f),
        new Keyframe(0.7f, 0.9f),
        new Keyframe(0.88f, 1.04f),
        new Keyframe(1f, 1f)
    );

    private Coroutine _animationCoroutine;
    private PortalViewMode _currentMode = PortalViewMode.Default;
    private bool _isAlternateWorld;

    private void Awake()
    {
        if (worldButton != null)
            worldButton.onClick.AddListener(OnClickWorldButton);

        ApplyModeImmediate(_currentMode);
        RefreshButtonImage();
    }

    private void OnDestroy()
    {
        if (worldButton != null)
            worldButton.onClick.RemoveListener(OnClickWorldButton);
    }

    private IReadOnlyList<PortalLensView> trackLenses
    {
        get
        {
            if (portalLensManager == null)
                return null;

            return portalLensManager.TrackLenses;
        }
    }

    private void OnClickWorldButton()
    {
        int nextModeIndex = ((int)_currentMode + 1) % 3;

        if (_currentMode == (PortalViewMode)nextModeIndex)
            return;

        _currentMode = (PortalViewMode)nextModeIndex;

        RefreshButtonImage();

        if (_animationCoroutine != null)
            StopCoroutine(_animationCoroutine);

        portalLensManager?.SetTrackLensDiameterLocked(true);

        switch (_currentMode)
        {
            case PortalViewMode.Default:
                SetAlternateWorld(false, true);
                break;

            case PortalViewMode.AlternateWorld:
                SetAlternateWorld(true, false);
                break;

            case PortalViewMode.HideTrackLens:
                SetAlternateWorld(false, false);
                break;
        }
    }

    private void ApplyModeImmediate(PortalViewMode mode)
    {
        _currentMode = mode;
        _isAlternateWorld = mode == PortalViewMode.AlternateWorld;

        if (transitionLens != null)
            transitionLens.SetDiameter(_isAlternateWorld ? transitionLens.GetFullScreenDiameter() : closedDiameter);

        switch (_currentMode)
        {
            case PortalViewMode.Default:
                portalLensManager?.SetTrackLensesVisible(true);
                break;

            case PortalViewMode.AlternateWorld:
                portalLensManager?.SetTrackLensesVisible(false);
                break;

            case PortalViewMode.HideTrackLens:
                portalLensManager?.SetTrackLensesVisible(false);
                break;
        }
    }

    private void SetAlternateWorld(bool isAlternateWorld, bool showTrackLensesAfterClose)
    {
        _isAlternateWorld = isAlternateWorld;

        if (transitionLens == null)
            return;

        if (_isAlternateWorld)
            portalLensManager?.SetTrackLensesVisible(false);
        else if (!showTrackLensesAfterClose)
            portalLensManager?.SetTrackLensesVisible(false);

        float targetDiameter = isAlternateWorld ? transitionLens.GetFullScreenDiameter() : closedDiameter;
        float duration = isAlternateWorld ? expandDuration : shrinkDuration;

        _animationCoroutine = StartCoroutine(AnimateDiameter(targetDiameter, duration, showTrackLensesAfterClose));
    }

    //private void SetTrackLensesVisible(bool isVisible, bool setDiameterImmediately)
    //{
    //    if (trackLenses == null)
    //        return;

    //    for (int i = 0; i < trackLenses.Count; i++)
    //    {
    //        PortalLensView lens = trackLenses[i];

    //        if (lens == null)
    //            continue;

    //        if (setDiameterImmediately)
    //        {
    //            float diameter = isVisible
    //                ? GetCurrentTrackLensDiameter()
    //                : 0f;

    //            lens.SetDiameter(diameter);
    //        }

    //        lens.gameObject.SetActive(isVisible);
    //    }
    //}


    private float GetCurrentTrackLensDiameter()
    {
        if (portalLensManager != null)
            return portalLensManager.CurrentTrackLensDiameter;

        return 200f;
    }

    private void RefreshButtonImage()
    {
        if (worldButtonImage == null)
            return;

        switch (_currentMode)
        {
            case PortalViewMode.Default:
                worldButtonImage.sprite = defaultModeSprite;
                break;

            case PortalViewMode.AlternateWorld:
                worldButtonImage.sprite = alternateWorldModeSprite;
                break;

            case PortalViewMode.HideTrackLens:
                worldButtonImage.sprite = hideTrackLensModeSprite;
                break;
        }
    }

    #region R_Animation
    private IEnumerator AnimateDiameter(float targetDiameter, float duration, bool showTrackLensesAfterClose)
    {
        float startDiameter = transitionLens.Diameter;
        float elapsedTime = Const.ZEROF;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float t = duration > Const.ZEROF ? Mathf.Clamp01(elapsedTime / duration) : 1f;
            float easedT = 1f - Mathf.Pow(1f - t, 3f);
            transitionLens.SetDiameter(Mathf.Lerp(startDiameter, targetDiameter, easedT));

            yield return null;
        }

        transitionLens.SetDiameter(targetDiameter);

        if (showTrackLensesAfterClose)
        {
            portalLensManager?.SetTrackLensesVisible(true);
            yield return ShowTrackLenses();
        }

        portalLensManager?.SetTrackLensDiameterLocked(false);
        _animationCoroutine = null;
    }

    private IEnumerator ShowTrackLenses()
    {
        if (trackLenses == null)
            yield break;

        for (int i = 0; i < trackLenses.Count; i++)
        {
            PortalLensView lens = trackLenses[i];

            if (lens == null)
                continue;

            if (portalLensManager != null && !portalLensManager.CanShowLens(lens))
            {
                lens.SetDiameter(Const.ZEROF);
                lens.gameObject.SetActive(false);
                continue;
            }

            lens.SetDiameter(Const.ZEROF);
            lens.gameObject.SetActive(true);

            StartCoroutine(AnimateTrackLensAppear(lens));

            yield return new WaitForSecondsRealtime(lensAppearInterval);
        }
    }

    private IEnumerator AnimateTrackLensAppear(PortalLensView lens)
    {
        float targetDiameter = GetCurrentTrackLensDiameter();
        float elapsedTime = Const.ZEROF;

        while (elapsedTime < lensAppearDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = lensAppearDuration > Const.ZEROF ? Mathf.Clamp01(elapsedTime / lensAppearDuration) : 1f;
            float scale = lensBounceCurve.Evaluate(t);

            lens.SetDiameter(targetDiameter * scale);

            yield return null;
        }

        lens.SetDiameter(targetDiameter);
    }
    #endregion
}