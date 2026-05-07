using UnityEngine;

namespace SlotMachine.Reels.Runtime
{
    public class DragonReactionController : MonoBehaviour
    {
        [Header("Animator")]
        [SerializeField] private Animator dragonAnimator;

        [Header("Win Thresholds")]
        [SerializeField] private float mediumWinThreshold = 40f;

        [SerializeField] private float bigWinThreshold = 100f;

        // =========================
        // ANIMATION NAMES
        // =========================

        private const string NO_WIN = "NoWin";

        private const string SMALL_WIN = "SmallWin";

        private const string MEDIUM_WIN = "MediumWin";

        private const string BIG_WIN = "BigWin";

        private const string SCATTER_LOOK = "ScatterLook";

        private const string FEATURE_TRIGGER = "FeatureTrigger";

        private const string WILD_TEASE = "WildTease";

        private const string MAX_MULTIPLIER = "MaxMultiplier";

        // =========================
        // SPIN REACTION
        // =========================

        public void ReactToSpin( float totalWin, int scatterCount, bool hasWild, bool featureTriggered)
        {

            Debug.Log("Dragon");

            // --------------------------------
            // 3+ SCATTERS
            // --------------------------------

            if (featureTriggered)
            {
                Play(FEATURE_TRIGGER);

                Debug.Log("Dragon Feature Trigger");

                return;
            }

            // --------------------------------
            // 1-2 SCATTERS
            // --------------------------------

            if (scatterCount >= 1 && scatterCount <= 2)
            {
                Play(SCATTER_LOOK);

                Debug.Log("Dragon Scatter Look");

                return;
            }

            // --------------------------------
            // WILD NO WIN
            // --------------------------------

            if (hasWild && totalWin <= 0)
            {
                Play(WILD_TEASE);

                Debug.Log("Dragon Wild Tease");

                return;
            }

            // --------------------------------
            // NO WIN
            // --------------------------------

            if (totalWin <= 0)
            {
                Play(NO_WIN);

                Debug.Log("NO WIN");

                return;
            }

            // --------------------------------
            // SMALL WIN
            // --------------------------------

            if (totalWin < mediumWinThreshold)
            {
                Play(SMALL_WIN);

                Debug.Log("SMALL WIN");

                return;
            }

            // --------------------------------
            // MEDIUM WIN
            // --------------------------------

            if (totalWin < bigWinThreshold)
            {
                Play(MEDIUM_WIN);

                Debug.Log("MEDIUM WIN");

                return;
            }

            // --------------------------------
            // BIG WIN
            // --------------------------------

            Play(BIG_WIN);

            Debug.Log("BIG WIN");

        }

        // =========================
        // MAX MULTIPLIER
        // =========================

        public void PlayMaxMultiplierReaction()
        {
            Play(MAX_MULTIPLIER);
        }

        // =========================
        // PLAY
        // =========================

        private void Play(string stateName)
        {
            if (dragonAnimator == null)
                return;

            dragonAnimator.Play(stateName);
        }
    }
}