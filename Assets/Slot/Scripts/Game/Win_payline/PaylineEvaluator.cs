using System.Collections.Generic;
using SlotMachine.Reels.Data;
using UnityEngine;

namespace SlotMachine.Reels.Runtime
{
    public class PaylineEvaluator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PaylineConfig paylineConfig;
        [SerializeField] private SymbolDatabase symbolDatabase;
        [SerializeField] private BetManager betManager;

        [Header("Rules")]
        [SerializeField] private int minimumMatchCount = 3;
        [SerializeField] private bool wildSubstitutes = true;
        [SerializeField] private bool allowAllWildLinePay = true;
        [SerializeField] private int wildSymbolId = 1;

        public PaylineEvaluationResult Evaluate(SpinOutcome outcome)
        {
            List<PaylineWinResult> wins = new List<PaylineWinResult>();

            if (outcome == null)
            {
                Debug.LogWarning("[PaylineEvaluator] Cannot evaluate. SpinOutcome is null.");
                return new PaylineEvaluationResult(wins);
            }

            if (paylineConfig == null)
            {
                Debug.LogWarning("[PaylineEvaluator] Cannot evaluate. PaylineConfig is missing.");
                return new PaylineEvaluationResult(wins);
            }

            IReadOnlyList<PaylineDefinition> paylines = paylineConfig.Paylines;

            for (int i = 0; i < paylines.Count; i++)
            {
                PaylineDefinition line = paylines[i];

                if (line == null)
                {
                    continue;
                }

                PaylineWinResult win = EvaluateLine(outcome, line);

                if (win != null)
                {
                    wins.Add(win);
                }
            }

            return new PaylineEvaluationResult(wins);
        }

        private PaylineWinResult EvaluateLine(SpinOutcome outcome, PaylineDefinition line)
        {
            int reelCount = outcome.Reels.Count;

            if (!line.IsValidForReelCount(reelCount))
            {
                Debug.LogWarning($"[PaylineEvaluator] Payline {line.LineId} is not valid for {reelCount} reels.");
                return null;
            }

            int baseSymbolId = -1;
            int matchCount = 0;

            for (int reelIndex = 0; reelIndex < reelCount; reelIndex++)
            {
                int rowIndex = line.Rows[reelIndex];

                if (!TryGetSymbolAt(outcome, reelIndex, rowIndex, out int currentSymbolId))
                {
                    break;
                }

                if (!TryGetSymbolDefinition(currentSymbolId, out SymbolDefinition currentSymbol))
                {
                    break;
                }

                if (currentSymbol.IsScatter)
                {
                    break;
                }

                bool currentIsWild = currentSymbol.IsWild;

                if (baseSymbolId < 0)
                {
                    if (currentIsWild && wildSubstitutes)
                    {
                        matchCount++;
                        continue;
                    }

                    baseSymbolId = currentSymbolId;
                    matchCount++;
                    continue;
                }

                bool matchesBase = currentSymbolId == baseSymbolId;
                bool wildMatches = currentIsWild && wildSubstitutes;

                if (matchesBase || wildMatches)
                {
                    matchCount++;
                    continue;
                }

                break;
            }

            if (baseSymbolId < 0)
            {
                if (!allowAllWildLinePay)
                {
                    return null;
                }

                baseSymbolId = wildSymbolId;
            }

            if (matchCount < minimumMatchCount)
            {
                return null;
            }

            float winAmount = ResolveWinAmount(baseSymbolId, matchCount);

            if (winAmount <= 0f)
            {
                return null;
            }

            return new PaylineWinResult(
                line.LineId,
                line.LineName,
                baseSymbolId,
                matchCount,
                winAmount,
                line.Rows
            );
        }

        private bool TryGetSymbolAt(
            SpinOutcome outcome,
            int reelIndex,
            int rowIndex,
            out int symbolId)
        {
            symbolId = -1;

            if (outcome == null)
            {
                return false;
            }

            if (reelIndex < 0 || reelIndex >= outcome.Reels.Count)
            {
                return false;
            }

            ReelOutcome reel = outcome.Reels[reelIndex];

            if (reel == null || reel.VisibleSymbolIds == null)
            {
                return false;
            }

            if (rowIndex < 0 || rowIndex >= reel.VisibleSymbolIds.Count)
            {
                return false;
            }

            symbolId = reel.VisibleSymbolIds[rowIndex];
            return true;
        }

        private bool TryGetSymbolDefinition(int symbolId, out SymbolDefinition definition)
        {
            definition = null;

            if (symbolDatabase == null)
            {
                Debug.LogWarning("[PaylineEvaluator] SymbolDatabase is missing.");
                return false;
            }

            return symbolDatabase.TryGetSymbol(symbolId, out definition) && definition != null;
        }

        private float ResolveWinAmount(int symbolId, int matchCount)
        {
            if (!TryGetSymbolDefinition(symbolId, out SymbolDefinition symbol))
            {
                return 0f;
            }

            if (!symbol.TryGetMultiplier(matchCount, out float multiplier))
            {
                return 0f;
            }

            float betPerLine = betManager != null ? betManager.BetPerLine : 1f;

            return betPerLine * multiplier;
        }
    }
}