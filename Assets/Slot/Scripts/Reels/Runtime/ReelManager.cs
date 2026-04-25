using System;
using System.Collections.Generic;
using SlotMachine.Reels.Data;
using UnityEngine;
using Random = System.Random;

namespace SlotMachine.Reels.Runtime
{
    [Serializable]
    public class ReelSequenceSettings
    {
        [Header("Cascade")]
        [Min(0f)]
        [SerializeField] private float reelStartDelay = 0.12f;

        [Header("Random")]
        [SerializeField] private bool useFixedSeed;
        [SerializeField] private int fixedSeed = 12345;

        public float ReelStartDelay => reelStartDelay;
        public bool UseFixedSeed => useFixedSeed;
        public int FixedSeed => fixedSeed;

        public void Clamp()
        {
            reelStartDelay = Mathf.Max(0f, reelStartDelay);
        }
    }

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

        [Header("Spin Settings")]
        [SerializeField] private ReelSequenceSettings sequenceSettings = new ReelSequenceSettings();
        [SerializeField] private ReelSpinSettings sharedSpinSettings = new ReelSpinSettings();
        [SerializeField] private ReelLayoutSettings sharedLayoutSettings = new ReelLayoutSettings();

        [Header("Debug State")]
        [SerializeField] private bool isSpinInProgress;

        private Random _random;
        private int _remainingReels;
        private Coroutine _spinRoutine;

        public bool IsSpinInProgress => isSpinInProgress;

        private void Awake()
        {
            _random = sequenceSettings.UseFixedSeed ? new Random(sequenceSettings.FixedSeed) : new Random();
            ApplySharedSettingsToReels();
        }

        private void OnValidate()
        {
            sequenceSettings?.Clamp();
            sharedSpinSettings?.Clamp();
            sharedLayoutSettings?.Clamp();
            ApplySharedSettingsToReels();
        }

        [ContextMenu("Spin All Reels")]
        public void SpinAll()
        {
            if (isSpinInProgress || reels.Count == 0)
            {
                return;
            }

            EnsureRandom();
            ApplySharedSettingsToReels();
            isSpinInProgress = true;
            _remainingReels = 0;
            List<SpinCommand> commands = BuildSpinCommands();
            if (commands.Count == 0)
            {
                isSpinInProgress = false;
                return;
            }

            _remainingReels = commands.Count;

            if (_spinRoutine != null)
            {
                StopCoroutine(_spinRoutine);
            }

            _spinRoutine = StartCoroutine(SpinRoutine(commands));
        }

        private void HandleReelCompleted(ReelController reel)
        {
            _remainingReels = Math.Max(0, _remainingReels - 1);
            if (_remainingReels == 0)
            {
                isSpinInProgress = false;
                _spinRoutine = null;
            }
        }

        private List<SpinCommand> BuildSpinCommands()
        {
            List<SpinCommand> commands = new List<SpinCommand>();

            for (int i = 0; i < reels.Count; i++)
            {
                ReelController reel = reels[i];
                if (reel == null || reel.ReelStrip == null)
                {
                    continue;
                }

                int stripLength = reel.ReelStrip.Length;
                if (stripLength <= 0)
                {
                    continue;
                }

                commands.Add(new SpinCommand(reel, _random.Next(0, stripLength)));
            }

            return commands;
        }

        private System.Collections.IEnumerator SpinRoutine(List<SpinCommand> commands)
        {
            for (int i = 0; i < commands.Count; i++)
            {
                SpinCommand command = commands[i];
                command.Reel.SpinToIndex(command.StopIndex, sharedSpinSettings, HandleReelCompleted);

                if (i < commands.Count - 1 && sequenceSettings.ReelStartDelay > 0f)
                {
                    yield return new WaitForSeconds(sequenceSettings.ReelStartDelay);
                }
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

        private void EnsureRandom()
        {
            if (_random == null)
            {
                _random = sequenceSettings.UseFixedSeed ? new Random(sequenceSettings.FixedSeed) : new Random();
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
