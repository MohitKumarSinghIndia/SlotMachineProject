using System;
using System.Collections.Generic;
using DG.Tweening;
using SlotMachine.Reels.Data;
using UnityEngine;

namespace SlotMachine.Reels.Runtime
{
   
    public class ReelController : MonoBehaviour
    {
        [Header("Reel Info")]
        public int ReelIndex;
        public ReelStripDefinition ReelStrip;
    }
}
