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
        [SerializeField] private SlotFlowController slotFlowController;
        [SerializeField] private FreeSpinManager freeSpinManager;
        [SerializeField] private PaylineEvaluator paylineEvaluator;
        [SerializeField] private BetManager betManager;
        [SerializeField] private PaylineVisualizer paylineVisualizer;
        [SerializeField] private BigWinController bigWinController;

        [Header("Feature Buy")]
        [Min(1f)]
        [SerializeField] private float featureBuyMultiplier = 100f;

        private bool pendingFeatureBuy;

        [Header("Shared Reel Settings")]
        [SerializeField] private bool useSharedReelTimingProfile = true;
        [SerializeField] private ReelTimingProfile sharedReelTimingProfile = new ReelTimingProfile();

        [Header("Spin Flow")]
        [SerializeField] private float scatterSymbolId = 0.08f;
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
            featureBuyMultiplier = Mathf.Max(1f, featureBuyMultiplier);

            reelStartDelay = Mathf.Max(0f, reelStartDelay);
            loopHoldDuration = Mathf.Max(0f, loopHoldDuration);
            reelStopDelay = Mathf.Max(0f, reelStopDelay);
            resultDisplayDuration = Mathf.Max(0f, resultDisplayDuration);
            paylineDisplayDuration = Mathf.Max(0f, paylineDisplayDuration);
            freeGameDisplayDuration = Mathf.Max(0f, freeGameDisplayDuration);

            sharedReelTimingProfile?.Clamp();

            if (!Application.isPlaying)
            {
                ApplySharedReelSettings();
            }
        }

        public void BuyFeature()
        {
            if (isSpinInProgress || (slotFlowController != null && slotFlowController.IsRunning))
            {
                return;
            }

            CacheLocalReferences();

            if (betManager == null)
            {
                Debug.LogError($"[{name}] BetManager reference is missing.");
                return;
            }

            bool isFreeSpinSpin = freeSpinManager != null && freeSpinManager.CurrentSpinUsesFreeSpin;

            if (isFreeSpinSpin)
            {
                Debug.LogWarning($"[{name}] Cannot buy feature during free spins.");
                return;
            }

            float buyCost = betManager.TotalBet * featureBuyMultiplier;

            if (!betManager.TrySpend(buyCost))
            {
                Debug.LogWarning($"[{name}] Not enough balance for Feature Buy. Cost: {buyCost}");
                return;
            }

            pendingFeatureBuy = true;
            StartSpin();
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

            if (!pendingFeatureBuy && !isFreeSpinSpin && betManager != null && !betManager.TrySpendCurrentBet())
            {
                isSpinInProgress = false;
                return;
            }

            SpinRequest request = new SpinRequest
            {
                IsFeatureBuy = pendingFeatureBuy,
                IsFreeSpin = isFreeSpinSpin
            };

            pendingFeatureBuy = false;

            SpinOutcome outcome = ResolveNextOutcome(request);

            if (outcome == null)
            {
                isSpinInProgress = false;
                return;
            }

            lastOutcome = outcome;

            ValidateOutcomeAgainstReels(outcome);

            lastPaylineEvaluation = paylineEvaluator != null ? paylineEvaluator.Evaluate(outcome) : null;

            if (lastPaylineEvaluation != null)
            {
                float totalWin = lastPaylineEvaluation.TotalWin;

                bool isBigWin = bigWinController != null && bigWinController.ResolveBigWinType(totalWin) != BigWinType.None;

                outcome.SetWinData(lastPaylineEvaluation.HasAnyWin, isBigWin, totalWin);
            }

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
                isSpinInProgress = false;
                return;
            }

            isSpinInProgress = true;
            _remainingReels = 0;

            slotFlowController.ClearAllQueues();

            slotFlowController.AddSpinStartStep(() => RunSpinStartPhase(commands));
            slotFlowController.AddSpinStopStep(() => RunSpinStopPhase(commands));
            slotFlowController.AddResultDisplayStep(() => RunResultDisplayPhase(outcome));
            slotFlowController.AddLineWinStep(RunPaylinePhase);
            if (outcome.TriggersFreeSpins)
            {
                slotFlowController.AddLineWinStep(RunScatterHighlightPhase);
            }

            // BigWinController decides internally whether to play or skip.
            slotFlowController.AddBigWinStep(RunBigWinPhase);

            if (outcome.TriggersFreeSpins)
            {
                slotFlowController.AddFreeGameStep(RunFreeGamePhase);
            }

            slotFlowController.AddCompleteStep(FinalizeSpinFlow);
            slotFlowController.StartSpinFlow();

        }

        private IEnumerator RunSpinStartPhase(IReadOnlyList<SpinCommand> commands)
        {
            GameEvent.onSpinStartPhase?.Invoke();

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

            while (!HaveAllReelsReachedLoopPhase(commands))
            {
                yield return null;
            }
        }

        private IEnumerator RunSpinStopPhase(IReadOnlyList<SpinCommand> commands)
        {

            if (loopHoldDuration > 0f)
            {
                yield return new WaitForSeconds(loopHoldDuration);
            }

            for (int i = 0; i < commands.Count; i++)
            {
                SpinCommand command = commands[i];

                command.Reel.StopSpin(
                    command.Reel.ReelIndex,
                    command.StopIndex,
                    command.VisibleSymbolIds
                );

                if (i < commands.Count - 1 && reelStopDelay > 0f)
                {
                    yield return new WaitForSeconds(reelStopDelay);
                }
            }

            while (_remainingReels > 0)
            {
                yield return null;
            }

            GameEvent.onSpinStopPhase?.Invoke();
        }

        private IEnumerator RunResultDisplayPhase(SpinOutcome outcome)
        {
            if (outcome.HasWin)
            {
                GameEvent.onResultDisplayPhase?.Invoke();

                if (resultDisplayDuration > 0f)
                {
                    yield return new WaitForSeconds(resultDisplayDuration);
                }
            }
            else
            {
                GameEvent.onDragonLose?.Invoke();
                yield return null;
            }
        }

        private IEnumerator RunPaylinePhase()
        {
            if (lastPaylineEvaluation == null || !lastPaylineEvaluation.HasAnyWin)
            {
                yield break;
            }

            GameEvent.onPaylinePhase?.Invoke();
            GameEvent.onDragonWin?.Invoke();

            // Show combined win first
            if (paylineVisualizer != null)
            {
                paylineVisualizer.ShowCombinedWin(lastPaylineEvaluation);
            }

            if (paylineDisplayDuration > 0f)
            {
                yield return new WaitForSeconds(paylineDisplayDuration);
            }

            // Show every single payline win
            for (int i = 0; i < lastPaylineEvaluation.PaylineWins.Count; i++)
            {
                PaylineWinResult win = lastPaylineEvaluation.PaylineWins[i];

                Debug.Log(
                    $"PAYLINE WIN | " +
                    $"Line {win.LineId} {win.LineName} | " +
                    $"Symbol {win.SymbolId} | " +
                    $"Match {win.MatchCount} | " +
                    $"Win {win.WinAmount}"
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

            if (paylineVisualizer != null)
            {
                paylineVisualizer.ClearVisuals();
            }

            GameEvent.onDragonIdle?.Invoke();
        }

        private IEnumerator RunScatterHighlightPhase()
        {
            if (lastOutcome == null || !lastOutcome.TriggersFreeSpins)
            {
                yield break;
            }

            if (paylineVisualizer != null)
            {
                paylineVisualizer.ShowScatters(scatterSymbolId);
            }

            yield return new WaitForSeconds(paylineDisplayDuration);

            if (paylineVisualizer != null)
            {
                paylineVisualizer.ClearVisuals();
            }
        }
        private IEnumerator RunBigWinPhase()
        {
            if (bigWinController == null)
            {
                yield break;
            }

            float totalWin = 0f;

            if (lastPaylineEvaluation != null)
            {
                totalWin = lastPaylineEvaluation.TotalWin;
            }

            yield return bigWinController.TryPlayBigWin(totalWin);
        }

        private IEnumerator RunFreeGamePhase()
        {
            freeSpinManager?.HandleCompletedSpin(lastOutcome);

            GameEvent.onFreeGamePhase?.Invoke();

            if (freeGameDisplayDuration > 0f)
            {
                yield return new WaitForSeconds(freeGameDisplayDuration);
            }
        }

        private IEnumerator FinalizeSpinFlow()
        {
            bool isFreeSpinSpin = freeSpinManager != null && freeSpinManager.CurrentSpinUsesFreeSpin;

            if (lastOutcome == null || !lastOutcome.TriggersFreeSpins)
            {
                freeSpinManager?.HandleCompletedSpin(lastOutcome);
            }

            float totalWin = 0f;

            if (lastPaylineEvaluation != null)
            {
                totalWin = lastPaylineEvaluation.TotalWin;
            }

            if (totalWin > 0f)
            {
                if (isFreeSpinSpin)
                {
                    freeSpinManager.AddFreeSpinWin(totalWin);
                }

                if (betManager != null)
                {
                    betManager.AddWin(totalWin);
                }
            }

            isSpinInProgress = false;

            GameEvent.onSpinFlowComplete?.Invoke();

            yield break;
        }

        private void ValidateOutcomeAgainstReels(SpinOutcome outcome)
        {
            if (outcome == null || outcome.Reels == null || outcome.Reels.Count == 0)
            {
                return;
            }

            foreach (ReelOutcome reelOutcome in outcome.Reels)
            {
                if (reelOutcome != null &&
                    (reelOutcome.VisibleSymbolIds == null || reelOutcome.VisibleSymbolIds.Count == 0))
                {
                    Debug.LogError($"[{name}] Reel {reelOutcome.ReelIndex} has no visible symbols.");
                }
            }
        }

        private List<SpinCommand> BuildCommands(SpinOutcome outcome)
        {
            List<SpinCommand> commands = new List<SpinCommand>();

            if (outcome == null)
            {
                return commands;
            }

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

                commands.Add(new SpinCommand(
                    reel,
                    reelOutcome.StopIndex,
                    reelOutcome.VisibleSymbolIds
                ));
            }

            return commands;
        }

        private void OnReelStopped(ReelController reel)
        {
            _remainingReels = Mathf.Max(0, _remainingReels - 1);
        }

        private SpinOutcome ResolveNextOutcome(SpinRequest request)
        {
            if (spinResultGenerator == null)
            {
                return null;
            }

            return spinResultGenerator.GenerateOutcome(reels, request);
        }

        private void CacheLocalReferences()
        {
            if (betManager == null)
            {
                betManager = GetComponent<BetManager>();
            }

            if (spinResultGenerator == null)
            {
                spinResultGenerator = GetComponent<SpinResultGenerator>();
            }

            if (slotFlowController == null)
            {
                slotFlowController = GetComponent<SlotFlowController>();
            }

            if (freeSpinManager == null)
            {
                freeSpinManager = GetComponent<FreeSpinManager>();
            }

            if (paylineEvaluator == null)
            {
                paylineEvaluator = GetComponent<PaylineEvaluator>();
            }

            if (paylineVisualizer == null)
            {
                paylineVisualizer = GetComponent<PaylineVisualizer>();
            }

            if (bigWinController == null)
            {
                bigWinController = GetComponent<BigWinController>();
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
                if (reels[i] != null)
                {
                    reels[i].ApplyTimingProfile(sharedReelTimingProfile);
                }
            }
        }

        private void StopAllReels()
        {
            isSpinInProgress = false;
            _remainingReels = 0;

            for (int i = 0; i < reels.Count; i++)
            {
                if (reels[i] != null)
                {
                    reels[i].ResetReel(reels[i].ReelIndex);
                }
            }
        }

        private static bool TryGetReelOutcome(
            SpinOutcome outcome,
            int reelIndex,
            out ReelOutcome reelOutcome)
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

                if (reel == null || reel.CurrentPhase != ReelSpinPhase.Loop)
                {
                    return false;
                }
            }

            return true;
        }

        private readonly struct SpinCommand
        {
            public SpinCommand(
                ReelController reel,
                int stopIndex,
                IReadOnlyList<int> visibleSymbolIds)
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