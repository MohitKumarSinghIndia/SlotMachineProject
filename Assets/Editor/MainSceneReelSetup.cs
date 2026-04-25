using System.Collections.Generic;
using System.IO;
using System.Linq;
using SlotMachine.Reels.Data;
using SlotMachine.Reels.Runtime;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MainSceneReelSetup
{
    private const string ScenePath = "Assets/Scenes/Main.unity";
    private const string DataFolder = "Assets/Slot/Generated/Reels";

    private static readonly SymbolSeed[] SymbolSeeds =
    {
        new(1, "WishingDragon", "Wishing Dragon", "WILD", new Color32(185, 43, 41, 255), new Color32(255, 236, 188, 255)),
        new(2, "DragonPearl", "Dragon Pearl", "SCAT", new Color32(78, 55, 156, 255), new Color32(250, 238, 200, 255)),
        new(3, "PotOfGold", "Pot of Gold", "POT", new Color32(205, 137, 30, 255), new Color32(255, 244, 205, 255)),
        new(4, "Lantern", "Lantern", "LANT", new Color32(188, 54, 36, 255), new Color32(255, 235, 196, 255)),
        new(5, "Drum", "Drum", "DRUM", new Color32(161, 39, 43, 255), new Color32(250, 220, 171, 255)),
        new(6, "TeaPot", "Tea Pot", "TEA", new Color32(89, 135, 109, 255), new Color32(248, 241, 225, 255)),
        new(7, "CoinKnot", "Coin Knot", "KNOT", new Color32(173, 64, 48, 255), new Color32(246, 226, 176, 255)),
        new(8, "Rocket", "Rocket", "ROCK", new Color32(215, 95, 42, 255), new Color32(255, 239, 201, 255)),
        new(9, "RubyGem", "Ruby Gem", "RUBY", new Color32(145, 28, 44, 255), new Color32(255, 231, 218, 255)),
        new(10, "JadeTile", "Jade Tile", "JADE", new Color32(68, 131, 96, 255), new Color32(234, 248, 225, 255)),
        new(11, "FortuneCard", "Fortune Card", "CARD", new Color32(170, 44, 42, 255), new Color32(250, 227, 186, 255)),
        new(12, "GoldStar", "Gold Star", "STAR", new Color32(196, 148, 37, 255), new Color32(255, 243, 205, 255)),
        new(13, "Blossom", "Blossom", "BLOOM", new Color32(214, 118, 155, 255), new Color32(255, 240, 244, 255))
    };

    private static readonly int[][] ReelSeeds =
    {
        new[] { 13, 12, 11, 9, 13, 10, 8, 13, 3, 7, 13, 4, 6, 13, 5, 1, 13, 9, 12, 13, 2, 10, 13, 8, 11, 13, 3, 12, 13, 4 },
        new[] { 13, 11, 9, 13, 10, 8, 13, 4, 7, 13, 6, 12, 13, 5, 9, 13, 3, 1, 13, 10, 12, 13, 2, 8, 11, 13, 4, 6, 13, 3 },
        new[] { 12, 13, 9, 11, 13, 10, 8, 13, 5, 7, 13, 4, 6, 13, 3, 12, 13, 1, 10, 13, 9, 2, 13, 8, 11, 13, 5, 12, 13, 4 },
        new[] { 13, 10, 12, 13, 9, 11, 13, 8, 4, 13, 7, 6, 13, 5, 3, 13, 12, 9, 13, 1, 10, 13, 2, 11, 13, 8, 4, 13, 6, 3 },
        new[] { 11, 13, 9, 12, 13, 10, 8, 13, 3, 6, 13, 4, 7, 13, 5, 12, 13, 9, 1, 13, 10, 11, 13, 2, 8, 13, 4, 6, 13, 3 }
    };

    [MenuItem("Tools/Slot/Setup Main Scene Reel System")]
    public static void SetupMainSceneReelSystem()
    {
        EnsureFolder(DataFolder);

        List<SymbolDefinition> symbols = CreateOrUpdateSymbolAssets();
        List<ReelStripDefinition> strips = CreateOrUpdateReelStrips();

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        EnsureEventSystem();

        GameObject systemRoot = FindOrCreateRoot("ReelSystem");
        SymbolManager symbolManager = GetOrAddComponent<SymbolManager>(FindOrCreateChild(systemRoot.transform, "SymbolManager"));
        AssignSymbols(symbolManager, symbols);

        ReelManager reelManager = GetOrAddComponent<ReelManager>(FindOrCreateChild(systemRoot.transform, "ReelManager"));
        AssignReelManager(symbolManager, reelManager);

        List<ReelController> sceneReels = new List<ReelController>();
        for (int reelNumber = 1; reelNumber <= 5; reelNumber++)
        {
            GameObject reelObject = FindSceneObject($"Reel_{reelNumber}");
            if (reelObject == null)
            {
                continue;
            }

            ReelController reel = GetOrAddComponent<ReelController>(reelObject);
            SetupReel(reel, reelNumber - 1, symbolManager, strips[reelNumber - 1]);
            sceneReels.Add(reel);
        }

        AssignReels(reelManager, sceneReels);
        HookSpinButton(reelManager);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeObject = reelManager.gameObject;
    }

    private static List<SymbolDefinition> CreateOrUpdateSymbolAssets()
    {
        List<SymbolDefinition> result = new List<SymbolDefinition>();
        for (int i = 0; i < SymbolSeeds.Length; i++)
        {
            SymbolSeed seed = SymbolSeeds[i];
            string path = $"{DataFolder}/{seed.AssetName}.asset";
            SymbolDefinition asset = AssetDatabase.LoadAssetAtPath<SymbolDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<SymbolDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            SerializedObject serialized = new SerializedObject(asset);
            serialized.FindProperty("symbolId").intValue = seed.Id;
            serialized.FindProperty("symbolName").stringValue = seed.DisplayName;
            serialized.FindProperty("shortCode").stringValue = seed.ShortCode;
            serialized.FindProperty("backgroundColor").colorValue = seed.BackgroundColor;
            serialized.FindProperty("labelColor").colorValue = seed.LabelColor;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            result.Add(asset);
        }

        AssetDatabase.SaveAssets();
        return result;
    }

    private static List<ReelStripDefinition> CreateOrUpdateReelStrips()
    {
        List<ReelStripDefinition> result = new List<ReelStripDefinition>();
        for (int i = 0; i < ReelSeeds.Length; i++)
        {
            string path = $"{DataFolder}/ReelStrip_{i + 1}.asset";
            ReelStripDefinition asset = AssetDatabase.LoadAssetAtPath<ReelStripDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<ReelStripDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            SerializedObject serialized = new SerializedObject(asset);
            SerializedProperty symbols = serialized.FindProperty("symbolIds");
            symbols.arraySize = ReelSeeds[i].Length;
            for (int index = 0; index < ReelSeeds[i].Length; index++)
            {
                symbols.GetArrayElementAtIndex(index).intValue = ReelSeeds[i][index];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            result.Add(asset);
        }

        AssetDatabase.SaveAssets();
        return result;
    }

    private static void SetupReel(ReelController reel, int reelIndex, SymbolManager symbolManager, ReelStripDefinition strip)
    {
        SerializedObject serialized = new SerializedObject(reel);
        serialized.FindProperty("reelIndex").intValue = reelIndex;
        serialized.FindProperty("reelStrip").objectReferenceValue = strip;
        serialized.FindProperty("symbolManager").objectReferenceValue = symbolManager;

        GameObject stripRootObject = FindChildByName(reel.transform, "StripRoot");
        if (stripRootObject != null)
        {
            RectTransform stripRoot = stripRootObject.GetComponent<RectTransform>();
            serialized.FindProperty("stripRoot").objectReferenceValue = stripRoot;

            List<SymbolView> symbolViews = stripRootObject.GetComponentsInChildren<RectTransform>(true)
                .Where(rect => rect.name.StartsWith("Symbol_"))
                .OrderBy(rect => rect.name)
                .Select(rect => SetupSymbolView(rect.gameObject))
                .ToList();

            AssignObjectList(serialized.FindProperty("symbolViews"), symbolViews);
            int topBufferRows = Mathf.Max(0, (symbolViews.Count - 3) / 2);
            serialized.FindProperty("topBufferRows").intValue = topBufferRows;

            float symbolHeight = InferSymbolHeight(reel.transform as RectTransform, symbolViews);
            serialized.FindProperty("symbolHeight").floatValue = symbolHeight;

            LayoutSymbols(symbolViews, topBufferRows, symbolHeight);
        }

        serialized.FindProperty("visibleRowCount").intValue = 3;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(reel);

        reel.RefreshImmediate(0);
    }

    private static SymbolView SetupSymbolView(GameObject symbolObject)
    {
        SymbolView view = GetOrAddComponent<SymbolView>(symbolObject);
        SerializedObject serialized = new SerializedObject(view);
        serialized.FindProperty("backgroundImage").objectReferenceValue = symbolObject.GetComponent<Image>();
        serialized.FindProperty("iconImage").objectReferenceValue = FindPreferredIcon(symbolObject.transform);
        serialized.FindProperty("labelText").objectReferenceValue = FindPreferredLabel(symbolObject.transform);
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(view);
        return view;
    }

    private static void AssignSymbols(SymbolManager symbolManager, List<SymbolDefinition> symbols)
    {
        SerializedObject serialized = new SerializedObject(symbolManager);
        AssignObjectList(serialized.FindProperty("symbols"), symbols);
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(symbolManager);
    }

    private static void AssignReelManager(SymbolManager symbolManager, ReelManager reelManager)
    {
        SerializedObject serialized = new SerializedObject(reelManager);
        serialized.FindProperty("symbolManager").objectReferenceValue = symbolManager;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(reelManager);
    }

    private static void AssignReels(ReelManager reelManager, List<ReelController> reels)
    {
        SerializedObject serialized = new SerializedObject(reelManager);
        AssignObjectList(serialized.FindProperty("reels"), reels);
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(reelManager);
    }

    private static void HookSpinButton(ReelManager reelManager)
    {
        GameObject spinObject = FindSceneObject("SpinButton");
        if (spinObject == null)
        {
            return;
        }

        Button button = GetOrAddComponent<Button>(spinObject);
        button.onClick = new Button.ButtonClickedEvent();
        UnityEventTools.AddPersistentListener(button.onClick, reelManager.SpinAll);
        EditorUtility.SetDirty(button);
    }

    private static Image FindPreferredIcon(Transform root)
    {
        return root.GetComponentsInChildren<Image>(true)
            .FirstOrDefault(image => image.gameObject != root.gameObject && image.name.IndexOf("icon", System.StringComparison.OrdinalIgnoreCase) >= 0);
    }

    private static TMP_Text FindPreferredLabel(Transform root)
    {
        TMP_Text preferred = root.GetComponentsInChildren<TMP_Text>(true)
            .FirstOrDefault(text =>
                text.name.IndexOf("code", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.name.IndexOf("label", System.StringComparison.OrdinalIgnoreCase) >= 0);

        if (preferred != null)
        {
            return preferred;
        }

        return root.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault();
    }

    private static float InferSymbolHeight(RectTransform reelRect, List<SymbolView> symbolViews)
    {
        if (reelRect != null && reelRect.rect.height > 0f)
        {
            return Mathf.Max(1f, reelRect.rect.height / 3f);
        }

        if (symbolViews.Count < 2)
        {
            RectTransform firstRect = symbolViews.Count == 1 ? symbolViews[0].GetComponent<RectTransform>() : null;
            return firstRect != null ? Mathf.Max(1f, firstRect.rect.height) : 164f;
        }

        RectTransform a = symbolViews[0].GetComponent<RectTransform>();
        RectTransform b = symbolViews[1].GetComponent<RectTransform>();
        if (a == null || b == null)
        {
            return 164f;
        }

        float inferred = Mathf.Abs(a.anchoredPosition.y - b.anchoredPosition.y);
        return inferred > 1f ? inferred : 164f;
    }

    private static void LayoutSymbols(List<SymbolView> symbolViews, int topBufferRows, float symbolHeight)
    {
        for (int i = 0; i < symbolViews.Count; i++)
        {
            if (symbolViews[i] == null)
            {
                continue;
            }

            RectTransform rect = symbolViews[i].GetComponent<RectTransform>();
            if (rect == null)
            {
                continue;
            }

            rect.anchoredPosition = new Vector2(0f, (topBufferRows + 1 - i) * symbolHeight);
            EditorUtility.SetDirty(rect);
        }
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystem = new GameObject("EventSystem", typeof(RectTransform), typeof(EventSystem), typeof(StandaloneInputModule));
        Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
    }

    private static GameObject FindSceneObject(string objectName)
    {
        Transform[] allTransforms = Object.FindObjectsOfType<Transform>(true);
        for (int i = 0; i < allTransforms.Length; i++)
        {
            if (allTransforms[i].name == objectName)
            {
                return allTransforms[i].gameObject;
            }
        }

        return null;
    }

    private static GameObject FindOrCreateRoot(string objectName)
    {
        GameObject existing = FindSceneObject(objectName);
        if (existing != null)
        {
            return existing;
        }

        return new GameObject(objectName);
    }

    private static GameObject FindOrCreateChild(Transform parent, string objectName)
    {
        GameObject existing = FindChildByName(parent, objectName);
        if (existing != null)
        {
            return existing;
        }

        GameObject created = new GameObject(objectName);
        created.transform.SetParent(parent, false);
        return created;
    }

    private static GameObject FindChildByName(Transform parent, string objectName)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == objectName)
            {
                return child.gameObject;
            }
        }

        return null;
    }

    private static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        return component != null ? component : Undo.AddComponent<T>(gameObject);
    }

    private static void AssignObjectList<T>(SerializedProperty listProperty, IList<T> objects) where T : Object
    {
        listProperty.arraySize = objects.Count;
        for (int i = 0; i < objects.Count; i++)
        {
            listProperty.GetArrayElementAtIndex(i).objectReferenceValue = objects[i];
        }
    }

    private static void EnsureFolder(string folderPath)
    {
        string[] parts = folderPath.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }

    private readonly struct SymbolSeed
    {
        public SymbolSeed(int id, string assetName, string displayName, string shortCode, Color backgroundColor, Color labelColor)
        {
            Id = id;
            AssetName = assetName;
            DisplayName = displayName;
            ShortCode = shortCode;
            BackgroundColor = backgroundColor;
            LabelColor = labelColor;
        }

        public int Id { get; }
        public string AssetName { get; }
        public string DisplayName { get; }
        public string ShortCode { get; }
        public Color BackgroundColor { get; }
        public Color LabelColor { get; }
    }
}
