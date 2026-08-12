using UnityEngine;
using UnityEngine.UI;

public class PortalLens : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform lensRect;
    [SerializeField] private RectTransform fullScreenImageRect;
    [SerializeField] private RawImage alternateWorldImage;

    [Header("Follow")]
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Transform followTarget;
    [SerializeField] private bool followMouse;

    [Header("Settings")]
    [SerializeField] private float diameter = 300f;
    [SerializeField] private Vector2 screenOffset;

    private RectTransform _canvasRect;
    private Canvas _canvas;

    public float Diameter => diameter;

    private void Awake()
    {
        if (lensRect == null)
            lensRect = transform as RectTransform;

        _canvas = GetComponentInParent<Canvas>();

        if (_canvas != null)
            _canvasRect = _canvas.transform as RectTransform;

        ApplyDiameter();
    }

    private void LateUpdate()
    {
        UpdateLensPosition();
        UpdateFullScreenImagePosition();
    }

    public void SetEnabled(bool isEnabled)
    {
        gameObject.SetActive(isEnabled);
    }

    public void SetDiameter(float value)
    {
        diameter = Mathf.Max(1f, value);
        ApplyDiameter();
    }

    public void SetFollowTarget(Transform target)
    {
        followTarget = target;
        followMouse = false;
    }

    public void SetFollowMouse(bool value)
    {
        followMouse = value;
    }

    public void SetScreenPosition(Vector2 screenPosition)
    {
        followMouse = false;
        followTarget = null;

        SetLensScreenPosition(screenPosition);
    }

    private void ApplyDiameter()
    {
        if (lensRect == null)
            return;

        lensRect.sizeDelta = Vector2.one * diameter;
    }

    private void UpdateLensPosition()
    {
        Vector2 screenPosition;

        if (followMouse)
        {
            screenPosition = Input.mousePosition;
        }
        else if (followTarget != null && worldCamera != null)
        {
            screenPosition = worldCamera.WorldToScreenPoint(
                followTarget.position
            );
        }
        else
        {
            return;
        }

        screenPosition += screenOffset;

        SetLensScreenPosition(screenPosition);
    }

    private void SetLensScreenPosition(Vector2 screenPosition)
    {
        if (_canvasRect == null || lensRect == null)
            return;

        Camera uiCamera = null;

        if (_canvas != null &&
            _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = _canvas.worldCamera;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect,
            screenPosition,
            uiCamera,
            out Vector2 localPosition
        ))
        {
            lensRect.anchoredPosition = localPosition;
        }
    }

    private void UpdateFullScreenImagePosition()
    {
        if (_canvasRect == null ||
            lensRect == null ||
            fullScreenImageRect == null)
        {
            return;
        }

        /*
         * RawImage는 화면 전체 크기를 유지한다.
         * Lens가 오른쪽으로 이동하면 RawImage는 Lens 내부에서
         * 왼쪽으로 같은 거리만큼 이동해야 동일한 화면 위치가 보인다.
         */
        fullScreenImageRect.sizeDelta = _canvasRect.rect.size;
        fullScreenImageRect.anchoredPosition = -lensRect.anchoredPosition;
    }
}