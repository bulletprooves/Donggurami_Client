using UnityEngine;

public class NoteEditArea : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform editAreaRect;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Camera uiCamera;

    #region R_Unity
    private void Awake()
    {
        if (editAreaRect == null)
            editAreaRect = GetComponent<RectTransform>();

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
            return;

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = null;
        }
        else
        {
            uiCamera = canvas.worldCamera;
        }
    }
    #endregion

    #region R_Public
    public bool ContainsScreenPoint(Vector2 screenPoint)
    {
        if (editAreaRect == null)
            return false;

        return RectTransformUtility.RectangleContainsScreenPoint(editAreaRect, screenPoint, uiCamera);
    }
    #endregion
}