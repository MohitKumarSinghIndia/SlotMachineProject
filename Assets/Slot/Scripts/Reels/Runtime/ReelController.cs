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

        [Header("Timing Graphs")]
        [SerializeField] private bool useAnimationCurves = true;

        [SerializeField]
        private AnimationCurve anticipationCurve = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 2.2f),
            new Keyframe(1f, 1f, 0.2f, 0f));

        [SerializeField]
        private AnimationCurve startDropCurve = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 0.8f),
            new Keyframe(1f, 1f, 3f, 0f));

        [SerializeField] private AnimationCurve loopCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [SerializeField]
        private AnimationCurve stopCurve = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 1.8f),
            new Keyframe(1f, 1f, 0.2f, 0f));

        [SerializeField]
        private AnimationCurve settleCurve = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 2.5f),
            new Keyframe(0.7f, 1.08f, 0f, -0.6f),
            new Keyframe(1f, 1f, 0f, 0f));

        [Header("Ease Fallback")]
        [SerializeField] private Ease anticipationEase = Ease.OutSine;
        [SerializeField] private Ease startDropEase = Ease.InQuad;
        [SerializeField] private Ease loopEase = Ease.Linear;
        [SerializeField] private Ease stopEase = Ease.OutCubic;
        [SerializeField] private Ease settleEase = Ease.OutBack;

        public bool UseAnimationCurves => useAnimationCurves;
        public float AnticipationLift => anticipationLift;
        public float AnticipationDuration => anticipationDuration;
        public float StartDropDistance => startDropDistance;
        public float StartDropDuration => startDropDuration;
        public float LoopStepDuration => loopStepDuration;
        public float StopEntryOffset => stopEntryOffset;
        public float StopOvershoot => stopOvershoot;
        public float StopDuration => stopDuration;
        public float SettleDuration => settleDuration;
        public AnimationCurve AnticipationCurve => anticipationCurve;
        public AnimationCurve StartDropCurve => startDropCurve;
        public AnimationCurve LoopCurve => loopCurve;
        public AnimationCurve StopCurve => stopCurve;
        public AnimationCurve SettleCurve => settleCurve;
        public Ease AnticipationEase => anticipationEase;
        public Ease StartDropEase => startDropEase;
        public Ease LoopEase => loopEase;
        public Ease StopEase => stopEase;
        public Ease SettleEase => settleEase;

        public void Clamp()
        {
            anticipationLift = Mathf.Max(0f, anticipationLift);
            anticipationDuration = Mathf.Max(0.01f, anticipationDuration);
            startDropDistance = Mathf.Max(0f, startDropDistance);
            startDropDuration = Mathf.Max(0.01f, startDropDuration);
            loopStepDuration = Mathf.Max(0.01f, loopStepDuration);
            stopEntryOffset = Mathf.Max(0f, stopEntryOffset);
            stopOvershoot = Mathf.Max(0f, stopOvershoot);
            stopDuration = Mathf.Max(0.01f, stopDuration);
            settleDuration = Mathf.Max(0.01f, settleDuration);

            anticipationCurve ??= AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            startDropCurve ??= AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            loopCurve ??= AnimationCurve.Linear(0f, 0f, 1f, 1f);
            stopCurve ??= AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            settleCurve ??= AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }

        public void CopyFrom(ReelTimingProfile source)
        {
            if (source == null)
            {
                return;
            }

            anticipationLift = source.anticipationLift;
            anticipationDuration = source.anticipationDuration;
            startDropDistance = source.startDropDistance;
            startDropDuration = source.startDropDuration;
            loopStepDuration = source.loopStepDuration;
            stopEntryOffset = source.stopEntryOffset;
            stopOvershoot = source.stopOvershoot;
            stopDuration = source.stopDuration;
            settleDuration = source.settleDuration;

            useAnimationCurves = source.useAnimationCurves;

            anticipationCurve = source.anticipationCurve != null
                ? new AnimationCurve(source.anticipationCurve.keys)
                : AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

            startDropCurve = source.startDropCurve != null
                ? new AnimationCurve(source.startDropCurve.keys)
                : AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

            loopCurve = source.loopCurve != null
                ? new AnimationCurve(source.loopCurve.keys)
                : AnimationCurve.Linear(0f, 0f, 1f, 1f);

            stopCurve = source.stopCurve != null
                ? new AnimationCurve(source.stopCurve.keys)
                : AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

            settleCurve = source.settleCurve != null
                ? new AnimationCurve(source.settleCurve.keys)
                : AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

            anticipationEase = source.anticipationEase;
            startDropEase = source.startDropEase;
            loopEase = source.loopEase;
            stopEase = source.stopEase;
            settleEase = source.settleEase;

            Clamp();
        }
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
        [SerializeField] private SymbolPoolManager symbolPoolManager;
        [SerializeField] private RectTransform actualSymbolsRoot;
        [SerializeField] private RectTransform looperRoot;

        [Header("Symbol Slots")]
        [Tooltip("Top, Middle, Bottom slots for final visible symbols.")]
        [SerializeField] private List<Transform> actualSymbolSlots = new List<Transform>();

        [Tooltip("Looper slots. Can include buffer slots.")]
        [SerializeField] private List<Transform> looperSymbolSlots = new List<Transform>();

        [Header("Spin Settings")]
        [SerializeField] private int visibleRowCount = 3;
        [SerializeField] private int looperBufferRows = 2;
        [SerializeField] private float symbolStepHeight = 172f;

        [Header("Debug State")]
        [SerializeField] private ReelSpinPhase currentPhase = ReelSpinPhase.Idle;
        [SerializeField] private int currentTopIndex;
        [SerializeField] private int pendingStopIndex;
        [SerializeField] private bool stopRequested;

        private readonly List<int> _pendingFinalSymbols = new List<int>();
        private readonly List<SymbolView> _pooledActualSymbols = new List<SymbolView>();
        private readonly List<SymbolView> _pooledLooperSymbols = new List<SymbolView>();

        [NonSerialized] private ReelTimingProfile timingProfile = new ReelTimingProfile();

        private Tween _activeTween;
        private Action<ReelController> _onStopped;

        public ReelSpinPhase CurrentPhase => currentPhase;
        public bool IsSpinning => currentPhase != ReelSpinPhase.Idle;
        public ReelTimingProfile TimingProfile => timingProfile;

        private void Awake()
        {
            EnsureTimingProfile();
            CacheLayerReferences();
            RestoreIdlePresentation();
        }

        private void OnValidate()
        {
            visibleRowCount = Mathf.Max(1, visibleRowCount);
            looperBufferRows = Mathf.Max(0, looperBufferRows);
            symbolStepHeight = Mathf.Max(1f, symbolStepHeight);

            EnsureTimingProfile();
            timingProfile.Clamp();

            if (!Application.isPlaying)
            {
                CacheLayerReferences();
            }
        }

        private void OnDestroy()
        {
            ReleasePooledSymbols(_pooledActualSymbols);
            ReleasePooledSymbols(_pooledLooperSymbols);
        }

        // --- NEW HELPER METHODS FOR VISUALS & ANIMATIONS ---
        public void SetReelStrip(ReelStripDefinition reelStrip)
        {
            if (reelStrip == null)
            {
                return;
            }

            ReelStrip = reelStrip;
            currentTopIndex = Wrap(currentTopIndex, ReelStrip.Length);
        }

        public Transform GetSymbolSlot(int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < actualSymbolSlots.Count)
            {
                return actualSymbolSlots[rowIndex];
            }
            return null;
        }

        public SymbolView GetVisibleSymbol(int rowIndex)
        {
            Transform slot = GetSymbolSlot(rowIndex);
            if (slot != null)
            {
                return slot.GetComponentInChildren<SymbolView>();
            }
            return null;
        }

        // ---------------------------------------------------

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

            EnsureTimingProfile();
            CacheLayerReferences();
            KillActiveTween();

            if (actualSymbolsRoot == null || looperRoot == null)
            {
                Debug.LogError($"[{name}] Reel layer roots are not assigned.");
                return;
            }

            if (actualSymbolSlots.Count == 0 && looperSymbolSlots.Count == 0)
            {
                Debug.LogError($"[{name}] Reel has no symbol slots assigned.");
                return;
            }

            _onStopped = onStopped;
            stopRequested = false;
            currentPhase = ReelSpinPhase.Start;

            ApplyActualWindow(currentTopIndex);
            SetRootPosition(actualSymbolsRoot, Vector2.zero);
            SetLayerVisibility(actualSymbolsRoot, true);

            if (HasSeparateLayers())
            {
                SetLayerVisibility(looperRoot, false);
                ReleasePooledSymbols(_pooledLooperSymbols);
            }

            if (HasSeparateLayers())
            {
                ApplyLooperWindow(currentTopIndex - looperBufferRows);
                SetRootPosition(looperRoot, Vector2.zero);
            }

            Sequence sequence = DOTween.Sequence();

            sequence.Append(ApplyConfiguredEase(
                actualSymbolsRoot.DOAnchorPosY(timingProfile.AnticipationLift, timingProfile.AnticipationDuration),
                timingProfile.AnticipationCurve,
                timingProfile.AnticipationEase));

            sequence.Append(ApplyConfiguredEase(
                actualSymbolsRoot.DOAnchorPosY(-timingProfile.StartDropDistance, timingProfile.StartDropDuration),
                timingProfile.StartDropCurve,
                timingProfile.StartDropEase));

            sequence.OnComplete(() =>
            {
                SetRootPosition(actualSymbolsRoot, Vector2.zero);

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

            EnsureTimingProfile();
            KillActiveTween();

            stopRequested = false;
            currentPhase = ReelSpinPhase.Idle;

            RestoreIdlePresentation();
        }

        public void ApplyTimingProfile(ReelTimingProfile sharedProfile)
        {
            EnsureTimingProfile();

            if (sharedProfile == null)
            {
                return;
            }

            timingProfile.CopyFrom(sharedProfile);
        }

        private void PlayLoopStep()
        {
            if (currentPhase != ReelSpinPhase.Loop)
            {
                return;
            }

            SetRootPosition(looperRoot, Vector2.zero);

            _activeTween = ApplyConfiguredEase(
                looperRoot.DOAnchorPosY(-symbolStepHeight, timingProfile.LoopStepDuration),
                timingProfile.LoopCurve,
                timingProfile.LoopEase)
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

            if (HasSeparateLayers())
            {
                SetLayerVisibility(looperRoot, false);
                ReleasePooledSymbols(_pooledLooperSymbols);
            }

            Sequence sequence = DOTween.Sequence();

            sequence.Append(ApplyConfiguredEase(
                actualSymbolsRoot.DOAnchorPosY(-timingProfile.StopOvershoot, timingProfile.StopDuration),
                timingProfile.StopCurve,
                timingProfile.StopEase));

            sequence.Append(ApplyConfiguredEase(
                actualSymbolsRoot.DOAnchorPosY(0f, timingProfile.SettleDuration),
                timingProfile.SettleCurve,
                timingProfile.SettleEase));

            sequence.OnComplete(() =>
            {
                currentPhase = ReelSpinPhase.Idle;
                stopRequested = false;

                SetRootPosition(actualSymbolsRoot, Vector2.zero);

                // --- TRIGGER LANDING ANIMATION ---
                // Tell every visible symbol on this reel to play Sequence ID 1
                for (int i = 0; i < visibleRowCount; i++)
                {
                    SymbolView symbol = GetVisibleSymbol(i);
                    if (symbol != null)
                    {
                        symbol.PlayLanding();
                    }
                }
                // ---------------------------------

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
                SetLayerVisibility(looperRoot, false);
                ReleasePooledSymbols(_pooledLooperSymbols);
            }
        }

        private void CacheLayerReferences()
        {
            if (!autoDiscoverLayers)
            {
                return;
            }

            if (symbolPoolManager == null)
            {
                symbolPoolManager = FindAnyObjectByType<SymbolPoolManager>();
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

            if (actualSymbolSlots.Count == 0 && actualSymbolsRoot != null)
            {
                actualSymbolSlots = CollectOrderedSlots(actualSymbolsRoot);
            }

            if (looperSymbolSlots.Count == 0 && looperRoot != null)
            {
                looperSymbolSlots = CollectOrderedSlots(looperRoot);
            }

            if (symbolStepHeight <= 1f)
            {
                symbolStepHeight = ResolveSymbolStepHeight();
            }
        }

        private List<Transform> CollectOrderedSlots(Transform root)
        {
            List<Transform> results = new List<Transform>();

            if (root == null)
            {
                return results;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);

                if (child != null)
                {
                    results.Add(child);
                }
            }

            results.Sort((a, b) =>
            {
                float ay = a.localPosition.y;
                float by = b.localPosition.y;
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
            IReadOnlyList<Transform> actualSlots = GetActualDisplaySlots();

            if (actualSlots.Count == 0)
            {
                return;
            }

            int[] visible = ReelStrip != null
                ? ReelStrip.GetVisibleWindow(topIndex, Mathf.Min(visibleRowCount, actualSlots.Count))
                : Array.Empty<int>();

            ApplySymbols(actualSlots, _pooledActualSymbols, visible);
        }

        private void ApplyActualSymbols(IReadOnlyList<int> finalSymbols)
        {
            IReadOnlyList<Transform> actualSlots = GetActualDisplaySlots();

            if (actualSlots.Count == 0 || finalSymbols == null)
            {
                return;
            }

            ApplySymbols(actualSlots, _pooledActualSymbols, finalSymbols);
        }

        private void ApplyLooperWindow(int topIndex)
        {
            if (looperSymbolSlots.Count == 0 || ReelStrip == null || ReelStrip.Length == 0)
            {
                return;
            }

            int[] window = ReelStrip.GetVisibleWindow(topIndex, looperSymbolSlots.Count);

            ApplySymbols(looperSymbolSlots, _pooledLooperSymbols, window);
        }

        private IReadOnlyList<Transform> GetActualDisplaySlots()
        {
            if (!HasSeparateLayers() && actualSymbolSlots.Count > visibleRowCount)
            {
                int offset = Mathf.Clamp(looperBufferRows, 0, actualSymbolSlots.Count - visibleRowCount);
                return actualSymbolSlots.GetRange(offset, visibleRowCount);
            }

            return actualSymbolSlots;
        }

        private void ApplySymbols(
            IReadOnlyList<Transform> slots,
            List<SymbolView> pooledSymbols,
            IReadOnlyList<int> symbolIds)
        {
            if (slots == null || symbolIds == null)
            {
                return;
            }

            if (symbolPoolManager == null)
            {
                Debug.LogError($"[{name}] SymbolPoolManager is missing.");
                return;
            }

            EnsurePooledListSize(pooledSymbols, slots.Count);

            for (int i = 0; i < slots.Count; i++)
            {
                Transform slot = slots[i];

                if (slot == null)
                {
                    continue;
                }

                if (i >= symbolIds.Count)
                {
                    ReleasePooledAt(pooledSymbols, i);
                    continue;
                }

                int targetId = symbolIds[i];
                SymbolView pooled = pooledSymbols[i];

                if (pooled == null || pooled.CurrentSymbolId != targetId)
                {
                    if (pooled != null)
                    {
                        symbolPoolManager.Release(pooled);
                    }

                    pooled = symbolPoolManager.Acquire(targetId, slot);
                    pooledSymbols[i] = pooled;
                }
                else
                {
                    pooled.transform.SetParent(slot, false);
                }

                if (pooled != null)
                {
                    SyncSymbolToSlot(pooled, slot);
                    pooled.gameObject.SetActive(true);
                }
            }
        }

        private void ReleasePooledAt(List<SymbolView> pooledSymbols, int index)
        {
            if (pooledSymbols == null || index < 0 || index >= pooledSymbols.Count)
            {
                return;
            }

            SymbolView pooled = pooledSymbols[index];

            if (pooled != null && symbolPoolManager != null)
            {
                symbolPoolManager.Release(pooled);
                pooledSymbols[index] = null;
            }
        }

        private float ResolveSymbolStepHeight()
        {
            List<Transform> source = actualSymbolSlots.Count > 1
                ? actualSymbolSlots
                : looperSymbolSlots;

            if (source.Count > 1)
            {
                return Mathf.Abs(source[0].localPosition.y - source[1].localPosition.y);
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

        private void EnsureTimingProfile()
        {
            timingProfile ??= new ReelTimingProfile();
        }

        private void KillActiveTween()
        {
            _activeTween?.Kill();
            _activeTween = null;
        }

        private static void EnsurePooledListSize(List<SymbolView> list, int count)
        {
            while (list.Count < count)
            {
                list.Add(null);
            }
        }

        private void ReleasePooledSymbols(List<SymbolView> pooledSymbols)
        {
            if (pooledSymbols == null || symbolPoolManager == null)
            {
                return;
            }

            for (int i = 0; i < pooledSymbols.Count; i++)
            {
                SymbolView pooled = pooledSymbols[i];

                if (pooled != null)
                {
                    symbolPoolManager.Release(pooled);
                    pooledSymbols[i] = null;
                }
            }
        }

        private static void SyncSymbolToSlot(SymbolView symbol, Transform slot)
        {
            if (symbol == null || slot == null)
            {
                return;
            }

            Transform symbolTransform = symbol.transform;

            symbolTransform.SetParent(slot, false);
            symbolTransform.localPosition = Vector3.zero;
            symbolTransform.localRotation = Quaternion.identity;
            symbolTransform.localScale = Vector3.one;
        }

        private T ApplyConfiguredEase<T>(T tween, AnimationCurve curve, Ease fallbackEase) where T : Tween
        {
            if (timingProfile != null && timingProfile.UseAnimationCurves && curve != null)
            {
                return tween.SetEase(curve);
            }

            return tween.SetEase(fallbackEase);
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