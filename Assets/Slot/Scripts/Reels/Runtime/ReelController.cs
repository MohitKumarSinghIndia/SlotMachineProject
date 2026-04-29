using System;
using System.Collections.Generic;
using DG.Tweening;
using SlotMachine.Reels.Data;
using UnityEngine;

namespace SlotMachine.Reels.Runtime
{
    [Serializable]
    public class ReelTimingProfile
    {
        [Header("Start")]
        [Min(0f)]
        [SerializeField] private float anticipationLift = 18f;
        [Min(0.01f)]
        [SerializeField] private float anticipationDuration = 0.12f;
        [Min(0f)]
        [SerializeField] private float startDropDistance = 140f;
        [Min(0.01f)]
        [SerializeField] private float startDropDuration = 0.22f;

        [Header("Loop")]
        [Min(0.01f)]
        [SerializeField] private float loopStepDuration = 0.075f;

        [Header("Stop")]
        [Min(0f)]
        [SerializeField] private float stopEntryOffset = 150f;
        [Min(0f)]
        [SerializeField] private float stopOvershoot = 18f;
        [Min(0.01f)]
        [SerializeField] private float stopDuration = 0.24f;
        [Min(0.01f)]
        [SerializeField] private float settleDuration = 0.12f;

        [Header("Easing")]
        [SerializeField] private Ease anticipationEase = Ease.OutSine;
        [SerializeField] private Ease startDropEase = Ease.InQuad;
        [SerializeField] private Ease loopEase = Ease.Linear;
        [SerializeField] private Ease stopEase = Ease.OutCubic;
        [SerializeField] private Ease settleEase = Ease.OutBack;

        public float AnticipationLift => anticipationLift;
        public float AnticipationDuration => anticipationDuration;
        public float StartDropDistance => startDropDistance;
        public float StartDropDuration => startDropDuration;
        public float LoopStepDuration => loopStepDuration;
        public float StopEntryOffset => stopEntryOffset;
        public float StopOvershoot => stopOvershoot;
        public float StopDuration => stopDuration;
        public float SettleDuration => settleDuration;
        public Ease AnticipationEase => anticipationEase;
        public Ease StartDropEase => startDropEase;
        public Ease LoopEase => loopEase;
        public Ease StopEase => stopEase;
        public Ease SettleEase => settleEase;
    }

    public enum ReelSpinPhase
    {
        Idle,
        Start,
        Loop,
        Stop
    }

    public class ReelController : MonoBehaviour
    {
        [Header("Reel Info")]
        public int ReelIndex;
        public ReelStripDefinition ReelStrip;

        [Header("Layer Setup")]
        [SerializeField] private bool autoDiscoverLayers = true;
        [SerializeField] private RectTransform actualSymbolsRoot;
        [SerializeField] private RectTransform looperRoot;
        [SerializeField] private List<SymbolView> actualSymbolViews = new List<SymbolView>();
        [SerializeField] private List<SymbolView> looperSymbolViews = new List<SymbolView>();

        [Header("Spin Settings")]
        [SerializeField] private ReelTimingProfile timingProfile = new ReelTimingProfile();
        [SerializeField] private int visibleRowCount = 3;
        [SerializeField] private int looperBufferRows = 2;
        [SerializeField] private float symbolStepHeight = 172f;

        [Header("Debug State")]
        [SerializeField] private ReelSpinPhase currentPhase = ReelSpinPhase.Idle;
        [SerializeField] private int currentTopIndex;
        [SerializeField] private int pendingStopIndex;
        [SerializeField] private bool stopRequested;

        private readonly List<int> _pendingFinalSymbols = new List<int>();
        private Tween _activeTween;
        private Action<ReelController> _onStopped;

        public ReelSpinPhase CurrentPhase => currentPhase;
        public bool IsSpinning => currentPhase != ReelSpinPhase.Idle;

        private void Awake()
        {
            CacheLayerReferences();
            RestoreIdlePresentation();
        }

        private void OnValidate()
        {
            visibleRowCount = Mathf.Max(1, visibleRowCount);
            looperBufferRows = Mathf.Max(0, looperBufferRows);
            symbolStepHeight = Mathf.Max(1f, symbolStepHeight);

            if (!Application.isPlaying)
            {
                CacheLayerReferences();
            }
        }

