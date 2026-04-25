using System;
using System.Collections.Generic;
using SlotMachine.Reels.Data;
using UnityEngine;
using DG.Tweening;

namespace SlotMachine.Reels.Runtime
{
    [Serializable]
    public class ReelSpinSettings
    {
        [Header("Anticipation")]
        [Min(0f)]
        [SerializeField] private float anticipationLift = 22f;

        [Min(0.01f)]
        [SerializeField] private float anticipationDuration = 0.12f;

        [Header("Cruise")]
        [Min(0.05f)]
        [SerializeField] private float mainSpinDuration = 1.1f;

        [Min(0)]
        [SerializeField] private int extraLoops = 12;

        [Header("Landing")]
        [Min(0f)]
        [SerializeField] private float landingOvershoot = 18f;

        [Min(0.01f)]
        [SerializeField] private float landingDownDuration = 0.09f;

        [Min(0.01f)]
        [SerializeField] private float landingReturnDuration = 0.12f;

        [Header("Ease Curves")]
        [SerializeField] private AnimationCurve anticipationEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve spinEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve landingDownEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve landingReturnEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public float AnticipationLift => anticipationLift;
        public float AnticipationDuration => anticipationDuration;
        public float MainSpinDuration => mainSpinDuration;
        public int ExtraLoops => extraLoops;
        public float LandingOvershoot => landingOvershoot;
        public float LandingDownDuration => landingDownDuration;
        public float LandingReturnDuration => landingReturnDuration;
        public AnimationCurve AnticipationEase => anticipationEase;
        public AnimationCurve SpinEase => spinEase;
        public AnimationCurve LandingDownEase => landingDownEase;
        public AnimationCurve LandingReturnEase => landingReturnEase;

        public void Clamp()
        {
            anticipationLift = Mathf.Max(0f, anticipationLift);
            anticipationDuration = Mathf.Max(0.01f, anticipationDuration);
            mainSpinDuration = Mathf.Max(0.05f, mainSpinDuration);
            extraLoops = Mathf.Max(0, extraLoops);
            landingOvershoot = Mathf.Max(0f, landingOvershoot);
            landingDownDuration = Mathf.Max(0.01f, landingDownDuration);
            landingReturnDuration = Mathf.Max(0.01f, landingReturnDuration);

            anticipationEase ??= AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            spinEase ??= AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            landingDownEase ??= AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            landingReturnEase ??= AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }
    }

    public class ReelController : MonoBehaviour
    {
        [Header("Reel Info")]
        [SerializeField] private int reelIndex;
        [SerializeField] private ReelStripDefinition reelStrip;
        [SerializeField] private SymbolManager symbolManager;

        [Header("Visual References")]
        [SerializeField] private RectTransform stripRoot;
        [SerializeField] private List<SymbolView> symbolViews = new List<SymbolView>();

        [Header("Debug State")]
        [SerializeField] private int currentTopIndex;
        [SerializeField] private bool isSpinning;
        [SerializeField] private int activeVisibleRowCount = 3;
        [SerializeField] private int activeTopBufferRows = 2;
        [SerializeField] private float activeSymbolHeight = 164f;

        private Sequence _spinSequence;

        public int ReelIndex => reelIndex;
        public int CurrentTopIndex => currentTopIndex;
        public bool IsSpinning => isSpinning;
        public ReelStripDefinition ReelStrip => reelStrip;

        private void Awake()
        {
            EnsureLayout();
            RefreshVisibleSymbols();
        }

        private void OnValidate()
        {
            activeVisibleRowCount = Mathf.Max(1, activeVisibleRowCount);
            activeTopBufferRows = Mathf.Max(0, activeTopBufferRows);
            activeSymbolHeight = Mathf.Max(1f, activeSymbolHeight);
            EnsureLayout();
        }

        public void ApplySharedLayout(ReelLayoutSettings layoutSettings)
        {
            if (layoutSettings == null)
            {
                return;
            }

            activeVisibleRowCount = layoutSettings.VisibleRowCount;
            activeTopBufferRows = layoutSettings.TopBufferRows;
            activeSymbolHeight = layoutSettings.ResolveSymbolHeight(transform as RectTransform);
            EnsureLayout();
        }

        public void RefreshImmediate(int topVisibleIndex)
        {
            currentTopIndex = topVisibleIndex;
            EnsureLayout();
            ResetStripPosition();
            RefreshVisibleSymbols();
        }

