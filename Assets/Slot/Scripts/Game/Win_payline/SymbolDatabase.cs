using System.Collections.Generic;
using SlotMachine.Reels.Data;
using UnityEngine;

namespace SlotMachine.Reels.Runtime
{
    public class SymbolDatabase : MonoBehaviour
    {
        [SerializeField] private List<SymbolDefinition> symbols = new List<SymbolDefinition>();

        private readonly Dictionary<int, SymbolDefinition> symbolById = new Dictionary<int, SymbolDefinition>();

        private void Awake()
        {
            Cache();
        }

        private void OnValidate()
        {
            Cache();
        }

        private void Cache()
        {
            symbolById.Clear();

            for (int i = 0; i < symbols.Count; i++)
            {
                SymbolDefinition symbol = symbols[i];

                if (symbol == null)
                {
                    continue;
                }

                if (!symbolById.ContainsKey(symbol.SymbolId))
                {
                    symbolById.Add(symbol.SymbolId, symbol);
                }
            }
        }

        public bool TryGetSymbol(int symbolId, out SymbolDefinition definition)
        {
            if (symbolById.Count == 0)
            {
                Cache();
            }

            return symbolById.TryGetValue(symbolId, out definition);
        }
    }
}