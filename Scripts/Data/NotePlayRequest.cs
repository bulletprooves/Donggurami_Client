/// <summary>
/// 단위 노트 딩 필요한 데이터
/// </summary>

public readonly struct NotePlayRequest
{
    public readonly int pitch;
    public readonly int octave;
    public readonly NoteAccidental accidental;
    public readonly InstrumentType instrumentType;
    public readonly float volumeScale;

    public NotePlayRequest(
        int pitch,
        int octave,
        NoteAccidental accidental,
        InstrumentType instrumentType,
        float volumeScale = 1f
    )
    {
        this.pitch = pitch;
        this.octave = octave;
        this.accidental = accidental;
        this.instrumentType = instrumentType;
        this.volumeScale = volumeScale;
    }
}