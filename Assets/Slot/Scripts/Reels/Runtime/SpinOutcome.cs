using System;
using System.Collections.Generic;

namespace SlotMachine.Reels.Runtime
{
    [Serializable]
    public class ReelOutcome
    {
        public int ReelIndex;
        public int StopIndex;
        public List<int> VisibleSymbolIds = new List<int>();
    }

    [Serializable]
    public class SpinOutcome
    {
        public string SpinId;
        public string TimestampUtc;
        public bool FromReplay;
        public bool HasWin;
        public bool TriggersFreeSpins;
        public bool IsBigWin;
        public int TotalWin;
        public int ScatterCount;
        public List<ReelOutcome> Reels = new List<ReelOutcome>();

        public bool TryGetStopIndex(int reelIndex, out int stopIndex)
        {
            for (int i = 0; i < Reels.Count; i++)
            {
                ReelOutcome reel = Reels[i];
                if (reel != null && reel.ReelIndex == reelIndex)
                {
                    stopIndex = reel.StopIndex;
                    return true;
                }
            }

            stopIndex = 0;
            return false;
        }
    }
}
