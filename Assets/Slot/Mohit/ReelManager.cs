using UnityEngine;
namespace Mohit
{
    public class ReelManager :MonoBehaviour
    {
        [SerializeField]
        private ReelColumn[] reels;

        public void StartSpin()
        {
            for (int i = 0; i < reels.Length; i++)
            {
                reels[i].SpawnLoopers();
            }
        }

        public void DisplaySpinResult(SpinOutcome outcome)
        {
            for (int i = 0;i < reels.Length;i++)
            {
                reels[i].ClearLoopers();

                reels[i].SetVisibleSymbols(outcome.Reels[i].VisibleSymbolIds);
            }
        }

        public void ClearAll()
        {
            for (int i = 0;i < reels.Length;i++)
            {
                reels[i].ClearSymbols();

                reels[i].ClearLoopers();
            }
        }
    }
}