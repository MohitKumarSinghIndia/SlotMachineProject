using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SlotMachine.Reels.Runtime
{
    public class SpinReplaySource : MonoBehaviour
    {
        [Header("Replay")]
        [SerializeField] private bool useReplayLog;
        [SerializeField] private string replayLogFilePath;

        [Header("Debug")]
        [SerializeField] private int loadedOutcomeCount;
        [SerializeField] private int nextReplayIndex;

        private readonly List<SpinOutcome> _loadedOutcomes = new List<SpinOutcome>();

        public bool TryDequeueOutcome(out SpinOutcome outcome)
        {
            outcome = null;

            if (!useReplayLog || string.IsNullOrWhiteSpace(replayLogFilePath))
            {
                return false;
            }

            if (_loadedOutcomes.Count == 0)
            {
                LoadFromDisk();
            }

            if (nextReplayIndex < 0 || nextReplayIndex >= _loadedOutcomes.Count)
            {
                return false;
            }

            outcome = _loadedOutcomes[nextReplayIndex];
            nextReplayIndex++;

            if (outcome != null)
            {
                outcome.FromReplay = true;
            }

            return outcome != null;
        }

        [ContextMenu("Reload Replay Log")]
        public void LoadFromDisk()
        {
            _loadedOutcomes.Clear();
            nextReplayIndex = 0;

            if (!useReplayLog || string.IsNullOrWhiteSpace(replayLogFilePath) || !File.Exists(replayLogFilePath))
            {
                loadedOutcomeCount = 0;
                return;
            }

            string[] lines = File.ReadAllLines(replayLogFilePath);
            for (int i = 0; i < lines.Length; i++)
            {
                const string prefix = "ReplayJson=";
                if (!lines[i].StartsWith(prefix))
                {
                    continue;
                }

                string json = lines[i].Substring(prefix.Length);
                if (string.IsNullOrWhiteSpace(json))
                {
                    continue;
                }

                SpinOutcome outcome = JsonUtility.FromJson<SpinOutcome>(json);
                if (outcome != null)
                {
                    _loadedOutcomes.Add(outcome);
                }
            }

            loadedOutcomeCount = _loadedOutcomes.Count;
        }
    }
}
