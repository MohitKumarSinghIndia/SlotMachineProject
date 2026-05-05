using UnityEngine;

namespace SlotMachine.Reels.Runtime
{
    public class BetManager : MonoBehaviour
    {
        [SerializeField] private int activeLineCount = 20;
        [SerializeField] private float totalBet = 200f;

        public int ActiveLineCount => activeLineCount;
        public float TotalBet => totalBet;

        public float BetPerLine
        {
            get
            {
                if (activeLineCount <= 0)
                {
                    return 0f;
                }

                return totalBet / activeLineCount;
            }
        }

        public void SetTotalBet(float value)
        {
            totalBet = Mathf.Max(0f, value);
        }
    }
}