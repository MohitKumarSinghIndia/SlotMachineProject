using System;
using System.Collections.Generic;
using UnityEngine;

namespace SlotMachine.Reels.Data
{
    [CreateAssetMenu(menuName = "Slot/Reels/Reel Strip", fileName = "ReelStripDefinition")]
    public class ReelStripDefinition : ScriptableObject
    {
        [SerializeField] private List<int> symbolIds = new List<int>();

        public int Length => symbolIds.Count;
        public IReadOnlyList<int> SymbolIds => symbolIds;

        public int GetSymbolIdAt(int index)
        {
            if (symbolIds.Count == 0)
            {
                throw new InvalidOperationException($"{name} does not contain any symbols.");
            }

            return symbolIds[Wrap(index, symbolIds.Count)];
        }

        public int[] GetVisibleWindow(int topVisibleIndex, int rowCount)
        {
            int[] result = new int[rowCount];
            for (int row = 0; row < rowCount; row++)
            {
                result[row] = GetSymbolIdAt(topVisibleIndex + row);
            }

            return result;
        }

        private static int Wrap(int value, int length)
        {
            return ((value % length) + length) % length;
        }
    }
}
