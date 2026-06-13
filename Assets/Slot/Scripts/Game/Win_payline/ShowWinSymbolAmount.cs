using SlotMachine.Reels.Runtime;
using System.Collections.Generic;
using UnityEngine;

public class ShowWinSymbolAmount : MonoBehaviour
{
    [Header("Refrences")]
    [SerializeField] private PaylineEvaluator paylineEvaluator;

    [Header("Symbols")]
    [SerializeField] private List<GameObject> symbols;

}