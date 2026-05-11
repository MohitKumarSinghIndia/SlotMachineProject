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
        [SerializeField] private FreeSpinManager freeSpinManager;
        [SerializeField] private PaylineEvaluator paylineEvaluator;
        [SerializeField] private BetManager betManager;

        // NEW: Added the reference to the Visualizer
        [SerializeField] private PaylineVisualizer paylineVisualizer;

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
        [SerializeField] private float paylineDisplayDuration = 0.45f;

        [Min(0f)]
        [SerializeField] private float freeGameDisplayDuration = 0f;

        [Min(0f)]
        [SerializeField] private float bigWinDisplayDuration = 0f;

        [Min(0)]
        [SerializeField] private int bigWinThreshold = 80;

        [Header("Phase Events")]
        [SerializeField] private UnityEvent onSpinStartPhase;
        [SerializeField] private UnityEvent onSpinStopPhase;
        [SerializeField] private UnityEvent onResultDisplayPhase;
        [SerializeField] private UnityEvent onPaylinePhase;
        [SerializeField] private UnityEvent onFreeGamePhase;
        [SerializeField] private UnityEvent onBigWinPhase;
        [SerializeField] private UnityEvent onSpinFlowComplete;

        [Header("Debug State")]
        [SerializeField] private bool isSpinInProgress;
        [SerializeField] private SpinOutcome lastOutcome;

        private PaylineEvaluationResult lastPaylineEvaluation;
        private int _remainingReels;

        public bool IsSpinInProgress => isSpinInProgress;
        public SpinOutcome LastOutcome => lastOutcome;
        public PaylineEvaluationResult LastPaylineEvaluation => lastPaylineEvaluation;

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
            paylineDisplayDuration = Mathf.Max(0f, paylineDisplayDuration);
            freeGameDisplayDuration = Mathf.Max(0f, freeGameDisplayDuration);
            bigWinDisplayDuration = Mathf.Max(0f, bigWinDisplayDuration);
            bigWinThreshold = Mathf.Max(0, bigWinThreshold);

            sharedReelTimingProfile?.Clamp();

            if (!Application.isPlaying)
            {
                ApplySharedReelSettings();
            }
        }

        [ContextMenu("Spin All Reels")]
        public void StartSpin()
        {
            if (isSpinInProgress || (slotFlowController != null && slotFlowController.IsRunning))
            {
                return;
            }

            CacheLocalReferences();
            ApplySharedReelSettings();
            StopAllReels();

            freeSpinManager?.NotifySpinStarted();

            bool isFreeSpinSpin = freeSpinManager != null && freeSpinManager.CurrentSpinUsesFreeSpin;

            if (!isFreeSpinSpin && betManager != null && !betManager.TrySpendCurrentBet())
            {
                isSpinInProgress = false;
                return;
            }

            SpinOutcome outcome = ResolveNextOutcome();
            if (outcome == null)
            {
                return;
            }

            lastOutcome = outcome;
            ValidateOutcomeAgainstReels(outcome);

            lastPaylineEvaluation = paylineEvaluator != null
                ? paylineEvaluator.Evaluate(outcome)
                : null;

            BuildAndRunSpinFlow(outcome);
        }

        private void ValidateOutcomeAgainstReels(SpinOutcome outcome)
        {
            if (outcome == null || outcome.Reels == null || outcome.Reels.Count == 0) return;

            foreach (ReelOutcome reelOutcome in outcome.Reels)
            {
                if (reelOutcome != null && (reelOutcome.VisibleSymbolIds == null || reelOutcome.VisibleSymbolIds.Count == 0))
                {
                    Debug.LogError($"[{name}] Reel {reelOutcome.ReelIndex} has no visible symbols.");
                }
            }
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
                isSpinInProgress = false;
                return;
            }

            isSpinInProgress = true;
            _remainingReels = 0;

            slotFlowController.ClearAllQueues();

            slotFlowController.AddSpinStartStep(() => RunSpinStartPhase(commands));
            slotFlowController.AddSpinStopStep(() => RunSpinStopPhase(commands));
            slotFlowController.AddResultDisplayStep(RunResultDisplayPhase);
            slotFlowController.AddLineWinStep(RunPaylinePhase);

            if (ShouldPlayBigWin(outcome)) slotFlowController.AddBigWinStep(RunBigWinPhase);
            if (outcome.TriggersFreeSpins) slotFlowController.AddFreeGameStep(RunFreeGamePhase);

            slotFlowController.AddCompleteStep(FinalizeSpinFlow);
            slotFlowController.StartSpinFlow();
        }

        private List<SpinCommand> BuildCommands(SpinOutcome outcome)
        {
            List<SpinCommand> commands = new List<SpinCommand>();

            if (outcome == null) return commands;

            for (int i = 0; i < reels.Count; i++)
            {
                ReelController reel = reels[i];
                if (reel == null) continue;

                if (!TryGetReelOutcome(outcome, reel.ReelIndex, out ReelOutcome reelOutcome)) continue;

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

                if (i < commands.Count - 1 && reelStartDelay > 0f) yield return new WaitForSeconds(reelStartDelay);
            }

            while (!HaveAllReelsReachedLoopPhase(commands)) yield return null;
        }

        private IEnumerator RunSpinStopPhase(IReadOnlyList<SpinCommand> commands)
        {
            if (loopHoldDuration > 0f) yield return new WaitForSeconds(loopHoldDuration);

            for (int i = 0; i < commands.Count; i++)
            {
                SpinCommand command = commands[i];
                command.Reel.StopSpin(command.Reel.ReelIndex, command.StopIndex, command.VisibleSymbolIds);

                if (i < commands.Count - 1 && reelStopDelay > 0f) yield return new WaitForSeconds(reelStopDelay);
            }

            while (_remainingReels > 0) yield return null;
            onSpinStopPhase?.Invoke();
        }

        private IEnumerator RunResultDisplayPhase()
        {
            onResultDisplayPhase?.Invoke();
            if (resultDisplayDuration > 0f) yield return new WaitForSeconds(resultDisplayDuration);
        }

        // =========================================================================
        // UPDATED: This is where we call the Visualizer!
        // =========================================================================
        private IEnumerator RunPaylinePhase()
        {
            if (lastPaylineEvaluation == null || !lastPaylineEvaluation.HasAnyWin)
            {
                yield break; // Exit if no wins
            }

            onPaylinePhase?.Invoke();

            // 1. SHOW COMBINED WINS
            if (paylineVisualizer != null)
            {
                paylineVisualizer.ShowCombinedWin(lastPaylineEvaluation);
            }

            // Hold the combined win so the player can see all the symbols bounce
            if (paylineDisplayDuration > 0f)
            {
                yield return new WaitForSeconds(paylineDisplayDuration);
            }

            // 2. SHOW INDIVIDUAL LINES
            // If there's more than 1 win line, cycle through them one by one
            if (lastPaylineEvaluation.PaylineWins.Count > 1)
            {
                for (int i = 0; i < lastPaylineEvaluation.PaylineWins.Count; i++)
                {
                    PaylineWinResult win = lastPaylineEvaluation.PaylineWins[i];

                    // RESTORED DEBUG LOG: Prints the math to the console for every single win
                    Debug.Log(
                        $"PAYLINE WIN | Line {win.LineId} {win.LineName} | " +
                        $"Symbol {win.SymbolId} | Match {win.MatchCount} | Win {win.WinAmount}"
                    );


                    if (paylineVisualizer != null)
                    {
                        paylineVisualizer.ShowSingleLine(win);
                    }

                    if (paylineDisplayDuration > 0f)
                    {
                        yield return new WaitForSeconds(paylineDisplayDuration);
                    }
                }
            }

            // 3. CLEAR VISUALS
            // Reset everything back to normal before moving to Big Win / Finalize
            if (paylineVisualizer != null)
            {
                paylineVisualizer.ClearVisuals();
            }
        }
        // =========================================================================

        private IEnumerator RunBigWinPhase()
        {
            onBigWinPhase?.Invoke();
            if (bigWinDisplayDuration > 0f) yield return new WaitForSeconds(bigWinDisplayDuration);
        }

        private IEnumerator RunFreeGamePhase()
        {
            freeSpinManager?.HandleCompletedSpin(lastOutcome);
            onFreeGamePhase?.Invoke();
            if (freeGameDisplayDuration > 0f) yield return new WaitForSeconds(freeGameDisplayDuration);
        }

        private IEnumerator FinalizeSpinFlow()
        {
            if (lastOutcome == null || !lastOutcome.TriggersFreeSpins)
            {
                freeSpinManager?.HandleCompletedSpin(lastOutcome);
            }

            float totalWin = lastPaylineEvaluation != null ? lastPaylineEvaluation.TotalWin : 0f;

            if (betManager != null && totalWin > 0f)
            {
                betManager.AddWin(totalWin);
            }

            isSpinInProgress = false;
            onSpinFlowComplete?.Invoke();
            yield break;
        }

        private bool ShouldPlayBigWin(SpinOutcome outcome)
        {
            float totalWin = GetCurrentTotalWin();
            return totalWin > 0f && totalWin >= bigWinThreshold;
        }

        private float GetCurrentTotalWin()
        {
            return lastPaylineEvaluation != null ? lastPaylineEvaluation.TotalWin : 0f;
        }

        private void OnReelStopped(ReelController reel)
        {
            _remainingReels = Mathf.Max(0, _remainingReels - 1);
        }

        private SpinOutcome ResolveNextOutcome()
        {
            if (spinResultGenerator == null) return null;
            return spinResultGenerator.GenerateOutcome(reels);
        }

        private void CacheLocalReferences()
        {
            if (betManager == null) betManager = GetComponent<BetManager>();
            if (spinResultGenerator == null) spinResultGenerator = GetComponent<SpinResultGenerator>();
            if (slotFlowController == null) slotFlowController = GetComponent<SlotFlowController>();
            if (freeSpinManager == null) freeSpinManager = GetComponent<FreeSpinManager>();
            if (paylineEvaluator == null) paylineEvaluator = GetComponent<PaylineEvaluator>();

            // Auto-cache visualizer if it's on the same object
            if (paylineVisualizer == null) paylineVisualizer = GetComponent<PaylineVisualizer>();
        }

        private void ApplySharedReelSettings()
        {
            if (!useSharedReelTimingProfile || sharedReelTimingProfile == null) return;
            for (int i = 0; i < reels.Count; i++) if (reels[i] != null) reels[i].ApplyTimingProfile(sharedReelTimingProfile);
        }

        private void StopAllReels()
        {
            isSpinInProgress = false;
            _remainingReels = 0;
            for (int i = 0; i < reels.Count; i++) if (reels[i] != null) reels[i].ResetReel(reels[i].ReelIndex);
        }

        private static bool TryGetReelOutcome(SpinOutcome outcome, int reelIndex, out ReelOutcome reelOutcome)
        {
            if (outcome == null || outcome.Reels == null)
            {
                reelOutcome = null;
                return false;
            }

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

        private static bool HaveAllReelsReachedLoopPhase(IReadOnlyList<SpinCommand> commands)
        {
            for (int i = 0; i < commands.Count; i++)
            {
                ReelController reel = commands[i].Reel;
                if (reel == null || reel.CurrentPhase != ReelSpinPhase.Loop) return false;
            }
            return true;
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