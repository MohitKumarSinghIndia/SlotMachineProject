using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SlotMachine.Reels.Runtime
{
    public enum BigWinType
    {
        None,
        Nice,
        Mega,
        Super,
        Sensational
    }

    public class BigWinController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject bigWinPanel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private BetManager betManager;

        [Header("Thresholds - X Times Current Bet")]
        [Min(0f)]
        [SerializeField] private float niceWinMultiplier = 10f;

        [Min(0f)]
        [SerializeField] private float megaWinMultiplier = 25f;

        [Min(0f)]
        [SerializeField] private float superWinMultiplier = 50f;

        [Min(0f)]
        [SerializeField] private float sensationalWinMultiplier = 100f;

        [Header("Durations")]
        [Min(0.1f)]
        [SerializeField] private float niceWinDuration = 1.5f;

        [Min(0.1f)]
        [SerializeField] private float megaWinDuration = 2f;

        [Min(0.1f)]
        [SerializeField] private float superWinDuration = 2.5f;

        [Min(0.1f)]
        [SerializeField] private float sensationalWinDuration = 3f;

        [Min(0f)]
        [SerializeField] private float holdAfterCountDuration = 0.5f;

        [Header("Display Text")]
        [SerializeField] private string niceWinText = "NICE WIN";
        [SerializeField] private string megaWinText = "MEGA WIN";
        [SerializeField] private string superWinText = "SUPER WIN";
        [SerializeField] private string sensationalWinText = "SENSATIONAL WIN";
        [SerializeField] private string amountFormat = "₹{0:0.00}";

        [Header("Events")]
        [SerializeField] private UnityEvent onBigWinStarted;
        [SerializeField] private UnityEvent onNiceWin;
        [SerializeField] private UnityEvent onMegaWin;
        [SerializeField] private UnityEvent onSuperWin;
        [SerializeField] private UnityEvent onSensationalWin;
        [SerializeField] private UnityEvent onBigWinCompleted;

        [Header("Debug")]
        [SerializeField] private BigWinType lastBigWinType = BigWinType.None;
        [SerializeField] private float lastWinMultiplier;
        [SerializeField] private float lastWinAmount;

        private Coroutine currentRoutine;

        public BigWinType LastBigWinType => lastBigWinType;
        public float LastWinMultiplier => lastWinMultiplier;
        public float LastWinAmount => lastWinAmount;

        private void Awake()
        {
            CacheReferences();
            Hide();
        }

        private void OnValidate()
        {
            niceWinMultiplier = Mathf.Max(0f, niceWinMultiplier);
            megaWinMultiplier = Mathf.Max(niceWinMultiplier, megaWinMultiplier);
            superWinMultiplier = Mathf.Max(megaWinMultiplier, superWinMultiplier);
            sensationalWinMultiplier = Mathf.Max(superWinMultiplier, sensationalWinMultiplier);

            niceWinDuration = Mathf.Max(0.1f, niceWinDuration);
            megaWinDuration = Mathf.Max(0.1f, megaWinDuration);
            superWinDuration = Mathf.Max(0.1f, superWinDuration);
            sensationalWinDuration = Mathf.Max(0.1f, sensationalWinDuration);
            holdAfterCountDuration = Mathf.Max(0f, holdAfterCountDuration);
        }

        public IEnumerator TryPlayBigWin(float totalWin)
        {
            Debug.Log(Time.time + "====");
            yield return new WaitForSeconds(10f);
            Debug.Log(Time.time + "++++++");


            CacheReferences();

            lastWinAmount = totalWin;
            lastBigWinType = ResolveBigWinType(totalWin);

            if (lastBigWinType == BigWinType.None)
            {
                Hide();
                yield break;
            }

            if (currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
                currentRoutine = null;
            }

            yield return PlayBigWinRoutine(totalWin, lastBigWinType);
        }

        public BigWinType ResolveBigWinType(float totalWin)
        {
            lastWinMultiplier = 0f;

            if (betManager == null)
            {
                Debug.LogWarning("[BigWinController] BetManager is missing.");
                return BigWinType.None;
            }

            if (betManager.TotalBet <= 0f || totalWin <= 0f)
            {
                return BigWinType.None;
            }

            lastWinMultiplier = totalWin / betManager.TotalBet;

            if (lastWinMultiplier >= sensationalWinMultiplier)
            {
                return BigWinType.Sensational;
            }

            if (lastWinMultiplier >= superWinMultiplier)
            {
                return BigWinType.Super;
            }

            if (lastWinMultiplier >= megaWinMultiplier)
            {
                return BigWinType.Mega;
            }

            if (lastWinMultiplier >= niceWinMultiplier)
            {
                return BigWinType.Nice;
            }

            return BigWinType.None;
        }

        private IEnumerator PlayBigWinRoutine(float totalWin, BigWinType winType)
        {
            
            onBigWinStarted?.Invoke();

            if (bigWinPanel != null)
            {
                bigWinPanel.SetActive(true);
            }

            SetTitle(winType);
            InvokeTypeEvent(winType);

            float duration = GetDuration(winType);
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;

                float t = Mathf.Clamp01(timer / duration);
                float value = Mathf.Lerp(0f, totalWin, t);

                SetAmount(value);

                yield return null;
            }

            SetAmount(totalWin);

            if (holdAfterCountDuration > 0f)
            {
                yield return new WaitForSeconds(holdAfterCountDuration);
            }

            onBigWinCompleted?.Invoke();

            Hide();
        }

        private void SetTitle(BigWinType winType)
        {
            if (titleText == null)
            {
                return;
            }

            switch (winType)
            {
                case BigWinType.Nice:
                    titleText.text = niceWinText;
                    break;

                case BigWinType.Mega:
                    titleText.text = megaWinText;
                    break;

                case BigWinType.Super:
                    titleText.text = superWinText;
                    break;

                case BigWinType.Sensational:
                    titleText.text = sensationalWinText;
                    break;

                default:
                    titleText.text = string.Empty;
                    break;
            }
        }

        private float GetDuration(BigWinType winType)
        {
            switch (winType)
            {
                case BigWinType.Nice:
                    return niceWinDuration;

                case BigWinType.Mega:
                    return megaWinDuration;

                case BigWinType.Super:
                    return superWinDuration;

                case BigWinType.Sensational:
                    return sensationalWinDuration;

                default:
                    return 0f;
            }
        }

        private void InvokeTypeEvent(BigWinType winType)
        {
            switch (winType)
            {
                case BigWinType.Nice:
                    onNiceWin?.Invoke();
                    break;

                case BigWinType.Mega:
                    onMegaWin?.Invoke();
                    break;

                case BigWinType.Super:
                    onSuperWin?.Invoke();
                    break;

                case BigWinType.Sensational:
                    onSensationalWin?.Invoke();
                    break;
            }
        }

        private void SetAmount(float amount)
        {
            if (amountText != null)
            {
                amountText.text = string.Format(amountFormat, amount);
            }
        }

        public void Hide()
        {
            if (bigWinPanel != null)
            {
                bigWinPanel.SetActive(false);
            }

            if (titleText != null)
            {
                titleText.text = string.Empty;
            }

            SetAmount(0f);
        }

        private void CacheReferences()
        {
            if (betManager == null)
            {
                betManager = GetComponent<BetManager>();
            }
        }
    }
}