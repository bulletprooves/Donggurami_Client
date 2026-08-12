using System.Collections.Generic;
using UnityEngine;

public sealed class CircleTrackNoteViewController
{
    private readonly MonoBehaviour _owner;
    private readonly Dictionary<NoteBlock, Transform> _noteViews = new();

    private Transform _notePrefab;
    private Transform _noteRoot;
    private Transform _fallbackRoot;
    private float _radius;
    private float _noteWidth;

    public CircleTrackNoteViewController(MonoBehaviour owner)
    {
        _owner = owner;
    }

    public void Configure(Transform notePrefab, Transform noteRoot, Transform fallbackRoot, float radius, float noteWidth)
    {
        _notePrefab = notePrefab;
        _noteRoot = noteRoot;
        _fallbackRoot = fallbackRoot;
        _radius = radius;
        _noteWidth = noteWidth;
    }

    public void CreateViews(IReadOnlyList<NoteBlock> notes)
    {
        if (notes == null)
            return;

        for (int i = 0; i < notes.Count; i++)
        {
            Create(notes[i]);
        }
    }

    public void RefreshViews(IReadOnlyList<NoteBlock> notes)
    {
        if (notes == null)
            return;

        for (int i = 0; i < notes.Count; i++)
        {
            Update(notes[i]);
        }
    }

    public void Create(NoteBlock note)
    {
        if (note == null)
            return;

        if (_noteViews.ContainsKey(note))
        {
            Update(note);
            return;
        }

        if (_notePrefab == null)
        {
            GameManager.Instance.LogWarning($"{_owner.name}: notePrefab이 없습니다.");
            return;
        }

        Transform parent = _noteRoot != null ? _noteRoot : _fallbackRoot;

        Transform view = Object.Instantiate(_notePrefab, parent);
        view.name = $"Note_{note.angleDeg:0}";

        _noteViews.Add(note, view);

        Update(note);
    }

    public void Update(NoteBlock note)
    {
        Transform view = Get(note);

        if (view == null)
            return;

        float rad = note.angleDeg * Mathf.Deg2Rad;

        Vector3 outwardDir = new Vector3(
            Mathf.Cos(rad),
            Mathf.Sin(rad),
            0f
        );

        Vector3 localPosition = outwardDir * (_radius + note.outwardLength * 0.5f);

        view.localPosition = localPosition;
        view.localRotation = Quaternion.Euler(0f, 0f, note.angleDeg - 90f);
        view.localScale = Vector3.one;

        NoteView noteView = view.GetComponent<NoteView>();

        if (noteView != null)
        {
            noteView.SetSize(_noteWidth, note.outwardLength);
            noteView.SetAccidental(note.accidental);
        }
    }

    public Transform Get(NoteBlock note)
    {
        if (note == null)
            return null;

        if (!_noteViews.TryGetValue(note, out Transform view))
            return null;

        if (view != null)
            return view;

        _noteViews.Remove(note);
        return null;
    }

    public void Destroy(NoteBlock note)
    {
        Transform view = Get(note);

        if (view != null)
            Object.Destroy(view.gameObject);

        if (note != null)
            _noteViews.Remove(note);
    }

    public void Clear()
    {
        foreach (Transform view in _noteViews.Values)
        {
            if (view == null)
                continue;

            if (Application.isPlaying)
                Object.Destroy(view.gameObject);
            else
                Object.DestroyImmediate(view.gameObject);
        }

        _noteViews.Clear();
    }
}
