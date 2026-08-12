using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrackFocusToggleUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RhythmManager rhythmManager;
    [SerializeField] private Toggle togglePrefab;
    [SerializeField] private Transform toggleRoot;
    [SerializeField] private TrackFocusCameraController focusCameraController;

    // Private Area
    private readonly Dictionary<CircleTrack, Toggle> _toggleMap = new();
    private readonly List<CircleTrack> _tracks = new();

    #region R_Unity
    private void OnEnable()
    {
        if (rhythmManager == null)
            return;

        rhythmManager.OnTrackCreated += HandleTrackCreated;
        rhythmManager.OnTrackRemoved += HandleTrackRemoved;
    }

    private void OnDisable()
    {
        if (rhythmManager == null)
            return;

        rhythmManager.OnTrackCreated -= HandleTrackCreated;
        rhythmManager.OnTrackRemoved -= HandleTrackRemoved;
    }

    private void Start()
    {
        if (rhythmManager != null)
        {
            IReadOnlyList<CircleTrack> tracks = rhythmManager.Tracks;

            for (int i = 0; i < tracks.Count; i++)
            {
                HandleTrackCreated(tracks[i]);
            }

            RefreshLabels();
        }

}
    #endregion


    private void HandleTrackCreated(CircleTrack track)
    {
        if (track == null || togglePrefab == null || toggleRoot == null)
            return;

        if (_toggleMap.ContainsKey(track))
            return;

        Toggle toggle = Instantiate(togglePrefab, toggleRoot);
        toggle.isOn = false;

        _tracks.Add(track);
        _toggleMap.Add(track, toggle);

        CircleTrack capturedTrack = track;

        toggle.onValueChanged.AddListener(isOn =>
        {
            if (isOn)
                SelectTrack(capturedTrack);
            else
                TryClearFocus(capturedTrack);
        });

        RefreshLabels();
    }

    private void HandleTrackRemoved(CircleTrack track)
    {
        if (track == null)
            return;

        if (_toggleMap.TryGetValue(track, out Toggle toggle))
        {
            if (toggle != null)
                Destroy(toggle.gameObject);

            _toggleMap.Remove(track);
        }

        _tracks.Remove(track);

        if (focusCameraController != null && focusCameraController.TargetTrack == track)
            focusCameraController.SetTarget(null);

        RefreshLabels();
    }

    private void SelectTrack(CircleTrack selectedTrack)
    {
        foreach (KeyValuePair<CircleTrack, Toggle> pair in _toggleMap)
        {
            CircleTrack track = pair.Key;
            Toggle toggle = pair.Value;

            if (toggle == null)
                continue;

            if (track != selectedTrack)
                toggle.isOn = false;
        }

        if (focusCameraController != null)
            focusCameraController.SetTarget(selectedTrack);
    }

    private void TryClearFocus(CircleTrack track)
    {
        if (focusCameraController == null)
            return;

        if (focusCameraController.TargetTrack != track)
            return;

        bool anySelected = false;

        foreach (KeyValuePair<CircleTrack, Toggle> pair in _toggleMap)
        {
            Toggle toggle = pair.Value;

            if (toggle != null && toggle.isOn)
            {
                anySelected = true;
                break;
            }
        }

        if (!anySelected)
            focusCameraController.SetTarget(null);
    }

    private void RefreshLabels()
    {
        for (int i = 0; i < _tracks.Count; i++)
        {
            CircleTrack track = _tracks[i];

            if (!_toggleMap.TryGetValue(track, out Toggle toggle))
                continue;

            TextMeshProUGUI label = toggle.GetComponentInChildren<TextMeshProUGUI>();

            if (label != null)
                label.text = $"{i + 1}";
        }
    }
}