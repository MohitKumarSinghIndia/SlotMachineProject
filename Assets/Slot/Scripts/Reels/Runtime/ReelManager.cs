using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SlotMachine.Reels.Runtime
{
    public class ReelManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private List<ReelController> reels = new List<ReelController>();
        [SerializeField] private SpinResultGenerator spinResultGenerator;

        [Header("Spin Flow")]
        [Min(0f)]
        [SerializeField] private float reelStartDelay = 0.08f;
        [Min(0f)]
        [SerializeField] private float loopHoldDuration = 0.8f;
        [Min(0f)]
        [SerializeField] private float reelStopDelay = 0.14f;

        [Header("Debug State")]
        [SerializeField] private bool isSpinInProgress;
        [SerializeField] private SpinOutcome lastOutcome;

        private int _remainingReels;
        private Coroutine _spinRoutine;

        public bool IsSpinInProgress => isSpinInProgress;

        private void Awake()
        {
            CacheLocalReferences();
        }

        [ContextMenu("Spin All Reels")]
        public void SpinAll()
        {
            if (isSpinInProgress)
            {
                return;
            }

            CacheLocalReferences();
            StopAllReels();

            SpinOutcome outcome = ResolveNextOutcome();
            if (outcome == null)
            {
                return;
            }

            lastOutcome = outcome;
            _spinRoutine = StartCoroutine(SpinRoutine(outcome));
        }

        private IEnumerator SpinRoutine(SpinOutcome outcome)
        {
            isSpinInProgress = true;
            _remainingReels = 0;

            List<SpinCommand> commands = BuildCommands(outcome);
            if (commands.Count == 0)
            {
                isSpinInProgress = false;
                yield break;
            }

            for (int i = 0; i < commands.Count; i++)
            {
                SpinCommand command = commands[i];
                command.Reel.PrepareStopResult(command.StopIndex, command.VisibleSymbolIds);
                command.Reel.StartSpin(command.Reel.ReelIndex, OnReelStopped);
                _remainingReels++;

                if (i < commands.Count - 1 && reelStartDelay > 0f)
                {
                    yield return new WaitForSeconds(reelStartDelay);
                }
            }

            if (loopHoldDuration > 0f)
            {
                yield return new WaitForSeconds(loopHoldDuration);
            }

            for (int i = 0; i < commands.Count; i++)
            {
                SpinCommand command = commands[i];
                command.Reel.StopSpin(command.Reel.ReelIndex, command.StopIndex, command.VisibleSymbolIds);

                if (i < commands.Count - 1 && reelStopDelay > 0f)
                {
                    yield return new WaitForSeconds(reelStopDelay);
                }
            }
        }

        private List<SpinCommand> BuildCommands(SpinOutcome outcome)
        {
            List<SpinCommand> commands = new List<SpinCommand>();

            for (int i = 0; i < reels.Count; i++)
            {
                ReelController reel = reels[i];
                if (reel == null)
                {
                    continue;
                }

                if (!TryGetReelOutcome(outcome, reel.ReelIndex, out ReelOutcome reelOutcome))
                {
                    continue;
                }

                commands.Add(new SpinCommand(reel, reelOutcome.StopIndex, reelOutcome.VisibleSymbolIds));
            }

            return commands;
        }

        private void OnReelStopped(ReelController reel)
        {
            _remainingReels = Mathf.Max(0, _remainingReels - 1);
            if (_remainingReels > 0)
            {
                return;
            }

            isSpinInProgress = false;
            if (_spinRoutine != null)
            {
                StopCoroutine(_spinRoutine);
                _spinRoutine = null;
            }
        }

        private SpinOutcome ResolveNextOutcome()
        {
            if (spinResultGenerator == null)
            {
                Debug.LogError($"[{name}] SpinResultGenerator reference is missing.");
                return null;
            }

            return spinResultGenerator.GenerateOutcome(reels);
        }

        private void CacheLocalReferences()
        {
            if (spinResultGenerator == null)
            {
                spinResultGenerator = GetComponent<SpinResultGenerator>();
            }
        }

        private void StopAllReels()
        {
            if (_spinRoutine != null)
            {
                StopCoroutine(_spinRoutine);
                _spinRoutine = null;
            }

            isSpinInProgress = false;
            _remainingReels = 0;

            for (int i = 0; i < reels.Count; i++)
            {
                ReelController reel = reels[i];
                if (reel == null)
                {
                    continue;
                }

                reel.ResetReel(reel.ReelIndex);
            }
        }

        private static bool TryGetReelOutcome(SpinOutcome outcome, int reelIndex, out ReelOutcome reelOutcome)
        {
            for (int i = 0; i < outcome.Reels.Count; i++)
            {
                ReelOutcome candidate = outcome.Reels[i];
                if (candidate != null && candidate.ReelIndex == reelIndex)
                {
                    reelOutcome = candidate;
                    return true;
                }
            }

            reelOutcome = null;
            return false;
        }

        private readonly struct SpinCommand
        {
            public SpinCommand(ReelController reel, int stopIndex, IReadOnlyList<int> visibleSymbolIds)
            {
                Reel = reel;
                StopIndex = stopIndex;
                VisibleSymbolIds = visibleSymbolIds;
            }

            public ReelController Reel { get; }
            public int StopIndex { get; }
            public IReadOnlyList<int> VisibleSymbolIds { get; }
        }
    }
}
