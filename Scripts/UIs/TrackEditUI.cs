using UnityEngine;
using UnityEngine.UI;

public class TrackEditUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RhythmManager rhythmManager;
    [SerializeField] private CreateTrackPopupUI createTrackPopup;
    [SerializeField] private TrackRemoveModePopupUI trackRemoveModeUI;

    [Header("Buttons")]
    [SerializeField] private Button addTrackButton;
    [SerializeField] private Button removeTrackButton;

    #region R_Unity
    private void Awake()
    {
        if (addTrackButton != null)
            addTrackButton.onClick.AddListener(OnClickAddTrack);

        if (removeTrackButton != null)
            removeTrackButton.onClick.AddListener(OnClickRemoveTrack);
    }

    private void Update()
    {
        if (rhythmManager == null)
            return;

        if (addTrackButton != null)
            addTrackButton.interactable = rhythmManager.IsStopped;

        if (removeTrackButton != null)
            removeTrackButton.interactable = rhythmManager.IsStopped;
    }

    private void OnDestroy()
    {
        if (addTrackButton != null)
            addTrackButton.onClick.RemoveListener(OnClickAddTrack);

        if (removeTrackButton != null)
            removeTrackButton.onClick.RemoveListener(OnClickRemoveTrack);
    }
    #endregion


    #region R_Callback
    private void OnClickAddTrack()
    {
        if (rhythmManager == null || !rhythmManager.IsStopped)
            return;

        if (createTrackPopup != null)
            createTrackPopup.Open();
    }

    private void OnClickRemoveTrack()
    {
        if (rhythmManager == null || !rhythmManager.IsStopped)
            return;

        if (trackRemoveModeUI != null)
            trackRemoveModeUI.EnterRemoveMode();
    }
    #endregion
}