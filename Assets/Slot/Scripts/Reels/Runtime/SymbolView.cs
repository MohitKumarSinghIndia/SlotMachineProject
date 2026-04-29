using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotMachine.Reels.Runtime
{
    public class SymbolView : MonoBehaviour
    {
        [System.Serializable]
        private readonly struct SymbolStyle
        {
            public SymbolStyle(string label, Color background, Color text)
            {
                Label = label;
                Background = background;
                Text = text;
            }

            public string Label { get; }
            public Color Background { get; }
            public Color Text { get; }
        }

        private static readonly SymbolStyle[] Styles =
        {
            new("PEARL", new Color(0.26f, 0.14f, 0.46f), new Color(1f, 0.92f, 0.45f)),
            new("WILD", new Color(0.76f, 0.18f, 0.14f), Color.white),
            new("POT", new Color(0.85f, 0.56f, 0.08f), Color.white),
            new("LANTERN", new Color(0.71f, 0.16f, 0.14f), new Color(1f, 0.95f, 0.82f)),
            new("DRUM", new Color(0.58f, 0.10f, 0.18f), Color.white),
            new("TEA", new Color(0.41f, 0.25f, 0.12f), new Color(1f, 0.95f, 0.85f)),
            new("KNOT", new Color(0.68f, 0.16f, 0.28f), Color.white),
            new("ROCKET", new Color(0.58f, 0.18f, 0.08f), new Color(1f, 0.93f, 0.82f)),
            new("RUBY", new Color(0.72f, 0.14f, 0.16f), Color.white),
            new("JADE", new Color(0.20f, 0.49f, 0.31f), new Color(0.92f, 1f, 0.93f)),
            new("CARD", new Color(0.79f, 0.12f, 0.20f), Color.white),
            new("STAR", new Color(0.76f, 0.56f, 0.12f), Color.white),
            new("BLOOM", new Color(0.84f, 0.50f, 0.70f), Color.white)
        };

        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private int currentSymbolId;

        public int CurrentSymbolId => currentSymbolId;

        public void ApplySymbolId(int symbolId)
        {
            currentSymbolId = symbolId;

            SymbolStyle style = ResolveStyle(symbolId);

            if (backgroundImage != null)
            {
                backgroundImage.color = style.Background;
            }

            if (labelText != null)
            {
                labelText.text = style.Label;
                labelText.color = style.Text;
            }
        
        }

        private static SymbolStyle ResolveStyle(int symbolId)
        {
            if (symbolId >= 0 && symbolId < Styles.Length)
            {
                return Styles[symbolId];
            }

            return new SymbolStyle($"SYM {symbolId}", Color.gray, Color.white);
        }
    }
}
