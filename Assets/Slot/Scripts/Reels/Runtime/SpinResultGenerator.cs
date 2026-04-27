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

        [Header("Deterministic Testing")]
        [SerializeField] private bool useFixedSeed;
        [SerializeField] private int fixedSeed = 12345;
        [SerializeField] private bool useForcedStops;
        [SerializeField] private List<int> forcedStopIndices = new List<int>();

        [Header("Feature Markers")]
        [SerializeField] private int scatterSymbolId = 0;
        [SerializeField] private int freeSpinScatterThreshold = 3;

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
            EnsureRandom();
        }

        private void OnValidate()
        {
            visibleRowCount = Mathf.Max(1, visibleRowCount);
            freeSpinScatterThreshold = Mathf.Max(1, freeSpinScatterThreshold);
            winChancePercent = Mathf.Clamp(winChancePercent, 0, 100);
            minWinAmount = Mathf.Max(0, minWinAmount);
            maxWinAmount = Mathf.Max(minWinAmount, maxWinAmount);
            bigWinThreshold = Mathf.Max(0, bigWinThreshold);
        }

        public SpinOutcome GenerateOutcome(IReadOnlyList<ReelController> reels)
        {
            EnsureRandom();

            SpinOutcome outcome = new SpinOutcome
            {
                SpinId = $"SPIN_{_nextSpinNumber:000000}",
                TimestampUtc = DateTime.UtcNow.ToString("O")
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

                int stopIndex = ResolveStopIndex(i, reel.ReelStrip);
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
            outcome.TriggersFreeSpins = scatterCount >= freeSpinScatterThreshold;
            outcome.TotalWin = GenerateWinAmount(outcome.TriggersFreeSpins);
            outcome.HasWin = outcome.TotalWin > 0;
            outcome.IsBigWin = outcome.TotalWin >= bigWinThreshold;
            return outcome;
        }

        private int ResolveStopIndex(int reelListIndex, ReelStripDefinition strip)
        {
            if (useForcedStops && reelListIndex < forcedStopIndices.Count)
            {
                return Wrap(forcedStopIndices[reelListIndex], strip.Length);
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

        private static int Wrap(int value, int length)
        {
            return ((value % length) + length) % length;
        }
    }
}