        public void PrepareStopResult(int stopIndex, IReadOnlyList<int> finalSymbols)
        {
            pendingStopIndex = stopIndex;
            _pendingFinalSymbols.Clear();

            if (finalSymbols == null)
            {
                return;
            }

            for (int i = 0; i < finalSymbols.Count; i++)
            {
                _pendingFinalSymbols.Add(finalSymbols[i]);
            }
        }

        public void StartSpin(int reelIndex)
        {
            StartSpin(reelIndex, null);
        }

        public void StartSpin(int reelIndex, Action<ReelController> onStopped)
        {
            if (reelIndex != ReelIndex || ReelStrip == null || ReelStrip.Length == 0)
            {
                return;
            }

            CacheLayerReferences();
            KillActiveTween();

            if (actualSymbolsRoot == null || looperRoot == null)
            {
                Debug.LogError($"[{name}] Reel layer roots are not assigned.");
                return;
            }

            if (actualSymbolViews.Count == 0 && looperSymbolViews.Count == 0)
            {
                Debug.LogError($"[{name}] Reel has no SymbolView components to animate.");
                return;
            }

            _onStopped = onStopped;
            stopRequested = false;
            currentPhase = ReelSpinPhase.Start;

            // Start phase always belongs to the Actual Symbols layer.
            ApplyActualWindow(currentTopIndex);
            SetRootPosition(actualSymbolsRoot, Vector2.zero);
            SetLayerVisibility(actualSymbolsRoot, true);

            if (HasSeparateLayers())
            {
                //SetLayerVisibility(looperRoot, false);
            }

            // Preload the Looper layer before the handoff only when it is a separate root.
            // When both layers share the same root, we keep the actual-symbol presentation
            // untouched until the start anticipation is finished.
            if (HasSeparateLayers())
            {
                ApplyLooperWindow(currentTopIndex - looperBufferRows);
                SetRootPosition(looperRoot, Vector2.zero);
            }

            Sequence sequence = DOTween.Sequence();
            sequence.Append(actualSymbolsRoot.DOAnchorPosY(timingProfile.AnticipationLift, timingProfile.AnticipationDuration)
                .SetEase(timingProfile.AnticipationEase));
            sequence.Append(actualSymbolsRoot.DOAnchorPosY(-timingProfile.StartDropDistance, timingProfile.StartDropDuration)
                .SetEase(timingProfile.StartDropEase));
            sequence.OnComplete(() =>
            {
                SetRootPosition(actualSymbolsRoot, Vector2.zero);

                // Handoff point: Actual Symbols hide, Looper becomes the only active layer.
                if (HasSeparateLayers())
                {
                    SetLayerVisibility(actualSymbolsRoot, false);
                    SetLayerVisibility(looperRoot, true);
                }
                else
                {
                    ApplyLooperWindow(currentTopIndex - looperBufferRows);
                }

                StartLoop(ReelIndex);
            });

            _activeTween = sequence;
        }

        public void StartLoop(int reelIndex)
        {
            if (reelIndex != ReelIndex || currentPhase == ReelSpinPhase.Stop || ReelStrip == null || ReelStrip.Length == 0)
            {
                return;
            }

            currentPhase = ReelSpinPhase.Loop;
            PlayLoopStep();
        }

        public void StopSpin(int reelIndex, IReadOnlyList<int> finalSymbols)
        {
            StopSpin(reelIndex, pendingStopIndex, finalSymbols);
        }

        public void StopSpin(int reelIndex, int finalStopIndex, IReadOnlyList<int> finalSymbols)
        {
            if (reelIndex != ReelIndex)
            {
                return;
            }

            PrepareStopResult(finalStopIndex, finalSymbols);
            stopRequested = true;

            if (currentPhase == ReelSpinPhase.Idle)
            {
                PlayStopPhase();
            }
        }

        public void ResetReel(int reelIndex)
        {
            if (reelIndex != ReelIndex)
            {
                return;
            }

            KillActiveTween();
            stopRequested = false;
            currentPhase = ReelSpinPhase.Idle;
            RestoreIdlePresentation();
        }

