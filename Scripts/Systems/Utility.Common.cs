using System;


// NOTE: 참고
// CAUTION: 참고하지 않을 시 문제 발생 가능
// WARNING: 매우 위험 중요 주의
// TODO: 이어서 작업할 부분


/////////////////////////////////////////////////////////////////////////////////////////////
//                                                                                         //
//                                                                                         //
//                                   일반 유틸리티                                          //
//                                                                                         //
//                                                                                         //
/////////////////////////////////////////////////////////////////////////////////////////////

public static partial class Utility
{
    // 게러
    #region R_Get
    /// <summary> ENUM 찾기 </summary>
    public static T GetEnumByString<T>(string str) where T : System.Enum
    {
        string[] strs = System.Enum.GetNames(typeof(T));
        for (int i = 0; i < strs.Length; ++i)
        {
            if (strs[i].CompareTo(str) == 0)
            {
                return (T)System.Enum.Parse(typeof(T), strs[i]);
            }
        }
        /*
        foreach (T type in (T[])System.Enum.GetValues(typeof(T)))
        {
            if (type.ToString() == str)
                return type;
        }
		*/
        return default;
    }
    #endregion

    // 표현
    #region R_Expression
    /// <summary> 서수 표현 (11, 12, 13 은 예외) </summary>
    public static string ToOrdinalNumber(int n)
    {
        if (n <= 0)
            return $"{n}th";
        if (n % 10 == 1 && n % 100 != 11)
            return $"{n}st";
        else if (n % 10 == 2 && n % 100 != 12)
            return $"{n}nd";
        else if (n % 10 == 3 && n % 100 != 13)
            return $"{n}rd";
        else
            return $"{n}th";
    }

    /// <summary> 숫자에 콤마 붙이기 </summary>
    public static string ToString_Comma(int n)
    {
        return n.ToString("N0"); // ("#,##0") 도 가능
    }
    public static string ToString_Comma(float n, int decPlaces = 0)
    {
        return n.ToString($"N{decPlaces}"); // ("#,##0.00~~~") 도 가능
    }

    /// <summary> 백분율 표현 (e.g. 0.1f -> 10%) </summary>
    public static string ToString_Percent(float n, int decPlaces = 0)
    {
        return n.ToString($"P{decPlaces}"); // ("0.00~~~%") 도 가능
    }

    /// <summary> 왕 큰 숫자 </summary>
    public static string ToString_Scientific(int n)
    {
        return n.ToString("E0"); // ("0E+00") 도 가능
    }

    /// <summary> 앞에 0 붙이기 </summary>
    public static string ToString_ZeroPad(int n, int totalWidth)
    {
        return n.ToString($"D{totalWidth}"); // ("0000~~~") 도 가능
    }
    #endregion

    // 바이트 배열 변환
    #region R_ByteConversion
    public static Int16 ToInt16(byte[] bytes, ref int offset)
    {
        Int16 value = BitConverter.ToInt16(bytes, offset);
        offset += sizeof(Int16);

        return value;
    }

    public static Int32 ToInt32(byte[] bytes, ref int offset)
    {
        Int32 value = BitConverter.ToInt32(bytes, offset);
        offset += sizeof(Int32);

        return value;
    }

    public static Int64 ToInt64(byte[] bytes, ref int offset)
    {
        Int64 value = BitConverter.ToInt64(bytes, offset);
        offset += sizeof(Int64);

        return value;
    }

    public static UInt16 ToUInt16(byte[] bytes, ref int offset)
    {
        UInt16 value = BitConverter.ToUInt16(bytes, offset);
        offset += sizeof(UInt16);

        return value;
    }

    public static UInt32 ToUInt32(byte[] bytes, ref int offset)
    {
        UInt32 value = BitConverter.ToUInt32(bytes, offset);
        offset += sizeof(UInt32);

        return value;
    }

    public static UInt64 ToUInt64(byte[] bytes, ref int offset)
    {
        UInt64 value = BitConverter.ToUInt64(bytes, offset);
        offset += sizeof(UInt64);

        return value;
    }

    public static float ToFloat(byte[] bytes, ref int offset)
    {
        float value = BitConverter.ToSingle(bytes, offset);
        offset += sizeof(float);

        return value;
    }

    public static bool ToBool(byte[] bytes, ref int offset)
    {
        bool value = BitConverter.ToBoolean(bytes, offset);
        offset += sizeof(bool);

        return value;
    }

