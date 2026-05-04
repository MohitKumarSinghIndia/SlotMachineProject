#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SpriteWorldUIBuilder
{
    private const string RootName = "SpriteWorldInterface";
    private const float SquareWorldSize = 5.12f;
    private const float CircleWorldSize = 5.12f;

    private static Sprite squareSprite;
    private static Sprite circleSprite;
    private static Material uiMaterial;

    [MenuItem("Tools/Slot Machine/Create Sprite World UI")]
    public static void Build()
    {
        squareSprite = LoadSprite("Assets/Sprites/Square.png");
        circleSprite = LoadSprite("Assets/Sprites/Circle.png");
        uiMaterial = LoadMaterial();

        if (squareSprite == null || circleSprite == null || uiMaterial == null)
        {
            Debug.LogError("SpriteWorldUIBuilder needs sprite assets and a usable sprite material.");
            return;
        }

        DestroyIfFound(RootName);
        DestroyIfFound("SpriteUI_TestSquare_ToDelete");
        ResetDefaultSpriteMaterialTint();

        var legacyFrame = GameObject.Find("Background/GameFrame");
        if (legacyFrame != null)
        {
            legacyFrame.SetActive(false);
        }

        var legacyBackground = GameObject.Find("Background");
        if (legacyBackground != null)
        {
            legacyBackground.SetActive(false);
        }

        var root = NewGroup(RootName, (Transform)null, new Vector3(0f, 0f, 0.02f));
        root.layer = LayerMask.NameToLayer("UI") >= 0 ? LayerMask.NameToLayer("UI") : 0;

        BuildBackdrop(root.transform);
        BuildHeader(root.transform);
        BuildReels(root.transform);
        BuildControls(root.transform);
        BuildSideRails(root.transform);

        var cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(0f, 0f, -5f);
            cam.transform.rotation = Quaternion.identity;
        }

        EditorUtility.SetDirty(root);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = root;
        Debug.Log("Created SpriteWorldInterface using SpriteRenderers only. The legacy RectTransform GameFrame was disabled, not deleted.");
    }

    private static void BuildBackdrop(Transform root)
    {
        var backdrop = NewGroup("FullScreen_Backdrop", root, Vector3.zero);
        AddRect(backdrop, "ScreenTint", new Vector2(0f, 0f), new Vector2(8.9f, 5.05f), new Color(0.03f, 0.06f, 0.10f, 1f), 0);
        AddRect(backdrop, "OuterFrame", new Vector2(0f, 0f), new Vector2(8.25f, 4.62f), new Color(0.12f, 0.19f, 0.28f, 1f), 1);
        AddRect(backdrop, "InnerFrame", new Vector2(0f, 0.02f), new Vector2(7.95f, 4.34f), new Color(0.04f, 0.08f, 0.13f, 1f), 2);
        AddRect(backdrop, "TopHighlight", new Vector2(0f, 2.14f), new Vector2(7.82f, 0.08f), new Color(0.30f, 0.72f, 0.92f, 1f), 3);
        AddRect(backdrop, "BottomShadow", new Vector2(0f, -2.10f), new Vector2(7.82f, 0.08f), new Color(0.01f, 0.02f, 0.04f, 1f), 3);
    }

    private static void BuildHeader(Transform root)
    {
        var header = NewGroup("HeaderBar_SpriteLayout", root, new Vector3(0f, 1.88f, 0f));
        AddCapsule(header, "BalancePill", new Vector2(-2.65f, 0f), new Vector2(2.05f, 0.46f), new Color(0.09f, 0.15f, 0.24f, 1f), 10);
        AddCapsule(header, "WinPill", new Vector2(0f, 0f), new Vector2(1.75f, 0.46f), new Color(0.10f, 0.20f, 0.18f, 1f), 10);
        AddCapsule(header, "JackpotPill", new Vector2(2.45f, 0f), new Vector2(2.35f, 0.46f), new Color(0.25f, 0.10f, 0.17f, 1f), 10);

        AddFauxText(header, "BalanceGlyphs", new Vector2(-3.28f, 0.01f), 7, 0.10f, new Color(0.55f, 0.80f, 1f, 1f), 16);
        AddSevenSegmentNumber(header, "BalanceDigits_12850", "12850", new Vector2(-2.55f, -0.02f), 0.115f, new Color(0.98f, 0.86f, 0.36f, 1f), 17);
        AddFauxText(header, "WinGlyphs", new Vector2(-0.42f, 0.01f), 4, 0.10f, new Color(0.58f, 1f, 0.78f, 1f), 16);
        AddSevenSegmentNumber(header, "WinDigits_0240", "0240", new Vector2(0.18f, -0.02f), 0.115f, new Color(0.74f, 1f, 0.62f, 1f), 17);
        AddFauxText(header, "JackpotGlyphs", new Vector2(1.72f, 0.01f), 8, 0.10f, new Color(1f, 0.58f, 0.78f, 1f), 16);
        AddSevenSegmentNumber(header, "JackpotDigits_777", "777", new Vector2(2.74f, -0.02f), 0.125f, new Color(1f, 0.72f, 0.22f, 1f), 17);
    }

    private static void BuildReels(Transform root)
    {
        var reels = NewGroup("Reels_RectLikeSpritePanel", root, new Vector3(0f, 0.28f, 0f));
        AddRect(reels, "PanelDropShadow", new Vector2(0.05f, -0.08f), new Vector2(6.9f, 2.72f), new Color(0f, 0f, 0f, 0.70f), 8);
        AddRect(reels, "PanelBody", new Vector2(0f, 0f), new Vector2(6.9f, 2.72f), new Color(0.08f, 0.12f, 0.18f, 1f), 9);
        AddRect(reels, "PanelInset", new Vector2(0f, 0f), new Vector2(6.55f, 2.36f), new Color(0.02f, 0.04f, 0.08f, 1f), 10);
        AddRect(reels, "WinLine_Middle", new Vector2(0f, 0f), new Vector2(6.44f, 0.10f), new Color(0.95f, 0.78f, 0.18f, 0.85f), 35);

        var symbolColors = new[]
        {
            new Color(0.87f, 0.24f, 0.37f, 1f),
            new Color(0.24f, 0.68f, 0.93f, 1f),
            new Color(0.95f, 0.76f, 0.20f, 1f),
            new Color(0.52f, 0.90f, 0.45f, 1f),
            new Color(0.78f, 0.49f, 1f, 1f)
        };

        for (int reelIndex = 0; reelIndex < 5; reelIndex++)
        {
            float x = -2.56f + reelIndex * 1.28f;
            var reel = NewGroup($"Reel_{reelIndex + 1:00}", reels, new Vector3(x, 0f, 0f));
            AddRect(reel, "ReelMaskLikeColumn", Vector2.zero, new Vector2(1.08f, 2.12f), new Color(0.13f, 0.17f, 0.24f, 1f), 12);
            AddRect(reel, "ReelGloss", new Vector2(0f, 0.76f), new Vector2(0.92f, 0.12f), new Color(1f, 1f, 1f, 0.16f), 25);

            for (int row = 0; row < 3; row++)
            {
                float y = 0.70f - row * 0.70f;
                var cell = NewGroup($"Cell_{row + 1:00}", reel, new Vector3(0f, y, 0f));
                AddRect(cell, "CellBack", Vector2.zero, new Vector2(0.82f, 0.56f), new Color(0.91f, 0.95f, 1f, 1f), 18);
                AddRect(cell, "CellInner", Vector2.zero, new Vector2(0.70f, 0.44f), new Color(0.15f, 0.20f, 0.29f, 1f), 19);
                var color = symbolColors[(reelIndex + row) % symbolColors.Length];
                if ((reelIndex + row) % 2 == 0)
                {
                    AddCircle(cell, "SymbolOrb", Vector2.zero, new Vector2(0.34f, 0.34f), color, 24);
                    AddRect(cell, "SymbolSpark", new Vector2(0.13f, 0.13f), new Vector2(0.08f, 0.08f), Color.white, 26);
                }
                else
                {
                    AddDiamond(cell, "SymbolDiamond", Vector2.zero, new Vector2(0.34f, 0.34f), color, 24);
                    AddRect(cell, "SymbolCore", Vector2.zero, new Vector2(0.13f, 0.13f), Color.white, 26);
                }
            }
        }
    }

    private static void BuildControls(Transform root)
    {
        var controls = NewGroup("ControlsBar_SpriteButtons", root, new Vector3(0f, -1.82f, 0f));
        AddRect(controls, "ControlsBack", Vector2.zero, new Vector2(7.42f, 0.72f), new Color(0.07f, 0.11f, 0.17f, 1f), 10);
        AddCapsule(controls, "BetMinusButton", new Vector2(-2.95f, 0f), new Vector2(0.72f, 0.50f), new Color(0.18f, 0.27f, 0.38f, 1f), 14);
        AddCapsule(controls, "BetPlusButton", new Vector2(-1.98f, 0f), new Vector2(0.72f, 0.50f), new Color(0.18f, 0.27f, 0.38f, 1f), 14);
        AddCapsule(controls, "AutoButton", new Vector2(1.92f, 0f), new Vector2(0.92f, 0.50f), new Color(0.15f, 0.25f, 0.30f, 1f), 14);
        AddCapsule(controls, "MaxButton", new Vector2(3.02f, 0f), new Vector2(0.92f, 0.50f), new Color(0.24f, 0.19f, 0.31f, 1f), 14);
        AddCircle(controls, "SpinButton", new Vector2(0f, 0f), new Vector2(0.88f, 0.88f), new Color(0.95f, 0.62f, 0.13f, 1f), 16);
        AddCircle(controls, "SpinButtonInner", new Vector2(0f, 0f), new Vector2(0.66f, 0.66f), new Color(0.99f, 0.82f, 0.28f, 1f), 17);
        AddPlayIcon(controls, "SpinPlayIcon", new Vector2(0.04f, 0f), new Color(0.17f, 0.09f, 0.03f, 1f), 24);

        AddRect(controls, "MinusIcon", new Vector2(-2.95f, 0f), new Vector2(0.28f, 0.06f), Color.white, 24);
        AddRect(controls, "PlusIconH", new Vector2(-1.98f, 0f), new Vector2(0.28f, 0.06f), Color.white, 24);
        AddRect(controls, "PlusIconV", new Vector2(-1.98f, 0f), new Vector2(0.06f, 0.28f), Color.white, 24);
        AddFauxText(controls, "AutoGlyphs", new Vector2(1.66f, 0f), 4, 0.09f, Color.white, 24);
        AddFauxText(controls, "MaxGlyphs", new Vector2(2.78f, 0f), 3, 0.09f, Color.white, 24);
        AddSevenSegmentNumber(controls, "BetDigits_50", "50", new Vector2(-2.56f, -0.02f), 0.105f, new Color(0.80f, 0.94f, 1f, 1f), 24);
    }

    private static void BuildSideRails(Transform root)
    {
        var left = NewGroup("Left_StatusRail_Sprites", root, new Vector3(-3.92f, 0.02f, 0f));
        var right = NewGroup("Right_PaylineRail_Sprites", root, new Vector3(3.92f, 0.02f, 0f));

        AddRect(left, "RailBack", Vector2.zero, new Vector2(0.44f, 2.74f), new Color(0.06f, 0.10f, 0.16f, 1f), 9);
        AddRect(right, "RailBack", Vector2.zero, new Vector2(0.44f, 2.74f), new Color(0.06f, 0.10f, 0.16f, 1f), 9);

        for (int i = 0; i < 5; i++)
        {
            float y = 0.95f - i * 0.45f;
            AddCircle(left, $"StatusDot_{i + 1:00}", new Vector2(0f, y), new Vector2(0.18f, 0.18f), i == 2 ? new Color(0.96f, 0.78f, 0.18f, 1f) : new Color(0.28f, 0.42f, 0.56f, 1f), 18);
            AddRect(right, $"PaylineTick_{i + 1:00}", new Vector2(0f, y), new Vector2(0.22f, 0.07f), i == 2 ? new Color(0.96f, 0.78f, 0.18f, 1f) : new Color(0.32f, 0.45f, 0.58f, 1f), 18);
        }
    }

    private static GameObject NewGroup(string name, Transform parent, Vector3 localPosition)
    {
        var go = new GameObject(name);
        if (parent != null)
        {
            go.transform.SetParent(parent, false);
        }
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        return go;
    }

    private static GameObject NewGroup(string name, GameObject parent, Vector3 localPosition)
    {
        return NewGroup(name, parent != null ? parent.transform : null, localPosition);
    }

    private static SpriteRenderer AddRect(Transform parent, string name, Vector2 localPosition, Vector2 size, Color color, int order)
    {
        return AddSprite(parent, name, squareSprite, localPosition, size, color, order, 0f);
    }

    private static SpriteRenderer AddRect(GameObject parent, string name, Vector2 localPosition, Vector2 size, Color color, int order)
    {
        return AddRect(parent.transform, name, localPosition, size, color, order);
    }

    private static SpriteRenderer AddCircle(Transform parent, string name, Vector2 localPosition, Vector2 size, Color color, int order)
    {
        return AddSprite(parent, name, circleSprite, localPosition, size, color, order, 0f);
    }

    private static SpriteRenderer AddCircle(GameObject parent, string name, Vector2 localPosition, Vector2 size, Color color, int order)
    {
        return AddCircle(parent.transform, name, localPosition, size, color, order);
    }

    private static SpriteRenderer AddDiamond(Transform parent, string name, Vector2 localPosition, Vector2 size, Color color, int order)
    {
        return AddSprite(parent, name, squareSprite, localPosition, size, color, order, 45f);
    }

    private static SpriteRenderer AddDiamond(GameObject parent, string name, Vector2 localPosition, Vector2 size, Color color, int order)
    {
        return AddDiamond(parent.transform, name, localPosition, size, color, order);
    }

    private static SpriteRenderer AddSprite(Transform parent, string name, Sprite sprite, Vector2 localPosition, Vector2 size, Color color, int order, float zRotation)
    {
        var go = NewGroup(name, parent, new Vector3(localPosition.x, localPosition.y, 0f));
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sharedMaterial = uiMaterial;
        sr.color = color;
        sr.sortingOrder = order;
        sr.sortingLayerName = "Default";
        var baseSize = sprite == circleSprite ? CircleWorldSize : SquareWorldSize;
        go.transform.localScale = new Vector3(size.x / baseSize, size.y / baseSize, 1f);
        go.transform.localRotation = Quaternion.Euler(0f, 0f, zRotation);
        return sr;
    }

    private static void AddCapsule(Transform parent, string name, Vector2 localPosition, Vector2 size, Color color, int order)
    {
        var group = NewGroup(name, parent, new Vector3(localPosition.x, localPosition.y, 0f));
        float cap = size.y;
        AddRect(group.transform, "Center", Vector2.zero, new Vector2(Mathf.Max(0.01f, size.x - cap), size.y), color, order);
        AddCircle(group.transform, "LeftCap", new Vector2(-(size.x - cap) * 0.5f, 0f), new Vector2(cap, cap), color, order);
        AddCircle(group.transform, "RightCap", new Vector2((size.x - cap) * 0.5f, 0f), new Vector2(cap, cap), color, order);
    }

    private static void AddCapsule(GameObject parent, string name, Vector2 localPosition, Vector2 size, Color color, int order)
    {
        AddCapsule(parent.transform, name, localPosition, size, color, order);
    }

    private static void AddPlayIcon(Transform parent, string name, Vector2 localPosition, Color color, int order)
    {
        var group = NewGroup(name, parent, new Vector3(localPosition.x, localPosition.y, 0f));
        AddRect(group.transform, "PlayBarTop", new Vector2(-0.03f, 0.09f), new Vector2(0.27f, 0.09f), color, order).transform.localRotation = Quaternion.Euler(0f, 0f, -28f);
        AddRect(group.transform, "PlayBarBottom", new Vector2(-0.03f, -0.09f), new Vector2(0.27f, 0.09f), color, order).transform.localRotation = Quaternion.Euler(0f, 0f, 28f);
        AddRect(group.transform, "PlayBarBack", new Vector2(-0.13f, 0f), new Vector2(0.09f, 0.26f), color, order);
    }

    private static void AddPlayIcon(GameObject parent, string name, Vector2 localPosition, Color color, int order)
    {
        AddPlayIcon(parent.transform, name, localPosition, color, order);
    }

    private static void AddFauxText(Transform parent, string name, Vector2 localPosition, int glyphCount, float scale, Color color, int order)
    {
        var group = NewGroup(name, parent, new Vector3(localPosition.x, localPosition.y, 0f));
        for (int i = 0; i < glyphCount; i++)
        {
            float x = i * scale * 1.15f;
            AddRect(group.transform, $"Glyph_{i + 1:00}_Top", new Vector2(x, scale * 0.55f), new Vector2(scale * 0.72f, scale * 0.16f), color, order);
            AddRect(group.transform, $"Glyph_{i + 1:00}_Mid", new Vector2(x + scale * 0.08f, 0f), new Vector2(scale * 0.56f, scale * 0.13f), color, order);
            if (i % 2 == 0)
            {
                AddRect(group.transform, $"Glyph_{i + 1:00}_Side", new Vector2(x - scale * 0.33f, -scale * 0.28f), new Vector2(scale * 0.14f, scale * 0.56f), color, order);
            }
        }
    }

    private static void AddFauxText(GameObject parent, string name, Vector2 localPosition, int glyphCount, float scale, Color color, int order)
    {
        AddFauxText(parent.transform, name, localPosition, glyphCount, scale, color, order);
    }

    private static void AddSevenSegmentNumber(Transform parent, string name, string text, Vector2 localPosition, float scale, Color color, int order)
    {
        var group = NewGroup(name, parent, new Vector3(localPosition.x, localPosition.y, 0f));
        for (int i = 0; i < text.Length; i++)
        {
            AddDigit(group.transform, text[i], new Vector2(i * scale * 0.78f, 0f), scale, color, order);
        }
    }

    private static void AddSevenSegmentNumber(GameObject parent, string name, string text, Vector2 localPosition, float scale, Color color, int order)
    {
        AddSevenSegmentNumber(parent.transform, name, text, localPosition, scale, color, order);
    }

    private static void AddDigit(Transform parent, char digit, Vector2 pos, float scale, Color color, int order)
    {
        var segments = new Dictionary<char, string>
        {
            {'0', "abcfed"}, {'1', "bc"}, {'2', "abged"}, {'3', "abgcd"}, {'4', "fgbc"},
            {'5', "afgcd"}, {'6', "afgecd"}, {'7', "abc"}, {'8', "abcdefg"}, {'9', "abfgcd"}
        };
        if (!segments.TryGetValue(digit, out var active)) return;

        var group = NewGroup($"Digit_{digit}", parent, new Vector3(pos.x, pos.y, 0f));
        void H(string id, float y) => AddRect(group.transform, id, new Vector2(0f, y * scale), new Vector2(0.42f * scale, 0.08f * scale), color, order);
        void V(string id, float x, float y) => AddRect(group.transform, id, new Vector2(x * scale, y * scale), new Vector2(0.08f * scale, 0.36f * scale), color, order);
        if (active.Contains("a")) H("SegA", 1.00f);
        if (active.Contains("g")) H("SegG", 0.00f);
        if (active.Contains("d")) H("SegD", -1.00f);
        if (active.Contains("f")) V("SegF", -0.25f, 0.50f);
        if (active.Contains("b")) V("SegB", 0.25f, 0.50f);
        if (active.Contains("e")) V("SegE", -0.25f, -0.50f);
        if (active.Contains("c")) V("SegC", 0.25f, -0.50f);
    }

    private static Sprite LoadSprite(string path)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite != null) return sprite;

        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return null;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Material LoadMaterial()
    {
        const string folder = "Assets/Sprites/WorldUI";
        const string path = folder + "/SpriteWorldUI_Unlit.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null)
        {
            SetMaterialWhite(material);
            return material;
        }

        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder("Assets/Sprites", "WorldUI");
        }

        var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        if (shader == null)
        {
            return null;
        }

        material = new Material(shader) { name = "SpriteWorldUI_Unlit" };
        SetMaterialWhite(material);
        AssetDatabase.CreateAsset(material, path);
        AssetDatabase.SaveAssets();
        return material;
    }

    private static void SetMaterialWhite(Material material)
    {
        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", Color.white);
        }
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", Color.white);
        }
    }

    private static void ResetDefaultSpriteMaterialTint()
    {
        var renderers = Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.sharedMaterials)
            {
                if (material == null) continue;
                var path = AssetDatabase.GetAssetPath(material);
                if (path.EndsWith("Sprite-Lit-Default.mat") || path.EndsWith("Sprite-Unlit-Default.mat"))
                {
                    SetMaterialWhite(material);
                }
            }
        }
    }

    private static void DestroyIfFound(string name)
    {
        var existing = GameObject.Find(name);
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
        }
    }
}
#endif
