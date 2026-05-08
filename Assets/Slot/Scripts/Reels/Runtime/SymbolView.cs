using SlotMachine.Reels.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotMachine.Reels.Runtime
{
    public class SymbolView : MonoBehaviour
    {
        [SerializeField] private int currentSymbolId;

        public int CurrentSymbolId => currentSymbolId;

        public void SetSymbolIdOnly(int symbolId)
        {
            currentSymbolId = symbolId;
        }

        public void ApplySymbolId(int symbolId)
        {
            currentSymbolId = symbolId;
        }

        public void ApplyDefinition(SymbolDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            currentSymbolId = definition.SymbolId;
        }

        // Kept only for old compatibility.
        // This script no longer changes image, text, icon, color, etc.
        public void ConfigureVisualReferences(Image icon, Image background, TMP_Text label)
        {
        }
    }
}