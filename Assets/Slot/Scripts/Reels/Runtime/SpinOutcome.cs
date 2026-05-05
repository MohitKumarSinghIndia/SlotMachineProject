using System;
using System.Collections.Generic;

namespace SlotMachine.Reels.Runtime
{
    [Serializable]
    public class ReelOutcome
    {
        public int ReelIndex => reelIndex;
        public int StopIndex => stopIndex;
        public IReadOnlyList<int> VisibleSymbolIds => visibleSymbolIds;

        private int reelIndex;
        private int stopIndex;
        private List<int> visibleSymbolIds = new List<int>();

        public ReelOutcome(int reelIndex, int stopIndex, IReadOnlyList<int> visibleSymbols)
        {
            this.reelIndex = reelIndex;
            this.stopIndex = stopIndex;
            this.visibleSymbolIds = visibleSymbols != null
                ? new List<int>(visibleSymbols)
                : new List<int>();
        }
    }

    [Serializable]
    public class SpinOutcome
    {
        public string SpinId => spinId;
        public string TimestampUtc => timestampUtc;
        public bool FromReplay => fromReplay;
        public bool IsFreeSpinSpin => isFreeSpinSpin;
        public bool HasWin => hasWin;
        public bool TriggersFreeSpins => triggersFreeSpins;
        public bool AwardsFreeSpins => awardsFreeSpins;
        public int AwardedFreeSpinCount => awardedFreeSpinCount;
        public bool IsBigWin => isBigWin;
        public int TotalWin => totalWin;
        public int ScatterCount => scatterCount;
        public IReadOnlyList<ReelOutcome> Reels => reels;

        private string spinId;
        private string timestampUtc;
        private bool fromReplay;
        private bool isFreeSpinSpin;
        private bool hasWin;
        private bool triggersFreeSpins;
        private bool awardsFreeSpins;
        private int awardedFreeSpinCount;
        private bool isBigWin;
        private int totalWin;
        private int scatterCount;
        private List<ReelOutcome> reels = new List<ReelOutcome>();

        public SpinOutcome(
            string spinId,
            string timestampUtc,
            bool fromReplay,
            bool isFreeSpinSpin,
            bool hasWin,
            bool triggersFreeSpins,
            bool awardsFreeSpins,
            int awardedFreeSpinCount,
            bool isBigWin,
            int totalWin,
            int scatterCount,
            IReadOnlyList<ReelOutcome> reels)
        {
            this.spinId = spinId;
            this.timestampUtc = timestampUtc;
            this.fromReplay = fromReplay;
            this.isFreeSpinSpin = isFreeSpinSpin;
            this.hasWin = hasWin;
            this.triggersFreeSpins = triggersFreeSpins;
            this.awardsFreeSpins = awardsFreeSpins;
            this.awardedFreeSpinCount = awardedFreeSpinCount;
            this.isBigWin = isBigWin;
            this.totalWin = totalWin;
            this.scatterCount = scatterCount;
            this.reels = reels != null
                ? new List<ReelOutcome>(reels)
                : new List<ReelOutcome>();
        }

        public bool TryGetStopIndex(int reelIndex, out int stopIndex)
        {
            for (int i = 0; i < reels.Count; i++)
            {
                ReelOutcome reel = reels[i];
                if (reel != null && reel.ReelIndex == reelIndex)
                {
                    stopIndex = reel.StopIndex;
                    return true;
                }
            }

            stopIndex = 0;
            return false;
        }

        public bool TryGetReelOutcome(int reelIndex, out ReelOutcome reelOutcome)
        {
            for (int i = 0; i < reels.Count; i++)
            {
                ReelOutcome reel = reels[i];
                if (reel != null && reel.ReelIndex == reelIndex)
                {
                    reelOutcome = reel;
                    return true;
                }
            }

            reelOutcome = null;
            return false;
        }
    }
}