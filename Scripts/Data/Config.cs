using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// Config에 넣을 기준
// - 여러 스크립트가 공유해야 하는 값인가?
// - 게임 조작감에 영향을 주는 값인가?
// - 빌드 후에도 자주 튜닝할 값인가?
// - 프리팹마다 다르면 안 되는 공통 규칙인가?
// 
// 넣지 말아야 할 기준
// - 특정 오브젝트 하나만 쓰는 것 (특정 오브젝트 참조)
// - 테마마다 달라지는 값

// 음계 기초 지식: https://namu.wiki/w/%EC%9D%8C%EA%B3%84

[CreateAssetMenu(fileName = "Config", menuName = "Rhythm/Config")]
public class Config : ScriptableObject
{
    [Header("Rhythm Defaults")]
    [SerializeField] private int defaultBeatsPerBar = 4;        // 기본 박자 수
    [SerializeField] private int defaultBpm = 120;              // 기본 BPM
    [SerializeField] private int maxBpm = 240;                  // 최대 BPM
    [SerializeField] private int minBpm = 40;                   // 최소 BPM
    [FormerlySerializedAs("baseRadius")]
    [SerializeField] private float playbackBaseRadius = 3f;     // 재생 속도 계산 기준 반지름

    [Header("Note")]
    [SerializeField] private float minOutwardLength = 0.5f;     // 최소 외부 길이
    [SerializeField] private float maxOutwardLength = 4f;       // 최대 외부 길이
    [SerializeField] private float maxDrumOutwardLength = 3.5f; // 드럼 최대 외부 길이
    [SerializeField] private float pitchUnitLength = 0.5f;      // 하나의 음 단위 길이
    [SerializeField] private float noteWidth = 0.15f;           // 노트 너비
    [SerializeField] private float previewOutwardLength = 1f;
    [SerializeField] private float previewWidth = 0.15f;

    [Header("Track")]
    [SerializeField] private int minTrackCount = 1;             // 최소 트랙 수
    [SerializeField] private int maxTrackCount = 8;             // 최대 트랙 수
    [SerializeField] private int defaultOctave = 3;             // 기본 옥타브
    [SerializeField] private int minOctave = 1;                 // 최소 옥타브
    [SerializeField] private int maxOctave = 7;                 // 최대 옥타브
    [SerializeField] private int factorSnapDivision = 8;        // 스냅 개수 팩터 (기본 스냅 분할 수는 r = 8 일때 64개, 개수 별로 바뀜)
    [SerializeField] private int minSnapDivision = 16;          // 최고 스냅 수
    [SerializeField] private int maxSnapDivision = 256;         // 최대 스냅 수
    [SerializeField] private int majorGridSnapThreshold = 64;   // 그리드 큰 눈금 크기 변경 기준
    [SerializeField] private int minGridSnapCount = 8;          // 최소 큰 눈금 개수
    [SerializeField] private int maxGridSnapCount = 16;         // 최대 큰 눈금 개수
    [SerializeField] private float defaultTrackRadius = 8f;     // 트랙 기본 반지름
    [SerializeField] private float[] trackRadiusSteps = { 4f, 8f, 16f, 32f, 64f };
    [SerializeField] private int defaultTrackRadiusIndex = 1;
    [SerializeField] private float centerClickRadius = 0.6f;    // 트랙 중앙 클릭 허용 반지름
    [SerializeField] private float normalGridTickWidth = 0.05f; // 일반 그리드 눈금 너비
    [SerializeField] private float normalGridTickLength = 0.25f;// 일반 그리드 눈금 길이
    [SerializeField] private float majorGridTickLength = 0.45f; // 조금 더 큰 그리드 눈금 길이

    [Header("Editor Interaction")]
    [SerializeField] private float maxDistanceFromCircle = 1.5f;
    [SerializeField] private float removeMaxDistance = 0.5f;
    [SerializeField] private float centerDragStartDistance = 0.15f;
    [SerializeField] private float moveSmoothTime = 0.04f;


    // Rythm Defaults
    public int DefaultBeatsPerBar => defaultBeatsPerBar;
    public int DefaultBpm => defaultBpm;
    public int MaxBpm => maxBpm;
    public int MinBpm => minBpm;
    public float PlaybackBaseRadius => playbackBaseRadius;

    // Note
    public float MinOutwardLength => minOutwardLength;
    public float MaxOutwardLength => maxOutwardLength;
    public float MaxDrumOutwardLength => maxDrumOutwardLength;
    public float PitchUnitLength => pitchUnitLength;
    public float NoteWidth => noteWidth;
    public float PreviewOutwardLength => previewOutwardLength;
    public float PreviewWidth => previewWidth;

