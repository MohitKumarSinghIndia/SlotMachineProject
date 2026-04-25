using System;
using System.IO;
using UnityEngine;

namespace SlotMachine.Reels.Runtime
{
    public class SpinSessionLogger : MonoBehaviour
    {
        [Header("Log Output")]
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private string logFolderName = "SpinLogs";
        [SerializeField] private string filePrefix = "spin_session";

        [Header("Debug")]
        [SerializeField] private string currentLogFilePath;

        public string CurrentLogFilePath => currentLogFilePath;

        private void Awake()
        {
            if (!enableLogging)
            {
                return;
            }

            EnsureLogFile();
        }

        public void Log(SpinOutcome outcome)
        {
            if (!enableLogging || outcome == null)
            {
                return;
            }

            EnsureLogFile();

            string json = JsonUtility.ToJson(outcome);
            using StreamWriter writer = new StreamWriter(currentLogFilePath, true);
            writer.WriteLine("BEGIN_SPIN");
            writer.WriteLine($"SpinId={outcome.SpinId}");
            writer.WriteLine($"TimestampUtc={outcome.TimestampUtc}");
            writer.WriteLine($"FromReplay={outcome.FromReplay}");
            writer.WriteLine($"HasWin={outcome.HasWin}");
            writer.WriteLine($"TotalWin={outcome.TotalWin}");
            writer.WriteLine($"IsBigWin={outcome.IsBigWin}");
            writer.WriteLine($"TriggersFreeSpins={outcome.TriggersFreeSpins}");
            writer.WriteLine($"ScatterCount={outcome.ScatterCount}");
            writer.WriteLine($"ReplayJson={json}");
            writer.WriteLine("END_SPIN");
            writer.WriteLine();
        }

        private void EnsureLogFile()
        {
            if (!string.IsNullOrWhiteSpace(currentLogFilePath))
            {
                return;
            }

            string folder = Path.Combine(Application.persistentDataPath, logFolderName);
            Directory.CreateDirectory(folder);
            string fileName = $"{filePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            currentLogFilePath = Path.Combine(folder, fileName);
        }
    }
}
