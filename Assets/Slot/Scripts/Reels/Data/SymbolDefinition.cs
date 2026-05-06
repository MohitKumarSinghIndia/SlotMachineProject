using System;
using System.Collections.Generic;
using UnityEngine;

namespace SlotMachine.Reels.Data
{
    public enum SlotSymbolType
    {
        Regular,
        Wild,
        Scatter
    }

    public enum SymbolValueTier
    {
        None,
        Low,
        Mid,
        High
    }

    [Serializable]
    public class SymbolPayEntry
    {
        [Min(1)]
        [SerializeField] private int matchCount = 3;

        [Min(0f)]
        [SerializeField] private float multiplier = 0.5f;

        public int MatchCount => matchCount;
        public float Multiplier => multiplier;
    }

    public class SymbolDefinition : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private int symbolId;
        [SerializeField] private string symbolName;
        [SerializeField] private string shortCode;

        [Header("Symbol Type")]
        [SerializeField] private SlotSymbolType symbolType = SlotSymbolType.Regular;
        [SerializeField] private SymbolValueTier valueTier = SymbolValueTier.Low;

        [Header("Visuals")]
        [SerializeField] private Sprite icon;
        [SerializeField] private Color backgroundColor = Color.white;
        [SerializeField] private Color labelColor = Color.black;

        [Header("Pooling")]
        [Min(0)]
        [SerializeField] private int initialPoolCount = 5;

        [Header("Paytable Multipliers")]
        [Tooltip("Multiplier is applied to bet per line. Example: BetPerLine 10 × multiplier 50 = 500 win.")]
        [SerializeField]
        private List<SymbolPayEntry> paytable = new List<SymbolPayEntry>
        {
            new SymbolPayEntry(),
            new SymbolPayEntry(),
            new SymbolPayEntry()
        };

        public int SymbolId => symbolId;
        public string SymbolName => symbolName;
        public string ShortCode => shortCode;

        public SlotSymbolType SymbolType => symbolType;
        public SymbolValueTier ValueTier => valueTier;

        public Sprite Icon => icon;
        public Color BackgroundColor => backgroundColor;
        public Color LabelColor => labelColor;
        public int InitialPoolCount => initialPoolCount;

        public bool IsRegular => symbolType == SlotSymbolType.Regular;
        public bool IsWild => symbolType == SlotSymbolType.Wild;
        public bool IsScatter => symbolType == SlotSymbolType.Scatter;

        public bool IsLowValue => valueTier == SymbolValueTier.Low;
        public bool IsMidValue => valueTier == SymbolValueTier.Mid;
        public bool IsHighValue => valueTier == SymbolValueTier.High;

        public bool HasCustomPresentation
        {
            get
            {
                return icon != null || !string.IsNullOrWhiteSpace(shortCode);
            }
        }

        public bool TryGetMultiplier(int matchCount, out float multiplier)
        {
            multiplier = 0f;

            if (paytable == null)
            {
                return false;
            }

            for (int i = 0; i < paytable.Count; i++)
            {
                SymbolPayEntry entry = paytable[i];

                if (entry == null)
                {
                    continue;
                }

                if (entry.MatchCount == matchCount)
                {
                    multiplier = entry.Multiplier;
                    return multiplier > 0f;
                }
            }

            return false;
        }

        private void OnValidate()
        {
            initialPoolCount = Mathf.Max(0, initialPoolCount);

            if (symbolType == SlotSymbolType.Wild || symbolType == SlotSymbolType.Scatter)
            {
                valueTier = SymbolValueTier.None;
            }

            if (symbolType == SlotSymbolType.Regular && valueTier == SymbolValueTier.None)
            {
                valueTier = SymbolValueTier.Low;
            }
        }
    }
}