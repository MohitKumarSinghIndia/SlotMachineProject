using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;

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

        // =====================================================
        // DRAGON BLESSING
        // =====================================================

        [Header("Dragon Blessing")]

        [SerializeField]
        private int[] multiplierSteps =
        {
            1,
            2,
            3,
            5,
            8,
            10,
            15,
            20
        };

        [SerializeField] private int currentMultiplier = 1;

        [SerializeField] private int winningSpinCount;

        [SerializeField] private float totalFeatureWin;

        [SerializeField] private bool wishGrantedTriggered;

        [SerializeField] private bool dragonBlessingActive;

        [Header("Dragon Blessing Events")]

        [SerializeField]
        private UnityEvent<int> onMultiplierChanged;

        [SerializeField]
        private UnityEvent<float> onFeatureWinChanged;

        [SerializeField]
        private UnityEvent onWishGranted;

        // =====================================================
        // DEBUG
        // =====================================================

        [Header("Debug State")]

        [SerializeField]
        private FreeSpinState state = new FreeSpinState();

        [SerializeField]
        private bool currentSpinUsesFreeSpin;

        private Coroutine autoSpinRoutine;

        // =====================================================
        // EVENTS
        // =====================================================

        public event Action<FreeSpinState> FreeSpinsStarted;

        public event Action<FreeSpinState> FreeSpinsUpdated;

        public event Action FreeSpinsEnded;

        // =====================================================
        // PUBLIC PROPERTIES
        // =====================================================

        public FreeSpinState State => state;

        public bool IsFreeSpinActive =>
            state != null && state.IsActive;

        public bool CurrentSpinUsesFreeSpin =>
            currentSpinUsesFreeSpin;

        public int RemainingSpins =>
            state != null ? state.RemainingSpins : 0;

        public int CurrentMultiplier =>
            dragonBlessingActive
                ? currentMultiplier
                : 1;

        public bool IsDragonBlessingActive =>
            dragonBlessingActive;

        public float TotalFeatureWin =>
            totalFeatureWin;

        // =====================================================
        // UNITY
        // =====================================================

        private void Awake()
        {
            EnsureState();
        }

        private void OnValidate()
        {
            freeSpinsForThreeScatters =
                Mathf.Max(0, freeSpinsForThreeScatters);

            freeSpinsForFourScatters =
                Mathf.Max(0, freeSpinsForFourScatters);

            freeSpinsForFiveOrMoreScatters =
                Mathf.Max(0, freeSpinsForFiveOrMoreScatters);

            EnsureState();
        }

        // =====================================================
        // SPIN START
        // =====================================================

        public void NotifySpinStarted()
        {
            EnsureState();

            currentSpinUsesFreeSpin = state.IsActive;
        }

        // =====================================================
        // HANDLE COMPLETED SPIN
        // =====================================================

        public void HandleCompletedSpin(SpinOutcome outcome)
        {
            EnsureState();

            if (outcome == null)
            {
                currentSpinUsesFreeSpin = false;
                return;
            }

            // =================================================
            // FREE SPIN ACTIVE
            // =================================================

            if (currentSpinUsesFreeSpin)
            {
                // ---------------------------------------------
                // DRAGON BLESSING PROCESS
                // ---------------------------------------------

                ProcessDragonBlessingSpin(outcome);

                // ---------------------------------------------
                // CONSUME SPIN
                // ---------------------------------------------

                state.ConsumeSpin();

                onFreeSpinsUpdated?.Invoke();

                FreeSpinsUpdated?.Invoke(state);

                // ---------------------------------------------
                // FEATURE END
                // ---------------------------------------------

                if (!state.IsActive)
                {
                    dragonBlessingActive = false;

                    Debug.Log(
                        $"DRAGON BLESSING ENDED | " +
                        $"Total Feature Win = {totalFeatureWin}"
                    );

                    StopAutoFreeSpin();

                    onFreeSpinsEnded?.Invoke();

                    FreeSpinsEnded?.Invoke();
                }
            }

            // =================================================
            // START FREE SPINS
            // =================================================

            else if (
                outcome.AwardsFreeSpins ||
                outcome.TriggersFreeSpins)
            {
                int awarded =
                    outcome.AwardedFreeSpinCount > 0
                    ? outcome.AwardedFreeSpinCount
                    : ResolveAwardCount(outcome.ScatterCount);

                if (awarded > 0)
                {
                    state.BeginSession(
                        awarded,
                        outcome.ScatterCount
                    );

                    // -----------------------------------------
                    // START DRAGON BLESSING
                    // -----------------------------------------

                    dragonBlessingActive = true;

                    currentMultiplier = 1;

                    winningSpinCount = 0;

                    totalFeatureWin = 0;

                    wishGrantedTriggered = false;

                    Debug.Log(
                        "DRAGON BLESSING STARTED"
                    );

                    onMultiplierChanged?.Invoke(
                        currentMultiplier
                    );

                    // -----------------------------------------
                    // EVENTS
                    // -----------------------------------------

                    onFreeSpinsStarted?.Invoke();

                    onFreeSpinsUpdated?.Invoke();

                    FreeSpinsStarted?.Invoke(state);

                    FreeSpinsUpdated?.Invoke(state);

                    StartAutoFreeSpin();
                }
            }

            currentSpinUsesFreeSpin = false;
        }

        // =====================================================
        // DRAGON BLESSING SPIN
        // =====================================================

        private void ProcessDragonBlessingSpin(
            SpinOutcome outcome)
        {
            if (!dragonBlessingActive)
                return;

            if (outcome == null)
                return;

            float baseWin = outcome.TotalWin;

            bool hasWin = baseWin > 0;

            // ---------------------------------------------
            // APPLY CURRENT MULTIPLIER
            // ---------------------------------------------

            float finalWin =
                baseWin * currentMultiplier;

            totalFeatureWin += finalWin;

            Debug.Log(
                $"Dragon Blessing Spin | " +
                $"Base Win = {baseWin} | " +
                $"Multiplier = {currentMultiplier}x | " +
                $"Final Win = {finalWin}"
            );

            onFeatureWinChanged?.Invoke(
                totalFeatureWin
            );

            // ---------------------------------------------
            // INCREASE MULTIPLIER AFTER WIN
            // ---------------------------------------------

            if (hasWin)
            {
                IncreaseMultiplier();
            }

            // ---------------------------------------------
            // WISH GRANTED
            // ---------------------------------------------

            CheckWishGranted(hasWin);
        }

        // =====================================================
        // MULTIPLIER
        // =====================================================

        private void IncreaseMultiplier()
        {
            winningSpinCount++;

            int index = Mathf.Clamp(
                winningSpinCount,
                0,
                multiplierSteps.Length - 1
            );

            int newMultiplier =
                multiplierSteps[index];

            if (newMultiplier != currentMultiplier)
            {
                currentMultiplier = newMultiplier;

                Debug.Log(
                    $"Multiplier Increased → " +
                    $"{currentMultiplier}x"
                );

                onMultiplierChanged?.Invoke(
                    currentMultiplier
                );
            }
        }

        // =====================================================
        // WISH GRANTED
        // =====================================================

        private void CheckWishGranted(
            bool hasWinningSpin)
        {
            if (wishGrantedTriggered)
                return;

            bool maxMultiplierReached =
                currentMultiplier >= 20;

            if (
                maxMultiplierReached &&
                hasWinningSpin)
            {
                wishGrantedTriggered = true;

                Debug.Log("WISH GRANTED!");

                onWishGranted?.Invoke();
            }
        }

        // =====================================================
        // FORCE START
        // =====================================================

        [ContextMenu("Force Start 10 Free Spins")]

        public void ForceStartTenFreeSpins()
        {
            EnsureState();

            state.BeginSession(10, 3);

            dragonBlessingActive = true;

            currentMultiplier = 1;

            winningSpinCount = 0;

            totalFeatureWin = 0;

            wishGrantedTriggered = false;

            onMultiplierChanged?.Invoke(
                currentMultiplier
            );

            onFreeSpinsStarted?.Invoke();

            onFreeSpinsUpdated?.Invoke();

            FreeSpinsStarted?.Invoke(state);

            FreeSpinsUpdated?.Invoke(state);

            StartAutoFreeSpin();
        }

        // =====================================================
        // FORCE END
        // =====================================================

        [ContextMenu("End Free Spins")]

        public void ForceEndFreeSpins()
        {
            EnsureState();

            bool wasActive = state.IsActive;

            state.EndSession();

            dragonBlessingActive = false;

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

        // =====================================================
        // RESOLVE AWARD COUNT
        // =====================================================

        private int ResolveAwardCount(
            int scatterCount)
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

        // =====================================================
        // ENSURE STATE
        // =====================================================

        private void EnsureState()
        {
            state ??= new FreeSpinState();
        }

        // =====================================================
        // AUTO SPIN
        // =====================================================

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

            autoSpinRoutine =
                StartCoroutine(
                    AutoFreeSpinRoutine()
                );
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

                yield return new WaitForSeconds(
                    autoSpinDelay
                );
            }

            autoSpinRoutine = null;
        }
    }
}