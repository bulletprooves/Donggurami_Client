using UnityEngine;

public class TrackFocusCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;

    [Header("Focus")]
    [SerializeField] private Vector3 focusOffset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float focusOrthographicSize = 5f;
    [SerializeField] private float smoothTime = 0.25f;

    // Private Area
    private CircleTrack targetTrack;
    private Vector3 defaultPosition;
    private Quaternion defaultRotation;
    private float defaultOrthographicSize;
    private float defaultFieldOfView;
    private Vector3 positionVelocity;
    private float zoomVelocity;
    private float currentFocusScale = 1f;

    // Properties
    public CircleTrack TargetTrack => targetTrack;
    public System.Action<float> OnFocusScaleChanged;

    #region R_Unity
    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
            return;

        defaultPosition = targetCamera.transform.position;
        defaultRotation = targetCamera.transform.rotation;
        defaultOrthographicSize = targetCamera.orthographicSize;
        defaultFieldOfView = targetCamera.fieldOfView;
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
            return;

        Vector3 targetPosition = defaultPosition;
        Quaternion targetRotation = defaultRotation;
        float targetZoom = defaultOrthographicSize;

        if (targetTrack != null && targetTrack.Arrow != null)
        {
            targetPosition = targetTrack.Arrow.position + focusOffset;
            targetRotation = targetTrack.Arrow.rotation;
            //targetRotation = defaultRotation;
            targetZoom = focusOrthographicSize;
        }

        targetCamera.transform.position = Vector3.SmoothDamp(targetCamera.transform.position, targetPosition, ref positionVelocity, smoothTime);

        targetCamera.transform.rotation = Quaternion.Slerp(targetCamera.transform.rotation, targetRotation, Time.deltaTime / smoothTime);

        float nextFocusScale = 1f;
        if (targetCamera.orthographic)
        {
            targetCamera.orthographicSize = Mathf.SmoothDamp(targetCamera.orthographicSize, targetZoom, ref zoomVelocity, smoothTime);

            if (targetCamera.orthographicSize > Const.ZEROF)
                nextFocusScale = defaultOrthographicSize / targetCamera.orthographicSize;
        }
        else
        {
            targetCamera.fieldOfView = Mathf.SmoothDamp(targetCamera.fieldOfView, targetTrack != null ? 35f : defaultFieldOfView, ref zoomVelocity, smoothTime);

            if (targetCamera.fieldOfView > Const.ZEROF)
                nextFocusScale = defaultFieldOfView / targetCamera.fieldOfView;
        }

        if (!Mathf.Approximately(currentFocusScale, nextFocusScale))
        {
            currentFocusScale = nextFocusScale;
            OnFocusScaleChanged?.Invoke(currentFocusScale);
        }

    }
    #endregion

    #region R_Public
    public void SetTarget(CircleTrack track)
    {
        targetTrack = track;
    }
    #endregion
}