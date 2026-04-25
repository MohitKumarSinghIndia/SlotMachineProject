using System.Collections.Generic;
using SlotMachine.Reels.Data;
using UnityEngine;

namespace SlotMachine.Reels.Runtime
{
    public class SymbolManager : MonoBehaviour
    {
        [SerializeField] private List<SymbolDefinition> symbols = new List<SymbolDefinition>();

        private readonly Dictionary<int, SymbolDefinition> _lookup = new Dictionary<int, SymbolDefinition>();

        private void Awake()
        {
            RebuildLookup();
        }

        private void OnValidate()
        {
            RebuildLookup();
        }

        public bool TryGetSymbol(int symbolId, out SymbolDefinition definition)
        {
            if (_lookup.Count == 0)
            {
                RebuildLookup();
            }

            return _lookup.TryGetValue(symbolId, out definition);
        }

        public SymbolDefinition GetSymbol(int symbolId)
        {
            return TryGetSymbol(symbolId, out SymbolDefinition definition) ? definition : null;
        }

        public void ApplySymbol(SymbolView view, int symbolId)
        {
            if (view == null)
            {
                return;
            }

            view.ApplySymbol(GetSymbol(symbolId));
        }

        private void RebuildLookup()
        {
            _lookup.Clear();

            for (int i = 0; i < symbols.Count; i++)
            {
                SymbolDefinition symbol = symbols[i];
                if (symbol == null)
                {
                    continue;
                }

                _lookup[symbol.SymbolId] = symbol;
            }
        }
    }
}
