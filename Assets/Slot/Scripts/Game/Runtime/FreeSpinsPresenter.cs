using SlotMachine.Reels.Runtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SlotMachine.Game.Runtime
{
    public class FreeSpinsPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FreeSpinManager freeSpinManager;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI bannerTitleText;
        [SerializeField] private TextMeshProUGUI bannerRemainingText;
        [SerializeField] private TextMeshProUGUI bannerMultiplierText;
        [SerializeField] private TextMeshProUGUI modeText;

        [Header("Inspector Visual Targets")]
        [SerializeField] private List<GameObject> showWhileFreeSpins = new List<GameObject>();
        [SerializeField] private List<GameObject> hideWhileFreeSpins = new List<GameObject>();
        [SerializeField] private List<Behaviour> enableWhileFreeSpins = new List<Behaviour>();
        [SerializeField] private List<Behaviour> disableWhileFreeSpins = new List<Behaviour>();

        [Header("Labels")]
        [SerializeField] private string baseGameLabel = "BASE GAME";
        [SerializeField] private string freeSpinsTitle = "FREE SPINS";
        [SerializeField] private string remainingFormat = "{0} LEFT";
        [SerializeField] private string modeFormat = "FREE SPINS {0}";
        [SerializeField] private string multiplierFormat = "x{0}";

        private void Awake()
        {
            CacheReferences();
            HideBannerImmediate();
            RefreshModeLabel();
        }

        private void OnEnable()
        {
            CacheReferences();
            Subscribe();
            RefreshFromState();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnValidate()
        {
            CacheReferences();
        }

        private void HandleFreeSpinsStarted(FreeSpinState state)
        {
            ApplyVisualTargets(true);
            ShowBanner(state);
            RefreshModeLabel(state);
        }

        private void HandleFreeSpinsUpdated(FreeSpinState state)
        {
            if (state != null && state.IsActive)
            {
                ApplyVisualTargets(true);
                ShowBanner(state);
            }
            else
            {
                ApplyVisualTargets(false);
                HideBannerImmediate();
            }

            RefreshModeLabel(state);
        }

        private void HandleFreeSpinsEnded()
        {
            ApplyVisualTargets(false);
            HideBannerImmediate();
            RefreshModeLabel();
        }

        private void RefreshFromState()
        {
            if (freeSpinManager != null && freeSpinManager.IsFreeSpinActive)
            {
                ApplyVisualTargets(true);
                ShowBanner(freeSpinManager.State);
                RefreshModeLabel(freeSpinManager.State);
            }
            else
            {
                ApplyVisualTargets(false);
                HideBannerImmediate();
                RefreshModeLabel();
            }
        }

        private void ShowBanner(FreeSpinState state)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            if (bannerTitleText != null)
            {
                bannerTitleText.text = freeSpinsTitle;
            }

            if (bannerRemainingText != null)
            {
                int remaining = state != null ? state.RemainingSpins : 0;
                bannerRemainingText.text = string.Format(remainingFormat, remaining);
            }

            if (bannerMultiplierText != null)
            {
                int multiplier = freeSpinManager != null ? freeSpinManager.CurrentMultiplier : 1;
                bannerMultiplierText.text = string.Format(multiplierFormat, multiplier);
            }
        }

        private void HideBannerImmediate()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
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

            if (state == null)
            {
                state = freeSpinManager != null ? freeSpinManager.State : null;
            }

            modeText.text = state != null && state.IsActive
                ? string.Format(modeFormat, state.RemainingSpins)
                : baseGameLabel;
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

            freeSpinManager.FreeSpinsStarted -= HandleFreeSpinsStarted;
            freeSpinManager.FreeSpinsUpdated -= HandleFreeSpinsUpdated;
            freeSpinManager.FreeSpinsEnded -= HandleFreeSpinsEnded;

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

        private void CacheReferences()
        {
            if (freeSpinManager == null)
            {
                freeSpinManager = FindAnyObjectByType<FreeSpinManager>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (bannerTitleText == null || bannerRemainingText == null || bannerMultiplierText == null)
            {
                TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
                for (int i = 0; i < texts.Length; i++)
                {
                    TextMeshProUGUI text = texts[i];
                    if (text == null)
                    {
                        continue;
                    }

                    if (bannerTitleText == null && text.gameObject.name == "BannerTitle")
                    {
                        bannerTitleText = text;
                    }
                    else if (bannerRemainingText == null && text.gameObject.name == "BannerRemaining")
                    {
                        bannerRemainingText = text;
                    }
                    else if (bannerMultiplierText == null && text.gameObject.name == "BannerMultiplier")
                    {
                        bannerMultiplierText = text;
                    }
                }
            }

            if (modeText == null)
            {
                TextMeshProUGUI[] sceneTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                for (int i = 0; i < sceneTexts.Length; i++)
                {
                    TextMeshProUGUI text = sceneTexts[i];
                    if (text != null && text.gameObject.name == "ModeText")
                    {
                        modeText = text;
                        break;
                    }
                }
            }
        }

        private static void SetGameObjectsActive(IReadOnlyList<GameObject> targets, bool isActive)
        {
            if (targets == null)
            {
                return;
            }

            for (int i = 0; i < targets.Count; i++)
            {
                GameObject target = targets[i];
                if (target != null)
                {
                    target.SetActive(isActive);
                }
            }
        }

        private static void SetBehavioursEnabled(IReadOnlyList<Behaviour> targets, bool isEnabled)
        {
            if (targets == null)
            {
                return;
            }

            for (int i = 0; i < targets.Count; i++)
            {
                Behaviour target = targets[i];
                if (target != null)
                {
                    target.enabled = isEnabled;
                }
            }
        }
    }
}