    // Track
    public int MinTrackCount => minTrackCount;
    public int MaxTrackCount => maxTrackCount;
    public int DefaultOctave => defaultOctave;
    public int MinOctave => minOctave;
    public int MaxOctave => maxOctave;
    public int FactorSnapDivision => factorSnapDivision;
    public int MinSnapDivision => minSnapDivision;
    public int MaxSnapDivision => maxSnapDivision;
    public int MajorGridSnapThreshold => majorGridSnapThreshold;
    public int MinGridSnapCount => minGridSnapCount;
    public int MaxGridSnapCount => maxGridSnapCount;
    public float DefaultTrackRadius => defaultTrackRadius;
    public IReadOnlyList<float> TrackRadiusSteps => trackRadiusSteps;
    public int DefaultTrackRadiusIndex => defaultTrackRadiusIndex;
    public float CenterClickRadius => centerClickRadius;
    public float NormalGridTickWidth => normalGridTickWidth;
    public float NormalGridTickLength => normalGridTickLength;
    public float MajorGridTickLength => majorGridTickLength;

    // Editor Interaction
    public float MaxDistanceFromCircle => maxDistanceFromCircle;
    public float RemoveMaxDistance => removeMaxDistance;
    public float CenterDragStartDistance => centerDragStartDistance;
    public float MoveSmoothTime => moveSmoothTime;
    

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Rhythm Defaults
        defaultBeatsPerBar = Mathf.Max(1, defaultBeatsPerBar);
        minBpm = Mathf.Max(1, minBpm);
        maxBpm = Mathf.Max(minBpm, maxBpm);
        defaultBpm = Mathf.Clamp(defaultBpm, minBpm, maxBpm);
        playbackBaseRadius = Mathf.Max(0.01f, playbackBaseRadius);

        // Note
        minOutwardLength = Mathf.Max(0.01f, minOutwardLength);
        maxOutwardLength = Mathf.Max(minOutwardLength, maxOutwardLength);
        maxDrumOutwardLength = Mathf.Clamp(maxDrumOutwardLength, minOutwardLength, maxOutwardLength);
        pitchUnitLength = Mathf.Max(0.01f, pitchUnitLength);
        noteWidth = Mathf.Max(0.01f, noteWidth);
        previewOutwardLength = Mathf.Clamp(previewOutwardLength, minOutwardLength, maxOutwardLength);
        previewWidth = Mathf.Max(0.01f, previewWidth);

        // Track
        minTrackCount = Mathf.Max(1, minTrackCount);
        maxTrackCount = Mathf.Max(minTrackCount, maxTrackCount);
        minOctave = Mathf.Max(1, minOctave);
        maxOctave = Mathf.Max(minOctave, maxOctave);
        defaultOctave = Mathf.Clamp(defaultOctave, minOctave, maxOctave);
        factorSnapDivision = Mathf.Max(1, factorSnapDivision);
        minSnapDivision = Mathf.Max(1, minSnapDivision);
        maxSnapDivision = Mathf.Max(minSnapDivision, maxSnapDivision);
        majorGridSnapThreshold = Mathf.Max(1, majorGridSnapThreshold);
        minGridSnapCount = Mathf.Max(1, minGridSnapCount);
        maxGridSnapCount = Mathf.Max(minGridSnapCount, maxGridSnapCount);
        defaultTrackRadius = Mathf.Max(0.01f, defaultTrackRadius);
        ValidateTrackRadiusSteps();
        centerClickRadius = Mathf.Max(0.01f, centerClickRadius);
        normalGridTickWidth = Mathf.Max(0.001f, normalGridTickWidth);
        normalGridTickLength = Mathf.Max(0.001f, normalGridTickLength);
        majorGridTickLength = Mathf.Max(normalGridTickLength, majorGridTickLength);

        // Editor Interaction
        maxDistanceFromCircle = Mathf.Max(0.01f, maxDistanceFromCircle);
        removeMaxDistance = Mathf.Max(0.01f, removeMaxDistance);
        centerDragStartDistance = Mathf.Max(0.01f, centerDragStartDistance);
        moveSmoothTime = Mathf.Max(0.001f, moveSmoothTime);

        
    }

    private void ValidateTrackRadiusSteps()
    {
        if (trackRadiusSteps == null || trackRadiusSteps.Length == 0)
            trackRadiusSteps = new[] { defaultTrackRadius };

        for (int i = 0; i < trackRadiusSteps.Length; i++)
        {
            trackRadiusSteps[i] = Mathf.Max(0.01f, trackRadiusSteps[i]);
        }

        defaultTrackRadiusIndex = Mathf.Clamp(defaultTrackRadiusIndex, 0, trackRadiusSteps.Length - 1);
        defaultTrackRadius = trackRadiusSteps[defaultTrackRadiusIndex];
    }
#endif
}