    public static byte ToByte(byte[] bytes, ref int offset)
    {
        byte value = bytes[offset];
        ++offset;

        return value;
    }

    public static sbyte ToSByte(byte[] bytes, ref int offset)
    {
        sbyte value = (sbyte)bytes[offset]; // sbyte: -128 ~ 127, byte: 0 ~ 255, (e.g. byte 값이 200 이라면, sbyte 로 캐스팅하면 -56)
        ++offset;

        return value;
    }

    public static string ToString(byte[] bytes, ref int offset, int length)
    {
        string str = System.Text.Encoding.UTF8.GetString(bytes, offset, length);
        offset += length;

        return str;
    }
    #endregion

    // CAUTION: InvariantCulture, 소수점 . 으로 고정시키기 (문화권에에 따라 , 로 바뀌는 경우 방지)
    // NOTE DAV: 제네릭 가능하게 바꿀 수 있을듯, TryParse는 out 매개변수 때문에 제네릭으로 바꾸기 좀 까다로울 듯
    #region R_SafeParse

    public static float FloatParse(string str)
    {
        return float.Parse(str, System.Globalization.CultureInfo.InvariantCulture);
    }

    public static bool FloatTryParse(string str, out float outRet)
    {
        return float.TryParse(str, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out outRet);
    }

    public static int IntParse(string str)
    {
        return int.Parse(str, System.Globalization.CultureInfo.InvariantCulture);
    }

    public static bool IntTryParse(string str, out int outRet)
    {
        return int.TryParse(str, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out outRet);
    }

    public static long LongParse(string str)
    {
        return long.Parse(str, System.Globalization.CultureInfo.InvariantCulture);
    }

    public static bool LongTryParse(string str, out long outRet)
    {
        return long.TryParse(str, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out outRet);
    }
    #endregion
}



/////////////////////////////////////////////////////////////////////////////////////////////
//                                                                                         //
//                                                                                         //
//                                   수학 유틸리티                                          //
//                                                                                         //
//                                                                                         //
/////////////////////////////////////////////////////////////////////////////////////////////

public static partial class Utility
{
    // 상수부
    #region R_Const
    public static readonly float E = 2.71828175F;
    public static readonly float PI = 3.14159274F;
    public static readonly float PI_DEG = 180F;             // ㅠ라디안 -> 디그리
    public static readonly float DEG2RAD = 0.0174532924F;   // 디그리 -> 라디안 (PI / 180)
    public static readonly float RAD2DEG = 57.2957795F;     // 라디안 -> 디그리 (180 / PI)
    #endregion

    // 연산
    #region R_Arithmetic
    /// <summary> 모듈러 연산 </summary>
    /// <param name="v"> 원본 값 </param>
    /// <param name="m"> 모듈러 </param>
    public static int Mod(int v, int m)
    {
        return (v % m + m) % m;
    }

    #endregion

    // 원, 각도
    #region R_AngleCircle
    /// <summary> Degree 0 ~ 360 로 정규화 </summary>
    public static float NormalizeAngle(float t)
    {
        t %= PI_DEG * 2;

        if (t < 0f)
            t += PI_DEG * 2;

        return t;
    }

    /// <summary> 2ㅠr 원주 ( = Circumference 원둘레) </summary>
    /// <param name="r">원의 반지름</param>
    public static float TwoPiRadius(float r)
    {
        return 2f * PI * r;
    }

    /// <summary> 오일러 각도 0 ~ 360 을 -180 ~ 180 부호(signed)있는 각도로 변환 </summary>
    /// <param name="e2s"> 오일러 각도 넣으셈 </param>
    public static float EulerToSigned(float e2s)
    {
        return (e2s > PI_DEG) ? e2s - (PI_DEG * 2) : e2s;
    }

    /// <summary> 각도(Euler) -> 벡터 (0 ~ 360)
    /// (e.g. 0도는 (1, 0), 90도는 (0, 1), 180도는 (-1, 0), 270도는 (0, -1)) </summary>
    public static System.Numerics.Vector3 DegreeToDirectionVector(float d)
    {
        float rad = d * DEG2RAD;
        return new System.Numerics.Vector3(MathF.Cos(rad), MathF.Sin(rad), 0);
    }
    #endregion

