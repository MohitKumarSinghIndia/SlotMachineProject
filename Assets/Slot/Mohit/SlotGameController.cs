using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Mohit
{
    public class SlotGameController :
        MonoBehaviour
    {
        [SerializeField]
        private ReelManager reelManager;

        [ContextMenu("TEST SPIN")]
        public void TestSpin()
        {
            StartCoroutine(
                SpinRoutine()
            );
        }

        private IEnumerator SpinRoutine()
        {
            reelManager.StartSpin();

            yield return new WaitForSeconds(2f);

            SpinOutcome outcome =
                CreateFakeResult();

            reelManager
                .DisplaySpinResult(
                    outcome
                );
        }

        private SpinOutcome
            CreateFakeResult()
        {
            return new SpinOutcome
            {
                SpinId = "SPIN_001",

                Reels =
                    new List<ReelOutcome>()
                    {
                    new ReelOutcome
                    {
                        ReelIndex = 0,

                        VisibleSymbolIds =
                            new List<int>()
                            {
                                8,
                                5,
                                2
                            }
                    },

                    new ReelOutcome
                    {
                        ReelIndex = 1,

                        VisibleSymbolIds =
                            new List<int>()
                            {
                                9,
                                1,
                                6
                            }
                    },

                    new ReelOutcome
                    {
                        ReelIndex = 2,

                        VisibleSymbolIds =
                            new List<int>()
                            {
                                2,
                                1,
                                5
                            }
                    },

                    new ReelOutcome
                    {
                        ReelIndex = 3,

                        VisibleSymbolIds =
                            new List<int>()
                            {
                                3,
                                7,
                                10
                            }
                    },

                    new ReelOutcome
                    {
                        ReelIndex = 4,

                        VisibleSymbolIds =
                            new List<int>()
                            {
                                4,
                                0,
                                8
                            }
                    }
                    }
            };
        }
    }
}