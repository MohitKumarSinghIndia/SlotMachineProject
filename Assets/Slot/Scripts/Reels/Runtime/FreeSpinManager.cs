using UnityEngine;
using UnityEngine.Events;
using System;

namespace SlotMachine.Reels.Runtime
{
    public class FreeSpinManager : MonoBehaviour
    {
        [Header("Award Rules")]
        [Min(0)]
        [SerializeField] private int freeSpinsForThreeScatters = 10;
        [Min(0)]
        [SerializeField] private int freeSpinsForFourScatters = 15;
        [Min(0)]
        [SerializeField] private int freeSpinsForFiveOrMoreScatters = 20;

        [Header("Events")]
        [SerializeField] private UnityEvent onFreeSpinsStarted;
        [SerializeField] private UnityEvent onFreeSpinsUpdated;
        [SerializeField] private UnityEvent onFreeSpinsEnded;

        [Header("Debug State")]
        [SerializeField] private FreeSpinState state = new FreeSpinState();
        [SerializeField] private bool currentSpinUsesFreeSpin;

        public event Action<FreeSpinState> FreeSpinsStarted;
        public event Action<FreeSpinState> FreeSpinsUpdated;
        public event Action FreeSpinsEnded;

        public FreeSpinState State => state;
        public bool IsFreeSpinActive => state != null && state.IsActive;
        public bool CurrentSpinUsesFreeSpin => currentSpinUsesFreeSpin;
        public int RemainingSpins => state != null ? state.RemainingSpins : 0;
        public int CurrentMultiplier => 1;

        private void Awake()
        {
            EnsureState();
        }

        private void OnValidate()
        {
            freeSpinsForThreeScatters = Mathf.Max(0, freeSpinsForThreeScatters);
            freeSpinsForFourScatters = Mathf.Max(0, freeSpinsForFourScatters);
            freeSpinsForFiveOrMoreScatters = Mathf.Max(0, freeSpinsForFiveOrMoreScatters);
            EnsureState();
        }

        public void NotifySpinStarted()
        {
            EnsureState();
            currentSpinUsesFreeSpin = state.IsActive;
        }

        public void HandleCompletedSpin(SpinOutcome outcome)
        {
            EnsureState();
            if (outcome == null)
            {
                currentSpinUsesFreeSpin = false;
                return;
            }

            if (currentSpinUsesFreeSpin)
            {
                state.ConsumeSpin();
                onFreeSpinsUpdated?.Invoke();
                FreeSpinsUpdated?.Invoke(state);

                if (!state.IsActive)
                {
                    onFreeSpinsEnded?.Invoke();
                    FreeSpinsEnded?.Invoke();
                }
            }
            else if (outcome.AwardsFreeSpins || outcome.TriggersFreeSpins)
            {
                int awarded = outcome.AwardedFreeSpinCount > 0
                    ? outcome.AwardedFreeSpinCount
                    : ResolveAwardCount(outcome.ScatterCount);

                if (awarded > 0)
                {
                    state.BeginSession(awarded, outcome.ScatterCount);
                    onFreeSpinsStarted?.Invoke();
                    onFreeSpinsUpdated?.Invoke();
                    FreeSpinsStarted?.Invoke(state);
                    FreeSpinsUpdated?.Invoke(state);
                }
            }

            currentSpinUsesFreeSpin = false;
        }

        [ContextMenu("Force Start 10 Free Spins")]
        public void ForceStartTenFreeSpins()
        {
            EnsureState();
            state.BeginSession(10, 3);
            onFreeSpinsStarted?.Invoke();
            onFreeSpinsUpdated?.Invoke();
            FreeSpinsStarted?.Invoke(state);
            FreeSpinsUpdated?.Invoke(state);
        }

        [ContextMenu("End Free Spins")]
        public void ForceEndFreeSpins()
        {
            EnsureState();
            bool wasActive = state.IsActive;
            state.EndSession();
            currentSpinUsesFreeSpin = false;
            onFreeSpinsUpdated?.Invoke();
            FreeSpinsUpdated?.Invoke(state);

            if (wasActive)
            {
                onFreeSpinsEnded?.Invoke();
                FreeSpinsEnded?.Invoke();
            }
        }

        private int ResolveAwardCount(int scatterCount)
        {
            if (scatterCount >= 5)
            {
                return freeSpinsForFiveOrMoreScatters;
            }

            if (scatterCount == 4)
            {
                return freeSpinsForFourScatters;
            }

            if (scatterCount == 3)
            {
                return freeSpinsForThreeScatters;
            }

            return 0;
        }

        private void EnsureState()
        {
            state ??= new FreeSpinState();
        }
    }
}
