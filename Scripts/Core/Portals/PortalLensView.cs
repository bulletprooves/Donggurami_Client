using UnityEngine;
using UnityEngine.UI;

public class PortalLensView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Transform followTarget;
    [SerializeField] private RectTransform lensRect;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private RectTransform alternateWorldImageRect;

    [Header("Shader")]
    [SerializeField] private RawImage alternateWorldImage;
    [SerializeField] private Material portalLensMaterial;

    [Header("Settings")]
    [SerializeField] private Vector2 screenOffset;
    [SerializeField] private bool followMouse = true;
    [SerializeField] private float diameter;

    // Proprties
    public float Diameter => diameter;

    // Private Area
    private Material _runtimeMaterial;

    #region R_Unity
    private void Awake()
    {
        InitializeMaterial();

        SetDiameter(diameter);
    }

    private void OnEnable()
    {
        if (_runtimeMaterial == null)
            InitializeMaterial();
    }

    private void LateUpdate()
    {
        if (lensRect == null || alternateWorldImageRect == null || canvasRect == null)
            return;

        Vector2 pos = new Vector2();
        if (followMouse)
            pos = Input.mousePosition;
        else if (followTarget != null && worldCamera != null)
            pos = worldCamera.WorldToScreenPoint(followTarget.position);
        else if (followTarget == null)  // followTarget가 null이면 화면 중앙으로 이동
            pos = canvasRect.rect.center;

        // 렌즈 포지션 (전체 효과는 늘 가운데로 직접 인스펙터에서 설정합세)
        pos += screenOffset;
        Camera uiCamera = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = canvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, pos, uiCamera, out Vector2 localPosition))
            lensRect.anchoredPosition = localPosition;

        // Note Dav: 반대로 움직여야 함
        alternateWorldImageRect.sizeDelta = canvasRect.rect.size;
        alternateWorldImageRect.anchoredPosition = -lensRect.anchoredPosition;

        UpdateShaderProperties();
    }

    private void OnDestroy()
    {
        if (_runtimeMaterial == null)
            return;

        if (Application.isPlaying)
            Destroy(_runtimeMaterial);
        else
            DestroyImmediate(_runtimeMaterial);

        _runtimeMaterial = null;
    }
    #endregion

    #region R_Get
    public float GetFullScreenDiameter()
    {
        if (canvasRect == null)
            return Const.ZEROF;

        Vector2 size = canvasRect.rect.size;

        return Mathf.Sqrt(size.x * size.x + size.y * size.y);
    }
    #endregion

    #region R_Set
    public void SetDiameter(float value)
    {
        diameter = Mathf.Max(Const.ZEROF, value);

        if (lensRect != null)
            lensRect.sizeDelta = Vector2.one * diameter;
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
    #endregion

    private void InitializeMaterial()
    {
        if (_runtimeMaterial != null)
            return;

        if (alternateWorldImage == null)
        {
            GameManager.Instance.LogError($"{name}: AlternateWorldImage가 없음!!!");
            return;
        }

        // 별도의 매터리얼이 없으면 복사해서 ㄱㄱ
        Material sourceMaterial = portalLensMaterial;

        if (sourceMaterial == null)
            sourceMaterial = alternateWorldImage.material;

        if (sourceMaterial == null)
        {
            GameManager.Instance.LogError($"{name}: PortalLensMaterial이 없습니다.");
            return;
        }

        _runtimeMaterial = new Material(sourceMaterial)
        {
            name = $"{sourceMaterial.name}_{name}_Runtime"
        };

        alternateWorldImage.material = _runtimeMaterial;
    }

    public void Initialize(Canvas targetCanvas, RectTransform targetCanvasRect, Camera targetWorldCamera, Transform targetFollowTarget)
    {
        canvas = targetCanvas;
        canvasRect = targetCanvasRect;
        worldCamera = targetWorldCamera;

        SetFollowTarget(targetFollowTarget);

        UpdateShaderProperties();
    }

    private void UpdateShaderProperties()
    {
        if (_runtimeMaterial == null || lensRect == null || canvasRect == null)
            return;

        Vector2 canvasSize = canvasRect.rect.size;

        if (canvasSize.x <= Const.ZEROF || canvasSize.y <= Const.ZEROF)
            return;

        Vector2 centerUv = new Vector2(lensRect.anchoredPosition.x / canvasSize.x + Const.HALF, lensRect.anchoredPosition.y / canvasSize.y + Const.HALF);

        float radiusUv = lensRect.rect.width * Const.HALF / canvasSize.y;
        float screenAspect = canvasSize.x / canvasSize.y;

        _runtimeMaterial.SetVector("_LensCenter", new Vector4(centerUv.x, centerUv.y, Const.ZEROF, Const.ZEROF));
        _runtimeMaterial.SetFloat("_LensRadius", radiusUv);
        _runtimeMaterial.SetFloat("_ScreenAspect", screenAspect);
    }
}