        public void SpinToIndex(int targetTopIndex, ReelSpinSettings spinSettings, Action<ReelController> onComplete = null)
        {
            if (reelStrip == null)
            {
                Debug.LogError($"[{name}] Reel strip is missing.");
                return;
            }

            if (symbolManager == null)
            {
                Debug.LogError($"[{name}] SymbolManager reference is missing.");
                return;
            }

            if (stripRoot == null)
            {
                Debug.LogError($"[{name}] StripRoot reference is missing.");
                return;
            }

            if (symbolViews.Count == 0)
            {
                Debug.LogError($"[{name}] No SymbolView entries assigned.");
                return;
            }

            if (spinSettings == null)
            {
                Debug.LogError($"[{name}] Shared spin settings are missing.");
                return;
            }

            EnsureLayout();

            StopSpin();

            int stripLength = reelStrip.Length;
            int normalizedTarget = Wrap(targetTopIndex, stripLength);
            int totalSteps = (spinSettings.ExtraLoops * stripLength) + GetBackwardDistance(currentTopIndex, normalizedTarget, stripLength);

            isSpinning = true;
            PlayWithDotween(normalizedTarget, totalSteps, spinSettings, onComplete);
        }

        public void StopSpin()
        {
            _spinSequence?.Kill();
            _spinSequence = null;

            isSpinning = false;
            ResetStripPosition();
        }

        private void RefreshVisibleSymbols()
        {
            if (reelStrip == null || symbolManager == null)
            {
                return;
            }

            for (int i = 0; i < symbolViews.Count; i++)
            {
                SymbolView view = symbolViews[i];
                if (view == null)
                {
                    continue;
                }

                int symbolId = reelStrip.GetSymbolIdAt(currentTopIndex + i - activeTopBufferRows);
                symbolManager.ApplySymbol(view, symbolId);
            }
        }

        private void EnsureLayout()
        {
            if (stripRoot == null || symbolViews.Count == 0)
            {
                return;
            }

            if (activeSymbolHeight <= 1f)
            {
                RectTransform viewport = transform as RectTransform;
                if (viewport != null && viewport.rect.height > 0f)
                {
                    activeSymbolHeight = viewport.rect.height / activeVisibleRowCount;
                }
                else
                {
                    activeSymbolHeight = 164f;
                }
            }

            for (int i = 0; i < symbolViews.Count; i++)
            {
                SymbolView view = symbolViews[i];
                if (view == null)
                {
                    continue;
                }

                RectTransform rect = view.transform as RectTransform;
                if (rect == null)
                {
                    continue;
                }

                float anchoredY = (activeTopBufferRows + 1 - i) * activeSymbolHeight;
                rect.anchoredPosition = new Vector2(0f, anchoredY);
            }
        }

        private void ResetStripPosition()
        {
            if (stripRoot != null)
            {
                stripRoot.anchoredPosition = Vector2.zero;
            }
        }

        private void UpdateSpinVisual(float travelledDistance)
        {
            int steppedRows = Mathf.FloorToInt(travelledDistance / activeSymbolHeight);
            float localOffset = travelledDistance - (steppedRows * activeSymbolHeight);
            int newTopIndex = Wrap(currentTopIndex - steppedRows, reelStrip.Length);

            if (newTopIndex != currentTopIndex)
            {
                currentTopIndex = newTopIndex;
                RefreshVisibleSymbols();
            }

            if (stripRoot != null)
            {
                stripRoot.anchoredPosition = new Vector2(0f, -localOffset);
            }
        }

        private void FinishSpin(int targetTopIndex, Action<ReelController> onComplete)
        {
            currentTopIndex = targetTopIndex;
            ResetStripPosition();
            RefreshVisibleSymbols();
            isSpinning = false;
            onComplete?.Invoke(this);
        }

        private int GetBackwardDistance(int startIndex, int targetIndex, int stripLength)
        {
            return Wrap(startIndex - targetIndex, stripLength);
        }

        private static int Wrap(int value, int length)
        {
            return ((value % length) + length) % length;
        }

        private void PlayWithDotween(int targetTopIndex, int totalSteps, ReelSpinSettings spinSettings, Action<ReelController> onComplete)
        {
            float spinDistance = totalSteps * activeSymbolHeight;
            float travelledDistance = 0f;

            _spinSequence = DOTween.Sequence();
            _spinSequence.Append(stripRoot.DOAnchorPosY(spinSettings.AnticipationLift, spinSettings.AnticipationDuration)
                .SetEase(spinSettings.AnticipationEase));

            _spinSequence.Append(DOTween.To(
                    () => travelledDistance,
                    value =>
                    {
                        travelledDistance = value;
                        UpdateSpinVisual(value);
                    },
                    spinDistance,
                    spinSettings.MainSpinDuration)
                .SetEase(spinSettings.SpinEase));

            _spinSequence.AppendCallback(() =>
            {
                currentTopIndex = targetTopIndex;
                ResetStripPosition();
                RefreshVisibleSymbols();
            });

            _spinSequence.Append(stripRoot.DOAnchorPosY(-spinSettings.LandingOvershoot, spinSettings.LandingDownDuration)
                .SetEase(spinSettings.LandingDownEase));
            _spinSequence.Append(stripRoot.DOAnchorPosY(0f, spinSettings.LandingReturnDuration)
                .SetEase(spinSettings.LandingReturnEase));
            _spinSequence.OnComplete(() => FinishSpin(targetTopIndex, onComplete));
        }
    }
}