        private void PlayLoopStep()
        {
            if (currentPhase != ReelSpinPhase.Loop)
            {
                return;
            }

            SetRootPosition(looperRoot, Vector2.zero);
            _activeTween = looperRoot.DOAnchorPosY(-symbolStepHeight, timingProfile.LoopStepDuration)
                .SetEase(timingProfile.LoopEase)
                .OnComplete(() =>
                {
                    currentTopIndex = Wrap(currentTopIndex - 1, ReelStrip.Length);
                    ApplyLooperWindow(currentTopIndex - looperBufferRows);
                    SetRootPosition(looperRoot, Vector2.zero);

                    if (stopRequested)
                    {
                        PlayStopPhase();
                        return;
                    }

                    PlayLoopStep();
                });
        }

        private void PlayStopPhase()
        {
            KillActiveTween();
            currentPhase = ReelSpinPhase.Stop;
            currentTopIndex = Wrap(pendingStopIndex, ReelStrip.Length);

            if (_pendingFinalSymbols.Count == 0)
            {
                int[] window = ReelStrip.GetVisibleWindow(currentTopIndex, visibleRowCount);
                _pendingFinalSymbols.Clear();
                for (int i = 0; i < window.Length; i++)
                {
                    _pendingFinalSymbols.Add(window[i]);
                }
            }

            ApplyActualSymbols(_pendingFinalSymbols);
            SetRootPosition(actualSymbolsRoot, new Vector2(0f, timingProfile.StopEntryOffset));

            SetLayerVisibility(actualSymbolsRoot, true);

            // Stop transition: Looper turns off, Actual Symbols return with the final result.
            if (HasSeparateLayers())
            {
                //SetLayerVisibility(looperRoot, false);
            }

            Sequence sequence = DOTween.Sequence();
            sequence.Append(actualSymbolsRoot.DOAnchorPosY(-timingProfile.StopOvershoot, timingProfile.StopDuration)
                .SetEase(timingProfile.StopEase));
            sequence.Append(actualSymbolsRoot.DOAnchorPosY(0f, timingProfile.SettleDuration)
                .SetEase(timingProfile.SettleEase));
            sequence.OnComplete(() =>
            {
                currentPhase = ReelSpinPhase.Idle;
                stopRequested = false;
                SetRootPosition(actualSymbolsRoot, Vector2.zero);
                _pendingFinalSymbols.Clear();
                _onStopped?.Invoke(this);
            });

            _activeTween = sequence;
        }

        private void RestoreIdlePresentation()
        {
            CacheLayerReferences();
            ApplyActualWindow(currentTopIndex);
            SetRootPosition(actualSymbolsRoot, Vector2.zero);
            SetRootPosition(looperRoot, Vector2.zero);
            SetLayerVisibility(actualSymbolsRoot, true);

            if (HasSeparateLayers())
            {
                //SetLayerVisibility(looperRoot, false);
            }
        }

        private void CacheLayerReferences()
        {
            if (!autoDiscoverLayers)
            {
                return;
            }

            if (actualSymbolsRoot == null)
            {
                actualSymbolsRoot = FindChildRect("SymbolRoot")
                    ?? FindChildRect("ActualSymbolsRoot")
                    ?? FindChildRect("StopDropRoot")
                    ?? FindChildRect("StripRoot");
            }

            if (looperRoot == null)
            {
                looperRoot = FindChildRect("LooperRoot")
                    ?? FindChildRect("StripRoot")
                    ?? actualSymbolsRoot;
            }

            if (actualSymbolViews.Count == 0 && actualSymbolsRoot != null)
            {
                actualSymbolViews = CollectOrderedViews(actualSymbolsRoot);
            }

            if (looperSymbolViews.Count == 0 && looperRoot != null)
            {
                looperSymbolViews = CollectOrderedViews(looperRoot);
            }

            if (symbolStepHeight <= 1f)
            {
                symbolStepHeight = ResolveSymbolStepHeight();
            }
        }

