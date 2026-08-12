using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GridToggleUI : MonoBehaviour
{
    [SerializeField] private RhythmManager rhythmManager;
    [SerializeField] private Toggle gridToggle;
    [SerializeField] private GameObject BorderObject;   // 노트 에딧 아레아 보더임.

    #region R_Unity
    private void Awake()
    {
        if (gridToggle != null)
            gridToggle.onValueChanged.AddListener(SetGridVisible);
    }

    private void Start()
    {
        if (gridToggle != null && rhythmManager != null && rhythmManager.Tracks.Count > Const.ZERO && rhythmManager.Tracks[Const.ZERO] != null)
            gridToggle.SetIsOnWithoutNotify(rhythmManager.Tracks[Const.ZERO].IsGridVisible);

        BorderObject.SetActive(true);
    }

    private void OnDestroy()
    {
        if (gridToggle != null)
            gridToggle.onValueChanged.RemoveListener(SetGridVisible);
    }
    #endregion

    private void SetGridVisible(bool isVisible)
    {
        if (rhythmManager == null || rhythmManager.Tracks == null || BorderObject == null)
            return;

        foreach (CircleTrack track in rhythmManager.Tracks)
        {
            if (track == null)
                continue;

            track.SetGridVisible(isVisible);
        }

        BorderObject.SetActive(isVisible);
    }
}