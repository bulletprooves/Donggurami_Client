using UnityEngine;

public class TrackFocusFollower : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private bool followPosition = true;
    [SerializeField] private bool followRotation = true;

    public CircleTrack TargetTrack { get; private set; }

    #region R_Unity
    private void LateUpdate()
    {
        if (TargetTrack == null || TargetTrack.Arrow == null)
            return;

        if (followPosition)
            transform.position = TargetTrack.Arrow.position;

        if (followRotation)
            transform.rotation = TargetTrack.Arrow.rotation;
    }
    #endregion

    #region R_Public
    public void SetTarget(CircleTrack targetTrack)
    {
        TargetTrack = targetTrack;
    }
    #endregion
}