using System;
using UnityEngine;

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
}
