using System;
using UnityEngine;

namespace SlotMachine.Reels.Runtime
{
    [Serializable]
    public class FreeSpinState
    {
        [SerializeField] private bool isActive;
        [SerializeField] private int totalAwardedSpins;
        [SerializeField] private int remainingSpins;
        [SerializeField] private int completedSpins;
        [SerializeField] private int triggerScatterCount;
        [SerializeField] private int sessionId;

        public bool IsActive => isActive;
        public int TotalAwardedSpins => totalAwardedSpins;
        public int RemainingSpins => remainingSpins;
        public int CompletedSpins => completedSpins;
        public int TriggerScatterCount => triggerScatterCount;
        public int SessionId => sessionId;

        public void BeginSession(int awardedSpins, int scatters)
        {
            sessionId++;
            isActive = awardedSpins > 0;
            totalAwardedSpins = Mathf.Max(0, awardedSpins);
            remainingSpins = Mathf.Max(0, awardedSpins);
            completedSpins = 0;
            triggerScatterCount = Mathf.Max(0, scatters);
        }

        public void ConsumeSpin()
        {
            if (!isActive || remainingSpins <= 0)
            {
                return;
            }

            remainingSpins--;
            completedSpins++;

            if (remainingSpins <= 0)
            {
                EndSession();
            }
        }

        public void EndSession()
        {
            isActive = false;
            remainingSpins = 0;
        }

        public void Reset()
        {
            isActive = false;
            totalAwardedSpins = 0;
            remainingSpins = 0;
            completedSpins = 0;
            triggerScatterCount = 0;
            sessionId = 0;
        }
    }
}