    // 보간, 매핑, 뱐환
    #region R_Interpolation
    /// <summary> 선형 보간 Linear Interpolation, a에서 b로 t만큼 보간 </summary>
    /// <param name="t"> 보간 계수 (0 ~ 1 만 넣으셈)</param>
    public static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    /// <summary> fromMin = fromMax라면 0 나누기 예외 나올 수 있음
    /// 참고: https://ko.wikipedia.org/wiki/%EC%84%A0%ED%98%95_%EB%B3%B4%EA%B0%84%EB%B2%95 </summary>
    public static float Map(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        // Mathf.Lerp(toMin, toMax, Mathf.InverseLerp(fromMin, fromMax, value))
        float df = fromMax - fromMin;
        float dt = toMax - toMin;
        return (value - fromMin) / df * dt + toMin;
    }

    /// <summary> 3개의 제어점을 사용한 2차 베지어 곡선 위 t에 해당하는 위치 계산
    /// 참고: https://rito15.github.io/posts/unity-study-bezier-curve/ </summary>
    /// <param name="p1"> 곡선 시작 제어점 </param>
    /// <param name="p2"> 중간 제어점 </param>
    /// <param name="p3"> 곡선 끝 제어점 </param>
    /// <param name="t"> 곡선상 위치를 지정하는 보간 계수 (0 ~ 1) </param>
    public static System.Numerics.Vector3 Bezier(System.Numerics.Vector3 p1, System.Numerics.Vector3 p2, System.Numerics.Vector3 p3, float t)
    {
        float mum1, mum12, mu2;
        System.Numerics.Vector3 p;

        mu2 = t * t;
        mum1 = 1 - t;
        mum12 = mum1 * mum1;

        p.X = p1.X * mum12 + 2 * p2.X * mum1 * t + p3.X * mu2;
        p.Y = p1.Y * mum12 + 2 * p2.Y * mum1 * t + p3.Y * mu2;
        p.Z = p1.Z * mum12 + 2 * p2.Z * mum1 * t + p3.Z * mu2;

        return p;
    }
    #endregion
}



/////////////////////////////////////////////////////////////////////////////////////////////
//                                                                                         //
//                                                                                         //
//                                   음악 관련 유틸리티                                     //
//                                                                                         //
//                                                                                         //
/////////////////////////////////////////////////////////////////////////////////////////////
public static partial class Utility
{
    // 접었다 폈다 편하게 그냥 리전해둠
    #region R_Music
    /// <summary> MIDI 음표 번호를 옥타브 번호로 변환할거지롱 (C4는 MIDI 60이며 결과는 4이다) </summary>
    /// <param name="m2o"> 0부터 127까지의 MIDI 음표 번호 </param>
    public static int MidiNoteToOctave(int m2o)
    {
        return (m2o / 12) - 1;
    }

    /// <summary> 장조 / 단조 반음 간격을 정수 배열로 반환 </summary>
    /// <param name="type"> 장조 단조 </param>
    public static int[] GetScaleIntervals(ScaleType type)
    {
        switch (type)
        {
            case ScaleType.Minor:
                return new[] { 0, 2, 3, 5, 7, 8, 10 };

            case ScaleType.PentatonicMajor:
                return new[] { 0, 2, 4, 7, 9 };

            case ScaleType.PentatonicMinor:
                return new[] { 0, 3, 5, 7, 10 };

            case ScaleType.Major:
            default:
                return new[] { 0, 2, 4, 5, 7, 9, 11 };
        }
    }

    /// <summary> 솔페지 (도레미파솔라시) 음 이름 모음 </summary>
    public static string[] Solfeges()
    {
        return new string[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
    }

    /// <summary> 해당 옥타브의 C 음에 해당하는 MIDI 노트 번호 반환, (e.g. C4 = 60, C3 = 48, C2 = 36, C1 = 24) </summary>
    /// <param name="o"> 옥타브 번호(e.g. C4 = 4) </param>
    public static int OctaveToCMidiNote(int o)
    {
        return 12 * (o + 1);
    }

    /// <summary> 드럼은 7개 Kick, Snare, HatClosed, HatOpened, TOm, Crash, ClapBell 사용</summary>
    public static string GetDrumNameByOctave(int octave)
    {
        switch (octave)
        {
            case 1:
                return "Kick";

            case 2:
                return "Snare";

            case 3:
                return "HatClosed";

            case 4:
                return "HatOpened";

            case 5:
                return "Tom";

            case 6:
                return "Crash";

            case 7:
                return "ClapBell";      // Clap 또는 Bell 을 사용할 수 있으므로 이름이 이렇다

            default:
                return "Kick";
        }
    }
    #endregion
}