using System;
using System.Globalization;

public static class Const
{
    // 문자열
    public static readonly string LANGUAGE_PREFS_KEY = "Language";  // 언어 설정 로컬 저장

    // 정수
    //public static readonly int LONG_INFINITY_NEG = -9223372036854775808L;
    //public static readonly int INT_INFINITY_NEG = -2147483648;
    public static readonly int COUNT_SUB = -1;
    public static readonly int MOUSE_LEFT = 0;              // 좌클릭
    public static readonly int ZERO = 0;                    // TODO DAV: 캐스팅 필요
    public static readonly int COUNT_ADD = 1;
    public static readonly int COUNT_MIN = 1;               // 최소 수량
    public static readonly int MOUSE_RIGHT = 1;             // 우클릭
    public static readonly int COMPUTE_TWO = 2;             // 2의배수나 제곱pow 연산용
    public static readonly int ACCIDENTAL_STANDARD = 7;     // 도-시
    public static readonly int CHROMATIC_SCALE = 12;        // 옥타브 하나 안에 들어가는 모든 음
    public static readonly int BYTE_LIMIT = byte.MaxValue;  // 255
    public static readonly int CHAR_LIMIT = char.MaxValue;  // 65535
    //public static readonly int INT_INFINITY = 2147483647;
    //public static readonly int LONG_INFINITY = 9223372036854775807L;

    // 실수
    //public static readonly double DOUBLE_INFINITY_NEG = -1.7976931348623157E+308;
    //public static readonly float FLOAT_INFINITY_NEG = -3.40282347E+38F;
    public static readonly float GRAVITY_Y = -9.80665F;
    public static readonly float NEGATIVE_ONE = -1.0F;      // 음수 연산용
    public static readonly float ZEROF = 0.0F;
    public static readonly float IDENTITY = 1.0F;           // 곱셈 항등원
    public static readonly float MINIMUM_ONE = 1.0F;
    public static readonly float HALF = 0.5F;               // 1/2
    public static readonly float MINIMUM_TENTH = 0.1F;      // 1/10
    public static readonly float MINIMUM_HUNDREDTH = 0.01F; // 1/100

    // 수학 상수
    public static readonly float E = 2.71828175F;
    public static readonly float PI = 3.14159274F;
    public static readonly float PI_DEG = 180F;             // ㅠ라디안 -> 디그리
    public static readonly float DEG2RAD = 0.0174532924F;   // 디그리 -> 라디안 (PI / 180)
    public static readonly float RAD2DEG = 57.2957795F;     // 라디안 -> 디그리 (180 / PI)
        
    // SI 유닛계
    public static readonly float INCH = 0.3937007874F;      // 역산: 2.54f
    public static readonly float FEET = 3.280839895F;       // 역산: 0.3048f
    public static readonly float MILE = 0.6213711922F;      // 역산: 1.609344f
    //public static readonly float FLOAT_INFINITY = 3.40282347E+38F;
    //public static readonly double DOUBLE_INFINITY = 1.7976931348623157E+308;
}

public enum LanguageType
{
    Korean,
    English
}
public enum ScaleType
{
    Major,
    Minor,
    PentatonicMajor,
    PentatonicMinor
}

public enum NoteAccidental
{
    Flat = -1,
    None = 0,
    Sharp = 1
}

public enum InstrumentType
{
    GrandPiano,
    LofiPiano,
    FMPiano,
    AcousticGuitar,
    Ukulele,
    Drum_Blues
}

public enum DrumSampleType
{
    Kick = 1,
    Snare = 2,
    HatClosed = 3,
    HatOpened = 4,
    Tom = 5,
    Crash = 6,
    ClapBell = 7
}

public enum TrackPlayMode
{
    Normal,
    Sleep,
    HalfVolume
}