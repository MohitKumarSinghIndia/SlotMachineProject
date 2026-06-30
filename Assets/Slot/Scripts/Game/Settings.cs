using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotMachine.Reels.Runtime
{
    public class Settings : MonoBehaviour
    {
        public bool isSettingOn = false;

        private const string MUSIC_KEY = "SoundEnabled";
        private const string SFX_KEY = "SFXEnabled";

        [Header("Panel")]
        [SerializeField] private GameObject settingsPanel;

        [Header("References")]
        [SerializeField] private ReelManager reelManager;
        [SerializeField] private BetManager betManager;

        [Header("Bet Text")]
        [SerializeField] private TMP_Text betText;

        [Header("Toggle Animators")]
        [SerializeField] private Animator musicAnimator;
        [SerializeField] private Animator sfxAnimator;

        [Header("Buttons")]
        [SerializeField] private Button musicBtn = null;
        [SerializeField] private Button sFXBtn = null;
        [SerializeField] private Button increaseBet = null;
        [SerializeField] private Button decreaseBet = null;

        private bool musicEnabled;
        private bool sfxEnabled;

        private void Start()
        {
            musicBtn.onClick.AddListener(ToggleMusic);
            sFXBtn.onClick.AddListener(ToggleSfx);
            increaseBet.onClick.AddListener(IncreaseBet);
            decreaseBet.onClick.AddListener(DecreaseBet);

            musicEnabled = PlayerPrefs.GetInt(MUSIC_KEY, 1) == 1;
            sfxEnabled = PlayerPrefs.GetInt(SFX_KEY, 1) == 1;

            UpdateToggleVisuals();
            if (betManager != null)
            {
                betManager.BetChanged += OnBetChanged;
                RefreshBetText();
            }

        }

        private void OnDestroy()
        {
            musicBtn.onClick.RemoveListener(ToggleMusic);
            sFXBtn.onClick.RemoveListener(ToggleSfx);
            increaseBet.onClick.RemoveListener(IncreaseBet);
            decreaseBet.onClick.RemoveListener(DecreaseBet);

            if (betManager != null)
            {
                betManager.BetChanged -= OnBetChanged;
            }
        }

        public void OpenSettings()
        {
            SoundController.Instance.PlaySound(SoundType.ButtonClick);
            if (reelManager != null && reelManager.IsSpinInProgress)
                return;

            RefreshBetText();
            settingsPanel.SetActive(true);
            UpdateToggleVisuals();
            isSettingOn = true;
        }

        public void CloseSettings()
        {
            SoundController.Instance.PlaySound(SoundType.ButtonClick);
            settingsPanel.SetActive(false);
            isSettingOn = false;
        }

        public void ToggleMusic()
        {
            SoundController.Instance.PlaySound(SoundType.ButtonClick);
            musicEnabled = !musicEnabled;

            PlayerPrefs.SetInt(MUSIC_KEY, musicEnabled ? 1 : 0);
            PlayerPrefs.Save();

            if (musicAnimator != null)
            {
                if (musicEnabled)
                {
                    musicAnimator.Play("ToggleOn");
                }
                else
                {
                    musicAnimator.Play("ToggleOff");
                }
                    
            }

            SoundController.Instance?.SetMusicEnabled(musicEnabled);
        }

        public void ToggleSfx()
        {
            SoundController.Instance.PlaySound(SoundType.ButtonClick);
            sfxEnabled = !sfxEnabled;

            PlayerPrefs.SetInt(SFX_KEY, sfxEnabled ? 1 : 0);
            PlayerPrefs.Save();

            if (sfxAnimator != null)
            {
                if (sfxEnabled)
                {
                    sfxAnimator.Play("ToggleOn");
                }
                else
                {
                    sfxAnimator.Play("ToggleOff");
                }
            }
            SoundController.Instance?.SetSfxEnabled(sfxEnabled);
        }

        public void IncreaseBet()
        {
            if (betManager == null)
                return;

            if (reelManager != null && reelManager.IsSpinInProgress)
                return;

            betManager.IncreaseBet();
        }

        public void DecreaseBet()
        {
            if (betManager == null)
                return;

            if (reelManager != null && reelManager.IsSpinInProgress)
                return;

            betManager.DecreaseBet();
        }

        private void OnBetChanged(float bet)
        {
            RefreshBetText();
        }

        private void RefreshBetText()
        {
            if (betText == null || betManager == null)
                return;

            betText.text = betManager.TotalBet.ToString("0");
        }

        private void UpdateToggleVisuals()
        {
            if (musicAnimator != null)
            {
                musicAnimator.Play(
                    musicEnabled ? "ToggleOn" : "ToggleOff",
                    0,
                    1f);
            }

            if (sfxAnimator != null)
            {
                sfxAnimator.Play(
                    sfxEnabled ? "ToggleOn" : "ToggleOff",
                    0,
                    1f);
            }
        }
    }
}