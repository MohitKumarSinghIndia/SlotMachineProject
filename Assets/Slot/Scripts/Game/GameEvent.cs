using UnityEngine;
using UnityEngine.Events;

public static class GameEvent 
{
    [Header("Game Events")]
    public static UnityEvent onSpinStartPhase = new UnityEvent();
    public static UnityEvent onSpinStopPhase = new UnityEvent();
    public static UnityEvent onResultDisplayPhase = new UnityEvent();
    public static UnityEvent onPaylinePhase = new UnityEvent();
    public static UnityEvent onFreeGamePhase = new UnityEvent();
    public static UnityEvent onSpinFlowComplete = new UnityEvent();

    [Header("Dragon Events")]
    public static UnityEvent onDragonIdle = new UnityEvent();
    public static UnityEvent onDragonWin = new UnityEvent();
    public static UnityEvent onDragonLose = new UnityEvent();
}