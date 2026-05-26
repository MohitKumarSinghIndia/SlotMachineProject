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
        [SerializeField] private EventSequencePlayer bannerSequencePlayer;

        [Header("UI")]
        [SerializeField] private GameObject freeSpinInfoPanel;
        [SerializeField] private GameObject freeSpinBanner;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI modeText;
        [SerializeField] private TextMeshProUGUI spinRemainingText;
        [SerializeField] private TextMeshProUGUI spinMultiplierText;

        [SerializeField] private TextMeshProUGUI freeSpinBonusText;
        [SerializeField] private TextMeshProUGUI amountBonusText;

        [Header("Inspector Visual Targets")]
        [SerializeField] private List<GameObject> showWhileFreeSpins = new();
        [SerializeField] private List<GameObject> hideWhileFreeSpins = new();
        [SerializeField] private List<Behaviour> enableWhileFreeSpins = new();
        [SerializeField] private List<Behaviour> disableWhileFreeSpins = new();

        [Header("Labels")]
        [SerializeField] private string remainingFormat = "REMAINING SPINS {0}";
        [SerializeField] private string multiplierFormat = "MULTIPLIER x{0}";

        private bool waitingForStartClick;

        #region Unity

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

        #endregion

        #region Free Spin Events

        private void HandleFreeSpinsStarted(FreeSpinState state)
        {
            ApplyVisualTargets(true);

            waitingForStartClick = true;

            ShowBanner();

            if (freeSpinBonusText != null)
            {
                freeSpinBonusText.text = state.TotalAwardedSpins.ToString();
            }

            RefreshModeLabel(state);
        }

        private void HandleFreeSpinsUpdated(FreeSpinState state)
        {
            if (state == null)
                return;

            if (state.IsActive)
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

            waitingForStartClick = true;

            // Show ending banner
            if (freeSpinBanner != null)
            {
                freeSpinBanner.SetActive(true);
            }

            // Play end animation
            if (bannerSequencePlayer != null)
            {
                bannerSequencePlayer.PlaySequenceById(1);
            }

            HideFreeSpinPanel();

            RefreshModeLabel();
        }

        #endregion

        #region Banner

        private void ShowBanner()
        {
            if (freeSpinBanner != null)
            {
                freeSpinBanner.SetActive(true);
            }

            // Start animation
            if (bannerSequencePlayer != null)
            {
                bannerSequencePlayer.PlaySequenceById(1);
            }
        }

        private void HideBanner()
        {
            if (freeSpinBanner != null)
            {
                freeSpinBanner.SetActive(false);
            }
        }

        public void OnFreeSpinBannerClick()
        {
            if (!waitingForStartClick)
                return;

            waitingForStartClick = false;

            // Click / close animation
            if (bannerSequencePlayer != null)
            {
                bannerSequencePlayer.PlaySequenceById(2);
            }

            HideBanner();

            // Only resume gameplay if still in free spins
            if (freeSpinManager != null && freeSpinManager.IsFreeSpinActive)
            {
                ShowFreeSpinPanel(freeSpinManager.State);

                freeSpinManager.StartFreeSpinGameplay();
            }
        }

        #endregion

        #region UI

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
            HideBanner();
            HideFreeSpinPanel();
        }

        #endregion

        #region Refresh

        private void RefreshFromState()
        {
            if (freeSpinManager == null)
                return;

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

        private void RefreshModeLabel()
        {
            RefreshModeLabel(null);
        }

        private void RefreshModeLabel(FreeSpinState state)
        {
            if (modeText == null)
                return;

            if (state == null && freeSpinManager != null)
            {
                state = freeSpinManager.State;
            }

            modeText.text = state != null && state.IsActive ? "FREE SPIN MODE" : "BASE GAME MODE";
        }

        #endregion

        #region Visual Targets

        private void ApplyVisualTargets(bool freeSpinsActive)
        {
            SetGameObjectsActive( showWhileFreeSpins, freeSpinsActive);

            SetGameObjectsActive( hideWhileFreeSpins, !freeSpinsActive);

            SetBehavioursEnabled( enableWhileFreeSpins, freeSpinsActive);

            SetBehavioursEnabled( disableWhileFreeSpins, !freeSpinsActive);
        }

        private static void SetGameObjectsActive( IReadOnlyList<GameObject> targets, bool isActive)
        {
            if (targets == null)
                return;

            foreach (GameObject target in targets)
            {
                if (target != null)
                {
                    target.SetActive(isActive);
                }
            }
        }

        private static void SetBehavioursEnabled( IReadOnlyList<Behaviour> targets, bool isEnabled)
        {
            if (targets == null)
                return;

            foreach (Behaviour target in targets)
            {
                if (target != null)
                {
                    target.enabled = isEnabled;
                }
            }
        }

        #endregion

        #region Events

        private void Subscribe()
        {
            if (freeSpinManager == null)
                return;

            freeSpinManager.FreeSpinsStarted += HandleFreeSpinsStarted;

            freeSpinManager.FreeSpinsUpdated += HandleFreeSpinsUpdated;

            freeSpinManager.FreeSpinsEnded += HandleFreeSpinsEnded;
        }

        private void Unsubscribe()
        {
            if (freeSpinManager == null)
                return;

            freeSpinManager.FreeSpinsStarted -= HandleFreeSpinsStarted;

            freeSpinManager.FreeSpinsUpdated -= HandleFreeSpinsUpdated;

            freeSpinManager.FreeSpinsEnded -= HandleFreeSpinsEnded;
        }

        #endregion
    }
}