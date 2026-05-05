using System;
using UnityEngine;

namespace SlotMachine.Reels.Runtime
{
    [Serializable]
    public class PaylineDefinition
    {
        [SerializeField] private int lineId;
        [SerializeField] private string lineName;
        [SerializeField] private int[] rows = new int[5];

        public int LineId => lineId;
        public string LineName => lineName;
        public int[] Rows => rows;

        public PaylineDefinition(int lineId, string lineName, int[] rows)
        {
            this.lineId = lineId;
            this.lineName = lineName;
            this.rows = rows;
        }

        public bool IsValidForReelCount(int reelCount)
        {
            return rows != null && rows.Length >= reelCount;
        }
    }
}