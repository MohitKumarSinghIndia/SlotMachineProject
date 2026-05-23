using System.Collections.Generic;
using UnityEngine;

namespace SlotMachine.Reels.Runtime
{
    public class PaylineVisualizer : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Assign all your ReelControllers here in order (Left to Right)")]
        [SerializeField] private List<ReelController> reels = new List<ReelController>();

        // The LineRenderer reference has been completely removed!

        private readonly List<SymbolView> activeWinSymbols = new List<SymbolView>();

        // Phase 1: Show ALL winning symbols at once
        public void ShowCombinedWin(PaylineEvaluationResult result)
        {
            ClearVisuals();

            foreach (PaylineWinResult win in result.PaylineWins)
            {
                for (int i = 0; i < win.MatchCount; i++)
                {
                    int rowIndex = win.Rows[i];
                    SymbolView symbol = reels[i].GetVisibleSymbol(rowIndex);

                    if (symbol != null && !activeWinSymbols.Contains(symbol))
                    {
                        activeWinSymbols.Add(symbol);
                        symbol.PlayHighlight();
                    }
                }
            }
        }

        // Phase 2: Show a specific line's winning symbols
        public void ShowSingleLine(PaylineWinResult win)
        {
            ClearVisuals();

            for (int i = 0; i < reels.Count; i++)
            {
                int rowIndex = win.Rows[i];

                // Only trigger the highlight state on symbols that contributed to the match
                if (i < win.MatchCount)
                {
                    SymbolView symbol = reels[i].GetVisibleSymbol(rowIndex);
                    if (symbol != null && !activeWinSymbols.Contains(symbol))
                    {
                        activeWinSymbols.Add(symbol);
                        symbol.PlayHighlight();
                    }
                }
            }
            GameEvent.onDragonIdle?.Invoke();

        }

        // Phase 3: Scatter Highlight
        public void ShowScatters(float scatterSymbolId)
        {
            ClearVisuals();
            foreach (ReelController reelController in reels)
            {
                for (int i = 0; i < 3; i++)
                {
                   SymbolView symbol =  reelController.GetVisibleSymbol(i);
                    if(symbol.CurrentSymbolId == scatterSymbolId)
                    {
                        symbol.PlayHighlight();
                    }

                }
            }

        }

        // Phase 3: Reset all symbols to normal
        public void ClearVisuals()
        {
            foreach (SymbolView symbol in activeWinSymbols)
            {
                if (symbol != null)
                {
                    symbol.PlayNormal();
                }
            }

            activeWinSymbols.Clear();
        }
    }
}