using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotMachine.Reels.Runtime
{
    public class FreeSpinsPresenter : MonoBehaviour
    {
        private enum BannerMode
        {
            None,
            Start,
            End
        }

        [Header("References")]
        [SerializeField] private FreeSpinManager freeSpinManager;
        [SerializeField] private EventSequencePlayer bannerSequencePlayer;
        [SerializeField] private EventSequencePlayer transitionSequencePlayer;

        [Header("Banner UI")]
        [SerializeField] private GameObject bannerRoot;
        [SerializeField] private Button bannerButton;

        [Header("Free Spin State UI")]
        [SerializeField] private TMP_Text freeSpinsLeftText;
        [SerializeField] private TMP_Text freeSpinWinText;
        [SerializeField] private TMP_Text freeSpinWinAmountText;

        [Header("Sequence IDs")]
        [SerializeField] private int startTransitionSequenceId = 0;
        [SerializeField] private int showBannerId = 0;
        [SerializeField] private int hideBannerId = 0;
        [SerializeField] private int startBannerId = 1;
        [SerializeField] private int endBannerSequenceId = 2;
        [SerializeField] private int transitToBaseId = 2;
        [SerializeField] private int transitToFreeId = 3;

        [Header("Sequence Wait Times")]
        [SerializeField] private float startTransitionDuration = 1f;
        [SerializeField] private float endBannerSequenceDuration = 1f;
        [SerializeField] private float bannerHideDuration = 1f;
        [SerializeField] private float exitTransitionDuration = 1f;

        [Header("Delays")]
        [SerializeField] private float delayBeforeFirstFreeSpin = 1f;

        [Header("Text Format")]
        [SerializeField] private string freeSpinsLeftFormat = "Spins Left {0}";
        [SerializeField] private string freeSpinWinFormat = "{0} Free Spins";
        [SerializeField] private string freeSpinWinAmountFormat = "Win Amount {0}";

        private BannerMode currentBannerMode = BannerMode.None;
        private Coroutine currentRoutine;
        private bool waitingForClick;

        private void Awake()
        {
            if (bannerButton != null)
            {
                bannerButton.onClick.AddListener(OnBannerClicked);
            }

            //HideBanner();
            UpdateStateUI();
        }

        private void OnEnable()
        {
            if (freeSpinManager != null)
            {
                freeSpinManager.FreeSpinsStarted += OnFreeSpinsStarted;
                freeSpinManager.FreeSpinsUpdated += OnFreeSpinsUpdated;
                freeSpinManager.FreeSpinsEnded += OnFreeSpinsEnded;
            }
        }

        private void OnDisable()
        {
            if (freeSpinManager != null)
            {
                freeSpinManager.FreeSpinsStarted -= OnFreeSpinsStarted;
                freeSpinManager.FreeSpinsUpdated -= OnFreeSpinsUpdated;
                freeSpinManager.FreeSpinsEnded -= OnFreeSpinsEnded;
            }

            if (currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
                currentRoutine = null;
            }
        }

        private void OnFreeSpinsStarted(FreeSpinState state)
        {
            UpdateStateUI();

            if (currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
            }

            currentRoutine = StartCoroutine(StartFreeSpinIntroRoutine(state));
        }

        private void OnFreeSpinsUpdated(FreeSpinState state)
        {
            UpdateStateUI();
        }

        private void OnFreeSpinsEnded()
        {
            UpdateStateUI();

            if (currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
            }

            currentRoutine = StartCoroutine(EndFreeSpinRoutine());
        }

        private IEnumerator StartFreeSpinIntroRoutine(FreeSpinState state)
        {
            currentBannerMode = BannerMode.Start;

            int totalSpins = state != null ? state.TotalAwardedSpins : 0;

            if(bannerSequencePlayer != null)
            {
                bannerSequencePlayer.PlaySequenceById(startBannerId);
            }

            waitingForClick = true;

            while (waitingForClick)
            {
                yield return null;
            }

            HideBanner();

            if(bannerHideDuration > 0)
            {
                yield return new WaitForSeconds(bannerHideDuration);
            }

            transitionSequencePlayer.PlaySequenceById(transitToFreeId);

            if (startTransitionDuration > 0f)
            {
                yield return new WaitForSeconds(startTransitionDuration);
            }


            if (delayBeforeFirstFreeSpin > 0f)
            {
                yield return new WaitForSeconds(delayBeforeFirstFreeSpin);
            }

            freeSpinManager?.StartFreeSpinGameplay();

            currentBannerMode = BannerMode.None;
            currentRoutine = null;
        }

        private IEnumerator EndFreeSpinRoutine()
        {
            currentBannerMode = BannerMode.End;
            float totalWin = freeSpinManager != null ? freeSpinManager.TotalFreeSpinWin : 0f;

            if (bannerSequencePlayer != null)
            {
                bannerSequencePlayer.PlaySequenceById(endBannerSequenceId);
            }

            if (endBannerSequenceDuration > 0f)
            {
                yield return new WaitForSeconds(endBannerSequenceDuration);
            }
            
            waitingForClick = true;

            while (waitingForClick)
            {
                yield return null;
            }


            HideBanner();

            if (bannerHideDuration > 0f)
            {
                yield return new WaitForSeconds(bannerHideDuration);
            }
            
            if (transitionSequencePlayer != null)
            {
                transitionSequencePlayer.PlaySequenceById(transitToBaseId);
            }

            if (exitTransitionDuration > 0f)
            {
                yield return new WaitForSeconds(exitTransitionDuration);
            }

            currentBannerMode = BannerMode.None;
            currentRoutine = null;
        }

        public void OnBannerClicked()
        {
            if (currentBannerMode == BannerMode.None)
            {
                return;
            }

            waitingForClick = false;
        }

        private void HideBanner()
        {
            if (bannerSequencePlayer != null)
            {
                bannerSequencePlayer.PlaySequenceById(hideBannerId);
            }
        }

        private void UpdateStateUI()
        {
            if (freeSpinManager == null)
            {
                return;
            }

            if (freeSpinsLeftText != null)
            {
                freeSpinsLeftText.text = string.Format(freeSpinsLeftFormat,freeSpinManager.RemainingSpins);
            }

            if (freeSpinWinText != null)
            {
                freeSpinWinText.text = string.Format(freeSpinWinFormat,freeSpinManager.State.TotalAwardedSpins);
            }

            if(freeSpinWinAmountText != null)
            {
                freeSpinWinAmountText.text = string.Format(freeSpinWinAmountFormat,freeSpinManager.TotalFreeSpinWin);
            }
        }
    }
}