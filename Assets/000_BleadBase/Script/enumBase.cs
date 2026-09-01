/// [스크립트 명세]------------------------------------------------------------------------------------------------------------------------
/// 
///  1. 정의 : 게임 Base에서 사용하는 enum값을 정리한 스크립트 파일
/// --------------------------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;

public enum ENUM_BASE
{
    SCENELOADING = 0,
    GAMELOADING,
    NETWORKING,
    CONFIRM,
    CANCEL,
    DAY,
    HOUR,
    MIN,
    SEC,
    ERROR = 100,
    TIMEOUT,
    READY,
    UNKNOWN_ERROR,
    QUIT = 200
}


