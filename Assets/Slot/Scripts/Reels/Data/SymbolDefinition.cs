using UnityEngine;

namespace SlotMachine.Reels.Data
{
    [CreateAssetMenu(menuName = "Slot/Reels/Symbol Definition", fileName = "SymbolDefinition")]
    public class SymbolDefinition : ScriptableObject
    {
        [SerializeField] private int symbolId;
        [SerializeField] private string symbolName = "Symbol";
        [SerializeField] private string shortCode = "SYM";
        [SerializeField] private Sprite icon;
        [SerializeField] private Color backgroundColor = Color.white;
        [SerializeField] private Color labelColor = Color.black;

        public int SymbolId => symbolId;
        public string SymbolName => symbolName;
        public string ShortCode => string.IsNullOrWhiteSpace(shortCode) ? symbolName : shortCode;
        public Sprite Icon => icon;
        public Color BackgroundColor => backgroundColor;
        public Color LabelColor => labelColor;
    }
}
