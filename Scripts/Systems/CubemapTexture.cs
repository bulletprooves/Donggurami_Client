using UnityEngine;

[ExecuteAlways]

[RequireComponent(typeof(MeshFilter))]
public class CubemapTexture : MonoBehaviour
{
    private MeshFilter _meshFilter;
    private Mesh _mesh;

    private void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();

        // NOTE DAV: sharedMesh 처럼 직접 원본 수정하지 않도록 복제된 Mesh 사용
        //_mesh = _meshFilter.mesh;
        // ...였지만 복제 이슈 때문에 아래 sharedMesh 사용
        _mesh = _meshFilter.sharedMesh;

        if (_mesh == null)
            return;

        Vector2[] uv = _mesh.uv;

        if (uv == null || uv.Length < 24)
        {
            Debug.LogError("24개 이상의 uv를 가진 CubeMesh 필요!!!");
            return;
        }

        // 4 by 3 으로 텍스쳐를 잘라 메쉬로 쓸 거라, 필요한 가장자리 x: 5, y: 4
        float x0 = 0f;
        float x1 = 0.25f;
        float x2 = 0.5f;
        float x3 = 0.75f;
        float x4 = 1f;

        float y0 = 0f;
        float y1 = 1f / 3f;
        float y2 = 2f / 3f;
        float y3 = 1f;

        // uv 좌표 참고
        // (0,1)---(1,1)
        //   |       |
        //   |       |
        // (0,0)---(1,0)

        // Front
        // 2 행, 2 열
        uv[0] = new Vector2(x1, y1);
        uv[1] = new Vector2(x2, y1);
        uv[2] = new Vector2(x1, y2);
        uv[3] = new Vector2(x2, y2);

        // Top
        // 1 행 , 2 열
        uv[4] = new Vector2(x1, y3);
        uv[5] = new Vector2(x2, y3);
        uv[8] = new Vector2(x1, y2);
        uv[9] = new Vector2(x2, y2);

        // Back
        // 2 행, 4 열 (기존 Cube 의 정점 방향 때문에 좌우 반전으로 배치)
        uv[6] = new Vector2(x4, y1);
        uv[7] = new Vector2(x3, y1);
        uv[10] = new Vector2(x4, y2);
        uv[11] = new Vector2(x3, y2);

        // Bottom
        // 3 행, 2 열
        uv[12] = new Vector2(x1, y0);
        uv[13] = new Vector2(x1, y1);
        uv[14] = new Vector2(x2, y1);
        uv[15] = new Vector2(x2, y0);

        // Left
        // 2 행, 1 열
        uv[16] = new Vector2(x0, y1);
        uv[17] = new Vector2(x0, y2);
        uv[18] = new Vector2(x1, y2);
        uv[19] = new Vector2(x1, y1);

        // Right
        // 2 행, 3 열
        uv[20] = new Vector2(x2, y1);
        uv[21] = new Vector2(x2, y2);
        uv[22] = new Vector2(x3, y2);
        uv[23] = new Vector2(x3, y1);

        _mesh.uv = uv;
    }
}