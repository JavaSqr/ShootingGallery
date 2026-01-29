using CustomInspector;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class SpawnPrefs
{
    public SpawnMode spawnMode;

    public List<SpawnAct> spawnActs;

    [ShowIfNot("IsRandom")]
    public bool canRepeatInARow;

    private bool IsRandom() => spawnMode == SpawnMode.Random;
}

[System.Serializable]
public class SpawnAct
{
    public List<SpawnStep> steps;
    public float actStartDelay;
}

[System.Serializable]
public class SpawnStep
{
    public TargetType targetType;
    public TargetState spawnType;
    public SpawnPosition spawnPosition;

    public float stepStartDelay;
    [ShowIf("IsMoving")]
    public int spawnCount = 1;

    [ShowIf("IsSeveral")]   public float spawnDelay;

    [ShowIf("IsMoving")]    public float speed;

    [ShowIf("IsEphemeralMoving"), Range(0, 1)]
    public float routeStartPos;

    [ShowIf("IsEphemeralMoving"), Range(0, 1)]
    public float routeEndPos;

    [ShowIf("IsStatic"), Range(0, 1)]
    public float routeSpawnPos;

    [ShowIfNot("IsMoving")] public float lifeTime;

    public bool IsMoving() => spawnType == TargetState.Moving || IsEphemeralMoving();
    public bool IsEphemeralMoving() => spawnType == TargetState.EphemeralMoving;
    public bool IsStatic() => spawnType == TargetState.Static;
    public bool IsSeveral() => spawnCount > 1;
}

[System.Serializable]
public enum SpawnMode
{
    Random,
    RandomActs,
    StepwiseActs,
}

[System.Serializable]
public enum TargetType
{
    Default,
    Mirror,
    Bomb,
    Armored,
    PlusLife,
}
[System.Serializable]
public enum TargetState
{
    Moving,
    Static,
    Hanging,
    EphemeralMoving
}

[System.Serializable]
public enum SpawnPosition
{
    LeftTop = 0,
    LeftMiddle,
    LeftBottom,
    RightTop,
    RightMiddle,
    RightBottom,
}
