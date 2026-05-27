using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

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

        [Header("Auto Free Spin")]
        [SerializeField] private bool autoFreeSpin = true;
        [SerializeField] private float autoSpinDelay = 1f;

        [Tooltip("Assign your spin button event here")]
        [SerializeField] private UnityEvent onAutoFreeSpin;

        [Header("Multiplier")]
        [SerializeField] private int currentMultiplier = 1;

        [Header("Debug State")]
        [SerializeField] private FreeSpinState state = new FreeSpinState();

        [Header("Free Spin Win Tracking")]
        [SerializeField] private float totalFreeSpinWin;

        private bool currentSpinUsesFreeSpin = false;

        private Coroutine autoSpinRoutine;

        public event Action<FreeSpinState> FreeSpinsStarted;
        public event Action<FreeSpinState> FreeSpinsUpdated;
        public event Action FreeSpinsEnded;

        public FreeSpinState State => state;
        public float TotalFreeSpinWin => totalFreeSpinWin;

        public bool IsFreeSpinActive => state != null && state.IsActive;
        public bool CurrentSpinUsesFreeSpin => currentSpinUsesFreeSpin;
        public int RemainingSpins => state != null ? state.RemainingSpins : 0;
        public int CurrentMultiplier => currentMultiplier;

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
                if (outcome.HasWin)
                    currentMultiplier++;

                state.ConsumeSpin();

                onFreeSpinsUpdated?.Invoke();
                FreeSpinsUpdated?.Invoke(state);

                if (!state.IsActive)
                {
                    currentMultiplier = 1;

                    StopAutoFreeSpin();

                    onFreeSpinsEnded?.Invoke();
                    FreeSpinsEnded?.Invoke();
                }
            }
            else if (outcome.AwardsFreeSpins || outcome.TriggersFreeSpins)
            {
                int awarded = outcome.AwardedFreeSpinCount > 0 ? outcome.AwardedFreeSpinCount : ResolveAwardCount(outcome.ScatterCount);

                if (awarded > 0)
                {
                    state.BeginSession(awarded, outcome.ScatterCount);

                    currentMultiplier = 1;
                    totalFreeSpinWin = 0f;

                    onFreeSpinsStarted?.Invoke();
                    onFreeSpinsUpdated?.Invoke();

                    FreeSpinsStarted?.Invoke(state);
                    FreeSpinsUpdated?.Invoke(state);
                }
            }

            currentSpinUsesFreeSpin = false;
        }

        public void StartFreeSpinGameplay()
        {
            StartAutoFreeSpin();
        }

        [ContextMenu("Force Start 10 Free Spins")]
        public void ForceStartTenFreeSpins()
        {
            EnsureState();

            state.BeginSession(10, 3);

            currentMultiplier = 1;

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

            currentMultiplier = 1;

            currentSpinUsesFreeSpin = false;

            StopAutoFreeSpin();

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

        private void StartAutoFreeSpin()
        {
            if (!autoFreeSpin)
            {
                return;
            }

            if (autoSpinRoutine != null)
            {
                StopCoroutine(autoSpinRoutine);
            }

            autoSpinRoutine = StartCoroutine(AutoFreeSpinRoutine());
        }

        private void StopAutoFreeSpin()
        {
            if (autoSpinRoutine != null)
            {
                StopCoroutine(autoSpinRoutine);
                autoSpinRoutine = null;
            }
        }

        private IEnumerator AutoFreeSpinRoutine()
        {
            yield return null;

            while (IsFreeSpinActive)
            {
                onAutoFreeSpin?.Invoke();

                yield return new WaitForSeconds(autoSpinDelay);
            }

            autoSpinRoutine = null;
        }
        public void ResetFreeSpinWin()
        {
            totalFreeSpinWin = 0f;
        }

        public void AddFreeSpinWin(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            totalFreeSpinWin += amount;
        }
    }
}