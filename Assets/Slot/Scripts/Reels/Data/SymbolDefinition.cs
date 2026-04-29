using UnityEngine;
using UnityEngine.UI;

namespace SlotMachine.Reels.Data
{
    public class SymbolDefinition : MonoBehaviour
    {
        [SerializeField] private int symbolId;
        [SerializeField] private string symbolName = "Symbol";
        [SerializeField] private string shortCode = "SYM";
        [SerializeField] private Image icon;
        [SerializeField] private Color backgroundColor = Color.white;
        [SerializeField] private Color labelColor = Color.black;

        public int SymbolId => symbolId;
        public string SymbolName => symbolName;
        public string ShortCode => string.IsNullOrWhiteSpace(shortCode) ? symbolName : shortCode;
        public Sprite Icon => icon.sprite;
        public Color BackgroundColor => backgroundColor;
        public Color LabelColor => labelColor;
    }
}
