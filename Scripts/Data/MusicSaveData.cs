using System;
using System.Collections.Generic;

// CAUTION DAV: 로컬저장용 & 공유코드 데이터들 !!!
//
//  SongSaveData: 버전과 가장 높은 기능, 트랙들
//      ㄴ> TrackSaveData[]: 위치 & 트랙정보, 노트들
//          ㄴ> NoteSaveData[]: 각도 & (길이&피치) & 액시던탈
//

[Serializable]
public class MusicSaveData
{
    public int version = 2;             // NOTE DAV: 버전 필요 예상, 기능 별로 달라질 수도 있음
    public int bpm;
    public WorldThemeType themeType;
    public List<TrackSaveData> tracks = new List<TrackSaveData>();
}

[Serializable]
public class TrackSaveData
{
    public float x;
    public float y;
    public float z; // Z 축은 나중을 위해 일단 저장

    public float radius;
    public int octave;
    public InstrumentType instrumentType;
    
    //public bool isSleeping;         // 구버전 호환용
    public TrackPlayMode playMode;  // 신버전

    public List<NoteSaveData> notes = new List<NoteSaveData>();
}

[Serializable]
public class NoteSaveData
{
    public float angleDeg;
    public float outwardLength; // / 서로 종속 변수 (빠른 참조를 위함)
    public int pitch;           // \ 서로 종속 변수 (빠른 참조를 위함)
    public NoteAccidental accidental;
}