        private List<SymbolView> CollectOrderedViews(RectTransform root)
        {
            List<SymbolView> results = new List<SymbolView>(root.GetComponentsInChildren<SymbolView>(true));
            results.Sort((a, b) =>
            {
                float ay = ((RectTransform)a.transform).anchoredPosition.y;
                float by = ((RectTransform)b.transform).anchoredPosition.y;
                return by.CompareTo(ay);
            });

            return results;
        }

        private RectTransform FindChildRect(string childName)
        {
            Transform child = transform.Find(childName);
            return child as RectTransform;
        }

        private void ApplyActualWindow(int topIndex)
        {
            IReadOnlyList<SymbolView> actualViews = GetActualDisplayViews();
            if (actualViews.Count == 0)
            {
                return;
            }

            int[] visible = ReelStrip != null
                ? ReelStrip.GetVisibleWindow(topIndex, Mathf.Min(visibleRowCount, actualViews.Count))
                : Array.Empty<int>();

            ApplySymbols(actualViews, visible);
        }

        private void ApplyActualSymbols(IReadOnlyList<int> finalSymbols)
        {
            IReadOnlyList<SymbolView> actualViews = GetActualDisplayViews();
            if (actualViews.Count == 0 || finalSymbols == null)
            {
                return;
            }

            for (int i = 0; i < actualViews.Count; i++)
            {
                SymbolView view = actualViews[i];
                if (view == null)
                {
                    continue;
                }

                if (i < finalSymbols.Count)
                {
                    view.ApplySymbolId(finalSymbols[i]);
                    view.gameObject.SetActive(true);
                }
                else
                {
                    view.gameObject.SetActive(false);
                }
            }
        }

        private void ApplyLooperWindow(int topIndex)
        {
            if (looperSymbolViews.Count == 0 || ReelStrip == null || ReelStrip.Length == 0)
            {
                return;
            }

            int[] window = ReelStrip.GetVisibleWindow(topIndex, looperSymbolViews.Count);
            ApplySymbols(looperSymbolViews, window);
        }

        private IReadOnlyList<SymbolView> GetActualDisplayViews()
        {
            if (!HasSeparateLayers() && actualSymbolViews.Count > visibleRowCount)
            {
                int offset = Mathf.Clamp(looperBufferRows, 0, actualSymbolViews.Count - visibleRowCount);
                return actualSymbolViews.GetRange(offset, visibleRowCount);
            }

            return actualSymbolViews;
        }

        private static void ApplySymbols(IReadOnlyList<SymbolView> views, IReadOnlyList<int> symbolIds)
        {
            for (int i = 0; i < views.Count; i++)
            {
                SymbolView view = views[i];
                if (view == null)
                {
                    continue;
                }

                if (i < symbolIds.Count)
                {
                    view.ApplySymbolId(symbolIds[i]);
                    view.gameObject.SetActive(true);
                }
                else
                {
                    view.gameObject.SetActive(false);
                }
            }
        }

        private float ResolveSymbolStepHeight()
        {
            List<SymbolView> source = actualSymbolViews.Count > 1 ? actualSymbolViews : looperSymbolViews;
            if (source.Count > 1)
            {
                RectTransform first = source[0].transform as RectTransform;
                RectTransform second = source[1].transform as RectTransform;
                return Mathf.Abs(first.anchoredPosition.y - second.anchoredPosition.y);
            }

            return 172f;
        }

        private bool HasSeparateLayers()
        {
            return actualSymbolsRoot != null && looperRoot != null && actualSymbolsRoot != looperRoot;
        }

        private static void SetLayerVisibility(RectTransform root, bool visible)
        {
            if (root == null)
            {
                return;
            }

            root.gameObject.SetActive(visible);
        }

        private static void SetRootPosition(RectTransform root, Vector2 position)
        {
            if (root == null)
            {
                return;
            }

            root.anchoredPosition = position;
        }

        private void KillActiveTween()
        {
            _activeTween?.Kill();
            _activeTween = null;
        }

        private static int Wrap(int value, int length)
        {
            if (length <= 0)
            {
                return 0;
            }

            return ((value % length) + length) % length;
        }
    }
}
