using System;
using System.Collections.Generic;
using UnityEngine;

namespace SlotMachine.Reels.Data
{
    [CreateAssetMenu(menuName = "Slot/Reel Strip Definition")]
    public class ReelStripDefinition : ScriptableObject
    {
        [Header("Reel Strip")]
        [SerializeField] private List<int> symbolIds = new List<int>();

        [Header("Paste Helper")]
        [TextArea(5, 15)]
        [SerializeField] private string pasteValues;

        public int Length => symbolIds != null ? symbolIds.Count : 0;
        public IReadOnlyList<int> SymbolIds => symbolIds;

        public int GetSymbolAt(int index)
        {
            if (symbolIds == null || symbolIds.Count == 0)
            {
                return 0;
            }

            int wrappedIndex = Wrap(index, symbolIds.Count);
            return symbolIds[wrappedIndex];
        }

        public int[] GetVisibleWindow(int topIndex, int visibleRowCount)
        {
            visibleRowCount = Mathf.Max(1, visibleRowCount);

            int[] result = new int[visibleRowCount];

            for (int row = 0; row < visibleRowCount; row++)
            {
                result[row] = GetSymbolAt(topIndex + row);
            }

            return result;
        }

        [ContextMenu("Import Paste Values")]
        private void ImportPasteValues()
        {
            if (string.IsNullOrWhiteSpace(pasteValues))
            {
                Debug.LogWarning($"[{name}] Paste values field is empty.");
                return;
            }

            string cleaned = pasteValues
                .Replace("{", "")
                .Replace("}", "")
                .Replace(";", ",")
                .Replace("\n", ",")
                .Replace("\r", ",");

            string[] parts = cleaned.Split(
                new[] { "," },
                StringSplitOptions.RemoveEmptyEntries
            );

            symbolIds.Clear();

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i].Trim();

                if (int.TryParse(part, out int value))
                {
                    symbolIds.Add(value);
                }
                else
                {
                    Debug.LogWarning($"[{name}] Could not parse value: {part}");
                }
            }

            Debug.Log($"[{name}] Imported {symbolIds.Count} reel symbols.");
        }

        [ContextMenu("Clear Reel Strip")]
        private void ClearReelStrip()
        {
            symbolIds.Clear();
            pasteValues = string.Empty;
        }

        private static int Wrap(int value, int length)
        {
            if (length <= 0)
            {
                return 0;
            }

            return ((value % length) + length) % length;
        }
    }
}