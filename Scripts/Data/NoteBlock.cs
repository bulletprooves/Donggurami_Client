using UnityEngine;

[System.Serializable]
public class NoteBlock
{
    [Header("Timing")]
    [Range(0f, 360f)]
    public float angleDeg;

    [Header("Pitch / Shape")]
    [Min(0.1f)]
    public float outwardLength = 1f;

    [Header("Sound")]
    public int pitch = 0;

    [Header("Accidental")]
    public NoteAccidental accidental = NoteAccidental.None;

    // Private Area
    private bool _wasInside;

    public bool TryTrigger(float arrowAngleDeg, float triggerToleranceDeg)
    {
        // 내부 확인 IsAngleInside
        bool isInside = false;
        if (Mathf.Abs(Mathf.DeltaAngle(angleDeg, arrowAngleDeg)) <= triggerToleranceDeg)
            isInside = true;

        if (isInside && !_wasInside)
        {
            _wasInside = true;
            return true;
        }

        if (!isInside)
            _wasInside = false;

        return false;
    }

    public void CycleAccidental()
    {
        switch (accidental)
        {
            case NoteAccidental.None:
                accidental = NoteAccidental.Sharp;
                return;
            case NoteAccidental.Sharp:
                accidental = NoteAccidental.Flat;
                return;
            case NoteAccidental.Flat:
                accidental = NoteAccidental.None;
                return;
        }
    }

    public void ResetTriggerState()
    {
        _wasInside = false;
    }
}
