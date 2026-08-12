using UnityEngine;

/// <summary>
/// 기능의 성격보다 의존하는 타입/네임스페이스를 기준으로 파일을 나누자
/// </summary>

/////////////////////////////////////////////////////////////////////////////////////////////
//                                                                                         //
//                                                                                         //
//                                      UNITY 유틸리티                                     //
//                                                                                         //
//                                                                                         //
/////////////////////////////////////////////////////////////////////////////////////////////
public static partial class Utility
{
    // 게러
    #region R_Get
    /// <summary> ratio 로 스케일 후, 정수 난수 생성하고 ratio 로 나눈 부동 소수점 값을 반환 </summary>
    /// <param name="ratio"> 비율, 너무 작지 않은 1 이상의 값 넣으셈 </param>
    public static float GetRandomByRatio(float min, float max, float ratio)
    {
        int rMin = (int)(min * ratio);
        int rMax = (int)(max * ratio);

        return Random.Range(rMin, rMax + 1) / ratio;
    }

    public static Vector3 Get3DPosFromMouse(Camera cam, Vector3 mousePos, float z)
    {
        return cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, z));
    }
    #endregion

    // 수학
    #region R_Math
    public static Vector3 DegreeToDirectionVector3D(float d)
    {
        System.Numerics.Vector3 v = DegreeToDirectionVector(d);
        return new Vector3(v.X, v.Y, v.Z);
    }
    #endregion

    // 변환
    #region R_Conversion
    /// <summary>
    /// UnityEngine.Color32 -> UnityEngine.Color
    /// </summary>
    public static Color Color32ToColor(Color32 c32)
    {
        float x = byte.MaxValue; // 255
        return new Color(c32.r / x, c32.g / x, c32.b / x, c32.a / x);
    }
    /// <summary>
    /// UnityEngine.Color -> UnityEngine.Color32
    /// </summary>
    public static Color32 ColorToColor32(Color c)
    {
        float x = byte.MaxValue; // 255
        return new Color32((byte)(c.r * x), (byte)(c.g * x), (byte)(c.b * x), (byte)(c.a * x));
    }
    #endregion
}