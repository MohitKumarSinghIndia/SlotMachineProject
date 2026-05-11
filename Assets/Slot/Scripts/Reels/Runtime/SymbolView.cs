using SlotMachine.Reels.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotMachine.Reels.Runtime
{
    public class SymbolView : MonoBehaviour
    {
        [SerializeField] private int currentSymbolId;

        [Tooltip("The sequence player attached to this symbol prefab")]
        [SerializeField] private EventSequencePlayer sequencePlayer;

        public int CurrentSymbolId => currentSymbolId;

        private void Awake()
        {
            // Auto-cache the sequence player if it wasn't manually assigned
            if (sequencePlayer == null)
            {
                sequencePlayer = GetComponent<EventSequencePlayer>();
            }
        }

        public void SetSymbolIdOnly(int symbolId)
        {
            currentSymbolId = symbolId;
            PlayNormal(); // Reset to normal state when recycled by the pool
        }

        public void ApplySymbolId(int symbolId)
        {
            currentSymbolId = symbolId;
        }

        public void ApplyDefinition(SymbolDefinition definition)
        {
            if (definition == null) return;
            currentSymbolId = definition.SymbolId;
        }

        // ==========================================
        // STATE TRIGGERS (Using EventSequencePlayer)
        // ==========================================

        public void PlayNormal()
        {
            if (sequencePlayer != null) sequencePlayer.PlaySequenceById(0);
        }

        public void PlayLanding()
        {
            if (sequencePlayer != null) sequencePlayer.PlaySequenceById(1);
        }

        public void PlayHighlight()
        {
            if (sequencePlayer != null) sequencePlayer.PlaySequenceById(2);
        }

        public void StopAnimations()
        {
            if (sequencePlayer != null) sequencePlayer.Stop();
        }

        // Kept for backward compatibility if you have old UI elements attached
        public void ConfigureVisualReferences(Image icon, Image background, TMP_Text label) { }
    }
}