#if UNITY_EDITOR
namespace Luzart.Editor
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEditor;
    using TMPro;

    /// <summary>
    /// One-click tool tạo toàn bộ SO configs + Room Prefabs + auto-gán sprites cho:
    ///   1. Storage Room (két sắt)
    ///   2. Bathroom Room (tủ + túi rác + mảnh thi thể)
    ///
    /// Menu: DoMiTruth → ⚡ Setup Room Scenarios
    /// </summary>
    public static class RoomScenarioSetup
    {
        // Output folders
        private const string SO_ROOT     = "Assets/Luzart/DoMiTruth/Data";
        private const string PREFAB_ROOT = "Assets/Luzart/DoMiTruth/Prefabs/RoomScenarios";

        // Sub-folders cho SO
        private const string CLUES_DIR         = SO_ROOT + "/Clues";
        private const string INTERACTABLES_DIR = SO_ROOT + "/Interactables";
        private const string LOCKS_DIR         = SO_ROOT + "/Locks";
        private const string ACTIONS_DIR       = SO_ROOT + "/Actions/RoomScenarios";
        private const string DIALOGUES_DIR     = SO_ROOT + "/Dialogues";
        private const string CHARACTERS_DIR    = SO_ROOT + "/Characters";

        // Art source folders
        private const string ART_STORAGE  = "Assets/Art/SOURCE CHÍNH THỨC/Map Storage";
        private const string ART_BATHROOM = "Assets/Art/SOURCE CHÍNH THỨC/Map bathroom";

        // ================================================================
        //  SPRITE PATHS — mapping rõ ràng từng file
        // ================================================================

        // STORAGE
        private const string SPR_STORAGE_BG         = ART_STORAGE + "/co filter no prop.png";
        private const string SPR_SAFE_LOCKED         = ART_STORAGE + "/Props/Item Ingame/prop ket sat.png";
        private const string SPR_SAFE_OPENED         = ART_STORAGE + "/Props/Item Ingame/ket sat opened.png";
        private const string SPR_KEY_INGAME          = ART_STORAGE + "/Props/Item Ingame/chia khoa iname.png";
        private const string SPR_SAFE_CLUE_POPUP     = ART_STORAGE + "/Props/Item Popup/Đồ vật bên trong két sắt Popup/photo frame popup.png";

        // BATHROOM
        private const string SPR_BATHROOM_BG         = ART_BATHROOM + "/bathroom no prop.png";
        private const string SPR_CABINET_CLOSED      = ART_BATHROOM + "/props/cửa đóng.png";
        private const string SPR_CABINET_OPENED      = ART_BATHROOM + "/props/cửa mở.png";
        private const string SPR_TRASHBAG_CLOSED     = ART_BATHROOM + "/props/bọc ni long.png";
        private const string SPR_TRASHBAG_OPENED     = ART_BATHROOM + "/props/bọc nilon mở.png";
        private const string SPR_BODYPART_INGAME     = ART_BATHROOM + "/props/prop ingame mảnh thi thể.png";
        private const string SPR_BODYPART_POPUP      = ART_BATHROOM + "/props/A dismembered arm_POP UP .png";

        // Prefab path
        private const string MONOLOGUE_PREFAB_PATH = "Assets/Luzart/DoMiTruth/Prefabs/Popups/UIDetectiveMonologue.prefab";
        private const string UIREGISTRY_PATH = "Assets/Luzart/DoMiTruth/Data/Config/UIRegistry.asset";

        [MenuItem("DoMiTruth/⚡ Setup Room Scenarios", false, 10)]
        public static void SetupAll()
        {
            var log = new List<string>();
            log.Add("=== Room Scenario Setup ===");
            log.Add("");

            EnsureAllDirs();

            // ── Load sprites ──
            log.Add("── AUTO-FIND SPRITES ──");
            var sprites = LoadAllSprites(log);

            // ── Shared assets ──
            var charDetective = LoadOrWarn<DialogueCharacterSO>(CHARACTERS_DIR + "/Char_Detective.asset", log);

            // ── Detective Monologue UI ──
            log.Add("");
            log.Add("── DETECTIVE MONOLOGUE UI ──");
            CreateDetectiveMonologuePrefab(charDetective, log);

            // ── Storage Room ──
            log.Add("");
            log.Add("── STORAGE ROOM ──");
            var storageSOs = CreateStorageRoomSOs(charDetective, sprites, log);
            CreateStorageRoomPrefab(storageSOs, sprites, log);

            // ── Bathroom Room ──
            log.Add("");
            log.Add("── BATHROOM ROOM ──");
            var bathroomSOs = CreateBathroomRoomSOs(storageSOs.clueKey, charDetective, sprites, log);
            CreateBathroomRoomPrefab(bathroomSOs, sprites, log);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string report = string.Join("\n", log);
            EditorUtility.DisplayDialog("✅ Room Scenarios Done!", report, "OK");
            Debug.Log(report);
        }

        // ================================================================
        //  DETECTIVE MONOLOGUE PREFAB + REGISTER
        // ================================================================

        private static void CreateDetectiveMonologuePrefab(DialogueCharacterSO detective, List<string> log)
        {
            // ── Build prefab GO ──
            var root = new GameObject("UIDetectiveMonologue", typeof(RectTransform), typeof(CanvasGroup));
            var rootRect = root.GetComponent<RectTransform>();
            StretchFull(rootRect);

            // Transparent full-screen click blocker (optional — bấm ngoài để next)
            var blocker = new GameObject("Blocker", typeof(RectTransform));
            blocker.transform.SetParent(root.transform, false);
            var blockerImg = blocker.AddComponent<Image>();
            blockerImg.color = new Color(0f, 0f, 0f, 0.15f);
            blockerImg.raycastTarget = true;
            StretchFull(blocker.GetComponent<RectTransform>());

            // Strip container (bottom, anchor bottom)
            var strip = new GameObject("Strip", typeof(RectTransform));
            strip.transform.SetParent(root.transform, false);
            var stripRect = strip.GetComponent<RectTransform>();
            stripRect.anchorMin = new Vector2(0.3f, 0f);
            stripRect.anchorMax = new Vector2(1f, 0f);
            stripRect.pivot = new Vector2(0.5f, 0f);
            stripRect.sizeDelta = new Vector2(0f, 80f);
            stripRect.anchoredPosition = new Vector2(0f, 20f);

            // Strip background
            var stripBg = strip.AddComponent<Image>();
            stripBg.color = new Color(0.18f, 0.15f, 0.12f, 0.92f);
            stripBg.raycastTarget = false;

            // Name bar (inside strip, top-left)
            var nameBar = new GameObject("TxtName", typeof(RectTransform));
            nameBar.transform.SetParent(strip.transform, false);
            var nameRect = nameBar.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0.65f);
            nameRect.anchorMax = new Vector2(0.6f, 1f);
            nameRect.offsetMin = new Vector2(15f, 0f);
            nameRect.offsetMax = new Vector2(-10f, -3f);
            var txtName = nameBar.AddComponent<TextMeshProUGUI>();
            txtName.text = "DETECTIVE";
            txtName.fontSize = 16f;
            txtName.fontStyle = FontStyles.Bold;
            txtName.alignment = TextAlignmentOptions.MidlineLeft;
            txtName.color = new Color(0.85f, 0.78f, 0.65f, 1f);
            txtName.raycastTarget = false;

            // Dialogue text (inside strip, body area)
            var textGo = new GameObject("TxtDialogue", typeof(RectTransform));
            textGo.transform.SetParent(strip.transform, false);
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(0.82f, 0.65f);
            textRect.offsetMin = new Vector2(15f, 8f);
            textRect.offsetMax = new Vector2(-10f, 0f);
            var txtDialogue = textGo.AddComponent<TextMeshProUGUI>();
            txtDialogue.text = "";
            txtDialogue.fontSize = 16f;
            txtDialogue.alignment = TextAlignmentOptions.TopLeft;
            txtDialogue.color = new Color(0.9f, 0.87f, 0.8f, 1f);
            txtDialogue.raycastTarget = false;

            // Portrait (right side of strip)
            var portrait = new GameObject("ImgPortrait", typeof(RectTransform));
            portrait.transform.SetParent(strip.transform, false);
            var portRect = portrait.GetComponent<RectTransform>();
            portRect.anchorMin = new Vector2(1f, 0f);
            portRect.anchorMax = new Vector2(1f, 1f);
            portRect.pivot = new Vector2(1f, 0.5f);
            portRect.sizeDelta = new Vector2(85f, 0f);
            portRect.anchoredPosition = new Vector2(5f, 0f);
            var portImg = portrait.AddComponent<Image>();
            portImg.preserveAspect = true;
            portImg.raycastTarget = false;
            if (detective != null && detective.portrait != null)
                portImg.sprite = detective.portrait;

            // Next button = blocker itself (full screen click area)
            var btnNext = blocker.AddComponent<Button>();
            btnNext.transition = Selectable.Transition.None;

            // ── Add UIDetectiveMonologue component ──
            var ui = root.AddComponent<UIDetectiveMonologue>();
            ui.uiName = UIName.DetectiveMonologue;

            var so = new SerializedObject(ui);
            so.FindProperty("imgPortrait").objectReferenceValue = portImg;
            so.FindProperty("txtName").objectReferenceValue = txtName;
            so.FindProperty("txtDialogue").objectReferenceValue = txtDialogue;
            so.FindProperty("btnNext").objectReferenceValue = btnNext;
            so.ApplyModifiedPropertiesWithoutUndo();

            // ── Save prefab ──
            EnsureDir(MONOLOGUE_PREFAB_PATH);
            PrefabUtility.SaveAsPrefabAsset(root, MONOLOGUE_PREFAB_PATH);
            Object.DestroyImmediate(root);
            log.Add("✅ Prefab: " + MONOLOGUE_PREFAB_PATH);

            // ── Register to UIRegistry ──
            AssetDatabase.Refresh();
            var registry = AssetDatabase.LoadAssetAtPath<UIRegistrySO>(UIREGISTRY_PATH);
            if (registry != null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MONOLOGUE_PREFAB_PATH);
                if (prefab != null)
                {
                    var regSo = new SerializedObject(registry);
                    var entries = regSo.FindProperty("entries");

                    var uiBase = prefab.GetComponent<UIBase>();
                    if (uiBase != null)
                    {
                        // Check/update existing
                        bool found = false;
                        for (int i = 0; i < entries.arraySize; i++)
                        {
                            var entry = entries.GetArrayElementAtIndex(i);
                            if (entry.FindPropertyRelative("uiName").intValue == (int)UIName.DetectiveMonologue)
                            {
                                entry.FindPropertyRelative("prefab").objectReferenceValue = uiBase;
                                entry.FindPropertyRelative("layerIndex").intValue = 1; // Popup layer
                                entry.FindPropertyRelative("useCache").boolValue = false;
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            entries.InsertArrayElementAtIndex(entries.arraySize);
                            var newEntry = entries.GetArrayElementAtIndex(entries.arraySize - 1);
                            newEntry.FindPropertyRelative("uiName").intValue = (int)UIName.DetectiveMonologue;
                            newEntry.FindPropertyRelative("prefab").objectReferenceValue = uiBase;
                            newEntry.FindPropertyRelative("layerIndex").intValue = 1;
                            newEntry.FindPropertyRelative("useCache").boolValue = false;
                        }
                        regSo.ApplyModifiedProperties();
                        EditorUtility.SetDirty(registry);
                        log.Add("✅ UIRegistry: DetectiveMonologue registered (layer=Popup)");
                    }
                }
            }
            else
            {
                log.Add("⚠ UIRegistry not found — register manually");
            }
        }

        // ================================================================
        //  SPRITE LOADING
        // ================================================================

        private struct SpriteSet
        {
            // Storage
            public Sprite storageBg, safeLocked, safeOpened, keyIngame, safeCluePopup;
            // Bathroom
            public Sprite bathroomBg, cabinetClosed, cabinetOpened;
            public Sprite trashBagClosed, trashBagOpened;
            public Sprite bodyPartIngame, bodyPartPopup;
        }

        private static SpriteSet LoadAllSprites(List<string> log)
        {
            var s = new SpriteSet();

            s.storageBg      = LoadSprite(SPR_STORAGE_BG, log);
            s.safeLocked     = LoadSprite(SPR_SAFE_LOCKED, log);
            s.safeOpened     = LoadSprite(SPR_SAFE_OPENED, log);
            s.keyIngame      = LoadSprite(SPR_KEY_INGAME, log);
            s.safeCluePopup  = LoadSprite(SPR_SAFE_CLUE_POPUP, log);

            s.bathroomBg     = LoadSprite(SPR_BATHROOM_BG, log);
            s.cabinetClosed  = LoadSprite(SPR_CABINET_CLOSED, log);
            s.cabinetOpened  = LoadSprite(SPR_CABINET_OPENED, log);
            s.trashBagClosed = LoadSprite(SPR_TRASHBAG_CLOSED, log);
            s.trashBagOpened = LoadSprite(SPR_TRASHBAG_OPENED, log);
            s.bodyPartIngame = LoadSprite(SPR_BODYPART_INGAME, log);
            s.bodyPartPopup  = LoadSprite(SPR_BODYPART_POPUP, log);

            return s;
        }

        private static Sprite LoadSprite(string path, List<string> log)
        {
            // Thử đường dẫn trực tiếp
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
            {
                log?.Add($"  ✅ {System.IO.Path.GetFileName(path)}");
                return sprite;
            }

            // Thử tìm bằng filename trong toàn bộ project
            string filename = System.IO.Path.GetFileNameWithoutExtension(path);
            string[] guids = AssetDatabase.FindAssets(filename + " t:Sprite");
            if (guids.Length > 0)
            {
                string foundPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(foundPath);
                if (sprite != null)
                {
                    log?.Add($"  ✅ {filename} (found at: {foundPath})");
                    return sprite;
                }
            }

            // Thử load Texture2D rồi lấy sprite từ sub-assets
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null)
            {
                // Có thể texture chưa được import là Sprite
                var allAtPath = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var obj in allAtPath)
                {
                    if (obj is Sprite sp)
                    {
                        log?.Add($"  ✅ {System.IO.Path.GetFileName(path)} (from sub-asset)");
                        return sp;
                    }
                }
                log?.Add($"  ⚠ TEXTURE found but not Sprite: {path}");
                log?.Add($"     → Set Texture Type = 'Sprite (2D and UI)' in Import Settings");
                return null;
            }

            log?.Add($"  ❌ NOT FOUND: {path}");
            return null;
        }

        // ================================================================
        //  STORAGE ROOM — Két sắt
        // ================================================================

        private struct StorageRoomSOs
        {
            public ClueSO clueSafeContent, clueKey;
            public LockConfigSO lockSafe;
            public LockedItemInteractableSO ioSafeLocked;
            public DecorationInteractableSO ioSafeOpened;
            public ClueInteractableSO ioKeyProp;
            public SetPropActiveStep stepHideSafeLocked, stepShowSafeOpened, stepShowKey;
            public ShowClueDetailConfig stepShowSafeClue;
        }

        private static StorageRoomSOs CreateStorageRoomSOs(DialogueCharacterSO detective, SpriteSet sprites, List<string> log)
        {
            var s = new StorageRoomSOs();

            // Clues
            s.clueSafeContent = CreateSO<ClueSO>(CLUES_DIR + "/Clue_SafeContent.asset", so =>
            {
                so.clueId = "clue_safe_content";
                so.clueName = "Safe Content";
                so.description = "Evidence found inside the locked safe.";
                so.category = ClueCategory.Physical;
                so.clueImage = sprites.safeCluePopup;
            }, log);

            s.clueKey = CreateSO<ClueSO>(CLUES_DIR + "/Clue_Key.asset", so =>
            {
                so.clueId = "clue_key";
                so.clueName = "Key";
                so.description = "A small key found inside the safe. It might open something...";
                so.category = ClueCategory.Physical;
                so.clueImage = sprites.keyIngame;
            }, log);

            // Lock
            s.lockSafe = CreateSO<LockConfigSO>(LOCKS_DIR + "/Lock_Safe.asset", so =>
            {
                so.lockType = LockType.Passcode;
                so.passcode = "1234";
                so.hintText = "Try the combination...";
            }, log);

            // Interactables
            s.ioSafeLocked = CreateSO<LockedItemInteractableSO>(INTERACTABLES_DIR + "/IO_SafeLocked.asset", so =>
            {
                so.objectId = "io_safe_locked";
                so.hitboxSize = SpriteSize(sprites.safeLocked, new Vector2(150f, 200f));
                so.isOneTimeOnly = false;
                so.lockConfig = s.lockSafe;
                so.highlightSprite = sprites.safeLocked;
            }, log);

            s.ioSafeOpened = CreateSO<DecorationInteractableSO>(INTERACTABLES_DIR + "/IO_SafeOpened.asset", so =>
            {
                so.objectId = "io_safe_opened";
                so.hitboxSize = SpriteSize(sprites.safeOpened, new Vector2(150f, 200f));
                so.isOneTimeOnly = false;
                so.highlightSprite = sprites.safeOpened;
            }, log);

            s.ioKeyProp = CreateSO<ClueInteractableSO>(INTERACTABLES_DIR + "/IO_KeyProp.asset", so =>
            {
                so.objectId = "io_key_prop";
                so.hitboxSize = SpriteSize(sprites.keyIngame, new Vector2(60f, 60f));
                so.isOneTimeOnly = true;
                so.clue = s.clueKey;
                so.highlightSprite = sprites.keyIngame;
            }, log);

            // Action Steps
            s.stepHideSafeLocked = CreateSO<SetPropActiveStep>(ACTIONS_DIR + "/Step_HideSafeLocked.asset", so =>
            { so.targetProp = s.ioSafeLocked; so.setActive = false; }, log);

            s.stepShowSafeOpened = CreateSO<SetPropActiveStep>(ACTIONS_DIR + "/Step_ShowSafeOpened.asset", so =>
            { so.targetProp = s.ioSafeOpened; so.setActive = true; }, log);

            s.stepShowSafeClue = CreateSO<ShowClueDetailConfig>(ACTIONS_DIR + "/Step_ShowSafeClue.asset", so =>
            { so.clue = s.clueSafeContent; }, log);

            s.stepShowKey = CreateSO<SetPropActiveStep>(ACTIONS_DIR + "/Step_ShowKeyProp.asset", so =>
            { so.targetProp = s.ioKeyProp; so.setActive = true; }, log);

            // Wire chains
            s.ioSafeLocked.onUnlockSuccess = new List<ActionStepConfig>
                { s.stepHideSafeLocked, s.stepShowSafeOpened, s.stepShowSafeClue, s.stepShowKey };
            s.ioSafeLocked.onUnlockFail = new List<ActionStepConfig>();

            // showConditions
            s.ioSafeLocked.showConditions = new List<PrerequisiteConfig>
            {
                new PrerequisiteConfig { type = PrerequisiteType.IsUnlocked, interactableRef = s.ioSafeLocked, negate = true }
            };

            s.ioSafeOpened.showConditions = new List<PrerequisiteConfig>
            {
                new PrerequisiteConfig { type = PrerequisiteType.IsUnlocked, interactableRef = s.ioSafeLocked, negate = false }
            };

            s.ioKeyProp.showConditions = new List<PrerequisiteConfig>
            {
                new PrerequisiteConfig { type = PrerequisiteType.IsUnlocked, interactableRef = s.ioSafeLocked, negate = false }
            };

            EditorUtility.SetDirty(s.ioSafeLocked);
            EditorUtility.SetDirty(s.ioSafeOpened);
            EditorUtility.SetDirty(s.ioKeyProp);

            log.Add("✅ Storage SOs created & wired");
            return s;
        }

        // ================================================================
        //  BATHROOM ROOM
        // ================================================================

        private struct BathroomRoomSOs
        {
            public ClueSO clueBodyPart;
            public DialogueSequenceSO dlgCabinetLocked;
            public DecorationInteractableSO ioCabinetClosed, ioCabinetOpened;
            public DecorationInteractableSO ioTrashBagClosed, ioTrashBagOpened;
            public ClueInteractableSO ioBodyPart;
            public DecorationInteractableSO ioFlies;
            public SetPropActiveStep stepHideCabinetClosed, stepShowCabinetOpened;
            public SetPropActiveStep stepShowTrashBagClosed, stepHideTrashBagClosed;
            public SetPropActiveStep stepShowTrashBagOpened, stepShowBodyPart, stepShowFlies;
            public ShowMonologueStep stepMonologueLocked;
        }

        private static BathroomRoomSOs CreateBathroomRoomSOs(ClueSO clueKey, DialogueCharacterSO detective, SpriteSet sprites, List<string> log)
        {
            var b = new BathroomRoomSOs();

            // Clue
            b.clueBodyPart = CreateSO<ClueSO>(CLUES_DIR + "/Clue_BodyPart.asset", so =>
            {
                so.clueId = "clue_body_part";
                so.clueName = "Body Part";
                so.description = "A piece of a corpse found in a trash bag inside the cabinet.";
                so.category = ClueCategory.Physical;
                so.clueImage = sprites.bodyPartPopup;
            }, log);

            // Dialogue
            b.dlgCabinetLocked = CreateSO<DialogueSequenceSO>(DIALOGUES_DIR + "/Dlg_CabinetLocked.asset", so =>
            {
                so.dialogueId = "dlg_cabinet_locked";
                so.lines = new List<DialogueLine>
                {
                    new DialogueLine
                    {
                        character = detective,
                        text = "Locked cabinet. Where is the key?",
                        typingSpeed = 30f,
                        waitForInput = true,
                    }
                };
            }, log);

            // Interactables
            b.ioCabinetClosed = CreateSO<DecorationInteractableSO>(INTERACTABLES_DIR + "/IO_CabinetClosed.asset", so =>
            {
                so.objectId = "io_cabinet_closed";
                so.hitboxSize = SpriteSize(sprites.cabinetClosed, new Vector2(180f, 250f));
                so.isOneTimeOnly = true;
                so.highlightSprite = sprites.cabinetClosed;
            }, log);

            b.ioCabinetOpened = CreateSO<DecorationInteractableSO>(INTERACTABLES_DIR + "/IO_CabinetOpened.asset", so =>
            {
                so.objectId = "io_cabinet_opened";
                so.hitboxSize = SpriteSize(sprites.cabinetOpened, new Vector2(180f, 250f));
                so.isOneTimeOnly = false;
                so.highlightSprite = sprites.cabinetOpened;
            }, log);

            b.ioTrashBagClosed = CreateSO<DecorationInteractableSO>(INTERACTABLES_DIR + "/IO_TrashBagClosed.asset", so =>
            {
                so.objectId = "io_trash_bag_closed";
                so.hitboxSize = SpriteSize(sprites.trashBagClosed, new Vector2(100f, 120f));
                so.isOneTimeOnly = true;
                so.highlightSprite = sprites.trashBagClosed;
            }, log);

            b.ioTrashBagOpened = CreateSO<DecorationInteractableSO>(INTERACTABLES_DIR + "/IO_TrashBagOpened.asset", so =>
            {
                so.objectId = "io_trash_bag_opened";
                so.hitboxSize = SpriteSize(sprites.trashBagOpened, new Vector2(100f, 120f));
                so.isOneTimeOnly = false;
                so.highlightSprite = sprites.trashBagOpened;
            }, log);

            b.ioBodyPart = CreateSO<ClueInteractableSO>(INTERACTABLES_DIR + "/IO_BodyPart.asset", so =>
            {
                so.objectId = "io_body_part";
                so.hitboxSize = SpriteSize(sprites.bodyPartIngame, new Vector2(80f, 80f));
                so.isOneTimeOnly = true;
                so.clue = b.clueBodyPart;
                so.highlightSprite = sprites.bodyPartIngame;
            }, log);

            b.ioFlies = CreateSO<DecorationInteractableSO>(INTERACTABLES_DIR + "/IO_Flies.asset", so =>
            {
                so.objectId = "io_flies";
                so.hitboxSize = new Vector2(120f, 120f);
                so.isOneTimeOnly = false;
                // Flies sprite = animation frames → không gán static sprite, dùng Animator
            }, log);

            // Action Steps
            b.stepMonologueLocked = CreateSO<ShowMonologueStep>(ACTIONS_DIR + "/Step_MonologueCabinetLocked.asset", so =>
            { so.dialogue = b.dlgCabinetLocked; }, log);

            b.stepHideCabinetClosed = CreateSO<SetPropActiveStep>(ACTIONS_DIR + "/Step_HideCabinetClosed.asset", so =>
            { so.targetProp = b.ioCabinetClosed; so.setActive = false; }, log);

            b.stepShowCabinetOpened = CreateSO<SetPropActiveStep>(ACTIONS_DIR + "/Step_ShowCabinetOpened.asset", so =>
            { so.targetProp = b.ioCabinetOpened; so.setActive = true; }, log);

            b.stepShowTrashBagClosed = CreateSO<SetPropActiveStep>(ACTIONS_DIR + "/Step_ShowTrashBagClosed.asset", so =>
            { so.targetProp = b.ioTrashBagClosed; so.setActive = true; }, log);

            b.stepHideTrashBagClosed = CreateSO<SetPropActiveStep>(ACTIONS_DIR + "/Step_HideTrashBagClosed.asset", so =>
            { so.targetProp = b.ioTrashBagClosed; so.setActive = false; }, log);

            b.stepShowTrashBagOpened = CreateSO<SetPropActiveStep>(ACTIONS_DIR + "/Step_ShowTrashBagOpened.asset", so =>
            { so.targetProp = b.ioTrashBagOpened; so.setActive = true; }, log);

            b.stepShowBodyPart = CreateSO<SetPropActiveStep>(ACTIONS_DIR + "/Step_ShowBodyPart.asset", so =>
            { so.targetProp = b.ioBodyPart; so.setActive = true; }, log);

            b.stepShowFlies = CreateSO<SetPropActiveStep>(ACTIONS_DIR + "/Step_ShowFlies.asset", so =>
            { so.targetProp = b.ioFlies; so.setActive = true; }, log);

            // Wire prerequisites + chains
            b.ioCabinetClosed.prerequisites = new List<PrerequisiteConfig>
            {
                new PrerequisiteConfig { type = PrerequisiteType.HasClue, clueRef = clueKey }
            };
            b.ioCabinetClosed.onPrerequisiteNotMet = new List<ActionStepConfig> { b.stepMonologueLocked };
            b.ioCabinetClosed.onInteract = new List<ActionStepConfig>
            {
                b.stepHideCabinetClosed, b.stepShowCabinetOpened, b.stepShowTrashBagClosed,
            };

            b.ioTrashBagClosed.onInteract = new List<ActionStepConfig>
            {
                b.stepHideTrashBagClosed, b.stepShowTrashBagOpened, b.stepShowBodyPart, b.stepShowFlies,
            };

            // showConditions
            b.ioCabinetOpened.showConditions = new List<PrerequisiteConfig>
            { new PrerequisiteConfig { type = PrerequisiteType.HasInteracted, interactableRef = b.ioCabinetClosed } };

            b.ioTrashBagClosed.showConditions = new List<PrerequisiteConfig>
            { new PrerequisiteConfig { type = PrerequisiteType.HasInteracted, interactableRef = b.ioCabinetClosed } };

            b.ioTrashBagOpened.showConditions = new List<PrerequisiteConfig>
            { new PrerequisiteConfig { type = PrerequisiteType.HasInteracted, interactableRef = b.ioTrashBagClosed } };

            b.ioBodyPart.showConditions = new List<PrerequisiteConfig>
            { new PrerequisiteConfig { type = PrerequisiteType.HasInteracted, interactableRef = b.ioTrashBagClosed } };

            b.ioFlies.showConditions = new List<PrerequisiteConfig>
            { new PrerequisiteConfig { type = PrerequisiteType.HasInteracted, interactableRef = b.ioTrashBagClosed } };

            EditorUtility.SetDirty(b.ioCabinetClosed);
            EditorUtility.SetDirty(b.ioCabinetOpened);
            EditorUtility.SetDirty(b.ioTrashBagClosed);
            EditorUtility.SetDirty(b.ioTrashBagOpened);
            EditorUtility.SetDirty(b.ioBodyPart);
            EditorUtility.SetDirty(b.ioFlies);

            log.Add("✅ Bathroom SOs created & wired");
            return b;
        }

        // ================================================================
        //  ROOM PREFAB — STORAGE
        // ================================================================

        private static void CreateStorageRoomPrefab(StorageRoomSOs s, SpriteSet sprites, List<string> log)
        {
            var root = new GameObject("Room_Storage_Scenario", typeof(RectTransform));
            StretchFull(root.GetComponent<RectTransform>());

            // Background
            var bg = MakeSpriteChild("Background", root.transform, Vector2.zero, sprites.storageBg);
            StretchFull(bg.GetComponent<RectTransform>());
            bg.GetComponent<Image>().raycastTarget = false;

            // Props
            MakeInteractableProp("Prop_SafeLocked", root.transform, new Vector2(200f, -50f),
                s.ioSafeLocked, sprites.safeLocked, true);
            MakeInteractableProp("Prop_SafeOpened", root.transform, new Vector2(200f, -50f),
                s.ioSafeOpened, sprites.safeOpened, false);
            MakeInteractableProp("Prop_Key", root.transform, new Vector2(300f, -100f),
                s.ioKeyProp, sprites.keyIngame, false);

            SavePrefab(root, PREFAB_ROOT + "/Room_Storage_Scenario.prefab", log);
        }

        // ================================================================
        //  ROOM PREFAB — BATHROOM
        // ================================================================

        private static void CreateBathroomRoomPrefab(BathroomRoomSOs b, SpriteSet sprites, List<string> log)
        {
            var root = new GameObject("Room_Bathroom_Scenario", typeof(RectTransform));
            StretchFull(root.GetComponent<RectTransform>());

            // Background
            var bg = MakeSpriteChild("Background", root.transform, Vector2.zero, sprites.bathroomBg);
            StretchFull(bg.GetComponent<RectTransform>());
            bg.GetComponent<Image>().raycastTarget = false;

            // Props
            MakeInteractableProp("Prop_CabinetClosed", root.transform, new Vector2(-200f, -30f),
                b.ioCabinetClosed, sprites.cabinetClosed, true);
            MakeInteractableProp("Prop_CabinetOpened", root.transform, new Vector2(-200f, -30f),
                b.ioCabinetOpened, sprites.cabinetOpened, false);
            MakeInteractableProp("Prop_TrashBagClosed", root.transform, new Vector2(-100f, -80f),
                b.ioTrashBagClosed, sprites.trashBagClosed, false);
            MakeInteractableProp("Prop_TrashBagOpened", root.transform, new Vector2(-100f, -80f),
                b.ioTrashBagOpened, sprites.trashBagOpened, false);
            MakeInteractableProp("Prop_BodyPart", root.transform, new Vector2(-50f, -120f),
                b.ioBodyPart, sprites.bodyPartIngame, false);

            // Flies — placeholder (animation frames cần Animator, tool không tự tạo Animator)
            var fliesGo = MakeInteractableProp("Prop_Flies", root.transform, new Vector2(-80f, -60f),
                b.ioFlies, null, false);
            log.Add("  ℹ Ruồi bu: 40 animation frames tại:");
            log.Add("    " + ART_BATHROOM + "/[1 giây 4] animation ruồi bu/");
            log.Add("    → Tạo AnimatorController + Animation Clip từ frames, gán vào Prop_Flies");

            SavePrefab(root, PREFAB_ROOT + "/Room_Bathroom_Scenario.prefab", log);
        }

        // ================================================================
        //  HELPERS
        // ================================================================

        private static GameObject MakeInteractableProp(string name, Transform parent, Vector2 pos,
            InteractableObjectSO data, Sprite sprite, bool startActive)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;

            // Image with sprite
            var img = go.AddComponent<Image>();
            if (sprite != null)
            {
                img.sprite = sprite;
                img.color = Color.white;
                img.preserveAspect = true;
                rt.sizeDelta = SpriteSize(sprite, data != null ? data.hitboxSize : new Vector2(100f, 100f));
            }
            else
            {
                img.color = new Color(0.6f, 0.6f, 0.6f, 0.5f); // placeholder
                rt.sizeDelta = data != null ? data.hitboxSize : new Vector2(100f, 100f);
            }
            img.raycastTarget = true;

            // InteractableObject
            var io = go.AddComponent<InteractableObject>();
            var so = new SerializedObject(io);
            so.FindProperty("data").objectReferenceValue = data;
            so.ApplyModifiedPropertiesWithoutUndo();

            go.SetActive(startActive);
            return go;
        }

        private static GameObject MakeSpriteChild(string name, Transform parent, Vector2 pos, Sprite sprite)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;

            var img = go.AddComponent<Image>();
            if (sprite != null)
            {
                img.sprite = sprite;
                img.color = Color.white;
            }
            else
            {
                img.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            }
            return go;
        }

        /// <summary>Lấy kích thước pixel của sprite, hoặc fallback.</summary>
        private static Vector2 SpriteSize(Sprite sprite, Vector2 fallback)
        {
            if (sprite == null || sprite.texture == null) return fallback;
            return new Vector2(sprite.rect.width, sprite.rect.height);
        }

        private static void SavePrefab(GameObject go, string path, List<string> log)
        {
            EnsureDir(path);
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            log?.Add("✅ Prefab: " + path);
        }

        private static T CreateSO<T>(string path, System.Action<T> setup, List<string> log) where T : ScriptableObject
        {
            EnsureDir(path);
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                setup(existing);
                EditorUtility.SetDirty(existing);
                log?.Add($"  ↻ {System.IO.Path.GetFileName(path)}");
                return existing;
            }
            var so = ScriptableObject.CreateInstance<T>();
            setup(so);
            AssetDatabase.CreateAsset(so, path);
            log?.Add($"  + {System.IO.Path.GetFileName(path)}");
            return so;
        }

        private static T LoadOrWarn<T>(string path, List<string> log) where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) log?.Add($"  ⚠ Not found: {path}");
            return asset;
        }

        private static void StretchFull(RectTransform r)
        {
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        }

        private static void EnsureDir(string assetPath)
        {
            string dir = System.IO.Path.GetDirectoryName(assetPath);
            if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
        }

        private static void EnsureAllDirs()
        {
            string[] dirs = { CLUES_DIR, INTERACTABLES_DIR, LOCKS_DIR, ACTIONS_DIR, DIALOGUES_DIR, CHARACTERS_DIR, PREFAB_ROOT };
            foreach (var d in dirs) EnsureDir(d + "/x");
        }
    }
}
#endif
