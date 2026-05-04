using System.Collections.Generic;
using SlotMachine.Reels.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotMachine.Reels.Runtime
{
    public class SymbolPoolManager : MonoBehaviour
    {
        [Header("Library")]
        [SerializeField] private bool autoCollectDefinitions = true;
        [SerializeField] private List<SymbolDefinition> symbolDefinitions = new List<SymbolDefinition>();

        [Header("Pooling")]
        [SerializeField] private Transform poolRoot;

        private readonly Dictionary<int, SymbolDefinition> _definitionById = new Dictionary<int, SymbolDefinition>();
        private readonly Dictionary<int, Queue<SymbolView>> _poolById = new Dictionary<int, Queue<SymbolView>>();

        private void Awake()
        {
            EnsurePoolRoot();
            CacheDefinitions();
            PrewarmPools();
        }

        private void OnValidate()
        {
            CacheDefinitions();
        }

        public SymbolView Acquire(int symbolId, Transform parent)
        {
            EnsurePoolRoot();
            CacheDefinitions();

            if (!_poolById.TryGetValue(symbolId, out Queue<SymbolView> queue))
            {
                queue = new Queue<SymbolView>();
                _poolById[symbolId] = queue;
            }

            SymbolView instance = null;
            while (queue.Count > 0 && instance == null)
            {
                instance = queue.Dequeue();
            }

            if (instance == null)
            {
                instance = CreatePooledSymbol(symbolId, parent);
            }
            else if (parent != null)
            {
                instance.transform.SetParent(parent, false);
            }

            if (instance != null)
            {
                ApplySymbol(instance, symbolId);
                instance.gameObject.SetActive(true);
            }

            return instance;
        }

        public void Release(SymbolView instance)
        {
            if (instance == null)
            {
                return;
            }

            EnsurePoolRoot();

            int symbolId = instance.CurrentSymbolId;
            if (!_poolById.TryGetValue(symbolId, out Queue<SymbolView> queue))
            {
                queue = new Queue<SymbolView>();
                _poolById[symbolId] = queue;
            }

            instance.gameObject.SetActive(false);
            instance.transform.SetParent(poolRoot, false);
            queue.Enqueue(instance);
        }

        private void CacheDefinitions()
        {
            if (autoCollectDefinitions || symbolDefinitions.Count == 0)
            {
                symbolDefinitions.Clear();
                SymbolDefinition[] foundDefinitions = GetComponentsInChildren<SymbolDefinition>(true);
                for (int i = 0; i < foundDefinitions.Length; i++)
                {
                    SymbolDefinition definition = foundDefinitions[i];
                    if (definition != null && !symbolDefinitions.Contains(definition))
                    {
                        symbolDefinitions.Add(definition);
                    }
                }
            }

            _definitionById.Clear();
            for (int i = 0; i < symbolDefinitions.Count; i++)
            {
                SymbolDefinition definition = symbolDefinitions[i];
                if (definition == null)
                {
                    continue;
                }

                if (!_definitionById.ContainsKey(definition.SymbolId))
                {
                    _definitionById.Add(definition.SymbolId, definition);
                }
            }
        }

        private void PrewarmPools()
        {
            foreach (KeyValuePair<int, SymbolDefinition> pair in _definitionById)
            {
                if (!_poolById.TryGetValue(pair.Key, out Queue<SymbolView> queue))
                {
                    queue = new Queue<SymbolView>();
                    _poolById[pair.Key] = queue;
                }

                int targetCount = pair.Value != null ? pair.Value.InitialPoolCount : 1;
                while (queue.Count < targetCount)
                {
                    SymbolView pooled = CreatePooledSymbol(pair.Key, poolRoot);
                    if (pooled == null)
                    {
                        break;
                    }

                    pooled.gameObject.SetActive(false);
                    queue.Enqueue(pooled);
                }
            }
        }

        private SymbolView CreatePooledSymbol(int symbolId, Transform parent)
        {
            GameObject root = new GameObject($"Symbol_{symbolId}_Pooled", typeof(RectTransform), typeof(CanvasGroup));
            RectTransform rootRect = root.GetComponent<RectTransform>();
            if (parent != null)
            {
                rootRect.SetParent(parent, false);
            }
            else
            {
                rootRect.SetParent(poolRoot, false);
            }

            GameObject backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(Image));
            RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
            backgroundRect.SetParent(rootRect, false);
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            Image backgroundImage = backgroundObject.GetComponent<Image>();

            GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            RectTransform iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.SetParent(rootRect, false);
            iconRect.anchorMin = new Vector2(0.5f, 0.66f);
            iconRect.anchorMax = new Vector2(0.5f, 0.66f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(72f, 72f);
            Image iconImage = iconObject.GetComponent<Image>();
            iconImage.color = new Color(1f, 1f, 1f, 0.18f);
            iconImage.raycastTarget = false;

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.SetParent(rootRect, false);
            labelRect.anchorMin = new Vector2(0f, 0.18f);
            labelRect.anchorMax = new Vector2(1f, 0.48f);
            labelRect.offsetMin = new Vector2(8f, 0f);
            labelRect.offsetMax = new Vector2(-8f, 0f);
            TextMeshProUGUI labelText = labelObject.GetComponent<TextMeshProUGUI>();
            labelText.raycastTarget = false;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.fontSize = 20f;
            labelText.fontStyle = FontStyles.Bold;
            labelText.textWrappingMode = TextWrappingModes.NoWrap;
            labelText.overflowMode = TextOverflowModes.Ellipsis;

            SymbolView symbolView = root.AddComponent<SymbolView>();
            symbolView.ConfigureVisualReferences(iconImage, backgroundImage, labelText);
            ApplySymbol(symbolView, symbolId);
            return symbolView;
        }

        private void ApplySymbol(SymbolView instance, int symbolId)
        {
            if (instance == null)
            {
                return;
            }

            if (_definitionById.TryGetValue(symbolId, out SymbolDefinition definition) && definition != null)
            {
                instance.ApplyDefinition(definition);
                return;
            }

            Debug.LogWarning($"[{name}] No SymbolDefinition found for id {symbolId}. Using fallback symbol styling.");
            instance.ApplySymbolId(symbolId);
        }

        private void EnsurePoolRoot()
        {
            if (poolRoot == null)
            {
                poolRoot = transform;
            }
        }
    }
}
