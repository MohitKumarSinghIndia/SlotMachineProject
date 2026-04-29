using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SlotMachine.Reels.Runtime
{
    public class ReelManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private List<ReelController> reels = new List<ReelController>();
        [SerializeField] private SpinResultGenerator spinResultGenerator;
        [SerializeField] private SlotFlowController slotFlowController;

        [Header("Shared Reel Settings")]
        [SerializeField] private bool useSharedReelTimingProfile = true;
        [SerializeField] private ReelTimingProfile sharedReelTimingProfile = new ReelTimingProfile();

        [Header("Spin Flow")]
        [Min(0f)]
        [SerializeField] private float reelStartDelay = 0.08f;
        [Min(0f)]
        [SerializeField] private float loopHoldDuration = 0.8f;
        [Min(0f)]
        [SerializeField] private float reelStopDelay = 0.14f;
        [Min(0f)]
        [SerializeField] private float resultDisplayDuration = 0f;
        [Min(0f)]
        [SerializeField] private float freeGameDisplayDuration = 0f;
        [Min(0f)]
        [SerializeField] private float bigWinDisplayDuration = 0f;

        [Header("Phase Events")]
        [SerializeField] private UnityEvent onSpinStartPhase;
        [SerializeField] private UnityEvent onSpinStopPhase;
        [SerializeField] private UnityEvent onResultDisplayPhase;
        [SerializeField] private UnityEvent onFreeGamePhase;
        [SerializeField] private UnityEvent onBigWinPhase;
        [SerializeField] private UnityEvent onSpinFlowComplete;

        [Header("Debug State")]
        [SerializeField] private bool isSpinInProgress;
        [SerializeField] private SpinOutcome lastOutcome;

        private int _remainingReels;

        public bool IsSpinInProgress => isSpinInProgress;

        private void Awake()
        {
            CacheLocalReferences();
            ApplySharedReelSettings();
        }

        private void OnValidate()
        {
            reelStartDelay = Mathf.Max(0f, reelStartDelay);
            loopHoldDuration = Mathf.Max(0f, loopHoldDuration);
            reelStopDelay = Mathf.Max(0f, reelStopDelay);
            resultDisplayDuration = Mathf.Max(0f, resultDisplayDuration);
            freeGameDisplayDuration = Mathf.Max(0f, freeGameDisplayDuration);
            bigWinDisplayDuration = Mathf.Max(0f, bigWinDisplayDuration);
            sharedReelTimingProfile?.Clamp();

            if (!Application.isPlaying)
            {
                ApplySharedReelSettings();
            }
        }

        [ContextMenu("Spin All Reels")]
        public void SpinAll()
        {
            if (isSpinInProgress || (slotFlowController != null && slotFlowController.IsRunning))
            {
                return;
            }

            CacheLocalReferences();
            ApplySharedReelSettings();
            StopAllReels();

            SpinOutcome outcome = ResolveNextOutcome();
            if (outcome == null)
            {
                return;
            }

            lastOutcome = outcome;
            BuildAndRunSpinFlow(outcome);
        }

        private void BuildAndRunSpinFlow(SpinOutcome outcome)
        {
            if (slotFlowController == null)
            {
                Debug.LogError($"[{name}] SlotFlowController reference is missing.");
                isSpinInProgress = false;
                return;
            }

            List<SpinCommand> commands = BuildCommands(outcome);
            if (commands.Count == 0)
            {
                return;
            }

            isSpinInProgress = true;
            _remainingReels = 0;
            slotFlowController.ClearAllQueues();
            slotFlowController.AddSpinStartStep(() => RunSpinStartPhase(commands));
            slotFlowController.AddSpinStopStep(() => RunSpinStopPhase(commands));
            slotFlowController.AddResultDisplayStep(RunResultDisplayPhase);

            if (outcome.TriggersFreeSpins)
            {
                slotFlowController.AddFreeGameStep(RunFreeGamePhase);
            }

            if (outcome.IsBigWin)
            {
                slotFlowController.AddBigWinStep(RunBigWinPhase);
            }

            slotFlowController.AddBigWinStep(FinalizeSpinFlow);
            slotFlowController.StartSpinFlow();
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

        private IEnumerator RunSpinStartPhase(IReadOnlyList<SpinCommand> commands)
        {
            onSpinStartPhase?.Invoke();

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
        }

        private IEnumerator RunSpinStopPhase(IReadOnlyList<SpinCommand> commands)
        {
            onSpinStopPhase?.Invoke();

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

            while (_remainingReels > 0)
            {
                yield return null;
            }
        }

        private IEnumerator RunResultDisplayPhase()
        {
            onResultDisplayPhase?.Invoke();

            if (resultDisplayDuration > 0f)
            {
                yield return new WaitForSeconds(resultDisplayDuration);
            }
        }

        private IEnumerator RunBigWinPhase()
        {
            onBigWinPhase?.Invoke();

            if (bigWinDisplayDuration > 0f)
            {
                yield return new WaitForSeconds(bigWinDisplayDuration);
            }
        }

        private IEnumerator RunFreeGamePhase()
        {
            onFreeGamePhase?.Invoke();

            if (freeGameDisplayDuration > 0f)
            {
                yield return new WaitForSeconds(freeGameDisplayDuration);
            }
        }

        private IEnumerator FinalizeSpinFlow()
        {
            isSpinInProgress = false;
            onSpinFlowComplete?.Invoke();
            yield break;
        }

        private void OnReelStopped(ReelController reel)
        {
            _remainingReels = Mathf.Max(0, _remainingReels - 1);
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

            if (slotFlowController == null)
            {
                slotFlowController = GetComponent<SlotFlowController>();
            }
        }

        private void ApplySharedReelSettings()
        {
            if (!useSharedReelTimingProfile || sharedReelTimingProfile == null)
            {
                return;
            }

            for (int i = 0; i < reels.Count; i++)
            {
                ReelController reel = reels[i];
                if (reel == null)
                {
                    continue;
                }

                reel.ApplyTimingProfile(sharedReelTimingProfile);
            }
        }

        private void StopAllReels()
        {
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
