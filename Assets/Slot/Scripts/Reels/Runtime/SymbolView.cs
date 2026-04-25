using SlotMachine.Reels.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotMachine.Reels.Runtime
{
    public class SymbolView : MonoBehaviour
    {
        [Header("Visual References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TMP_Text labelText;

        [Header("Runtime Info")]
        [SerializeField] private int currentSymbolId = -1;

        public int CurrentSymbolId => currentSymbolId;

        public void ApplySymbol(SymbolDefinition definition)
        {
            if (definition == null)
            {
                currentSymbolId = -1;

                if (iconImage != null)
                {
                    iconImage.sprite = null;
                    iconImage.enabled = false;
                }

                if (labelText != null)
                {
                    labelText.text = "?";
                    labelText.color = Color.white;
                }

                if (backgroundImage != null)
                {
                    backgroundImage.color = Color.gray;
                }

                return;
            }

            currentSymbolId = definition.SymbolId;

            if (iconImage != null)
            {
                iconImage.sprite = definition.Icon;
                iconImage.enabled = definition.Icon != null;
            }

            if (labelText != null)
            {
                labelText.text = definition.ShortCode;
                labelText.color = definition.LabelColor;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = definition.BackgroundColor;
            }
        }
    }
}
