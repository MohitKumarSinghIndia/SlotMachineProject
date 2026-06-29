using UnityEngine;
using UnityEngine.UI;


namespace SlotMachine.Reels.Runtime
{
    public class GameInputs : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ReelManager reelManager;
        [SerializeField] private FreeSpinsPresenter freeSpinsPresenter;
        [SerializeField] private FreeSpinManager freeSpinManager;
        [SerializeField] private BannerManager bannerManager;

        [Header("Buttons")]
        [SerializeField] private Button closeButton;

        public bool isFreeSpinStarted;
        public bool isFreeSpinEnded;

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(CloseGameClicked);
                closeButton.onClick.AddListener(CloseGameClicked);
            }
        }

        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(CloseGameClicked);
            }
        }

        private void Update()
        {
            UpdateCloseButtonState();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                HandleSpaceInput();
            }
        }

        private void UpdateCloseButtonState()
        {
            if (closeButton == null || reelManager == null || freeSpinManager == null)
                return;

            bool canClose = !reelManager.IsSpinInProgress && !freeSpinManager.State.IsActive;

            closeButton.interactable = canClose;
        }

        private void HandleSpaceInput()
        {
            if (reelManager == null || freeSpinManager == null || bannerManager == null)
            {
                return;
            }

            if (bannerManager.isIntroBannerActive)
            {
                bannerManager.OnIntroBannerClick();
                return;
            }

            if (isFreeSpinStarted)
            {
                freeSpinsPresenter?.OnBannerClicked();
                isFreeSpinStarted = false;
                return;
            }

            if (isFreeSpinEnded)
            {
                freeSpinsPresenter?.OnBannerClicked();
                isFreeSpinEnded = false;
                return;
            }

            if (!reelManager.IsSpinInProgress && !freeSpinManager.IsFreeSpinActive)
            {
                reelManager.StartSpin();
            }
        }

        public void CloseGameClicked()
        {
            if (reelManager != null && reelManager.IsSpinInProgress)
            {
                return;
            }

            isFreeSpinStarted = false;
            isFreeSpinEnded = false;

            if (freeSpinManager != null)
            {
                freeSpinManager.ResetFreeSpinsSilently();
            }

            if (SoundController.Instance != null)
            {
                SoundController.Instance.StopSFX();
                SoundController.Instance.PlayIntroMusic();
            }

            if (bannerManager != null)
            {
                bannerManager.CrossButtonClicked();
            }
        }

        public void SetFreeSpinStarted(bool value)
        {
            isFreeSpinStarted = value;
        }

        public void SetFreeSpinEnded(bool value)
        {
            isFreeSpinEnded = value;
        }
    }
}