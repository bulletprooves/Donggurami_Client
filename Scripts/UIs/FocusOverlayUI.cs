using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FocusOverlayUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private RectTransform topDim;
    [SerializeField] private RectTransform bottomDim;
    [SerializeField] private RectTransform leftDim;
    [SerializeField] private RectTransform rightDim;
    [SerializeField] private RectTransform centerBorder;

    [SerializeField] private float borderPunchScale = 1.35f;
    [SerializeField] private float borderPunchDuration = 0.18f;

    [Header("Settings")]
    [SerializeField] private bool hideOnAwake = true;

    // Private Area
    private RectTransform _targetRect;
    private Coroutine _borderPunchCoroutine;
    private Vector2 _targetBorderSize;

    #region R_Unity
    private void Awake()
    {
        if (canvasRect == null)
            canvasRect = GetComponentInParent<Canvas>()?.GetComponent<RectTransform>();

        if (hideOnAwake)
            Hide();
    }
    #endregion

    #region R_Public
    public void Show(RectTransform targetRect)
    {
        if (targetRect == null || canvasRect == null)
            return;

        this._targetRect = targetRect;

        gameObject.SetActive(true);

        Refresh();
    }

    public void Hide()
    {
        _targetRect = null;

        if (_borderPunchCoroutine != null)
        {
            StopCoroutine(_borderPunchCoroutine);
            _borderPunchCoroutine = null;
        }

        if (centerBorder != null)
            centerBorder.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }

    public void Refresh()
    {
        if (_targetRect == null || canvasRect == null)
            return;

        Vector3[] targetCorners = new Vector3[4];
        Vector3[] canvasCorners = new Vector3[4];

        // 배열 순서: bl, tl, tr, br
        canvasRect.GetWorldCorners(canvasCorners);
        _targetRect.GetWorldCorners(targetCorners);

        Vector2 cMin = canvasRect.InverseTransformPoint(canvasCorners[0]);
        Vector2 cMax = canvasRect.InverseTransformPoint(canvasCorners[2]);
        Vector2 tMin = canvasRect.InverseTransformPoint(targetCorners[0]);
        Vector2 tMax = canvasRect.InverseTransformPoint(targetCorners[2]);

        float cL = cMin.x;
        float cB = cMin.y;
        float cR = cMax.x;
        float cT = cMax.y;

        float tL = tMin.x;
        float tB = tMin.y;
        float tR = tMax.x;
        float tT = tMax.y;

        SetScale(
            topDim,
            cL,
            tT,
            cR,
            cT
        );

        SetScale(
            bottomDim,
            cL,
            cB,
            cR,
            tB
        );

        SetScale(
            leftDim,
            cL,
            tB,
            tL,
            tT
        );

        SetScale(
            rightDim,
            tR,
            tB,
            cR,
            tT
        );

        SetScale(
            centerBorder,
            tL,
            tB,
            tR,
            tT
        );

        // 포커스 중앙 테부리 애니메이션
        _targetBorderSize = centerBorder.sizeDelta;
        PlayCenterBorderPunch();
    }
    #endregion

    private void SetScale(RectTransform panel, float l, float b, float r, float t)
    {
        if (panel == null)
            return;

        float w = Mathf.Max(Const.ZEROF, r - l);
        float h = Mathf.Max(Const.ZEROF, t - b);

        Vector2 v = new Vector2(Const.HALF, Const.HALF);
        panel.gameObject.SetActive(w > Const.ZEROF && h > Const.ZEROF);             // 0 이면 비활성
        panel.anchorMin = v;                                                        // 0.5
        panel.anchorMax = v;                                                        // 0.5
        panel.pivot = v;                                                            // 0.5
        panel.anchoredPosition = new Vector2(l + w * Const.HALF, b + h * Const.HALF);//중앙 위치
        panel.sizeDelta = new Vector2(w, h);
    }

    private void PlayCenterBorderPunch()
    {
        if (centerBorder == null)
            return;

        if (_borderPunchCoroutine != null)
            StopCoroutine(_borderPunchCoroutine);

        _borderPunchCoroutine = StartCoroutine(CenterBorderPunchRoutine());
    }

    private IEnumerator CenterBorderPunchRoutine()
    {
        // 시작 큼 -> 끝 작아짐
        Vector2 startSize = _targetBorderSize * borderPunchScale;
        Vector2 endSize = _targetBorderSize;

        centerBorder.sizeDelta = startSize;

        float elapsed = Const.ZEROF;

        while (elapsed < borderPunchDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = borderPunchDuration > Const.ZEROF ? Mathf.Clamp01(elapsed / borderPunchDuration) : Const.IDENTITY;
            t = 1f - Mathf.Pow(1f - t, 3f); // 세제곱 보간 (ease out cubic)
            centerBorder.sizeDelta = Vector2.LerpUnclamped(startSize, endSize, t);

            yield return null;
        }

        centerBorder.sizeDelta = endSize;
        _borderPunchCoroutine = null;
    }
}