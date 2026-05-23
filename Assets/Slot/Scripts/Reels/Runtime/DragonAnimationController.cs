using UnityEngine;

public class DragonAnimationController : MonoBehaviour
{
    [SerializeField] private EventSequencePlayer dragonEventPlayer;

    [Header("Sequence IDs")]
    [SerializeField] private int idleSequenceId = 0;

    [SerializeField] private int idleBreakSequenceId = 1;

    [SerializeField] private int winSequenceId = 2;

    [SerializeField] private int sadSequenceId = 3;

    private void Awake()
    {
        PlayDragonIdle();
    }

    private void OnEnable()
    {
        GameEvent.onDragonIdle.AddListener(PlayDragonIdle);
        GameEvent.onDragonWin.AddListener(PlayDragonWin);
        GameEvent.onDragonLose.AddListener(PlayDragonLose);
    }

    private void OnDisable()
    {
        GameEvent.onDragonIdle.RemoveListener(PlayDragonIdle);
        GameEvent.onDragonWin.RemoveListener(PlayDragonWin);
        GameEvent.onDragonLose.RemoveListener(PlayDragonLose);
    }

    private void PlayDragonIdle()
    {
        dragonEventPlayer.PlaySequenceById(idleSequenceId);
    }

    private void PlayDragonWin()
    {
        dragonEventPlayer.PlaySequenceById(winSequenceId);
    }

    private void PlayDragonLose()
    {
        dragonEventPlayer.PlaySequenceById(sadSequenceId);
    }
}