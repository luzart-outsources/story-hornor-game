#if UNITY_EDITOR
namespace Luzart.Editor
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEditor;
    using TMPro;

    public static class DoMiTruthOneClickSetup
    {
        // Prefab paths
        private const string MAINMENU_PATH = "Assets/Luzart/DoMiTruth/Prefabs/Screens/UIMainMenu.prefab";
        private const string PAUSE_PATH = "Assets/Luzart/DoMiTruth/Prefabs/Popups/UIPause.prefab";
        private const string BRIEFING_PATH = "Assets/Luzart/DoMiTruth/Prefabs/Screens/UIBriefing.prefab";

        // Data paths
        private const string UIREGISTRY_PATH = "Assets/Luzart/DoMiTruth/Data/Config/UIRegistry.asset";
        private const string GAMECONFIG_PATH = "Assets/Luzart/DoMiTruth/Data/Config/GameConfig.asset";
        private const string DLG_INTRO_PATH = "Assets/Luzart/DoMiTruth/Data/Dialogues/Dlg_Intro.asset";

        // Colors
        private static readonly Color C_NORMAL_BG = new Color(0.18f, 0.12f, 0.08f, 0.95f);
        private static readonly Color C_HOVER_BG = new Color(0.35f, 0.22f, 0.12f, 1f);
        private static readonly Color C_TEXT = new Color(0.92f, 0.85f, 0.72f, 1f);
        private static readonly Color C_HOVER_TEXT = new Color(1f, 0.95f, 0.82f, 1f);
        private static readonly Color C_PAUSE_BTN = new Color(0.15f, 0.15f, 0.15f, 0.95f);
        private static readonly Color C_PAUSE_BTN_TEXT = new Color(0.95f, 0.92f, 0.88f, 1f);
        private static readonly Color C_PAUSE_TITLE = new Color(0.95f, 0.9f, 0.8f, 1f);
        private static readonly Color C_BRIEF_BG = new Color(0.08f, 0.07f, 0.06f, 1f);
        private static readonly Color C_BRIEF_DLG_BG = new Color(0.16f, 0.13f, 0.1f, 0.92f);
        private static readonly Color C_BRIEF_LABEL = new Color(0.7f, 0.82f, 0.72f, 1f);
        private static readonly Color C_BRIEF_TEXT = new Color(0.92f, 0.88f, 0.8f, 1f);
        private static readonly Color C_CASEBOARD = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        // ================================================================
        //  ONE CLICK — Tạo tất cả và setup hoàn chỉnh
        // ================================================================

        [MenuItem("DoMiTruth/⚡ One Click Setup All", false, 0)]
        public static void OneClickSetupAll()
        {
            var log = new List<string>();

            // 1. Create prefabs
            var mainMenuPrefab = CreateMainMenuPrefab_Internal(log);
            var pausePrefab = CreatePausePrefab_Internal(log);
            var briefingPrefab = CreateBriefingPrefab_Internal(log);

            // 2. Register to UIRegistry
            RegisterPrefabsToRegistry(mainMenuPrefab, pausePrefab, briefingPrefab, log);

            // 3. Setup GameConfig
            SetupGameConfig(log);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string report = string.Join("\n", log);
            EditorUtility.DisplayDialog(
                "✅ One Click Setup Done!",
                report + "\n\n" +
                "Tất cả đã được setup tự động.\n" +
                "Gán fullBodySprite cho các DialogueCharacterSO (Police, Detective) trong Inspector.",
                "OK");
        }

        // ================================================================
        //  INDIVIDUAL MENU ITEMS (vẫn giữ để dùng riêng)
        // ================================================================

        [MenuItem("DoMiTruth/Create MainMenu Prefab", false, 100)]
        public static void CreateMainMenuPrefab() => SaveAndLog(CreateMainMenuPrefab_Internal(null), MAINMENU_PATH, "UIMainMenu");

        [MenuItem("DoMiTruth/Create Pause Prefab", false, 101)]
        public static void CreatePausePrefab() => SaveAndLog(CreatePausePrefab_Internal(null), PAUSE_PATH, "UIPause");

        [MenuItem("DoMiTruth/Create Briefing Prefab", false, 102)]
        public static void CreateBriefingPrefab() => SaveAndLog(CreateBriefingPrefab_Internal(null), BRIEFING_PATH, "UIBriefing");

        private static void SaveAndLog(GameObject go, string path, string name)
        {
            if (go == null) return;
            EnsureDirectory(path);
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Done!", $"{name} prefab created at:\n{path}", "OK");
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        // ================================================================
        //  REGISTER TO UIREGISTRY
        // ================================================================

        private static void RegisterPrefabsToRegistry(GameObject mainMenu, GameObject pause, GameObject briefing, List<string> log)
        {
            var registry = AssetDatabase.LoadAssetAtPath<UIRegistrySO>(UIREGISTRY_PATH);
            if (registry == null)
            {
                log?.Add("⚠ UIRegistry not found at: " + UIREGISTRY_PATH);
                return;
            }

            // Save prefabs first so we can reference them
            EnsureDirectory(MAINMENU_PATH);
            EnsureDirectory(PAUSE_PATH);
            EnsureDirectory(BRIEFING_PATH);

            if (mainMenu != null) PrefabUtility.SaveAsPrefabAsset(mainMenu, MAINMENU_PATH);
            if (pause != null) PrefabUtility.SaveAsPrefabAsset(pause, PAUSE_PATH);
            if (briefing != null) PrefabUtility.SaveAsPrefabAsset(briefing, BRIEFING_PATH);

            // Cleanup temp objects
            if (mainMenu != null) Object.DestroyImmediate(mainMenu);
            if (pause != null) Object.DestroyImmediate(pause);
            if (briefing != null) Object.DestroyImmediate(briefing);

            AssetDatabase.Refresh();

            // Load saved prefabs
            var mainMenuPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MAINMENU_PATH);
            var pausePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PAUSE_PATH);
            var briefingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BRIEFING_PATH);

            var so = new SerializedObject(registry);
            var entries = so.FindProperty("entries");

            // Register each if not already present
            RegisterEntry(entries, UIName.MainMenu, mainMenuPrefab, 0, true, log);
            RegisterEntry(entries, UIName.Pause, pausePrefab, 1, false, log);
            RegisterEntry(entries, UIName.Briefing, briefingPrefab, 0, false, log);

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(registry);

            log?.Add("✅ UIRegistry: MainMenu, Pause, Briefing registered");
        }

        private static void RegisterEntry(SerializedProperty entries, UIName uiName, GameObject prefab, int layer, bool cache, List<string> log)
        {
            if (prefab == null) return;
            var uiBase = prefab.GetComponent<UIBase>();
            if (uiBase == null) return;

            // Check if already exists
            for (int i = 0; i < entries.arraySize; i++)
            {
                var entry = entries.GetArrayElementAtIndex(i);
                var nameVal = entry.FindPropertyRelative("uiName");
                if (nameVal.intValue == (int)uiName)
                {
                    // Update existing entry
                    entry.FindPropertyRelative("prefab").objectReferenceValue = uiBase;
                    entry.FindPropertyRelative("layerIndex").intValue = layer;
                    entry.FindPropertyRelative("useCache").boolValue = cache;
                    return;
                }
            }

            // Add new entry
            entries.InsertArrayElementAtIndex(entries.arraySize);
            var newEntry = entries.GetArrayElementAtIndex(entries.arraySize - 1);
            newEntry.FindPropertyRelative("uiName").intValue = (int)uiName;
            newEntry.FindPropertyRelative("prefab").objectReferenceValue = uiBase;
            newEntry.FindPropertyRelative("layerIndex").intValue = layer;
            newEntry.FindPropertyRelative("useCache").boolValue = cache;
        }

        // ================================================================
        //  SETUP GAMECONFIG
        // ================================================================

        private static void SetupGameConfig(List<string> log)
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfigSO>(GAMECONFIG_PATH);
            if (config == null)
            {
                log?.Add("⚠ GameConfig not found at: " + GAMECONFIG_PATH);
                return;
            }

            var so = new SerializedObject(config);

            // Set briefingDialogue = Dlg_Intro
            var dlgIntro = AssetDatabase.LoadAssetAtPath<DialogueSequenceSO>(DLG_INTRO_PATH);
            if (dlgIntro != null)
            {
                so.FindProperty("briefingDialogue").objectReferenceValue = dlgIntro;
                log?.Add("✅ GameConfig.briefingDialogue = Dlg_Intro");
            }
            else
            {
                log?.Add("⚠ Dlg_Intro not found at: " + DLG_INTRO_PATH);
            }

            log?.Add("ℹ Gán fullBodySprite cho các DialogueCharacterSO (Police, Detective) trong Inspector");

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
        }

        // ================================================================
        //  PREFAB BUILDERS (return GameObject, không save)
        // ================================================================

        private static GameObject CreateMainMenuPrefab_Internal(List<string> log)
        {
            var root = new GameObject("UIMainMenu", typeof(RectTransform), typeof(CanvasGroup));
            StretchFull(root.GetComponent<RectTransform>());

            // Background
            var bg = MakeUI("Background", root.transform);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.85f);
            StretchFull(bg.GetComponent<RectTransform>());

            // Content
            var content = MakeUI("Content", root.transform);
            var cRect = content.GetComponent<RectTransform>();
            cRect.anchorMin = cRect.anchorMax = new Vector2(0.5f, 0.5f);
            cRect.sizeDelta = new Vector2(500f, 600f);

            // Title
            var title = MakeUI("Title", content.transform);
            var tRect = title.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0.5f, 1f);
            tRect.anchorMax = new Vector2(0.5f, 1f);
            tRect.pivot = new Vector2(0.5f, 1f);
            tRect.sizeDelta = new Vector2(500f, 80f);
            tRect.anchoredPosition = new Vector2(0f, -10f);
            var tText = title.AddComponent<TextMeshProUGUI>();
            tText.text = "ĐỠ-MI TRUTH"; tText.fontSize = 48f;
            tText.alignment = TextAlignmentOptions.Center;
            tText.color = C_TEXT; tText.raycastTarget = false;

            // Button container
            var btnC = MakeUI("ButtonContainer", content.transform);
            var bcRect = btnC.GetComponent<RectTransform>();
            bcRect.anchorMin = bcRect.anchorMax = new Vector2(0.5f, 0.5f);
            bcRect.sizeDelta = new Vector2(360f, 420f);
            bcRect.anchoredPosition = new Vector2(0f, -30f);
            var vlg = btnC.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 16f; vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true; vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;

            string[] names = { "BtnPlay", "BtnContinue", "BtnSettings", "BtnGuide", "BtnExit" };
            string[] labels = { "PLAY", "CONTINUE", "SETTINGS", "GUIDE", "EXIT" };
            Button[] btns = new Button[5];
            ButtonHoverSelect[] hovers = new ButtonHoverSelect[5];
            SelectToggleGameObject continueToggle = null;

            for (int i = 0; i < 5; i++)
            {
                var r = MakeMenuButton(btnC.transform, names[i], labels[i], 65f);
                btns[i] = r.button; hovers[i] = r.hover;
                if (i == 1)
                {
                    continueToggle = r.button.gameObject.AddComponent<SelectToggleGameObject>();
                    continueToggle.obSelect = new[] { r.button.gameObject };
                    continueToggle.obUnSelect = new GameObject[0];
                }
            }

            var ui = root.AddComponent<UIMainMenu>();
            ui.uiName = UIName.MainMenu;
            var so = new SerializedObject(ui);
            so.FindProperty("btnPlay").objectReferenceValue = btns[0];
            so.FindProperty("btnContinue").objectReferenceValue = btns[1];
            so.FindProperty("btnSettings").objectReferenceValue = btns[2];
            so.FindProperty("btnGuide").objectReferenceValue = btns[3];
            so.FindProperty("btnExit").objectReferenceValue = btns[4];
            so.FindProperty("continueToggle").objectReferenceValue = continueToggle;
            var hp = so.FindProperty("hoverButtons");
            hp.arraySize = 5;
            for (int i = 0; i < 5; i++) hp.GetArrayElementAtIndex(i).objectReferenceValue = hovers[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            log?.Add("✅ MainMenu prefab created");
            return root;
        }

        private static GameObject CreatePausePrefab_Internal(List<string> log)
        {
            var root = new GameObject("UIPause", typeof(RectTransform), typeof(CanvasGroup));
            StretchFull(root.GetComponent<RectTransform>());

            // Overlay
            var ov = MakeUI("Overlay", root.transform);
            ov.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);
            StretchFull(ov.GetComponent<RectTransform>());

            // Panel
            var panel = MakeUI("Panel", root.transform);
            var pRect = panel.GetComponent<RectTransform>();
            pRect.anchorMin = pRect.anchorMax = new Vector2(0.5f, 0.5f);
            pRect.sizeDelta = new Vector2(420f, 380f);
            panel.AddComponent<Image>().color = new Color(0.12f, 0.1f, 0.08f, 0.95f);

            // Title
            var title = MakeUI("Title", panel.transform);
            var tRect = title.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0.5f, 1f); tRect.anchorMax = new Vector2(0.5f, 1f);
            tRect.pivot = new Vector2(0.5f, 1f);
            tRect.sizeDelta = new Vector2(400f, 70f);
            tRect.anchoredPosition = new Vector2(0f, -15f);
            var tText = title.AddComponent<TextMeshProUGUI>();
            tText.text = "Pause"; tText.fontSize = 42f; tText.fontStyle = FontStyles.Bold;
            tText.alignment = TextAlignmentOptions.Center;
            tText.color = C_PAUSE_TITLE; tText.raycastTarget = false;

            // Buttons
            var btnC = MakeUI("Buttons", panel.transform);
            var bcRect = btnC.GetComponent<RectTransform>();
            bcRect.anchorMin = bcRect.anchorMax = new Vector2(0.5f, 0.5f);
            bcRect.sizeDelta = new Vector2(320f, 230f);
            bcRect.anchoredPosition = new Vector2(0f, -20f);
            var vlg = btnC.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 18f; vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true; vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;

            string[] bNames = { "BtnResume", "BtnOption", "BtnExitGame" };
            string[] bLabels = { "RESUME", "OPTION", "EXIT GAME" };
            string[] bIcons = { "↻", "♫", "✕" };
            Button[] btns = new Button[3];
            for (int i = 0; i < 3; i++)
                btns[i] = MakePauseButton(btnC.transform, bNames[i], bIcons[i], bLabels[i], 58f);

            var ui = root.AddComponent<UIPause>();
            ui.uiName = UIName.Pause;
            var so = new SerializedObject(ui);
            so.FindProperty("btnResume").objectReferenceValue = btns[0];
            so.FindProperty("btnOption").objectReferenceValue = btns[1];
            so.FindProperty("btnExitGame").objectReferenceValue = btns[2];
            so.ApplyModifiedPropertiesWithoutUndo();

            log?.Add("✅ Pause prefab created");
            return root;
        }

        private static GameObject CreateBriefingPrefab_Internal(List<string> log)
        {
            var root = new GameObject("UIBriefing", typeof(RectTransform), typeof(CanvasGroup));
            StretchFull(root.GetComponent<RectTransform>());

            // Background
            var bg = MakeUI("Background", root.transform);
            bg.AddComponent<Image>().color = C_BRIEF_BG;
            StretchFull(bg.GetComponent<RectTransform>());

            // NPC Left (left 30%) with CanvasGroup for highlight
            var npcLeftGo = MakeUI("NPCLeft", root.transform);
            var nlRect = npcLeftGo.GetComponent<RectTransform>();
            nlRect.anchorMin = new Vector2(0f, 0f); nlRect.anchorMax = new Vector2(0.3f, 1f);
            nlRect.offsetMin = new Vector2(20f, 20f); nlRect.offsetMax = new Vector2(0f, -60f);
            var canvasGroupLeft = npcLeftGo.AddComponent<CanvasGroup>();
            canvasGroupLeft.alpha = 0.4f;

            var npcLeftImg = npcLeftGo.AddComponent<Image>();
            npcLeftImg.preserveAspect = true; npcLeftImg.raycastTarget = false;
            var animNPCLeft = npcLeftGo.AddComponent<Animator>();
            animNPCLeft.enabled = false; // disabled by default, enabled at runtime if character has animator

            var leftLabelGo = MakeUI("Label", npcLeftGo.transform);
            var llRect = leftLabelGo.GetComponent<RectTransform>();
            llRect.anchorMin = new Vector2(0f, 0f); llRect.anchorMax = new Vector2(1f, 0f);
            llRect.pivot = new Vector2(0.5f, 0f);
            llRect.sizeDelta = new Vector2(0f, 35f);
            llRect.anchoredPosition = new Vector2(0f, 5f);
            leftLabelGo.AddComponent<Image>().color = new Color(0.25f, 0.35f, 0.3f, 0.85f);
            var leftLabelTextGo = MakeUI("Text", leftLabelGo.transform);
            StretchFull(leftLabelTextGo.GetComponent<RectTransform>());
            var leftLabelTMP = leftLabelTextGo.AddComponent<TextMeshProUGUI>();
            leftLabelTMP.text = "NPC LEFT"; leftLabelTMP.fontSize = 18f; leftLabelTMP.fontStyle = FontStyles.Bold;
            leftLabelTMP.alignment = TextAlignmentOptions.Center;
            leftLabelTMP.color = C_BRIEF_LABEL; leftLabelTMP.raycastTarget = false;

            // NPC Right (right 30%) with CanvasGroup for highlight
            var npcRightGo = MakeUI("NPCRight", root.transform);
            var nrRect = npcRightGo.GetComponent<RectTransform>();
            nrRect.anchorMin = new Vector2(0.7f, 0f); nrRect.anchorMax = new Vector2(1f, 1f);
            nrRect.offsetMin = new Vector2(0f, 20f); nrRect.offsetMax = new Vector2(-20f, -60f);
            var canvasGroupRight = npcRightGo.AddComponent<CanvasGroup>();
            canvasGroupRight.alpha = 0.4f;

            var npcRightImg = npcRightGo.AddComponent<Image>();
            npcRightImg.preserveAspect = true; npcRightImg.raycastTarget = false;
            var animNPCRight = npcRightGo.AddComponent<Animator>();
            animNPCRight.enabled = false;

            var rightLabelGo = MakeUI("Label", npcRightGo.transform);
            var rlRect = rightLabelGo.GetComponent<RectTransform>();
            rlRect.anchorMin = new Vector2(0f, 0f); rlRect.anchorMax = new Vector2(1f, 0f);
            rlRect.pivot = new Vector2(0.5f, 0f);
            rlRect.sizeDelta = new Vector2(0f, 35f);
            rlRect.anchoredPosition = new Vector2(0f, 5f);
            rightLabelGo.AddComponent<Image>().color = new Color(0.25f, 0.35f, 0.3f, 0.85f);
            var rightLabelTextGo = MakeUI("Text", rightLabelGo.transform);
            StretchFull(rightLabelTextGo.GetComponent<RectTransform>());
            var rightLabelTMP = rightLabelTextGo.AddComponent<TextMeshProUGUI>();
            rightLabelTMP.text = "NPC RIGHT"; rightLabelTMP.fontSize = 18f; rightLabelTMP.fontStyle = FontStyles.Bold;
            rightLabelTMP.alignment = TextAlignmentOptions.Center;
            rightLabelTMP.color = C_BRIEF_LABEL; rightLabelTMP.raycastTarget = false;

            // Settings Button (top right)
            var sBtn = MakeUI("BtnSettings", root.transform);
            var sbRect = sBtn.GetComponent<RectTransform>();
            sbRect.anchorMin = sbRect.anchorMax = Vector2.one;
            sbRect.pivot = Vector2.one;
            sbRect.sizeDelta = new Vector2(55f, 55f);
            sbRect.anchoredPosition = new Vector2(-15f, -15f);
            sBtn.AddComponent<Image>().color = new Color(0.4f, 0.38f, 0.35f, 0.8f);
            var btnSettings = sBtn.AddComponent<Button>();
            var sIcon = MakeUI("Icon", sBtn.transform);
            StretchFull(sIcon.GetComponent<RectTransform>());
            var sText = sIcon.AddComponent<TextMeshProUGUI>();
            sText.text = "⚙"; sText.fontSize = 32f;
            sText.alignment = TextAlignmentOptions.Center;
            sText.color = Color.white; sText.raycastTarget = false;

            // Case Board (center)
            var board = MakeUI("CaseBoard", root.transform);
            var bRect = board.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.3f, 0.35f); bRect.anchorMax = new Vector2(0.7f, 0.85f);
            board.AddComponent<Image>().color = C_CASEBOARD;
            var cInfoGo = MakeUI("CaseInfoText", board.transform);
            var ciRect = cInfoGo.GetComponent<RectTransform>();
            StretchFull(ciRect); ciRect.offsetMin = new Vector2(20f, 20f); ciRect.offsetMax = new Vector2(-20f, -20f);
            var caseInfoTMP = cInfoGo.AddComponent<TextMeshProUGUI>();
            caseInfoTMP.text = "HERE THE CASE\nINFORMATION"; caseInfoTMP.fontSize = 20f;
            caseInfoTMP.alignment = TextAlignmentOptions.Center;
            caseInfoTMP.color = new Color(0.7f, 0.65f, 0.55f, 1f); caseInfoTMP.raycastTarget = false;

            // Dialogue Box (bottom center)
            var dlgBox = MakeUI("DialogueBox", root.transform);
            var dbRect = dlgBox.GetComponent<RectTransform>();
            dbRect.anchorMin = new Vector2(0.15f, 0f); dbRect.anchorMax = new Vector2(0.85f, 0.3f);
            dbRect.offsetMin = new Vector2(10f, 20f); dbRect.offsetMax = new Vector2(-10f, 0f);
            dlgBox.AddComponent<Image>().color = C_BRIEF_DLG_BG;

            // Portrait
            var port = MakeUI("Portrait", dlgBox.transform);
            var prRect = port.GetComponent<RectTransform>();
            prRect.anchorMin = new Vector2(0f, 0f); prRect.anchorMax = new Vector2(0f, 1f);
            prRect.pivot = new Vector2(0f, 0.5f);
            prRect.sizeDelta = new Vector2(100f, 0f);
            prRect.anchoredPosition = new Vector2(10f, 0f);
            var portImg = port.AddComponent<Image>();
            portImg.preserveAspect = true; portImg.raycastTarget = false;
            var animPortrait = port.AddComponent<Animator>();
            animPortrait.enabled = false;

            // Name
            var nameGo = MakeUI("TxtName", dlgBox.transform);
            var nRect = nameGo.GetComponent<RectTransform>();
            nRect.anchorMin = new Vector2(0f, 1f); nRect.anchorMax = new Vector2(0.7f, 1f);
            nRect.pivot = new Vector2(0f, 1f);
            nRect.sizeDelta = new Vector2(0f, 35f);
            nRect.anchoredPosition = new Vector2(120f, -5f);
            var nameTMP = nameGo.AddComponent<TextMeshProUGUI>();
            nameTMP.text = "Character Name"; nameTMP.fontSize = 20f; nameTMP.fontStyle = FontStyles.Bold;
            nameTMP.alignment = TextAlignmentOptions.MidlineLeft;
            nameTMP.color = C_BRIEF_LABEL; nameTMP.raycastTarget = false;

            // Dialogue text
            var dlgTextGo = MakeUI("TxtDialogue", dlgBox.transform);
            var dtRect = dlgTextGo.GetComponent<RectTransform>();
            dtRect.anchorMin = Vector2.zero; dtRect.anchorMax = new Vector2(0.85f, 0.75f);
            dtRect.offsetMin = new Vector2(120f, 10f); dtRect.offsetMax = new Vector2(-10f, -5f);
            var dlgTMP = dlgTextGo.AddComponent<TextMeshProUGUI>();
            dlgTMP.text = "..."; dlgTMP.fontSize = 18f;
            dlgTMP.alignment = TextAlignmentOptions.TopLeft;
            dlgTMP.color = C_BRIEF_TEXT; dlgTMP.raycastTarget = false;

            // Next button
            var nxtGo = MakeUI("BtnNext", dlgBox.transform);
            var nxRect = nxtGo.GetComponent<RectTransform>();
            nxRect.anchorMin = nxRect.anchorMax = new Vector2(1f, 0f);
            nxRect.pivot = new Vector2(1f, 0f);
            nxRect.sizeDelta = new Vector2(80f, 35f);
            nxRect.anchoredPosition = new Vector2(-10f, 10f);
            nxtGo.AddComponent<Image>().color = new Color(0.3f, 0.4f, 0.35f, 0.9f);
            var btnNext = nxtGo.AddComponent<Button>();
            var nxLabel = MakeUI("Label", nxtGo.transform);
            StretchFull(nxLabel.GetComponent<RectTransform>());
            var nxTMP = nxLabel.AddComponent<TextMeshProUGUI>();
            nxTMP.text = "▶"; nxTMP.fontSize = 22f;
            nxTMP.alignment = TextAlignmentOptions.Center;
            nxTMP.color = Color.white; nxTMP.raycastTarget = false;

            // Wire UIBriefing
            var ui = root.AddComponent<UIBriefing>();
            ui.uiName = UIName.Briefing;
            var so = new SerializedObject(ui);
            so.FindProperty("imgNPCLeft").objectReferenceValue = npcLeftImg;
            so.FindProperty("animNPCLeft").objectReferenceValue = animNPCLeft;
            so.FindProperty("txtNPCLeftLabel").objectReferenceValue = leftLabelTMP;
            so.FindProperty("canvasGroupLeft").objectReferenceValue = canvasGroupLeft;
            so.FindProperty("imgNPCRight").objectReferenceValue = npcRightImg;
            so.FindProperty("animNPCRight").objectReferenceValue = animNPCRight;
            so.FindProperty("txtNPCRightLabel").objectReferenceValue = rightLabelTMP;
            so.FindProperty("canvasGroupRight").objectReferenceValue = canvasGroupRight;
            so.FindProperty("txtCaseInfo").objectReferenceValue = caseInfoTMP;
            so.FindProperty("imgDialoguePortrait").objectReferenceValue = portImg;
            so.FindProperty("animDialoguePortrait").objectReferenceValue = animPortrait;
            so.FindProperty("txtDialogueName").objectReferenceValue = nameTMP;
            so.FindProperty("txtDialogueText").objectReferenceValue = dlgTMP;
            so.FindProperty("btnNext").objectReferenceValue = btnNext;
            so.FindProperty("btnSettings").objectReferenceValue = btnSettings;
            so.ApplyModifiedPropertiesWithoutUndo();

            log?.Add("✅ Briefing prefab created (2 NPC slots with highlight)");
            return root;
        }

        // ================================================================
        //  HELPERS
        // ================================================================

        private struct MenuBtnResult { public Button button; public ButtonHoverSelect hover; }

        private static MenuBtnResult MakeMenuButton(Transform parent, string name, string label, float h)
        {
            var root = MakeUI(name, parent);
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, h);
            root.AddComponent<LayoutElement>().preferredHeight = h;

            // Normal
            var normal = MakeUI("Normal", root.transform);
            StretchFull(normal.GetComponent<RectTransform>());
            normal.AddComponent<Image>().color = C_NORMAL_BG;
            var nLabel = MakeUI("Label", normal.transform);
            StretchFull(nLabel.GetComponent<RectTransform>());
            var nTMP = nLabel.AddComponent<TextMeshProUGUI>();
            nTMP.text = label; nTMP.fontSize = 28f;
            nTMP.alignment = TextAlignmentOptions.Center;
            nTMP.color = C_TEXT; nTMP.raycastTarget = false;

            // Hover
            var hover = MakeUI("Hover", root.transform);
            var hRect = hover.GetComponent<RectTransform>();
            StretchFull(hRect); hRect.localScale = new Vector3(1.06f, 1.06f, 1f);
            hover.AddComponent<Image>().color = C_HOVER_BG;
            var hLabel = MakeUI("Label", hover.transform);
            StretchFull(hLabel.GetComponent<RectTransform>());
            var hTMP = hLabel.AddComponent<TextMeshProUGUI>();
            hTMP.text = label; hTMP.fontSize = 30f;
            hTMP.alignment = TextAlignmentOptions.Center;
            hTMP.color = C_HOVER_TEXT; hTMP.raycastTarget = false;

            hover.SetActive(false);

            // Button (transparent clickable area)
            root.AddComponent<Image>().color = new Color(1, 1, 1, 0);
            var btn = root.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;

            // SelectToggle + HoverSelect
            var toggle = root.AddComponent<SelectToggleGameObject>();
            toggle.obSelect = new[] { hover };
            toggle.obUnSelect = new[] { normal };
            var hs = root.AddComponent<ButtonHoverSelect>();
            var hso = new SerializedObject(hs);
            hso.FindProperty("hoverToggle").objectReferenceValue = toggle;
            hso.ApplyModifiedPropertiesWithoutUndo();

            return new MenuBtnResult { button = btn, hover = hs };
        }

        private static Button MakePauseButton(Transform parent, string name, string icon, string label, float h)
        {
            var root = MakeUI(name, parent);
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, h);
            root.AddComponent<LayoutElement>().preferredHeight = h;
            root.AddComponent<Image>().color = C_PAUSE_BTN;

            var hlg = root.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12f; hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false; hlg.childControlHeight = false;
            hlg.padding = new RectOffset(20, 20, 0, 0);

            var iGo = MakeUI("Icon", root.transform);
            iGo.GetComponent<RectTransform>().sizeDelta = new Vector2(40f, 40f);
            var iTMP = iGo.AddComponent<TextMeshProUGUI>();
            iTMP.text = icon; iTMP.fontSize = 28f;
            iTMP.alignment = TextAlignmentOptions.Center;
            iTMP.color = C_PAUSE_BTN_TEXT; iTMP.raycastTarget = false;

            var lGo = MakeUI("Label", root.transform);
            lGo.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, h);
            var lTMP = lGo.AddComponent<TextMeshProUGUI>();
            lTMP.text = label; lTMP.fontSize = 26f; lTMP.fontStyle = FontStyles.Bold;
            lTMP.alignment = TextAlignmentOptions.MidlineLeft;
            lTMP.color = C_PAUSE_BTN_TEXT; lTMP.raycastTarget = false;

            var btn = root.AddComponent<Button>();
            btn.transition = Selectable.Transition.ColorTint;
            var c = btn.colors;
            c.highlightedColor = new Color(0.3f, 0.25f, 0.2f, 1f);
            c.pressedColor = new Color(0.4f, 0.32f, 0.25f, 1f);
            btn.colors = c;

            return btn;
        }

        private static GameObject MakeUI(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void StretchFull(RectTransform r)
        {
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        }

        private static void EnsureDirectory(string assetPath)
        {
            string dir = System.IO.Path.GetDirectoryName(assetPath);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
        }
    }
}
#endif
