using System.Collections.Generic;
using TMPro;
using UnityEngine;
using SlotMachine.Reels.Runtime;

namespace SlotMachine.Game.Runtime
{
    public class FreeSpinsPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FreeSpinManager freeSpinManager;

        [Header("UI")]
        [SerializeField] private GameObject freeSpinInfoPanel;
        [SerializeField] private GameObject freeSpinStartBanner;
        [SerializeField] private GameObject freeSpinEndBanner;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI modeText;
        [SerializeField] private TextMeshProUGUI spinRemainingText;
        [SerializeField] private TextMeshProUGUI spinMultiplierText;

        [Header("Inspector Visual Targets")]
        [SerializeField] private List<GameObject> showWhileFreeSpins = new();
        [SerializeField] private List<GameObject> hideWhileFreeSpins = new();
        [SerializeField] private List<Behaviour> enableWhileFreeSpins = new();
        [SerializeField] private List<Behaviour> disableWhileFreeSpins = new();

        [Header("Labels")]
        [SerializeField] private string remainingFormat = "REMAING SPIN {0} LEFT";
        [SerializeField] private string multiplierFormat = "MULTIPLIER x{0}";

        private bool waitingForStartClick;

        private void Awake()
        {
            HideAllUI();
            RefreshModeLabel();
        }

        private void OnEnable()
        {
            Subscribe();
            RefreshFromState();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void HandleFreeSpinsStarted(FreeSpinState state)
        {
            ApplyVisualTargets(true);

            waitingForStartClick = true;

            if (freeSpinStartBanner != null)
            {
                freeSpinStartBanner.SetActive(true);
            }

            RefreshModeLabel(state);
        }

        private void HandleFreeSpinsUpdated(FreeSpinState state)
        {
            if (state != null && state.IsActive)
            {
                ApplyVisualTargets(true);

                if (!waitingForStartClick)
                {
                    ShowFreeSpinPanel(state);
                }
            }
            else
            {
                ApplyVisualTargets(false);
                HideFreeSpinPanel();
            }

            RefreshModeLabel(state);
        }

        private void HandleFreeSpinsEnded()
        {
            ApplyVisualTargets(false);

            HideFreeSpinPanel();

            if (freeSpinEndBanner != null)
            {
                freeSpinEndBanner.SetActive(true);
            }

            RefreshModeLabel();
        }

        public void OnClickStartBanner()
        {
            if (!waitingForStartClick)
            {
                return;
            }

            waitingForStartClick = false;

            if (freeSpinStartBanner != null)
            {
                freeSpinStartBanner.SetActive(false);
            }

            ShowFreeSpinPanel(freeSpinManager.State);

            if (freeSpinManager != null)
            {
                freeSpinManager.StartFreeSpinGameplay();
            }
        }

        public void OnClickEndBanner()
        {
            if (freeSpinEndBanner != null)
            {
                freeSpinEndBanner.SetActive(false);
            }

            RefreshModeLabel();
        }

        private void RefreshFromState()
        {
            if (freeSpinManager == null)
            {
                return;
            }

            if (freeSpinManager.IsFreeSpinActive)
            {
                ApplyVisualTargets(true);

                if (!waitingForStartClick)
                {
                    ShowFreeSpinPanel(freeSpinManager.State);
                }

                RefreshModeLabel(freeSpinManager.State);
            }
            else
            {
                ApplyVisualTargets(false);
                HideFreeSpinPanel();
                RefreshModeLabel();
            }
        }

        private void ShowFreeSpinPanel(FreeSpinState state)
        {
            if (freeSpinInfoPanel != null)
            {
                freeSpinInfoPanel.SetActive(true);
            }

            if (spinRemainingText != null)
            {
                int remaining = state != null ? state.RemainingSpins : 0;

                spinRemainingText.text = string.Format(remainingFormat, remaining);
            }

            if (spinMultiplierText != null)
            {
                int multiplier = freeSpinManager != null ? freeSpinManager.CurrentMultiplier : 1;

                spinMultiplierText.text = string.Format(multiplierFormat, multiplier);
            }
        }

        private void HideFreeSpinPanel()
        {
            if (freeSpinInfoPanel != null)
            {
                freeSpinInfoPanel.SetActive(false);
            }
        }

        private void HideAllUI()
        {
            HideFreeSpinPanel();

            if (freeSpinStartBanner != null)
            {
                freeSpinStartBanner.SetActive(false);
            }

            if (freeSpinEndBanner != null)
            {
                freeSpinEndBanner.SetActive(false);
            }
        }

        private void RefreshModeLabel()
        {
            RefreshModeLabel(null);
        }

        private void RefreshModeLabel(FreeSpinState state)
        {
            if (modeText == null)
            {
                return;
            }

            if (state == null && freeSpinManager != null)
            {
                state = freeSpinManager.State;
            }

            modeText.text = state != null && state.IsActive ? "FREE SPIN MODE" : "BASE GAME MODE";
        }

        private void ApplyVisualTargets(bool freeSpinsActive)
        {
            SetGameObjectsActive(showWhileFreeSpins, freeSpinsActive);
            SetGameObjectsActive(hideWhileFreeSpins, !freeSpinsActive);

            SetBehavioursEnabled(enableWhileFreeSpins, freeSpinsActive);
            SetBehavioursEnabled(disableWhileFreeSpins, !freeSpinsActive);
        }

        private void Subscribe()
        {
            if (freeSpinManager == null)
            {
                return;
            }

            freeSpinManager.FreeSpinsStarted += HandleFreeSpinsStarted;
            freeSpinManager.FreeSpinsUpdated += HandleFreeSpinsUpdated;
            freeSpinManager.FreeSpinsEnded += HandleFreeSpinsEnded;
        }

        private void Unsubscribe()
        {
            if (freeSpinManager == null)
            {
                return;
            }

            freeSpinManager.FreeSpinsStarted -= HandleFreeSpinsStarted;
            freeSpinManager.FreeSpinsUpdated -= HandleFreeSpinsUpdated;
            freeSpinManager.FreeSpinsEnded -= HandleFreeSpinsEnded;
        }

        private static void SetGameObjectsActive(IReadOnlyList<GameObject> targets,bool isActive)
        {
            if (targets == null)
            {
                return;
            }

            foreach (GameObject target in targets)
            {
                if (target != null)
                {
                    target.SetActive(isActive);
                }
            }
        }

        private static void SetBehavioursEnabled(IReadOnlyList<Behaviour> targets,bool isEnabled)
        {
            if (targets == null)
            {
                return;
            }

            foreach (Behaviour target in targets)
            {
                if (target != null)
                {
                    target.enabled = isEnabled;
                }
            }
        }
    }
}