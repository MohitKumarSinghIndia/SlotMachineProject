using System;
using System.Collections.Generic;
using DG.Tweening;
using SlotMachine.Reels.Data;
using UnityEngine;

namespace SlotMachine.Reels.Runtime
{
    [Serializable]
    public class ReelLayoutSettings
    {
        [Min(1)]
        [SerializeField] private int visibleRowCount = 3;

        [Min(0)]
        [SerializeField] private int topBufferRows = 2;

        [Tooltip("Use 0 to auto-calculate from the reel viewport height.")]
        [Min(0f)]
        [SerializeField] private float symbolHeight = 0f;

        public int VisibleRowCount => visibleRowCount;
        public int TopBufferRows => topBufferRows;
        public float SymbolHeight => symbolHeight;

        public void Clamp()
        {
            visibleRowCount = Mathf.Max(1, visibleRowCount);
            topBufferRows = Mathf.Max(0, topBufferRows);
            symbolHeight = Mathf.Max(0f, symbolHeight);
        }

        public float ResolveSymbolHeight(RectTransform reelViewport)
        {
            if (symbolHeight > 0f)
            {
                return symbolHeight;
            }

            if (reelViewport != null && reelViewport.rect.height > 0f)
            {
                return reelViewport.rect.height / visibleRowCount;
            }

            return 164f;
        }
    }

    public class ReelManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SymbolManager symbolManager;
        [SerializeField] private List<ReelController> reels = new List<ReelController>();
        [SerializeField] private SpinResultGenerator spinResultGenerator;
        [SerializeField] private SpinSessionLogger spinSessionLogger;
        [SerializeField] private SpinReplaySource spinReplaySource;

        [Header("Spin Settings")]
        [SerializeField] private ReelSequenceSettings sequenceSettings = new ReelSequenceSettings();
        [SerializeField] private ReelSpinSettings sharedSpinSettings = new ReelSpinSettings();
        [SerializeField] private ReelLayoutSettings sharedLayoutSettings = new ReelLayoutSettings();

        [Header("Debug State")]
        [SerializeField] private bool isSpinInProgress;
        [SerializeField] private SpinOutcome lastOutcome;

        private int _remainingReels;
        private readonly List<Tween> _scheduledStarts = new List<Tween>();

        public bool IsSpinInProgress => isSpinInProgress;

        private void Awake()
        {
            CacheLocalPipelineReferences();
            ApplySharedSettingsToReels();
        }

        private void OnValidate()
        {
            CacheLocalPipelineReferences();
            sequenceSettings?.Clamp();
            sharedSpinSettings?.Clamp();
            sharedLayoutSettings?.Clamp();
            ApplySharedSettingsToReels();
        }

        [ContextMenu("Spin All Reels")]
        public void SpinAll()
        {
            if (reels.Count == 0)
            {
                return;
            }

            CacheLocalPipelineReferences();
            isSpinInProgress = false;
            KillScheduledStarts();
            StopAllReels();
            _remainingReels = 0;

            SpinOutcome outcome = ResolveNextOutcome();
            if (outcome == null)
            {
                isSpinInProgress = false;
                return;
            }

            lastOutcome = outcome;
            List<SpinCommand> commands = BuildSpinCommands(outcome);
            if (commands.Count == 0)
            {
                isSpinInProgress = false;
                return;
            }

            ApplySharedSettingsToReels();
            _remainingReels = commands.Count;
            isSpinInProgress = true;
            spinSessionLogger?.Log(outcome);
            ScheduleSpinCommands(commands);
        }

        private void HandleReelCompleted(ReelController reel)
        {
            _remainingReels = Math.Max(0, _remainingReels - 1);
            if (_remainingReels == 0)
            {
                isSpinInProgress = false;
                KillScheduledStarts();
            }
        }

        private List<SpinCommand> BuildSpinCommands(SpinOutcome outcome)
        {
            List<SpinCommand> commands = new List<SpinCommand>();
            if (outcome == null)
            {
                return commands;
            }

            for (int i = 0; i < reels.Count; i++)
            {
                ReelController reel = reels[i];
                if (reel == null || reel.ReelStrip == null)
                {
                    continue;
                }

                if (!outcome.TryGetStopIndex(reel.ReelIndex, out int stopIndex))
                {
                    continue;
                }

                commands.Add(new SpinCommand(reel, stopIndex));
            }

            return commands;
        }

        private void ScheduleSpinCommands(List<SpinCommand> commands)
        {
            for (int i = 0; i < commands.Count; i++)
            {
                SpinCommand command = commands[i];
                float delay = Mathf.Max(0f, i * sequenceSettings.ReelStartDelay);
                if (delay <= 0f)
                {
                    command.Reel.SpinToIndex(command.StopIndex, sharedSpinSettings, HandleReelCompleted);
                    continue;
                }

                Tween delayedStart = DOVirtual.DelayedCall(delay, () =>
                {
                    if (command.Reel != null)
                    {
                        command.Reel.SpinToIndex(command.StopIndex, sharedSpinSettings, HandleReelCompleted);
                    }
                }).SetUpdate(false);

                _scheduledStarts.Add(delayedStart);
            }
        }

        [ContextMenu("Refresh Reel Layout")]
        public void ApplySharedSettingsToReels()
        {
            for (int i = 0; i < reels.Count; i++)
            {
                ReelController reel = reels[i];
                if (reel == null)
                {
                    continue;
                }

                reel.ApplySharedLayout(sharedLayoutSettings);
                reel.RefreshImmediate(reel.CurrentTopIndex);
            }
        }

        private SpinOutcome ResolveNextOutcome()
        {
            if (spinReplaySource != null && spinReplaySource.TryDequeueOutcome(out SpinOutcome replayOutcome))
            {
                return replayOutcome;
            }

            if (spinResultGenerator == null)
            {
                Debug.LogError($"[{name}] SpinResultGenerator reference is missing.");
                return null;
            }

            return spinResultGenerator.GenerateOutcome(reels);
        }

        private void CacheLocalPipelineReferences()
        {
            if (spinResultGenerator == null)
            {
                spinResultGenerator = GetComponent<SpinResultGenerator>();
            }

            if (spinSessionLogger == null)
            {
                spinSessionLogger = GetComponent<SpinSessionLogger>();
            }

            if (spinReplaySource == null)
            {
                spinReplaySource = GetComponent<SpinReplaySource>();
            }
        }

        private void KillScheduledStarts()
        {
            for (int i = 0; i < _scheduledStarts.Count; i++)
            {
                _scheduledStarts[i]?.Kill();
            }

            _scheduledStarts.Clear();
        }

        private bool HasActiveSpinWork()
        {
            if (_scheduledStarts.Count > 0)
            {
                return true;
            }

            for (int i = 0; i < reels.Count; i++)
            {
                if (reels[i] != null && reels[i].IsSpinning)
                {
                    return true;
                }
            }

            return false;
        }

        private void StopAllReels()
        {
            for (int i = 0; i < reels.Count; i++)
            {
                if (reels[i] != null)
                {
                    reels[i].StopSpin();
                }
            }
        }

        private readonly struct SpinCommand
        {
            public SpinCommand(ReelController reel, int stopIndex)
            {
                Reel = reel;
                StopIndex = stopIndex;
            }

            public ReelController Reel { get; }
            public int StopIndex { get; }
        }
    }
}
