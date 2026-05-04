using System;
using System.Collections.Generic;
using SlotMachine.Reels.Data;
using UnityEngine;
using Random = System.Random;

namespace SlotMachine.Reels.Runtime
{
    public class SpinResultGenerator : MonoBehaviour
    {
        [Header("Result Shape")]
        [Min(1)]
        [SerializeField] private int visibleRowCount = 3;

        [Header("References")]
        [SerializeField] private FreeSpinManager freeSpinManager;

        [Header("Deterministic Testing")]
        [SerializeField] private bool useFixedSeed;
        [SerializeField] private int fixedSeed = 12345;
        [SerializeField] private bool useForcedStops;
        [SerializeField] private List<int> forcedStopIndices = new List<int>();

        [Header("Feature Markers")]
        [SerializeField] private int scatterSymbolId = 0;
        [SerializeField] private int freeSpinScatterThreshold = 3;
        [SerializeField] private bool allowScatterDuringFreeSpins = false;
        [Min(1)]
        [SerializeField] private int maxFreeSpinStopSearchAttempts = 200;

        [Header("Free Spin Awards")]
        [Min(0)]
        [SerializeField] private int freeSpinsForThreeScatters = 10;
        [Min(0)]
        [SerializeField] private int freeSpinsForFourScatters = 15;
        [Min(0)]
        [SerializeField] private int freeSpinsForFiveOrMoreScatters = 20;

        [Header("Placeholder Win Meta")]
        [SerializeField] private bool simulateWins = true;
        [Range(0, 100)]
        [SerializeField] private int winChancePercent = 35;
        [Min(0)]
        [SerializeField] private int minWinAmount = 5;
        [Min(0)]
        [SerializeField] private int maxWinAmount = 120;
        [Min(0)]
        [SerializeField] private int bigWinThreshold = 80;

        private Random _random;
        private int _nextSpinNumber = 1;

        private void Awake()
        {
            CacheLocalReferences();
            EnsureRandom();
        }

        private void OnValidate()
        {
            CacheLocalReferences();
            visibleRowCount = Mathf.Max(1, visibleRowCount);
            freeSpinScatterThreshold = Mathf.Max(1, freeSpinScatterThreshold);
            maxFreeSpinStopSearchAttempts = Mathf.Max(1, maxFreeSpinStopSearchAttempts);
            freeSpinsForThreeScatters = Mathf.Max(0, freeSpinsForThreeScatters);
            freeSpinsForFourScatters = Mathf.Max(0, freeSpinsForFourScatters);
            freeSpinsForFiveOrMoreScatters = Mathf.Max(0, freeSpinsForFiveOrMoreScatters);
            winChancePercent = Mathf.Clamp(winChancePercent, 0, 100);
            minWinAmount = Mathf.Max(0, minWinAmount);
            maxWinAmount = Mathf.Max(minWinAmount, maxWinAmount);
            bigWinThreshold = Mathf.Max(0, bigWinThreshold);
        }

        public SpinOutcome GenerateOutcome(IReadOnlyList<ReelController> reels)
        {
            CacheLocalReferences();
            EnsureRandom();

            bool isFreeSpinSpin = freeSpinManager != null && freeSpinManager.CurrentSpinUsesFreeSpin;
            SpinOutcome outcome = new SpinOutcome
            {
                SpinId = $"SPIN_{_nextSpinNumber:000000}",
                TimestampUtc = DateTime.UtcNow.ToString("O"),
                IsFreeSpinSpin = isFreeSpinSpin
            };

            _nextSpinNumber++;

            int scatterCount = 0;

            for (int i = 0; i < reels.Count; i++)
            {
                ReelController reel = reels[i];
                if (reel == null || reel.ReelStrip == null || reel.ReelStrip.Length == 0)
                {
                    continue;
                }

                int stopIndex = ResolveStopIndex(i, reel.ReelStrip, isFreeSpinSpin);
                int[] visibleWindow = reel.ReelStrip.GetVisibleWindow(stopIndex, visibleRowCount);

                ReelOutcome reelOutcome = new ReelOutcome
                {
                    ReelIndex = reel.ReelIndex,
                    StopIndex = stopIndex
                };

                for (int row = 0; row < visibleWindow.Length; row++)
                {
                    int symbolId = visibleWindow[row];
                    reelOutcome.VisibleSymbolIds.Add(symbolId);
                    if (symbolId == scatterSymbolId)
                    {
                        scatterCount++;
                    }
                }

                outcome.Reels.Add(reelOutcome);
            }

            outcome.ScatterCount = scatterCount;
            outcome.AwardsFreeSpins = !isFreeSpinSpin && scatterCount >= freeSpinScatterThreshold;
            outcome.AwardedFreeSpinCount = outcome.AwardsFreeSpins ? ResolveFreeSpinAwardCount(scatterCount) : 0;
            outcome.TriggersFreeSpins = outcome.AwardsFreeSpins;
            outcome.TotalWin = GenerateWinAmount(outcome.AwardsFreeSpins);
            outcome.HasWin = outcome.TotalWin > 0;
            outcome.IsBigWin = outcome.TotalWin >= bigWinThreshold;
            return outcome;
        }

        private int ResolveStopIndex(int reelListIndex, ReelStripDefinition strip, bool isFreeSpinSpin)
        {
            if (useForcedStops && reelListIndex < forcedStopIndices.Count)
            {
                return Wrap(forcedStopIndices[reelListIndex], strip.Length);
            }

            if (isFreeSpinSpin && !allowScatterDuringFreeSpins)
            {
                for (int attempt = 0; attempt < maxFreeSpinStopSearchAttempts; attempt++)
                {
                    int candidate = _random.Next(0, strip.Length);
                    if (!WindowContainsSymbol(strip.GetVisibleWindow(candidate, visibleRowCount), scatterSymbolId))
                    {
                        return candidate;
                    }
                }

                for (int candidate = 0; candidate < strip.Length; candidate++)
                {
                    if (!WindowContainsSymbol(strip.GetVisibleWindow(candidate, visibleRowCount), scatterSymbolId))
                    {
                        return candidate;
                    }
                }
            }

            return _random.Next(0, strip.Length);
        }

        private int GenerateWinAmount(bool featureTriggered)
        {
            if (!simulateWins)
            {
                return 0;
            }

            int roll = _random.Next(0, 100);
            if (roll >= winChancePercent && !featureTriggered)
            {
                return 0;
            }

            return _random.Next(minWinAmount, maxWinAmount + 1);
        }

        private void EnsureRandom()
        {
            if (_random == null)
            {
                _random = useFixedSeed ? new Random(fixedSeed) : new Random();
            }
        }

        private int ResolveFreeSpinAwardCount(int scatterCount)
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

        private void CacheLocalReferences()
        {
            if (freeSpinManager == null)
            {
                freeSpinManager = GetComponent<FreeSpinManager>();
            }
        }

        private static bool WindowContainsSymbol(IReadOnlyList<int> window, int symbolId)
        {
            if (window == null)
            {
                return false;
            }

            for (int i = 0; i < window.Count; i++)
            {
                if (window[i] == symbolId)
                {
                    return true;
                }
            }

            return false;
        }

        private static int Wrap(int value, int length)
        {
            return ((value % length) + length) % length;
        }
    }
}
