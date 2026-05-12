using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotMachine.Reels.Runtime
{
    public class BetManager : MonoBehaviour
    {
        [Header("Bet Settings")]
        [SerializeField] private int activeLineCount = 20;
        [SerializeField]
        private float[] betSteps =
        {
            20f, 40f, 60f, 100f, 200f, 400f, 600f, 1000f
        };

        [SerializeField] private int currentBetIndex = 4;

        [Header("Credits")]
        [SerializeField] private float credits = 10000f;

        [Header("UI References")]
        [SerializeField] private TMP_Text betText;
        [SerializeField] private TMP_Text betPerLineText;
        [SerializeField] private TMP_Text creditsText;
        [SerializeField] private Button plusButton;
        [SerializeField] private Button minusButton;

        public event Action<float> BetChanged;
        public event Action<float> CreditsChanged;

        public int ActiveLineCount => activeLineCount;

        public float TotalBet
        {
            get
            {
                if (betSteps == null || betSteps.Length == 0)
                {
                    return 0f;
                }

                currentBetIndex = Mathf.Clamp(currentBetIndex, 0, betSteps.Length - 1);
                return betSteps[currentBetIndex];
            }
        }

        public float BetPerLine
        {
            get
            {
                if (activeLineCount <= 0)
                {
                    return 0f;
                }

                return TotalBet / activeLineCount;
            }
        }

        public float Credits => credits;
        public bool CanAffordCurrentBet => credits >= TotalBet;
        public bool CanIncreaseBet => betSteps != null && currentBetIndex < betSteps.Length - 1;
        public bool CanDecreaseBet => betSteps != null && currentBetIndex > 0;

        private void Awake()
        {
            HookButtons();
            RefreshUI();
        }

        private void OnValidate()
        {
            activeLineCount = Mathf.Max(1, activeLineCount);

            if (betSteps != null && betSteps.Length > 0)
            {
                currentBetIndex = Mathf.Clamp(currentBetIndex, 0, betSteps.Length - 1);

                for (int i = 0; i < betSteps.Length; i++)
                {
                    betSteps[i] = Mathf.Max(0f, betSteps[i]);
                }
            }

            credits = Mathf.Max(0f, credits);
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
            CreditsChanged?.Invoke(credits);
            RefreshUI();
            return true;
        }

        public void AddWin(float winAmount)
        {
            if (winAmount <= 0f)
            {
                return;
            }

            credits += winAmount;
            CreditsChanged?.Invoke(credits);
            RefreshUI();
        }

        public void SetCredits(float value)
        {
            credits = Mathf.Max(0f, value);
            CreditsChanged?.Invoke(credits);
            RefreshUI();
        }

        public void RefreshUI()
        {
            if (betText != null)
            {
                betText.text = TotalBet.ToString("0.##");
            }

            if (betPerLineText != null)
            {
                betPerLineText.text = BetPerLine.ToString("0.##");
            }

            if (creditsText != null)
            {
                creditsText.text = credits.ToString("0.##");
            }

            if (plusButton != null)
            {
                plusButton.interactable = CanIncreaseBet;
            }

            if (minusButton != null)
            {
                minusButton.interactable = CanDecreaseBet;
            }
        }
        public bool TrySpend(float amount)
        {
            if (credits < amount)
            {
                return false;
            }

            credits -= amount;
            CreditsChanged?.Invoke(credits);
            RefreshUI();
            return true;
        }
    }
}