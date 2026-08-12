using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class CircleTrackNoteStore
{
    private readonly List<NoteBlock> _notes;
    private readonly Func<string> _ownerNameProvider;

    private NoteBlock[] _notesByIdx;
    private int _snapDivision = Const.COUNT_MIN;

    public CircleTrackNoteStore(List<NoteBlock> notes, Func<string> ownerNameProvider)
    {
        _notes = notes ?? new List<NoteBlock>();
        _ownerNameProvider = ownerNameProvider;
    }

    public IReadOnlyList<NoteBlock> Notes => _notes;
    public int SnapDivision => Mathf.Max(Const.COUNT_MIN, _snapDivision);

    public void RebuildLookup(int snapDivision)
    {
        _snapDivision = Mathf.Max(Const.COUNT_MIN, snapDivision);
        _notesByIdx = new NoteBlock[SnapDivision];

        List<int> removeIndices = null;

        for (int i = 0; i < _notes.Count; i++)
        {
            NoteBlock note = _notes[i];

            if (note == null)
            {
                removeIndices ??= new List<int>();
                removeIndices.Add(i);
                continue;
            }

            int snapIndex = AngleToIndex(note.angleDeg);

            if (_notesByIdx[snapIndex] != null)
            {
                string ownerName = _ownerNameProvider != null ? _ownerNameProvider() : "CircleTrack";
                GameManager.Instance.LogWarning($"{ownerName}: SnapIndex {snapIndex} has duplicate notes. Keeping the first note and removing the duplicate.");
                removeIndices ??= new List<int>();
                removeIndices.Add(i);
                continue;
            }

            note.angleDeg = IndexToAngle(snapIndex);
            _notesByIdx[snapIndex] = note;
        }

        if (removeIndices == null)
            return;

        for (int i = removeIndices.Count - 1; i >= 0; i--)
        {
            _notes.RemoveAt(removeIndices[i]);
        }
    }

    public NoteBlock FindByAngle(float targetAngleDeg)
    {
        EnsureLookup();

        return _notesByIdx[AngleToIndex(targetAngleDeg)];
    }

    public NoteBlock FindByIndex(int snapIndex)
    {
        EnsureLookup();

        return _notesByIdx[Utility.Mod(snapIndex, SnapDivision)];
    }

    public NoteBlock Add(float targetAngleDeg, float outwardLength, int pitch)
    {
        EnsureLookup();

        int snapIndex = AngleToIndex(targetAngleDeg);

        if (_notesByIdx[snapIndex] != null)
            return _notesByIdx[snapIndex];

        NoteBlock note = new NoteBlock
        {
            angleDeg = IndexToAngle(snapIndex),
            outwardLength = outwardLength,
            pitch = pitch
        };

        _notes.Add(note);
        _notesByIdx[snapIndex] = note;

        return note;
    }

    public bool Remove(NoteBlock note)
    {
        if (note == null)
            return false;

        bool removed = _notes.Remove(note);

        if (!removed)
            return false;

        RebuildLookup(SnapDivision);
        return true;
    }

    public void ResetTriggerStates()
    {
        for (int i = 0; i < _notes.Count; i++)
        {
            NoteBlock note = _notes[i];

            if (note == null)
                continue;

            note.ResetTriggerState();
        }
    }

    public int AngleToIndex(float angleDeg)
    {
        float stepDeg = Const.PI_DEG * 2f / SnapDivision;
        int index = Mathf.RoundToInt(Utility.NormalizeAngle(angleDeg) / stepDeg);
        return Utility.Mod(index, SnapDivision);
    }

    public float IndexToAngle(int snapIndex)
    {
        float stepDeg = Const.PI_DEG * 2f / SnapDivision;
        return Utility.NormalizeAngle(snapIndex * stepDeg);
    }

    private void EnsureLookup()
    {
        if (_notesByIdx == null || _notesByIdx.Length != SnapDivision)
            RebuildLookup(SnapDivision);
    }
}
