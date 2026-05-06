using System.Collections.Generic;
using UnityEngine;

namespace Mohit
{
    public class ReelColumn : MonoBehaviour
    {
        [Header("Rows Container")]
        [SerializeField]
        private Transform[] rowPositions;

        private readonly List<SymbolView> activeSymbols = new List<SymbolView>();

        private readonly List<GameObject> activeLoopers = new List<GameObject>();

        // =====================================
        // SYMBOLS
        // =====================================

        public void SetVisibleSymbols(List<int> visibleSymbolIds)
        {
            ClearSymbols();

            for (int i = 0; i < visibleSymbolIds.Count; i++)
            {
                SymbolType type = SymbolMapper.GetSymbolType(visibleSymbolIds[i]);

                SymbolView symbol = PoolManager.Instance.GetSymbol(type);

                SpawnSymbol(symbol, rowPositions[i]);

                activeSymbols.Add(symbol);
            }
        }

        private void SpawnSymbol(SymbolView symbol, Transform row)
        {
            if (symbol == null)
            {
                Debug.LogError("SYMBOL NULL");

                return;
            }

            Transform t = symbol.CachedTransform;

            t.SetParent(row, false);

            t.localScale = Vector3.one;

            t.localRotation = Quaternion.identity;

            t.localPosition = Vector3.zero;

            symbol.gameObject.SetActive(true);
        }

        public void ClearSymbols()
        {
            for (int i = 0; i < activeSymbols.Count; i++)
            {
                PoolManager.Instance.ReturnSymbol(activeSymbols[i]);
            }

            activeSymbols.Clear();
        }

        // =====================================
        // LOOPERS
        // =====================================

        public void SpawnLoopers()
        {
            ClearSymbols();
            ClearLoopers();

            for (int i = 0; i < rowPositions.Length; i++)
            {
                GameObject looper = PoolManager.Instance.GetLooper();

                Transform t = looper.transform;

                t.SetParent(rowPositions[i], false);

                t.localScale = Vector3.one;

                t.localRotation = Quaternion.identity;

                t.localPosition = Vector3.zero;

                looper.SetActive(true);

                activeLoopers.Add(looper);
            }
        }

        public void ClearLoopers()
        {
            for (int i = 0; i < activeLoopers.Count; i++)
            {
                PoolManager.Instance.ReturnLooper(activeLoopers[i]);
            }

            activeLoopers.Clear();
        }
    }
}