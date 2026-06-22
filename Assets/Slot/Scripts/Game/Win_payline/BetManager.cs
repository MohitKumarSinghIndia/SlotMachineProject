using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotMachine.Reels.Runtime
{
    public class BetManager : MonoBehaviour
    {
        private const string CreditsKey = "PLAYER_CREDITS";

        [Header("Bet Settings")]
        [SerializeField] private int activeLineCount = 20;

        [SerializeField]
        private float[] betSteps =
        {
            20f, 40f, 60f, 100f, 200f, 400f, 600f, 1000f
        };

        [SerializeField] private int currentBetIndex = 4;

        [Header("Credits")]
        [SerializeField] private float defaultCredits = 10000f;
        [SerializeField] private float credits;

        [Header("UI References")]
        [SerializeField] private TMP_Text betText;
        [SerializeField] private TMP_Text betPerLineText;
        [SerializeField] private TMP_Text creditsText;
        [SerializeField] private Button plusButton;
        [SerializeField] private Button minusButton;

        public event Action<float> BetChanged;
        public event Action<float> CreditsChanged;

        public int ActiveLineCount => activeLineCount;
        public int CurrentBetIndex => currentBetIndex;

        public float TotalBet
        {
            get
            {
                if (betSteps == null || betSteps.Length == 0)
                    return 0f;

                currentBetIndex = Mathf.Clamp(currentBetIndex, 1, betSteps.Length);
                return betSteps[currentBetIndex - 1];
            }
        }

        public float BetPerLine
        {
            get
            {
                if (activeLineCount <= 0)
                    return 0f;

                return TotalBet / activeLineCount;
            }
        }

        public float Credits => credits;
        public bool CanAffordCurrentBet => credits >= TotalBet;
        public bool CanIncreaseBet => betSteps != null && currentBetIndex < betSteps.Length;
        public bool CanDecreaseBet => betSteps != null && currentBetIndex > 1;

        private void Awake()
        {
            LoadCredits();
            HookButtons();
            RefreshUI();
        }

        private void OnApplicationQuit()
        {
            SaveCredits();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
                SaveCredits();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
                SaveCredits();
        }

        private void OnValidate()
        {
            activeLineCount = Mathf.Max(1, activeLineCount);

            if (betSteps != null && betSteps.Length > 0)
            {
                currentBetIndex = Mathf.Clamp(currentBetIndex, 1, betSteps.Length);

                for (int i = 0; i < betSteps.Length; i++)
                {
                    betSteps[i] = Mathf.Max(0f, betSteps[i]);
                }
            }

            RefreshUI();
        }

        private void HookButtons()
        {
            if (plusButton != null)
            {
                plusButton.onClick.RemoveListener(IncreaseBet);
                plusButton.onClick.AddListener(IncreaseBet);
            }

            if (minusButton != null)
            {
                minusButton.onClick.RemoveListener(DecreaseBet);
                minusButton.onClick.AddListener(DecreaseBet);
            }
        }

        private void SaveCredits()
        {
            PlayerPrefs.SetFloat(CreditsKey, credits);
            PlayerPrefs.Save();
        }

        private void LoadCredits()
        {
            credits = PlayerPrefs.GetFloat(CreditsKey, defaultCredits);
        }

        public void IncreaseBet()
        {
            if (!CanIncreaseBet)
            {
                RefreshUI();
                return;
            }

            currentBetIndex++;
            BetChanged?.Invoke(TotalBet);
            RefreshUI();
        }

        public void DecreaseBet()
        {
            if (!CanDecreaseBet)
            {
                RefreshUI();
                return;
            }

            currentBetIndex--;
            BetChanged?.Invoke(TotalBet);
            RefreshUI();
        }

        public bool TrySpendCurrentBet()
        {
            if (!CanAffordCurrentBet)
            {
                Debug.LogWarning("[BetManager] Not enough credits to spin.");
                RefreshUI();
                return false;
            }

            credits -= TotalBet;

            SaveCredits();

            CreditsChanged?.Invoke(credits);
            RefreshUI();
            return true;
        }

        public bool TrySpend(float amount)
        {
            if (credits < amount)
                return false;

            credits -= amount;

            SaveCredits();

            CreditsChanged?.Invoke(credits);
            RefreshUI();
            return true;
        }

        public void AddWin(float winAmount)
        {
            if (winAmount <= 0f)
                return;

            credits += winAmount;

            SaveCredits();

            CreditsChanged?.Invoke(credits);
            RefreshUI();
        }

        public void SetCredits(float value)
        {
            credits = Mathf.Max(0f, value);

            SaveCredits();

            CreditsChanged?.Invoke(credits);
            RefreshUI();
        }

        public void RefreshUI()
        {
            if (betText != null)
                betText.text = TotalBet.ToString("0.##");

            if (betPerLineText != null)
                betPerLineText.text = BetPerLine.ToString("0.##");

            if (creditsText != null)
                creditsText.text = credits.ToString("0.##");

            if (plusButton != null)
                plusButton.interactable = CanIncreaseBet;

            if (minusButton != null)
                minusButton.interactable = CanDecreaseBet;
        }

        [ContextMenu("Reset Credits")]
        public void ResetCredits()
        {
            credits = defaultCredits;
            SaveCredits();
            RefreshUI();
        }
    }
}