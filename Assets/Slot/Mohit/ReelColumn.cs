using System.Collections.Generic;
using UnityEngine;

namespace Mohit
{
    public class ReelColumn : MonoBehaviour
    {
        [Header("Rows Container")]
        [SerializeField]
        private RectTransform[] rowPositions;

        private readonly List<SymbolView> activeSymbols = new List<SymbolView>();

        private readonly List<GameObject> activeLoopers = new List<GameObject>();

        // =====================================
        // SYMBOLS
        // =====================================

        public void SetVisibleSymbols(List<int> visibleSymbolIds)
        {
            ClearSymbols();

            for (int i = 0;i < visibleSymbolIds.Count;i++)
            {
                SymbolType type =SymbolMapper.GetSymbolType(visibleSymbolIds[i]);

                SymbolView symbol = PoolManager.Instance.GetSymbol(type);

                SpawnSymbol(symbol,rowPositions[i]);

                activeSymbols.Add(symbol);
            }
        }

        private void SpawnSymbol(SymbolView symbol,RectTransform row)
        {
            if (symbol == null)
            {
                Debug.LogError("SYMBOL NULL");

                return;
            }

            RectTransform rect = symbol.RectTransform;

            rect.SetParent(row,false);

            rect.anchorMin = new Vector2(0.5f, 0.5f);

            rect.anchorMax = new Vector2(0.5f, 0.5f);

            rect.pivot = new Vector2(0.5f, 0.5f);

            rect.localScale = Vector3.one;

            rect.localRotation = Quaternion.identity;

            rect.anchoredPosition = Vector3.zero;

            symbol.gameObject.SetActive(true);
        }

        public void ClearSymbols()
        {
            for (int i = 0;i < activeSymbols.Count;i++)
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

                RectTransform rect = looper.GetComponent<RectTransform>();

                rect.SetParent( rowPositions[i],false);

                rect.anchorMin = new Vector2(0.5f, 0.5f);

                rect.anchorMax = new Vector2(0.5f, 0.5f);

                rect.pivot = new Vector2(0.5f, 0.5f);

                rect.localScale = Vector3.one;

                rect.localRotation = Quaternion.identity;

                rect.anchoredPosition = Vector2.zero;

                looper.SetActive(true);

                activeLoopers.Add(looper);
            }
        }

        public void ClearLoopers()
        {
            for (int i = 0;i < activeLoopers.Count;i++)
            {
                PoolManager.Instance.ReturnLooper(activeLoopers[i]);
            }

            activeLoopers.Clear();
        }
    }
}