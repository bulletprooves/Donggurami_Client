using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class CircleTrackThemeController
{
    private readonly Action<Transform> _setArrow;
    private readonly Action _onArrowChanged;

    private Transform _centerRoot;
    private Transform _arrowRoot;
    private GameObject _currentArrowPrefab;
    private GameObject _currentCenterPrefab;

    public CircleTrackThemeController(
        Action<Transform> setArrow,
        Action onArrowChanged
    )
    {
        _setArrow = setArrow;
        _onArrowChanged = onArrowChanged;
    }

    public WorldTheme CurrentTheme { get; private set; }

    public void Configure(Transform centerRoot, Transform arrowRoot)
    {
        _centerRoot = centerRoot;
        _arrowRoot = arrowRoot;
    }

    public void Apply(WorldTheme theme, IReadOnlyList<NoteBlock> notes, CircleTrackNoteViewController noteViewController)
    {
        if (theme == null)
            return;

        CurrentTheme = theme;

        ApplyCenterTheme(theme);
        ApplyArrowTheme(theme);
        ApplyNoteThemes(notes, noteViewController);
    }

    public void ApplyNoteThemes(IReadOnlyList<NoteBlock> notes, CircleTrackNoteViewController noteViewController)
    {
        if (CurrentTheme == null || notes == null || noteViewController == null)
            return;

        for (int i = 0; i < notes.Count; i++)
        {
            NoteBlock note = notes[i];

            if (note == null)
                continue;

            Transform noteView = noteViewController.Get(note);

            if (noteView == null)
                continue;

            NoteView view = noteView.GetComponent<NoteView>();

            if (view != null)
                view.ApplyTheme(CurrentTheme);
        }
    }

    private void ApplyCenterTheme(WorldTheme theme)
    {
        if (theme.centerPrefab == null || _centerRoot == null)
            return;

        if (_currentCenterPrefab == theme.centerPrefab)
            return;

        ClearChildren(_centerRoot);

        GameObject centerObject = UnityEngine.Object.Instantiate(theme.centerPrefab, _centerRoot);
        centerObject.name = $"CenterVisual_{theme.themeType}";
        centerObject.transform.localPosition = Vector3.zero;
        centerObject.transform.localRotation = Quaternion.identity;
        centerObject.transform.localScale = Vector3.one;

        _currentCenterPrefab = theme.centerPrefab;
    }

    private void ApplyArrowTheme(WorldTheme theme)
    {
        if (theme.arrowPrefab == null || _arrowRoot == null)
            return;

        if (_currentArrowPrefab == theme.arrowPrefab && _arrowRoot.childCount > 0)
        {
            _setArrow?.Invoke(_arrowRoot);
            _onArrowChanged?.Invoke();
            return;
        }

        ClearChildren(_arrowRoot);

        GameObject arrowObject = UnityEngine.Object.Instantiate(theme.arrowPrefab, _arrowRoot);
        arrowObject.name = $"ArrowVisual_{theme.themeType}";
        arrowObject.transform.localPosition = Vector3.zero;
        arrowObject.transform.localRotation = Quaternion.identity;
        arrowObject.transform.localScale = Vector3.one;

        _currentArrowPrefab = theme.arrowPrefab;

        _setArrow?.Invoke(_arrowRoot);
        _onArrowChanged?.Invoke();
    }

    private static void ClearChildren(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Transform child = root.GetChild(i);

            if (child == null)
                continue;

            UnityEngine.Object.Destroy(child.gameObject);
        }
    }
}
