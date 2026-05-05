using System;
using System.Collections.Generic;

namespace SlotMachine.Reels.Runtime
{
    [Serializable]
    public class PaylineWinResult
    {
        public int LineId { get; }
        public string LineName { get; }
        public int SymbolId { get; }
        public int MatchCount { get; }
        public float WinAmount { get; }
        public IReadOnlyList<int> Rows => rows;

        private readonly List<int> rows;

        public PaylineWinResult(
            int lineId,
            string lineName,
            int symbolId,
            int matchCount,
            float winAmount,
            IReadOnlyList<int> rows)
        {
            LineId = lineId;
            LineName = lineName;
            SymbolId = symbolId;
            MatchCount = matchCount;
            WinAmount = winAmount;
            this.rows = rows != null ? new List<int>(rows) : new List<int>();
        }
    }

    [Serializable]
    public class PaylineEvaluationResult
    {
        public bool HasAnyWin => paylineWins.Count > 0;
        public float TotalWin { get; }
        public IReadOnlyList<PaylineWinResult> PaylineWins => paylineWins;

        private readonly List<PaylineWinResult> paylineWins = new List<PaylineWinResult>();

        public PaylineEvaluationResult(IReadOnlyList<PaylineWinResult> wins)
        {
            if (wins != null)
            {
                paylineWins.AddRange(wins);
            }

            float total = 0;
            for (int i = 0; i < paylineWins.Count; i++)
            {
                total += paylineWins[i].WinAmount;
            }

            TotalWin = total;
        }
    }
}