using UnityEngine;
using UnityEngine.UI;

public class PlaybackControlUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RhythmManager rhythmManager;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button stopButton;

    #region R_Unity
    private void Awake()
    {
        if (playButton != null)
            playButton.onClick.AddListener(OnClickPlay);

        if (pauseButton != null)
            pauseButton.onClick.AddListener(OnClickPause);

        if (stopButton != null)
            stopButton.onClick.AddListener(OnClickStop);
    }

    private void OnDestroy()
    {
        if (playButton != null)
            playButton.onClick.RemoveListener(OnClickPlay);

        if (pauseButton != null)
            pauseButton.onClick.RemoveListener(OnClickPause);

        if (stopButton != null)
            stopButton.onClick.RemoveListener(OnClickStop);
    }
    #endregion

    private void OnClickPlay()
    {
        if (rhythmManager == null)
            return;

        rhythmManager.Play();
    }

    private void OnClickPause()
    {
        if (rhythmManager == null)
            return;

        rhythmManager.Pause();
    }

    private void OnClickStop()
    {
        if (rhythmManager == null)
            return;

        rhythmManager.Stop();
    }
}