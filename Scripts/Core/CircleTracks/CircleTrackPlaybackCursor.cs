using System.Collections.Generic;
using UnityEngine;

public sealed class CircleTrackPlaybackCursor
{
    private readonly string _ownerName;
    private int _previousNoteIdx;
    private bool _initialized;

    public CircleTrackPlaybackCursor(string ownerName)
    {
        _ownerName = ownerName;
    }

    public void Reset(CircleTrackNoteStore noteStore, float angleDeg)
    {
        if (noteStore == null)
            return;

        _previousNoteIdx = noteStore.AngleToIndex(angleDeg);
        _initialized = true;
    }

    public void CollectPassedNotes(
    CircleTrackNoteStore noteStore,
    float currentAngleDeg,
    int currentOctave,
    InstrumentType instrumentType,
    float volumeScale,
    List<NotePlayRequest> notePlayRequests
)
    {
        if (noteStore == null || notePlayRequests == null)
            return;

        int currentSnapIndex = noteStore.AngleToIndex(currentAngleDeg);

        if (!_initialized)
        {
            _previousNoteIdx = currentSnapIndex;
            _initialized = true;
            return;
        }

        if (currentSnapIndex == _previousNoteIdx)
            return;

        int index = _previousNoteIdx;
        int safetyCount = 0;

        while (index != currentSnapIndex)
        {
            index = Utility.Mod(index - 1, noteStore.SnapDivision);

            NoteBlock note = noteStore.FindByIndex(index);

            if (note != null)
                notePlayRequests.Add(new NotePlayRequest(
    note.pitch,
    currentOctave,
    note.accidental,
    instrumentType,
    volumeScale
));

            safetyCount++;

            if (safetyCount > noteStore.SnapDivision)
            {
                GameManager.Instance.LogError($"{_ownerName}: Snap index loop error");
                break;
            }
        }

        _previousNoteIdx = currentSnapIndex;
    }
}
