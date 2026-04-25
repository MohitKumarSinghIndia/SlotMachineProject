using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class WishingDragonLayoutBuilder
{
    private const string ScenePath = "Assets/Scenes/Main.unity";
    private const float ReferenceWidth = 1920f;
    private const float ReferenceHeight = 1080f;
    private const float TopBarHeight = 92f;
    private const float BottomHudHeight = 164f;
    private const float OuterPadding = 28f;
    private const float ContentGap = 24f;

    private static readonly Color RootBackground = new Color32(13, 16, 31, 255);
    private static readonly Color DaySky = new Color32(95, 156, 232, 255);
    private static readonly Color NightSky = new Color32(21, 31, 80, 255);
    private static readonly Color Gold = new Color32(232, 189, 87, 255);
    private static readonly Color DarkGold = new Color32(135, 91, 24, 255);
    private static readonly Color Crimson = new Color32(120, 20, 28, 255);
    private static readonly Color DeepCrimson = new Color32(69, 7, 15, 255);
    private static readonly Color Jade = new Color32(48, 128, 111, 255);
    private static readonly Color Navy = new Color32(21, 28, 48, 255);
    private static readonly Color Ink = new Color32(9, 12, 20, 255);
    private static readonly Color Cream = new Color32(244, 229, 196, 255);
    private static readonly Color SoftWhite = new Color32(248, 244, 231, 255);
    private static readonly Color TransparentGold = new Color32(232, 189, 87, 70);
    private static readonly string[] SymbolLabels =
    {
        "A", "K", "Q", "J", "10",
        "WILD", "SCATTER", "A", "K", "Q",
        "J", "10", "A", "K", "Q"
    };

    [MenuItem("Tools/Slot/Build Wishing Dragon Desktop Layout")]
    public static void BuildLayout()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == "Main Camera" || root.name == "Global Light 2D")
            {
                continue;
            }

            Object.DestroyImmediate(root);
        }

        TMP_FontAsset font = LoadDefaultFont();
        CreateEventSystem();

        GameObject uiRoot = CreateUIRoot(font);
        RectTransform canvasRect = uiRoot.GetComponent<RectTransform>();

        BuildTopBar(uiRoot.transform, font);
        BuildMainContent(uiRoot.transform, font);
        BuildBottomHud(uiRoot.transform, font);

        Selection.activeGameObject = uiRoot;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static TMP_FontAsset LoadDefaultFont()
    {
        TMP_FontAsset font = TMP_Settings.defaultFontAsset;
        if (font != null)
        {
            return font;
        }

        return Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
    }

    private static void CreateEventSystem()
    {
        EventSystem existing = Object.FindObjectOfType<EventSystem>();
        if (existing != null)
        {
            return;
        }

        GameObject go = new GameObject("EventSystem", typeof(RectTransform), typeof(EventSystem), typeof(StandaloneInputModule));
        Undo.RegisterCreatedObjectUndo(go, "Create EventSystem");
    }

    private static GameObject CreateUIRoot(TMP_FontAsset font)
    {
        GameObject canvasGo = new GameObject("UIRoot", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = false;

        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform rect = canvasGo.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        CreateImage("Backdrop", canvasGo.transform, RootBackground, true);

        return canvasGo;
    }

    private static void BuildTopBar(Transform parent, TMP_FontAsset font)
    {
        GameObject topBar = CreatePanel("TopBar", parent, DeepCrimson, Gold, 4f);
        RectTransform rect = topBar.GetComponent<RectTransform>();
        StretchHorizontally(rect, 0f, 1f, -TopBarHeight, 0f, OuterPadding, -OuterPadding);

        CreateImage("TopBarGlow", topBar.transform, new Color32(235, 193, 84, 24), false)
            .GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0f, 14f);

        CreateText(
            "Title",
            topBar.transform,
            "WISHING DRAGON · Thunder Struck Gaming",
            font,
            34,
            FontStyles.Bold,
            SoftWhite,
            TextAlignmentOptions.MidlineLeft,
            new Vector2(0f, 0f),
            new Vector2(0.72f, 1f),
            new Vector2(24f, 0f),
            new Vector2(-24f, 0f));

        GameObject icons = new GameObject("MenuIcons", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        icons.transform.SetParent(topBar.transform, false);
        RectTransform iconsRect = icons.GetComponent<RectTransform>();
        iconsRect.anchorMin = new Vector2(0.72f, 0f);
        iconsRect.anchorMax = new Vector2(1f, 1f);
        iconsRect.offsetMin = new Vector2(0f, 16f);
        iconsRect.offsetMax = new Vector2(-24f, -16f);

        HorizontalLayoutGroup layout = icons.GetComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleRight;
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.spacing = 12f;

        CreateIconPill("Menu", icons.transform, font);
        CreateIconPill("Audio", icons.transform, font);
        CreateIconPill("Help", icons.transform, font);
    }

    private static void BuildMainContent(Transform parent, TMP_FontAsset font)
    {
        float topOffset = TopBarHeight + OuterPadding;
        float bottomOffset = BottomHudHeight + OuterPadding;

        GameObject main = new GameObject("MainContent", typeof(RectTransform));
        main.transform.SetParent(parent, false);
        RectTransform mainRect = main.GetComponent<RectTransform>();
        StretchHorizontally(mainRect, 0f, 1f, bottomOffset, -topOffset, OuterPadding, -OuterPadding);

        float totalUsableWidth = ReferenceWidth - (OuterPadding * 2f) - ContentGap;
        float leftWidth = totalUsableWidth * 0.639f;
        float leftNormalized = leftWidth / (ReferenceWidth - (OuterPadding * 2f));

        GameObject leftZone = new GameObject("LeftZone", typeof(RectTransform));
        leftZone.transform.SetParent(main.transform, false);
        RectTransform leftRect = leftZone.GetComponent<RectTransform>();
        leftRect.anchorMin = new Vector2(0f, 0f);
        leftRect.anchorMax = new Vector2(leftNormalized, 1f);
        leftRect.offsetMin = Vector2.zero;
        leftRect.offsetMax = Vector2.zero;

        GameObject rightZone = new GameObject("RightZone", typeof(RectTransform));
        rightZone.transform.SetParent(main.transform, false);
        RectTransform rightRect = rightZone.GetComponent<RectTransform>();
        rightRect.anchorMin = new Vector2(leftNormalized, 0f);
        rightRect.anchorMax = new Vector2(1f, 1f);
        rightRect.offsetMin = new Vector2(ContentGap, 0f);
        rightRect.offsetMax = Vector2.zero;

        BuildLeftZone(leftZone.transform, font);
        BuildRightZone(rightZone.transform, font);
    }

    private static void BuildLeftZone(Transform parent, TMP_FontAsset font)
    {
        GameObject stage = CreatePanel("BackgroundStage", parent, Navy, Gold, 4f);
        Stretch(stage.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f);

        CreateInsetFrame("StageInnerGlow", stage.transform, TransparentGold, 0f, 18f);

        GameObject day = CreatePanel("DayBackground", stage.transform, DaySky, new Color32(255, 224, 150, 120), 0f);
        Stretch(day.GetComponent<RectTransform>(), 14f, 14f, 14f, 14f);

        GameObject skyBand = CreateImage("SkyBand", day.transform, new Color32(247, 197, 124, 65), false);
        RectTransform skyRect = skyBand.GetComponent<RectTransform>();
        skyRect.anchorMin = new Vector2(0f, 0.55f);
        skyRect.anchorMax = new Vector2(1f, 1f);
        skyRect.offsetMin = Vector2.zero;
        skyRect.offsetMax = Vector2.zero;

        CreateDecorDots(day.transform, 18, new Color32(255, 239, 202, 70), 16f, 22f);
        BuildCitySilhouette(day.transform);

        GameObject night = CreatePanel("NightBackground", stage.transform, NightSky, new Color32(140, 119, 241, 100), 0f);
        Stretch(night.GetComponent<RectTransform>(), 14f, 14f, 14f, 14f);
        CanvasGroup nightGroup = night.AddComponent<CanvasGroup>();
        nightGroup.alpha = 0f;

        CreateText(
            "BackgroundStateLabel",
            stage.transform,
            "DAY SCENE  |  NIGHT SCENE (hidden for now)",
            font,
            20,
            FontStyles.Bold,
            SoftWhite,
            TextAlignmentOptions.TopLeft,
            new Vector2(0f, 1f),
            new Vector2(0.6f, 1f),
            new Vector2(28f, -22f),
            new Vector2(0f, -12f));

        GameObject board = CreatePanel("ReelBoard", stage.transform, new Color32(36, 16, 20, 225), Gold, 8f);
        RectTransform boardRect = board.GetComponent<RectTransform>();
        boardRect.anchorMin = new Vector2(0.12f, 0.16f);
        boardRect.anchorMax = new Vector2(0.88f, 0.79f);
        boardRect.offsetMin = Vector2.zero;
        boardRect.offsetMax = Vector2.zero;

        CreateInsetFrame("BoardInner", board.transform, new Color32(250, 218, 123, 60), 0f, 14f);

        GameObject boardHeader = new GameObject("BoardHeader", typeof(RectTransform));
        boardHeader.transform.SetParent(board.transform, false);
        RectTransform boardHeaderRect = boardHeader.GetComponent<RectTransform>();
        boardHeaderRect.anchorMin = new Vector2(0f, 1f);
        boardHeaderRect.anchorMax = new Vector2(1f, 1f);
        boardHeaderRect.pivot = new Vector2(0.5f, 1f);
        boardHeaderRect.sizeDelta = new Vector2(0f, 56f);
        boardHeaderRect.anchoredPosition = new Vector2(0f, -12f);

        CreateText(
            "BoardTitle",
            boardHeader.transform,
            "5 × 3 REEL GRID",
            font,
            24,
            FontStyles.Bold,
            Gold,
            TextAlignmentOptions.Center,
            new Vector2(0f, 0f),
            new Vector2(1f, 1f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f));

        GameObject gridArea = new GameObject("GridArea", typeof(RectTransform), typeof(GridLayoutGroup));
        gridArea.transform.SetParent(board.transform, false);
        RectTransform gridRect = gridArea.GetComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0f, 0f);
        gridRect.anchorMax = new Vector2(1f, 1f);
        gridRect.offsetMin = new Vector2(26f, 24f);
        gridRect.offsetMax = new Vector2(-26f, -82f);

        GridLayoutGroup grid = gridArea.GetComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;
        grid.spacing = new Vector2(12f, 12f);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.padding = new RectOffset(0, 0, 0, 0);
        grid.childAlignment = TextAnchor.UpperLeft;

        float cellWidth = (boardRect.rect.width - 52f - (12f * 4f)) / 5f;
        float cellHeight = (boardRect.rect.height - 106f - (12f * 2f)) / 3f;
        grid.cellSize = new Vector2(cellWidth, cellHeight);

        int symbolIndex = 0;
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 5; col++)
            {
                string label = SymbolLabels[symbolIndex % SymbolLabels.Length];
                Color fill = label == "WILD"
                    ? new Color32(181, 39, 41, 255)
                    : label == "SCATTER"
                        ? new Color32(84, 62, 162, 255)
                        : ((row + col) % 2 == 0 ? new Color32(92, 36, 24, 255) : new Color32(126, 48, 30, 255));

                GameObject cell = CreatePanel($"Cell_{row}_{col}", gridArea.transform, fill, Gold, 3f);
                CreateInsetFrame("CellShine", cell.transform, new Color32(255, 236, 193, 24), 0f, 8f);
                CreateText(
                    "SymbolLabel",
                    cell.transform,
                    label,
                    font,
                    label.Length > 2 ? 24 : 36,
                    FontStyles.Bold,
                    SoftWhite,
                    TextAlignmentOptions.Center,
                    new Vector2(0f, 0f),
                    new Vector2(1f, 1f),
                    new Vector2(0f, 0f),
                    new Vector2(0f, 0f));

                symbolIndex++;
            }
        }

        CreateText(
            "BoardFooter",
            stage.transform,
            "Background scene + centered reel board placeholder",
            font,
            18,
            FontStyles.Normal,
            Cream,
            TextAlignmentOptions.BottomLeft,
            new Vector2(0f, 0f),
            new Vector2(0.68f, 0.1f),
            new Vector2(28f, 0f),
            new Vector2(0f, 18f));
    }

    private static void BuildRightZone(Transform parent, TMP_FontAsset font)
    {
        GameObject panel = CreatePanel("DragonZone", parent, new Color32(23, 25, 41, 255), Gold, 4f);
        Stretch(panel.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f);

        CreateInsetFrame("DragonZoneGlow", panel.transform, new Color32(228, 182, 88, 50), 0f, 18f);

        CreateText(
            "DragonTitle",
            panel.transform,
            "DRAGON MASCOT",
            font,
            28,
            FontStyles.Bold,
            Gold,
            TextAlignmentOptions.Top,
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0f, -24f),
            new Vector2(0f, -12f));

        GameObject silhouette = CreatePanel("DragonSilhouette", panel.transform, new Color32(139, 28, 42, 255), new Color32(255, 214, 119, 160), 4f);
        RectTransform silhouetteRect = silhouette.GetComponent<RectTransform>();
        silhouetteRect.anchorMin = new Vector2(0.16f, 0.16f);
        silhouetteRect.anchorMax = new Vector2(0.84f, 0.83f);
        silhouetteRect.offsetMin = Vector2.zero;
        silhouetteRect.offsetMax = Vector2.zero;

        CreateImage("HeadOrb", silhouette.transform, new Color32(184, 40, 52, 255), false)
            .GetComponent<RectTransform>().SetAnchorsAndOffsets(new Vector2(0.18f, 0.72f), new Vector2(0.62f, 0.96f), Vector2.zero, Vector2.zero);

        GameObject body = CreateImage("Body", silhouette.transform, new Color32(167, 32, 46, 255), false);
        body.GetComponent<RectTransform>().SetAnchorsAndOffsets(new Vector2(0.28f, 0.2f), new Vector2(0.74f, 0.76f), Vector2.zero, Vector2.zero);

        GameObject tail = CreateImage("Tail", silhouette.transform, new Color32(122, 18, 30, 255), false);
        tail.GetComponent<RectTransform>().SetAnchorsAndOffsets(new Vector2(0.08f, 0.18f), new Vector2(0.38f, 0.42f), Vector2.zero, Vector2.zero);

        GameObject plaque = CreatePanel("MultiplierPlaque", panel.transform, Jade, Gold, 3f);
        RectTransform plaqueRect = plaque.GetComponent<RectTransform>();
        plaqueRect.anchorMin = new Vector2(0.07f, 0.34f);
        plaqueRect.anchorMax = new Vector2(0.55f, 0.48f);
        plaqueRect.offsetMin = Vector2.zero;
        plaqueRect.offsetMax = Vector2.zero;

        CreateText(
            "PlaqueText",
            plaque.transform,
            "MULTIPLIER\nPLAQUE",
            font,
            22,
            FontStyles.Bold,
            SoftWhite,
            TextAlignmentOptions.Center,
            new Vector2(0f, 0f),
            new Vector2(1f, 1f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f));

        CreateText(
            "DragonNotes",
            panel.transform,
            "Full-body standing mascot\nFacing left toward reels\nAlways animated later with Spine",
            font,
            18,
            FontStyles.Normal,
            Cream,
            TextAlignmentOptions.Bottom,
            new Vector2(0.08f, 0.04f),
            new Vector2(0.92f, 0.18f),
            Vector2.zero,
            Vector2.zero);
    }

    private static void BuildBottomHud(Transform parent, TMP_FontAsset font)
    {
        GameObject hud = CreatePanel("BottomHUD", parent, DeepCrimson, Gold, 4f);
        RectTransform rect = hud.GetComponent<RectTransform>();
        StretchHorizontally(rect, 0f, 1f, 0f, BottomHudHeight, OuterPadding, -OuterPadding);

        CreateInsetFrame("HudGlow", hud.transform, new Color32(240, 203, 103, 38), 0f, 14f);

        GameObject primary = new GameObject("PrimaryRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        primary.transform.SetParent(hud.transform, false);
        RectTransform primaryRect = primary.GetComponent<RectTransform>();
        primaryRect.anchorMin = new Vector2(0f, 0.42f);
        primaryRect.anchorMax = new Vector2(1f, 1f);
        primaryRect.offsetMin = new Vector2(20f, -4f);
        primaryRect.offsetMax = new Vector2(-20f, -14f);

        HorizontalLayoutGroup primaryLayout = primary.GetComponent<HorizontalLayoutGroup>();
        primaryLayout.spacing = 14f;
        primaryLayout.childAlignment = TextAnchor.MiddleCenter;
        primaryLayout.childForceExpandHeight = true;
        primaryLayout.childForceExpandWidth = false;
        primaryLayout.childControlWidth = false;
        primaryLayout.childControlHeight = true;

        CreateHudBox("WinBox", primary.transform, "WIN", "0.00", font, 230f);
        CreateHudBox("BalanceBox", primary.transform, "BALANCE", "10,000.00", font, 260f);
        CreateBetBox(primary.transform, font);
        CreateHudBox("TotalBetBox", primary.transform, "TOTAL BET", "20.00", font, 230f);
        CreateSpinButton(primary.transform, font);

        GameObject secondary = new GameObject("SecondaryRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        secondary.transform.SetParent(hud.transform, false);
        RectTransform secondaryRect = secondary.GetComponent<RectTransform>();
        secondaryRect.anchorMin = new Vector2(0f, 0f);
        secondaryRect.anchorMax = new Vector2(1f, 0.4f);
        secondaryRect.offsetMin = new Vector2(20f, 16f);
        secondaryRect.offsetMax = new Vector2(-20f, -8f);

        HorizontalLayoutGroup secondaryLayout = secondary.GetComponent<HorizontalLayoutGroup>();
        secondaryLayout.spacing = 12f;
        secondaryLayout.childAlignment = TextAnchor.MiddleLeft;
        secondaryLayout.childForceExpandHeight = true;
        secondaryLayout.childForceExpandWidth = false;
        secondaryLayout.childControlWidth = false;
        secondaryLayout.childControlHeight = true;

        CreateCommandChip("LinesChip", secondary.transform, "LINES: 20 FIXED", font, 188f);
        CreateCommandChip("AutoChip", secondary.transform, "AUTO", font, 120f);
        CreateCommandChip("TurboChip", secondary.transform, "TURBO", font, 120f);
        CreateCommandChip("PaytableChip", secondary.transform, "PAYTABLE", font, 156f);
        CreateCommandChip("InfoChip", secondary.transform, "INFO", font, 120f);
    }

    private static void BuildCitySilhouette(Transform parent)
    {
        GameObject city = new GameObject("CitySilhouette", typeof(RectTransform));
        city.transform.SetParent(parent, false);
        RectTransform cityRect = city.GetComponent<RectTransform>();
        cityRect.anchorMin = new Vector2(0f, 0f);
        cityRect.anchorMax = new Vector2(1f, 0.42f);
        cityRect.offsetMin = Vector2.zero;
        cityRect.offsetMax = Vector2.zero;

        float[] widths = { 0.1f, 0.08f, 0.13f, 0.1f, 0.07f, 0.12f, 0.09f, 0.11f };
        float[] heights = { 0.55f, 0.42f, 0.66f, 0.5f, 0.38f, 0.6f, 0.46f, 0.57f };
        float cursor = 0.02f;

        for (int i = 0; i < widths.Length; i++)
        {
            GameObject building = CreateImage($"Building_{i}", city.transform, new Color32(112, 55, 40, 160), false);
            RectTransform rect = building.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(cursor, 0f);
            rect.anchorMax = new Vector2(cursor + widths[i], heights[i]);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            cursor += widths[i] + 0.02f;
        }
    }

    private static void CreateDecorDots(Transform parent, int count, Color color, float minSize, float maxSize)
    {
        Random.InitState(1234);
        for (int i = 0; i < count; i++)
        {
            GameObject dot = CreateImage($"Dot_{i}", parent, color, false);
            RectTransform rect = dot.GetComponent<RectTransform>();
            float size = Random.Range(minSize, maxSize);
            rect.anchorMin = new Vector2(Random.Range(0.04f, 0.96f), Random.Range(0.5f, 0.95f));
            rect.anchorMax = rect.anchorMin;
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = Vector2.zero;
        }
    }

    private static void CreateHudBox(string name, Transform parent, string label, string value, TMP_FontAsset font, float width)
    {
        GameObject box = CreatePanel(name, parent, Ink, Gold, 3f);
        LayoutElement layout = box.AddComponent<LayoutElement>();
        layout.preferredWidth = width;

        CreateText("Label", box.transform, label, font, 16, FontStyles.Bold, Gold, TextAlignmentOptions.Top, new Vector2(0f, 0.55f), new Vector2(1f, 1f), new Vector2(0f, -8f), new Vector2(0f, 0f));
        CreateText("Value", box.transform, value, font, 32, FontStyles.Bold, SoftWhite, TextAlignmentOptions.Center, new Vector2(0f, 0f), new Vector2(1f, 0.72f), new Vector2(0f, 0f), new Vector2(0f, 0f));
    }

    private static void CreateBetBox(Transform parent, TMP_FontAsset font)
    {
        GameObject box = CreatePanel("BetBox", parent, Ink, Gold, 3f);
        LayoutElement layout = box.AddComponent<LayoutElement>();
        layout.preferredWidth = 310f;

        CreateText("Label", box.transform, "BET", font, 16, FontStyles.Bold, Gold, TextAlignmentOptions.Top, new Vector2(0f, 0.55f), new Vector2(1f, 1f), new Vector2(0f, -8f), new Vector2(0f, 0f));
        CreateText("Value", box.transform, "1.00", font, 30, FontStyles.Bold, SoftWhite, TextAlignmentOptions.Center, new Vector2(0.28f, 0f), new Vector2(0.72f, 0.72f), Vector2.zero, Vector2.zero);

        CreateMiniButton("Minus", box.transform, "-", font, new Vector2(0.04f, 0.14f), new Vector2(0.22f, 0.68f));
        CreateMiniButton("Plus", box.transform, "+", font, new Vector2(0.78f, 0.14f), new Vector2(0.96f, 0.68f));
    }

    private static void CreateSpinButton(Transform parent, TMP_FontAsset font)
    {
        GameObject button = CreatePanel("SpinButton", parent, new Color32(207, 49, 45, 255), Gold, 4f);
        LayoutElement layout = button.AddComponent<LayoutElement>();
        layout.preferredWidth = 210f;

        CreateInsetFrame("SpinGlow", button.transform, new Color32(255, 224, 167, 30), 0f, 10f);
        CreateText("Label", button.transform, "SPIN", font, 34, FontStyles.Bold, SoftWhite, TextAlignmentOptions.Center, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
    }

    private static void CreateCommandChip(string name, Transform parent, string label, TMP_FontAsset font, float width)
    {
        GameObject chip = CreatePanel(name, parent, Navy, Gold, 2f);
        LayoutElement layout = chip.AddComponent<LayoutElement>();
        layout.preferredWidth = width;

        CreateText("Label", chip.transform, label, font, 18, FontStyles.Bold, Cream, TextAlignmentOptions.Center, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
    }

    private static void CreateMiniButton(string name, Transform parent, string label, TMP_FontAsset font, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject button = CreatePanel(name, parent, Crimson, Gold, 2f);
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        CreateText("Label", button.transform, label, font, 28, FontStyles.Bold, SoftWhite, TextAlignmentOptions.Center, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
    }

    private static void CreateIconPill(string label, Transform parent, TMP_FontAsset font)
    {
        GameObject pill = CreatePanel($"{label}Icon", parent, Navy, Gold, 2f);
        LayoutElement layout = pill.AddComponent<LayoutElement>();
        layout.preferredWidth = 110f;
        CreateText("Label", pill.transform, label.ToUpperInvariant(), font, 18, FontStyles.Bold, Cream, TextAlignmentOptions.Center, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
    }

    private static GameObject CreatePanel(string name, Transform parent, Color fill, Color border, float borderSize)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Outline));
        go.transform.SetParent(parent, false);

        Image image = go.GetComponent<Image>();
        image.color = fill;

        Outline outline = go.GetComponent<Outline>();
        outline.effectColor = border;
        outline.effectDistance = new Vector2(borderSize, -borderSize);

        return go;
    }

    private static GameObject CreateImage(string name, Transform parent, Color color, bool stretch)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        Image image = go.GetComponent<Image>();
        image.color = color;

        if (stretch)
        {
            Stretch(go.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f);
        }

        return go;
    }

    private static void CreateInsetFrame(string name, Transform parent, Color color, float borderSize, float inset)
    {
        GameObject frame = CreateImage(name, parent, color, false);
        RectTransform rect = frame.GetComponent<RectTransform>();
        Stretch(rect, inset, inset, inset, inset);
        if (borderSize > 0f)
        {
            Outline outline = frame.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(borderSize, -borderSize);
        }
    }

    private static void CreateText(
        string name,
        Transform parent,
        string text,
        TMP_FontAsset font,
        float fontSize,
        FontStyles style,
        Color color,
        TextAlignmentOptions alignment,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.font = font;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.enableWordWrapping = false;
    }

    private static void Stretch(RectTransform rect, float left, float right, float bottom, float top)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    private static void StretchHorizontally(RectTransform rect, float anchorMinY, float anchorMaxY, float offsetMinY, float offsetMaxY, float left, float right)
    {
        rect.anchorMin = new Vector2(0f, anchorMinY);
        rect.anchorMax = new Vector2(1f, anchorMaxY);
        rect.offsetMin = new Vector2(left, offsetMinY);
        rect.offsetMax = new Vector2(right, offsetMaxY);
    }
}

internal static class RectTransformExtensions
{
    public static void SetAnchorsAndOffsets(this RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }
}
