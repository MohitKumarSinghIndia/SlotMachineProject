using System;
using System.Collections.Generic;
using DG.Tweening;
using SlotMachine.Reels.Data;
using UnityEngine;

namespace SlotMachine.Reels.Runtime
{
    public class ReelManager : MonoBehaviour
    {
        [Header("References")]
        //[SerializeField] private SymbolManager symbolManager;
        [SerializeField] private List<ReelController> reels = new List<ReelController>();
        [SerializeField] private SpinResultGenerator spinResultGenerator;

        [Header("Debug State")]
        [SerializeField] private bool isSpinInProgress;
        [SerializeField] private SpinOutcome lastOutcome;

        private int _remainingReels;
        

        public bool IsSpinInProgress => isSpinInProgress;

        private void Awake()
        {
            CacheLocalReferences();
        }

        [ContextMenu("Spin All Reels")]
        public void SpinAll()
        {
            if (reels.Count == 0)
            {
                return;
            }

            CacheLocalReferences();
            isSpinInProgress = false;
            StopAllReels();
            _remainingReels = 0;

            SpinOutcome outcome = ResolveNextOutcome();
            if (outcome == null)
            {
                isSpinInProgress = false;
                return;
            }

            lastOutcome = outcome;
            isSpinInProgress = true;
        }

        

        

        

       
        private SpinOutcome ResolveNextOutcome()
        {
            if (spinResultGenerator == null)
            {
                Debug.LogError($"[{name}] SpinResultGenerator reference is missing.");
                return null;
            }

            return spinResultGenerator.GenerateOutcome(reels);
        }

        private void CacheLocalReferences()
        {
            if (spinResultGenerator == null)
            {
                spinResultGenerator = GetComponent<SpinResultGenerator>();
            }
        }

        

        

        private void StopAllReels()
        {
            for (int i = 0; i < reels.Count; i++)
            {
                if (reels[i] != null)
                {
                    //reels[i].StopSpin();
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
