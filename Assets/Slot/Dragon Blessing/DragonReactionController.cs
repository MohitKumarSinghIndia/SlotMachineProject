using UnityEngine;

namespace SlotMachine.Reels.Runtime
{
    public class DragonReactionController : MonoBehaviour
    {
        [SerializeField] private EventSequencePlayer eventSequencePlayer;

        [Header("Win Thresholds")]
        [SerializeField] private float mediumWinThreshold = 40f;

        [SerializeField] private float bigWinThreshold = 100f;

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
                Debug.Log("Dragon Feature Trigger");

                return;
            }

            // --------------------------------
            // 1-2 SCATTERS
            // --------------------------------

            if (scatterCount >= 1 && scatterCount <= 2)
            {
                eventSequencePlayer.PlaySequenceById(0);

                Debug.Log("Dragon Scatter Look");

                return;
            }

            // --------------------------------
            // WILD NO WIN
            // --------------------------------

            if (hasWild && totalWin <= 0)
            {
                Debug.Log("Dragon Wild Tease");

                return;
            }

            // --------------------------------
            // NO WIN
            // --------------------------------

            if (totalWin <= 0)
            {
                Debug.Log("NO WIN");
                eventSequencePlayer.PlaySequenceById(1);

                return;
            }

            // --------------------------------
            // SMALL WIN
            // --------------------------------

            if (totalWin < mediumWinThreshold)
            {
                Debug.Log("SMALL WIN");

                return;
            }

            // --------------------------------
            // MEDIUM WIN
            // --------------------------------

            if (totalWin < bigWinThreshold)
            {
                Debug.Log("MEDIUM WIN");

                return;
            }

            // --------------------------------
            // BIG WIN
            // --------------------------------


            Debug.Log("BIG WIN");

        }

        // =========================
        // MAX MULTIPLIER
        // =========================

        public void PlayMaxMultiplierReaction()
        {

        }
    }
}