#if UNITY_EDITOR
namespace Luzart.Editor
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEditor;
    using TMPro;

    public static class GDDDataCreator
    {
        private const string DATA_ROOT = "Assets/Luzart/DoMiTruth/Data";
        private const string ANIM_ROOT = "Assets/Luzart/DoMiTruth/Animation";
        private const string PREFAB_ROOT = "Assets/Luzart/DoMiTruth/Prefabs";
        private const string SFX_ROOT = "Assets/Art/Sound effects";
        private const string ART_ROOT = "Assets/Art/SOURCE CHÍNH THỨC";

        // Colors matching briefing style
        private static readonly Color C_BOARD_BG = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        private static readonly Color C_LABEL_BG = new Color(0.25f, 0.35f, 0.3f, 0.85f);
        private static readonly Color C_LABEL_TEXT = new Color(0.7f, 0.82f, 0.72f, 1f);
        private static readonly Color C_BOARD_TEXT = new Color(0.7f, 0.65f, 0.55f, 1f);
        private static readonly Color C_CHOICE_BG = new Color(0.16f, 0.13f, 0.1f, 0.92f);
        private static readonly Color C_CHOICE_TEXT = new Color(0.92f, 0.88f, 0.8f, 1f);
        private static readonly Color C_DIALOGUE_BG = new Color(0.22f, 0.18f, 0.14f, 0.92f);
        private static readonly Color C_DIALOGUE_TEXT = new Color(0.92f, 0.88f, 0.8f, 1f);
        private static readonly Color C_DARK_BG = new Color(0.08f, 0.07f, 0.06f, 1f);

        // ================================================================
        //  MENU ITEMS
        // ================================================================

        [MenuItem("DoMiTruth/📦 Create All GDD Data", false, 200)]
        public static void CreateAllGDDData()
        {
            var log = new List<string>();

            var chars = CreateCharacters(log);
            var clues = CreateClues(log);
            var dialogueTrees = CreateDialogueTrees(chars, clues, log);
            var interactables = CreateInteractables(clues, dialogueTrees, log);
            CreateRoomPrefabs(interactables, log);
            var rooms = CreateRooms(log);
            AssignRoomsToMaps(rooms, log);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("✅ GDD Data Created!",
                string.Join("\n", log), "OK");
        }

        [MenuItem("DoMiTruth/🔊 Create All Sound", false, 104)]
        public static void CreateAllSound()
        {
            var log = new List<string>();
            AssignSFXToGameConfig(log);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("✅ Sound Setup Done!", string.Join("\n", log), "OK");
        }

        [MenuItem("DoMiTruth/Create NPC Dialogue Prefab", false, 103)]
        public static void CreateNPCDialoguePrefab()
        {
            var go = CreateNPCDialoguePrefab_Internal(null);
            if (go == null) return;
            string path = PREFAB_ROOT + "/Screens/UINPCDialogue.prefab";
            EnsureDir(path);
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            AssetDatabase.Refresh();

            // Register to UIRegistry
            var registry = AssetDatabase.LoadAssetAtPath<UIRegistrySO>(
                "Assets/Luzart/DoMiTruth/Data/Config/UIRegistry.asset");
            if (registry != null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var uiBase = prefab.GetComponent<UIBase>();
                if (uiBase != null)
                {
                    var so = new SerializedObject(registry);
                    var entries = so.FindProperty("entries");
                    RegisterEntry(entries, UIName.NPCDialogue, uiBase);
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(registry);
                }
            }

            EditorUtility.DisplayDialog("Done!", "UINPCDialogue prefab created at:\n" + path, "OK");
        }

        // ================================================================
        //  CHARACTERS
        // ================================================================

        private struct CharRefs
        {
            public DialogueCharacterSO police, detective, doctor, debtCollector;
        }

        private static CharRefs CreateCharacters(List<string> log)
        {
            var refs = new CharRefs();

            // Load existing
            refs.police = LoadOrCreate<DialogueCharacterSO>(
                DATA_ROOT + "/Characters/Char_Police.asset", "Char_Police");
            refs.detective = LoadOrCreate<DialogueCharacterSO>(
                DATA_ROOT + "/Characters/Char_Detective.asset", "Char_Detective");

            // Setup Police
            refs.police.characterId = "police";
            refs.police.characterName = "POLICE";
            refs.police.nameColor = Color.white;
            refs.police.portraitAnimator = LoadAnim("Anim_Police");
            EditorUtility.SetDirty(refs.police);

            // Setup Detective
            refs.detective.characterId = "detective";
            refs.detective.characterName = "DETECTIVE";
            refs.detective.nameColor = Color.white;
            EditorUtility.SetDirty(refs.detective);

            // Doctor
            refs.doctor = LoadOrCreate<DialogueCharacterSO>(
                DATA_ROOT + "/Characters/Char_Doctor.asset", "Char_Doctor");
            refs.doctor.characterId = "doctor";
            refs.doctor.characterName = "DOCTOR";
            refs.doctor.nameColor = Color.white;
            refs.doctor.portraitAnimator = LoadAnim("Anim_Doctor_Chill");
            EditorUtility.SetDirty(refs.doctor);

            // Debt Collector
            refs.debtCollector = LoadOrCreate<DialogueCharacterSO>(
                DATA_ROOT + "/Characters/Char_DebtCollector.asset", "Char_DebtCollector");
            refs.debtCollector.characterId = "debt_collector";
            refs.debtCollector.characterName = "DEBT COLLECTOR";
            refs.debtCollector.nameColor = Color.white;
            refs.debtCollector.portraitAnimator = LoadAnim("Anim_Thuno_Angry");
            EditorUtility.SetDirty(refs.debtCollector);

            log?.Add("✅ Characters: Police, Detective, Doctor, DebtCollector");
            return refs;
        }

        // ================================================================
        //  CLUES
        // ================================================================

        private struct ClueRefs
        {
            public ClueSO contract, debitNote, contactInfo;
            public ClueSO brokenCabinet, godOfWealth;
            public ClueSO brokenPhoto, bloodShawl, sleepingPills;
            public ClueSO frenchMaterials, storagePhotos;
        }

        private static ClueRefs CreateClues(List<string> log)
        {
            var r = new ClueRefs();

            // Home Office
            r.contract = MakeClue("Clue_Contract", "contract",
                "Private Clinic Investment Contract",
                "A contract for co-investing in a private clinic. The victim's signature is missing.",
                ClueCategory.Document,
                LoadSprite("Map home office/Props/Item Popups/hop dong popup.png"));

            r.debitNote = MakeClue("Clue_DebitNote", "debit_note",
                "Debit note",
                "A debit note showing a large outstanding debt.",
                ClueCategory.Document,
                LoadSprite("Map home office/Props/Item Popups/giay no popup.png"));

            r.contactInfo = MakeClue("Clue_ContactInfo", "contact_info",
                "Contact Information",
                "A list of phone numbers and addresses.",
                ClueCategory.Document,
                LoadSprite("Map home office/Props/Item Popups/danh ba popup.png"));

            // Living Room
            r.brokenCabinet = MakeClue("Clue_BrokenCabinet", "broken_cabinet",
                "The broken cabinet door",
                "A cabinet door that has been smashed open with force.",
                ClueCategory.Physical,
                LoadSprite("Map living room/Prop pop up/kinh vo popup.png"));

            r.godOfWealth = MakeClue("Clue_GodOfWealth", "god_of_wealth",
                "The statue of the God of Wealth",
                "A golden statue of the God of Wealth. Seems important to the victim.",
                ClueCategory.Physical,
                LoadSprite("Map living room/Prop pop up/tuong than tai popup.png"));

            // Master Bedroom
            r.brokenPhoto = MakeClue("Clue_BrokenPhoto", "broken_photo",
                "Broken photo portrait",
                "A family portrait that has been smashed. Glass shards everywhere.",
                ClueCategory.Photo,
                LoadSprite("Map master bedroom/Props/Items popup/Chan dung popup.png"));

            r.bloodShawl = MakeClue("Clue_BloodShawl", "blood_shawl",
                "Blood-stained shawl",
                "A silk shawl with visible bloodstains.",
                ClueCategory.Physical,
                LoadSprite("Map master bedroom/Props/Items popup/ao choang popup.png"));

            r.sleepingPills = MakeClue("Clue_SleepingPills", "sleeping_pills",
                "Sleeping pills",
                "A bottle of prescription sleeping pills. Nearly empty.",
                ClueCategory.Physical,
                LoadSprite("Map master bedroom/Props/Items popup/thuoc ngu popup.png"));

            // Storage
            r.frenchMaterials = MakeClue("Clue_FrenchMaterials", "french_materials",
                "French learning materials",
                "Textbooks and notes for learning French.",
                ClueCategory.Document,
                LoadSprite("Map Storage/Props/Item Popup/Đồ vật bên trong két sắt Popup/tai lieu popup.png"));

            r.storagePhotos = MakeClue("Clue_StoragePhotos", "storage_photos",
                "Photos",
                "A collection of photographs found in the storage room.",
                ClueCategory.Photo,
                LoadSprite("Map Storage/Props/Item Popup/Đồ vật bên trong két sắt Popup/photo frame popup.png"));

            log?.Add("✅ Clues: 10 clues created");
            return r;
        }

        private static ClueSO MakeClue(string fileName, string clueId, string clueName, string desc,
            ClueCategory cat, Sprite popupSprite = null)
        {
            string path = DATA_ROOT + "/Clues/" + fileName + ".asset";
            var clue = LoadOrCreate<ClueSO>(path, fileName);
            clue.clueId = clueId;
            clue.clueName = clueName;
            clue.description = desc;
            clue.category = cat;
            if (popupSprite != null)
                clue.clueImage = popupSprite;
            EditorUtility.SetDirty(clue);
            return clue;
        }

        // ================================================================
        //  DIALOGUE TREES
        // ================================================================

        private struct DlgTreeRefs
        {
            public NPCDialogueTreeSO doctor, debtCollector;
            public DialogueSequenceSO hallwayPolice;
        }

        private static DlgTreeRefs CreateDialogueTrees(CharRefs chars, ClueRefs clues, List<string> log)
        {
            var r = new DlgTreeRefs();

            // ── Doctor Dialogue Tree ──
            r.doctor = LoadOrCreate<NPCDialogueTreeSO>(
                DATA_ROOT + "/Dialogues/DlgTree_Doctor.asset", "DlgTree_Doctor");
            r.doctor.dialogueId = "dlg_doctor";
            r.doctor.npcCharacter = chars.doctor;
            r.doctor.playerCharacter = chars.detective;

            r.doctor.greetingLines = new List<DialogueLine>
            {
                new DialogueLine
                {
                    character = chars.doctor,
                    text = "I'm just a doctor. I truly have nothing to do with this.",
                    typingSpeed = 30f, waitForInput = true
                }
            };

            r.doctor.choices = new List<DialogueChoice>
            {
                new DialogueChoice
                {
                    choiceLabel = "The victim's medical records",
                    npcMoodAnimator = LoadAnim("Anim_Doctor_Chill"),
                    responseLines = new List<DialogueLine>
                    {
                        new DialogueLine
                        {
                            character = chars.doctor,
                            text = "She suffered from insomnia and frequently requested sleeping pills.",
                            typingSpeed = 30f, waitForInput = true
                        },
                        new DialogueLine
                        {
                            character = chars.doctor,
                            text = "She had been under significant stress lately and was often irritable.",
                            typingSpeed = 30f, waitForInput = true
                        },
                        new DialogueLine
                        {
                            character = chars.doctor,
                            text = "Other than that, she had been fine, had no known medical conditions.",
                            typingSpeed = 30f, waitForInput = true
                        }
                    },
                    hideAfterChosen = true
                },
                new DialogueChoice
                {
                    choiceLabel = "Relationship with the victim",
                    npcMoodAnimator = LoadAnim("Anim_Doctor_Stress"),
                    responseLines = new List<DialogueLine>
                    {
                        new DialogueLine
                        {
                            character = chars.doctor,
                            text = "We partnered to open a private clinic, but we have recently encountered a little disagreement.",
                            typingSpeed = 30f, waitForInput = true
                        },
                        new DialogueLine
                        {
                            character = chars.doctor,
                            text = "She kept refusing to sign the investment agreement, giving various reasons — like waiting for paperwork, to concerns about feng shui numbers, or claiming she saw no benefit in the investment.",
                            typingSpeed = 30f, waitForInput = true
                        }
                    },
                    hideAfterChosen = true
                }
            };

            r.doctor.farewellLines = new List<DialogueLine>();
            EditorUtility.SetDirty(r.doctor);

            // ── Debt Collector Dialogue Tree ──
            r.debtCollector = LoadOrCreate<NPCDialogueTreeSO>(
                DATA_ROOT + "/Dialogues/DlgTree_DebtCollector.asset", "DlgTree_DebtCollector");
            r.debtCollector.dialogueId = "dlg_debt_collector";
            r.debtCollector.npcCharacter = chars.debtCollector;
            r.debtCollector.playerCharacter = chars.detective;

            r.debtCollector.greetingLines = new List<DialogueLine>
            {
                new DialogueLine
                {
                    character = chars.debtCollector,
                    text = "What? The hell do you want? So how exactly is that chick planning to settle her debt?!",
                    typingSpeed = 30f, waitForInput = true
                }
            };

            r.debtCollector.choices = new List<DialogueChoice>
            {
                new DialogueChoice
                {
                    choiceLabel = "Relationship with the victim",
                    npcMoodAnimator = LoadAnim("Anim_Thuno_Angry"),
                    responseLines = new List<DialogueLine>
                    {
                        new DialogueLine
                        {
                            character = chars.debtCollector,
                            text = "She borrowed money to gamble and dug herself into a massive debt. So now I'm the one who's gotta go out and collect?",
                            typingSpeed = 30f, waitForInput = true
                        },
                        new DialogueLine
                        {
                            character = chars.debtCollector,
                            text = "Been threatening her, scaring the hell outta her, and she still wouldn't cough up the cash. Kept making promises left and right, and even a few days ago she still didn't pay a damn thing.",
                            typingSpeed = 30f, waitForInput = true
                        },
                        new DialogueLine
                        {
                            character = chars.debtCollector,
                            text = "That's a huge amount of money. If she's dead, who the hell's gonna pay me back?!",
                            typingSpeed = 30f, waitForInput = true
                        }
                    },
                    hideAfterChosen = true
                },
                new DialogueChoice
                {
                    choiceLabel = "Information about the victim's personality",
                    npcMoodAnimator = LoadAnim("Anim_Thuno_Thinking"),
                    responseLines = new List<DialogueLine>
                    {
                        new DialogueLine
                        {
                            character = chars.debtCollector,
                            text = "Huh!? She was obsessed with getting rich — had connections everywhere, knew how to do business — but she was hooked on gambling bad. Burned every damn cent at the tables.",
                            typingSpeed = 30f, waitForInput = true
                        },
                        new DialogueLine
                        {
                            character = chars.debtCollector,
                            text = "Kept saying her so-called 'fortune numbers' were gonna hit big and she'd pay everything back. Tch, Lucky numbers my a-.",
                            typingSpeed = 30f, waitForInput = true
                        }
                    },
                    hideAfterChosen = true
                }
            };

            r.debtCollector.farewellLines = new List<DialogueLine>();
            EditorUtility.SetDirty(r.debtCollector);

            // ── Hallway Police (linear) ──
            r.hallwayPolice = LoadOrCreate<DialogueSequenceSO>(
                DATA_ROOT + "/Dialogues/Dlg_HallwayPolice.asset", "Dlg_HallwayPolice");
            r.hallwayPolice.dialogueId = "dlg_hallway_police";
            r.hallwayPolice.lines = new List<DialogueLine>
            {
                new DialogueLine
                {
                    character = chars.police,
                    text = "The case still contains many blind spots. Proceed with caution.",
                    typingSpeed = 30f, waitForInput = true
                }
            };
            EditorUtility.SetDirty(r.hallwayPolice);

            log?.Add("✅ Dialogue Trees: Doctor, DebtCollector, HallwayPolice");
            return r;
        }

        // ================================================================
        //  INTERACTABLES
        // ================================================================

        private struct InteractableRefs
        {
            // Clue interactables
            public ClueInteractableSO contract, debitNote, contactInfo;
            public ClueInteractableSO brokenCabinet, godOfWealth;
            public ClueInteractableSO brokenPhoto, bloodShawl, sleepingPills;
            public ClueInteractableSO frenchMaterials, storagePhotos;

            // NPC interactables
            public NPCInteractableSO doctor, debtCollector, hallwayPolice;

            // Locked
            public LockedItemInteractableSO storageLock;
        }

        private static InteractableRefs CreateInteractables(ClueRefs clues, DlgTreeRefs dlgs, List<string> log)
        {
            var r = new InteractableRefs();
            string dir = DATA_ROOT + "/Interactables/";

            // ── Clue Interactables ──
            r.contract = MakeClueIO(dir, "IO_Contract", "io_contract", clues.contract);
            r.debitNote = MakeClueIO(dir, "IO_DebitNote", "io_debit_note", clues.debitNote);
            r.contactInfo = MakeClueIO(dir, "IO_ContactInfo", "io_contact_info", clues.contactInfo);
            r.brokenCabinet = MakeClueIO(dir, "IO_BrokenCabinet", "io_broken_cabinet", clues.brokenCabinet);
            r.godOfWealth = MakeClueIO(dir, "IO_GodOfWealth", "io_god_of_wealth", clues.godOfWealth);
            r.brokenPhoto = MakeClueIO(dir, "IO_BrokenPhoto", "io_broken_photo", clues.brokenPhoto);
            r.bloodShawl = MakeClueIO(dir, "IO_BloodShawl", "io_blood_shawl", clues.bloodShawl);
            r.sleepingPills = MakeClueIO(dir, "IO_SleepingPills", "io_sleeping_pills", clues.sleepingPills);
            r.frenchMaterials = MakeClueIO(dir, "IO_FrenchMaterials", "io_french_materials", clues.frenchMaterials);
            r.storagePhotos = MakeClueIO(dir, "IO_StoragePhotos", "io_storage_photos", clues.storagePhotos);

            // ── NPC Interactables ──
            r.doctor = LoadOrCreate<NPCInteractableSO>(dir + "IO_Doctor.asset", "IO_Doctor");
            r.doctor.objectId = "io_doctor";
            r.doctor.dialogueTree = dlgs.doctor;
            r.doctor.npcFullBodyAnimator = LoadAnim("Anim_Doctor_Chill");
            r.doctor.hitboxSize = new Vector2(200f, 400f);
            EditorUtility.SetDirty(r.doctor);

            r.debtCollector = LoadOrCreate<NPCInteractableSO>(dir + "IO_DebtCollector.asset", "IO_DebtCollector");
            r.debtCollector.objectId = "io_debt_collector";
            r.debtCollector.dialogueTree = dlgs.debtCollector;
            r.debtCollector.npcFullBodyAnimator = LoadAnim("Anim_Thuno_Angry");
            r.debtCollector.hitboxSize = new Vector2(200f, 400f);
            EditorUtility.SetDirty(r.debtCollector);

            r.hallwayPolice = LoadOrCreate<NPCInteractableSO>(dir + "IO_HallwayPolice.asset", "IO_HallwayPolice");
            r.hallwayPolice.objectId = "io_hallway_police";
            r.hallwayPolice.fallbackDialogue = dlgs.hallwayPolice;
            r.hallwayPolice.npcFullBodyAnimator = LoadAnim("Anim_Police");
            r.hallwayPolice.hitboxSize = new Vector2(200f, 400f);
            EditorUtility.SetDirty(r.hallwayPolice);

            // ── Locked Interactable (Storage) ──
            r.storageLock = LoadOrCreate<LockedItemInteractableSO>(
                dir + "IO_StorageLock.asset", "IO_StorageLock");
            r.storageLock.objectId = "io_storage_lock";
            r.storageLock.hitboxSize = new Vector2(150f, 150f);

            // Create lock config
            var lockCfg = LoadOrCreate<LockConfigSO>(
                DATA_ROOT + "/Locks/Lock_Storage.asset", "Lock_Storage");
            lockCfg.lockType = LockType.Passcode;
            lockCfg.passcode = "6893";
            lockCfg.hintText = "Lộc phát trường tài";
            EditorUtility.SetDirty(lockCfg);
            r.storageLock.lockConfig = lockCfg;

            // Create unlock success actions: toast + show clue detail (collect + UI popup + wait)
            var actToast = LoadOrCreate<ShowToastConfig>(
                DATA_ROOT + "/Actions/Act_Toast_Unlocked.asset", "Act_Toast_Unlocked");
            var soToast = new SerializedObject(actToast);
            soToast.FindProperty("message").stringValue = "Unlocked successfully";
            soToast.ApplyModifiedProperties();
            EditorUtility.SetDirty(actToast);

            // ShowClueDetail = collect data + show popup + wait cho player đóng
            var actShowFrench = LoadOrCreate<ShowClueDetailConfig>(
                DATA_ROOT + "/Actions/Act_ShowClue_FrenchMaterials.asset", "Act_ShowClue_FrenchMaterials");
            var soFrench = new SerializedObject(actShowFrench);
            soFrench.FindProperty("clue").objectReferenceValue = clues.frenchMaterials;
            soFrench.ApplyModifiedProperties();
            EditorUtility.SetDirty(actShowFrench);

            var actShowPhotos = LoadOrCreate<ShowClueDetailConfig>(
                DATA_ROOT + "/Actions/Act_ShowClue_StoragePhotos.asset", "Act_ShowClue_StoragePhotos");
            var soPhotos = new SerializedObject(actShowPhotos);
            soPhotos.FindProperty("clue").objectReferenceValue = clues.storagePhotos;
            soPhotos.ApplyModifiedProperties();
            EditorUtility.SetDirty(actShowPhotos);

            r.storageLock.onUnlockSuccess = new List<ActionStepConfig>
            {
                actToast, actShowFrench, actShowPhotos
            };
            r.storageLock.onUnlockFail = new List<ActionStepConfig>();
            EditorUtility.SetDirty(r.storageLock);

            log?.Add("✅ Interactables: 10 Clue + 3 NPC + 1 Locked");
            return r;
        }

        private static ClueInteractableSO MakeClueIO(string dir, string fileName, string objId, ClueSO clue)
        {
            var io = LoadOrCreate<ClueInteractableSO>(dir + fileName + ".asset", fileName);
            io.objectId = objId;
            io.clue = clue;
            io.isOneTimeOnly = true;
            io.hitboxSize = new Vector2(100f, 100f);
            EditorUtility.SetDirty(io);
            return io;
        }

        // ================================================================
        //  ROOM PREFABS
        // ================================================================

        private static void CreateRoomPrefabs(InteractableRefs io, List<string> log)
        {
            string dir = PREFAB_ROOT + "/Rooms/";
            EnsureDir(dir + "dummy.prefab");

            // ── Background-only rooms ──
            SaveRoomPrefab(dir, "Room_Bathroom", LoadSprite("Map bathroom/bathroom.png"), 1920f, 1080f, null);
            SaveRoomPrefab(dir, "Room_DiningRoom", LoadSprite("Map Dining room/dinning room.png"), 1920f, 1080f, null);
            SaveRoomPrefab(dir, "Room_Courtyard", LoadSprite("Map courty yard/garden co filter.png"), 1920f, 1080f, null);
            SaveRoomPrefab(dir, "Room_SubBedroom", LoadSprite("Map Sub Bedroom/Map sub-bedroom.png"), 1920f, 1080f, null);

            // ── Home Office (props + NPC) ──
            SaveRoomPrefab(dir, "Room_HomeOffice",
                LoadSprite("Map home office/OFFICE room fix co filter.png"), 1920f, 2160f,
                new IOEntry[]
                {
                    new IOEntry(io.contract, LoadSprite("Map home office/Props/Item Ingame/hop dong benh vien ingame.png"), new Vector2(400f, 600f)),
                    new IOEntry(io.debitNote, LoadSprite("Map home office/Props/Item Ingame/Giay to ingame.png"), new Vector2(700f, 500f)),
                    new IOEntry(io.contactInfo, LoadSprite("Map home office/Props/Item Ingame/danh ba dien thoai ingame.png"), new Vector2(1000f, 550f)),
                    new IOEntry(io.doctor, null, new Vector2(960f, 800f), true),
                });

            // ── Living Room (props + NPC) ──
            SaveRoomPrefab(dir, "Room_LivingRoom",
                LoadSprite("Map living room/Living room no prop.png"), 1920f, 2160f,
                new IOEntry[]
                {
                    new IOEntry(io.brokenCabinet, LoadSprite("Map living room/Prop in game/kinh vo.png"), new Vector2(400f, 700f)),
                    new IOEntry(io.godOfWealth, LoadSprite("Map living room/Prop in game/tuong than tai.png"), new Vector2(1200f, 600f)),
                    new IOEntry(io.debtCollector, null, new Vector2(960f, 800f), true),
                });

            // ── Master Bedroom (props only) ──
            SaveRoomPrefab(dir, "Room_MasterBedroom",
                LoadSprite("Map master bedroom/MASTER BEDROOM ap filter no prop.png"), 1920f, 2160f,
                new IOEntry[]
                {
                    new IOEntry(io.brokenPhoto, LoadSprite("Map master bedroom/Props/Item Ingame/anh chan dung ingame.png"), new Vector2(400f, 500f)),
                    new IOEntry(io.bloodShawl, LoadSprite("Map master bedroom/Props/Item Ingame/khăn choàng dính máu.png"), new Vector2(800f, 700f)),
                    new IOEntry(io.sleepingPills, LoadSprite("Map master bedroom/Props/Item Ingame/thuoc ngu ingame.png"), new Vector2(1200f, 600f)),
                });

            // ── Storage (lock → props) ──
            SaveRoomPrefab(dir, "Room_Storage",
                LoadSprite("Map Storage/co filter no prop.png"), 1920f, 1080f,
                new IOEntry[]
                {
                    new IOEntry(io.storageLock, LoadSprite("Map Storage/Props/Item Ingame/prop ket sat.png"), new Vector2(960f, 540f)),
                });

            // ── Hallway (NPC only) ──
            SaveRoomPrefab(dir, "Room_Hallway",
                LoadSprite("Map Hallway/HALLWAY ap filter.png"), 1920f, 1080f,
                new IOEntry[]
                {
                    new IOEntry(io.hallwayPolice, null, new Vector2(960f, 540f), true),
                });

            log?.Add("✅ Room Prefabs: 9 prefabs created (drag IO positions in prefab mode)");
        }

        private struct IOEntry
        {
            public InteractableObjectSO data;
            public Sprite sprite;
            public Vector2 position;
            public bool isNPC;

            public IOEntry(InteractableObjectSO data, Sprite sprite, Vector2 pos, bool isNPC = false)
            {
                this.data = data;
                this.sprite = sprite;
                this.position = pos;
                this.isNPC = isNPC;
            }
        }

        private static void SaveRoomPrefab(string dir, string roomName, Sprite bgSprite,
            float bgW, float bgH, IOEntry[] entries)
        {
            var root = new GameObject(roomName, typeof(RectTransform));
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(bgW, bgH);
            rootRect.pivot = new Vector2(0.5f, 0.5f);

            // Background
            var bgGo = MakeUI("Background", root.transform);
            var bgRect = bgGo.GetComponent<RectTransform>();
            StretchFull(bgRect);
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.raycastTarget = true;
            if (bgSprite != null)
            {
                bgImg.sprite = bgSprite;
                bgImg.preserveAspect = false;
            }

            // Interactable Objects
            if (entries != null)
            {
                foreach (var e in entries)
                {
                    string objName = e.data != null ? e.data.objectId : "Unknown";
                    var ioGo = MakeUI("IO_" + objName, root.transform);
                    var ioRect = ioGo.GetComponent<RectTransform>();
                    ioRect.anchorMin = ioRect.anchorMax = new Vector2(0.5f, 0.5f);
                    ioRect.pivot = new Vector2(0.5f, 0.5f);

                    // Vị trí tương đối so với center (bạn kéo lại trong Prefab Mode)
                    ioRect.anchoredPosition = e.position - new Vector2(bgW / 2f, bgH / 2f);

                    var hitboxSize = e.data != null ? e.data.hitboxSize : new Vector2(100f, 100f);
                    ioRect.sizeDelta = e.isNPC ? new Vector2(200f, 400f) : hitboxSize;

                    // Sprite hiển thị (ingame sprite)
                    var ioImg = ioGo.AddComponent<Image>();
                    ioImg.raycastTarget = true;
                    if (e.sprite != null)
                    {
                        ioImg.sprite = e.sprite;
                        ioImg.preserveAspect = true;
                        ioImg.SetNativeSize();
                    }
                    else
                    {
                        ioImg.color = new Color(1f, 1f, 1f, 0.3f); // placeholder
                    }

                    // Highlight child
                    var hlGo = MakeUI("Highlight", ioGo.transform);
                    StretchFull(hlGo.GetComponent<RectTransform>());
                    var hlImg = hlGo.AddComponent<Image>();
                    hlImg.color = new Color(1f, 1f, 0f, 0.3f);
                    hlImg.enabled = false;

                    // Add InteractableObject component + wire
                    var ioComp = ioGo.AddComponent<InteractableObject>();
                    var soIO = new SerializedObject(ioComp);
                    soIO.FindProperty("data").objectReferenceValue = e.data;
                    soIO.FindProperty("imgHighlight").objectReferenceValue = hlImg;
                    soIO.FindProperty("rectTransform").objectReferenceValue = ioRect;
                    soIO.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            // Save prefab
            string path = dir + roomName + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static Sprite LoadSprite(string relativePath)
        {
            string path = ART_ROOT + "/" + relativePath;
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        // ================================================================
        //  ROOMS
        // ================================================================

        private struct RoomRefs
        {
            public RoomSO bathroom, diningRoom, courtyard, subBedroom;
            public RoomSO homeOffice, livingRoom, masterBedroom, storage, hallway;
        }

        private static RoomRefs CreateRooms(List<string> log)
        {
            var r = new RoomRefs();
            string dir = DATA_ROOT + "/Rooms/";
            string prefabDir = PREFAB_ROOT + "/Rooms/";

            r.bathroom = MakeRoom(dir, "Room_Bathroom", "bathroom", "BATHROOM", prefabDir);
            r.diningRoom = MakeRoom(dir, "Room_DiningRoom", "dining_room", "DINING ROOM", prefabDir);
            r.courtyard = MakeRoom(dir, "Room_Courtyard", "courtyard", "COURTYARD", prefabDir);
            r.subBedroom = MakeRoom(dir, "Room_SubBedroom", "sub_bedroom", "SUB BEDROOM", prefabDir);
            r.homeOffice = MakeRoom(dir, "Room_HomeOffice", "home_office", "HOME OFFICE", prefabDir);
            r.livingRoom = MakeRoom(dir, "Room_LivingRoom", "living_room", "LIVING ROOM", prefabDir);
            r.masterBedroom = MakeRoom(dir, "Room_MasterBedroom", "master_bedroom", "MASTER BEDROOM", prefabDir);
            r.storage = MakeRoom(dir, "Room_Storage", "storage", "STORAGE", prefabDir);
            r.hallway = MakeRoom(dir, "Room_Hallway", "hallway", "HALLWAY", prefabDir);

            log?.Add("✅ Rooms: 9 rooms created with prefabs assigned");
            return r;
        }

        private static RoomSO MakeRoom(string dir, string fileName, string roomId, string roomName, string prefabDir)
        {
            var room = LoadOrCreate<RoomSO>(dir + fileName + ".asset", fileName);
            room.roomId = roomId;
            room.roomName = roomName;

            // Auto-assign prefab nếu tồn tại
            string prefabPath = prefabDir + fileName + ".prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
                room.roomPrefab = prefab;

            EditorUtility.SetDirty(room);
            return room;
        }

        // ================================================================
        //  ASSIGN ROOMS TO MAPS
        // ================================================================

        private static void AssignRoomsToMaps(RoomRefs rooms, List<string> log)
        {
            string mapDir = DATA_ROOT + "/Maps/";

            // Map name → room mapping
            var mapping = new Dictionary<string, RoomSO>
            {
                { "1.LivingRoom", rooms.livingRoom },
                { "2.BedRoom", rooms.subBedroom },
                { "3.Storage", rooms.storage },
                { "4.Bathroom", rooms.bathroom },
                { "5.HomeOffice", rooms.homeOffice },
                { "6.Courtyard", rooms.courtyard },
                { "7. MasterBedroom", rooms.masterBedroom },
                { "8.Dinningroom", rooms.diningRoom },
                { "9.Hallway", rooms.hallway },
            };

            int assigned = 0;
            foreach (var kvp in mapping)
            {
                string path = mapDir + kvp.Key + ".asset";
                var map = AssetDatabase.LoadAssetAtPath<MapSO>(path);
                if (map != null)
                {
                    map.rooms = new List<RoomSO> { kvp.Value };
                    EditorUtility.SetDirty(map);
                    assigned++;
                }
            }

            log?.Add($"✅ Maps: {assigned}/9 maps assigned rooms");
        }

        // ================================================================
        //  NPC DIALOGUE PREFAB
        // ================================================================

        public static GameObject CreateNPCDialoguePrefab_Internal(List<string> log)
        {
            var root = new GameObject("UINPCDialogue", typeof(RectTransform), typeof(CanvasGroup));
            StretchFull(root.GetComponent<RectTransform>());

            // Background
            var bg = MakeUI("Background", root.transform);
            bg.AddComponent<Image>().color = C_DARK_BG;
            StretchFull(bg.GetComponent<RectTransform>());

            // ── NPC Full Body (left 40%) ──
            var npcBody = MakeUI("NPCFullBody", root.transform);
            var nbRect = npcBody.GetComponent<RectTransform>();
            nbRect.anchorMin = new Vector2(0f, 0f); nbRect.anchorMax = new Vector2(0.4f, 1f);
            nbRect.offsetMin = new Vector2(30f, 20f); nbRect.offsetMax = new Vector2(0f, -20f);
            var canvasGroupNPC = npcBody.AddComponent<CanvasGroup>();
            var npcImg = npcBody.AddComponent<Image>();
            npcImg.preserveAspect = true; npcImg.raycastTarget = false;
            var animNPC = npcBody.AddComponent<Animator>();
            animNPC.enabled = false;

            // ── NPC Label (above board) ──
            var label = MakeUI("NPCLabel", root.transform);
            var lRect = label.GetComponent<RectTransform>();
            lRect.anchorMin = new Vector2(0.5f, 1f); lRect.anchorMax = new Vector2(0.5f, 1f);
            lRect.pivot = new Vector2(0.5f, 1f);
            lRect.sizeDelta = new Vector2(250f, 45f);
            lRect.anchoredPosition = new Vector2(0f, -15f);
            label.AddComponent<Image>().color = C_LABEL_BG;
            var labelTextGo = MakeUI("Text", label.transform);
            StretchFull(labelTextGo.GetComponent<RectTransform>());
            var labelTMP = labelTextGo.AddComponent<TextMeshProUGUI>();
            labelTMP.text = "NPC NAME"; labelTMP.fontSize = 22f; labelTMP.fontStyle = FontStyles.Bold;
            labelTMP.alignment = TextAlignmentOptions.Center;
            labelTMP.color = C_LABEL_TEXT; labelTMP.raycastTarget = false;

            // ── Settings Button (top right) ──
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

            // ── Board (center-right, NPC text) ──
            var board = MakeUI("Board", root.transform);
            var bRect = board.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.35f, 0.3f); bRect.anchorMax = new Vector2(0.88f, 0.88f);
            board.AddComponent<Image>().color = C_BOARD_BG;
            var boardTextGo = MakeUI("BoardText", board.transform);
            var btRect = boardTextGo.GetComponent<RectTransform>();
            StretchFull(btRect); btRect.offsetMin = new Vector2(20f, 20f); btRect.offsetMax = new Vector2(-20f, -20f);
            var boardTMP = boardTextGo.AddComponent<TextMeshProUGUI>();
            boardTMP.text = ""; boardTMP.fontSize = 20f;
            boardTMP.alignment = TextAlignmentOptions.Center;
            boardTMP.color = C_BOARD_TEXT; boardTMP.raycastTarget = false;

            // ── Choice Buttons Container (below board, above detective portrait) ──
            var choiceCont = MakeUI("ChoiceContainer", root.transform);
            var ccRect = choiceCont.GetComponent<RectTransform>();
            ccRect.anchorMin = new Vector2(0.35f, 0.02f); ccRect.anchorMax = new Vector2(0.82f, 0.28f);
            ccRect.offsetMin = new Vector2(10f, 0f); ccRect.offsetMax = new Vector2(-10f, 0f);
            var vlg = choiceCont.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8f; vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true; vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;

            // Choice button prefab (1 prefab, MasterHelper.InitListObj sẽ pool)
            var choicePfGo = MakeUI("ChoiceButtonPrefab", choiceCont.transform);
            choicePfGo.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 50f);
            choicePfGo.AddComponent<LayoutElement>().preferredHeight = 50f;
            choicePfGo.AddComponent<Image>().color = C_CHOICE_BG;
            var choicePfBtn = choicePfGo.AddComponent<Button>();
            var choicePfLabel = MakeUI("Label", choicePfGo.transform);
            StretchFull(choicePfLabel.GetComponent<RectTransform>());
            var choicePfTMP = choicePfLabel.AddComponent<TextMeshProUGUI>();
            choicePfTMP.text = "Choice"; choicePfTMP.fontSize = 18f;
            choicePfTMP.alignment = TextAlignmentOptions.Center;
            choicePfTMP.color = C_CHOICE_TEXT; choicePfTMP.raycastTarget = false;
            var choicePfItem = choicePfGo.AddComponent<ChoiceButtonItem>();
            var choicePfSO = new SerializedObject(choicePfItem);
            choicePfSO.FindProperty("txtLabel").objectReferenceValue = choicePfTMP;
            choicePfSO.FindProperty("btnSelect").objectReferenceValue = choicePfBtn;
            choicePfSO.ApplyModifiedPropertiesWithoutUndo();
            choicePfGo.SetActive(false); // prefab bắt đầu ẩn, InitListObj sẽ clone

            // ── Detective Dialogue Box (bottom strip — portrait + text) ──
            var detDialogueBox = MakeUI("DetectiveDialogueBox", root.transform);
            var ddbRect = detDialogueBox.GetComponent<RectTransform>();
            ddbRect.anchorMin = new Vector2(0.35f, 0f); ddbRect.anchorMax = new Vector2(1f, 0.15f);
            ddbRect.offsetMin = new Vector2(10f, 10f); ddbRect.offsetMax = new Vector2(-10f, 0f);
            detDialogueBox.AddComponent<Image>().color = C_DIALOGUE_BG;

            // Detective text (left side of dialogue box)
            var detTextGo = MakeUI("DetectiveText", detDialogueBox.transform);
            var dtRect = detTextGo.GetComponent<RectTransform>();
            dtRect.anchorMin = Vector2.zero; dtRect.anchorMax = new Vector2(0.82f, 1f);
            dtRect.offsetMin = new Vector2(15f, 8f); dtRect.offsetMax = new Vector2(-10f, -8f);
            var detTMP = detTextGo.AddComponent<TextMeshProUGUI>();
            detTMP.text = ""; detTMP.fontSize = 18f;
            detTMP.alignment = TextAlignmentOptions.MidlineLeft;
            detTMP.color = C_DIALOGUE_TEXT; detTMP.raycastTarget = false;

            // Detective portrait (right side of dialogue box)
            var detPort = MakeUI("DetectivePortrait", detDialogueBox.transform);
            var dpRect = detPort.GetComponent<RectTransform>();
            dpRect.anchorMin = new Vector2(1f, 0f); dpRect.anchorMax = new Vector2(1f, 1f);
            dpRect.pivot = new Vector2(1f, 0.5f);
            dpRect.sizeDelta = new Vector2(100f, 0f);
            dpRect.anchoredPosition = new Vector2(5f, 0f);
            var detImg = detPort.AddComponent<Image>();
            detImg.preserveAspect = true; detImg.raycastTarget = false;
            var detAnim = detPort.AddComponent<Animator>();
            detAnim.enabled = false;

            detDialogueBox.SetActive(false); // ẩn ban đầu

            // ── Next Button (bottom center-right) ──
            var nxtGo = MakeUI("BtnNext", root.transform);
            var nxRect = nxtGo.GetComponent<RectTransform>();
            nxRect.anchorMin = nxRect.anchorMax = new Vector2(0.85f, 0.02f);
            nxRect.pivot = new Vector2(0.5f, 0f);
            nxRect.sizeDelta = new Vector2(80f, 35f);
            nxtGo.AddComponent<Image>().color = new Color(0.3f, 0.4f, 0.35f, 0.9f);
            var btnNext = nxtGo.AddComponent<Button>();
            var nxLabel = MakeUI("Label", nxtGo.transform);
            StretchFull(nxLabel.GetComponent<RectTransform>());
            var nxTMP = nxLabel.AddComponent<TextMeshProUGUI>();
            nxTMP.text = "▶"; nxTMP.fontSize = 22f;
            nxTMP.alignment = TextAlignmentOptions.Center;
            nxTMP.color = Color.white; nxTMP.raycastTarget = false;

            // ── Wire UINPCDialogue ──
            var ui = root.AddComponent<UINPCDialogue>();
            ui.uiName = UIName.NPCDialogue;
            var so = new SerializedObject(ui);
            so.FindProperty("imgNPCFullBody").objectReferenceValue = npcImg;
            so.FindProperty("animNPCFullBody").objectReferenceValue = animNPC;
            so.FindProperty("canvasGroupNPC").objectReferenceValue = canvasGroupNPC;
            so.FindProperty("txtNPCLabel").objectReferenceValue = labelTMP;
            so.FindProperty("txtBoardText").objectReferenceValue = boardTMP;
            so.FindProperty("detectiveDialogueBox").objectReferenceValue = detDialogueBox;
            so.FindProperty("txtDetectiveText").objectReferenceValue = detTMP;
            so.FindProperty("imgDetectivePortrait").objectReferenceValue = detImg;
            so.FindProperty("animDetectivePortrait").objectReferenceValue = detAnim;
            so.FindProperty("choiceContainer").objectReferenceValue = choiceCont.transform;
            so.FindProperty("choiceButtonPrefab").objectReferenceValue = choicePfItem;
            so.FindProperty("btnNext").objectReferenceValue = btnNext;
            so.FindProperty("btnSettings").objectReferenceValue = btnSettings;
            so.ApplyModifiedPropertiesWithoutUndo();

            log?.Add("✅ UINPCDialogue prefab created");
            return root;
        }

        // ================================================================
        //  PAUSE PREFAB
        // ================================================================

        [MenuItem("DoMiTruth/Create Pause Prefab", false, 104)]
        public static void CreatePausePrefab()
        {
            var log = new List<string>();
            var go = CreatePausePrefab_Internal(log);
            if (go == null) return;
            string path = PREFAB_ROOT + "/Popups/UIPause.prefab";
            EnsureDir(path);
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);

            // Register to UIRegistry
            var registry = AssetDatabase.LoadAssetAtPath<UIRegistrySO>(
                "Assets/Luzart/DoMiTruth/Data/Config/UIRegistry.asset");
            if (registry != null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var uiBase = prefab.GetComponent<UIBase>();
                if (uiBase != null)
                {
                    var so = new SerializedObject(registry);
                    var entries = so.FindProperty("entries");
                    RegisterEntry(entries, UIName.Pause, uiBase);
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(registry);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Done!", string.Join("\n", log), "OK");
        }

        public static GameObject CreatePausePrefab_Internal(List<string> log)
        {
            var root = new GameObject("UIPause", typeof(RectTransform), typeof(CanvasGroup));
            StretchFull(root.GetComponent<RectTransform>());

            // Semi-transparent overlay
            var overlay = MakeUI("Overlay", root.transform);
            StretchFull(overlay.GetComponent<RectTransform>());
            var overlayImg = overlay.AddComponent<Image>();
            overlayImg.color = new Color(0f, 0f, 0f, 0.7f);

            // Panel center
            var panel = MakeUI("Panel", root.transform);
            var pRect = panel.GetComponent<RectTransform>();
            pRect.anchorMin = new Vector2(0.5f, 0.5f);
            pRect.anchorMax = new Vector2(0.5f, 0.5f);
            pRect.pivot = new Vector2(0.5f, 0.5f);
            pRect.sizeDelta = new Vector2(400f, 350f);
            panel.AddComponent<Image>().color = new Color(0.12f, 0.10f, 0.08f, 0.95f);

            // Title
            var titleGo = MakeUI("Title", panel.transform);
            var tRect = titleGo.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0f, 1f);
            tRect.anchorMax = new Vector2(1f, 1f);
            tRect.pivot = new Vector2(0.5f, 1f);
            tRect.sizeDelta = new Vector2(0f, 60f);
            tRect.anchoredPosition = new Vector2(0f, -15f);
            var titleTMP = titleGo.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "PAUSED";
            titleTMP.fontSize = 32f;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color = C_LABEL_TEXT;
            titleTMP.raycastTarget = false;

            // Buttons container
            var btnContainer = MakeUI("Buttons", panel.transform);
            var bcRect = btnContainer.GetComponent<RectTransform>();
            bcRect.anchorMin = new Vector2(0.1f, 0.05f);
            bcRect.anchorMax = new Vector2(0.9f, 0.78f);
            var vlg = btnContainer.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 15f;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Continue button
            var btnContinue = MakePauseButton(btnContainer.transform, "BtnContinue", "CONTINUE", C_LABEL_BG);
            // Settings button
            var btnSettings = MakePauseButton(btnContainer.transform, "BtnSettings", "SETTINGS", C_CHOICE_BG);
            // Back to Map button
            var btnBackToMap = MakePauseButton(btnContainer.transform, "BtnBackToMap", "BACK TO MAP",
                new Color(0.5f, 0.2f, 0.15f, 0.9f));

            // Wire UIPause
            var ui = root.AddComponent<UIPause>();
            ui.uiName = UIName.Pause;
            var so = new SerializedObject(ui);
            so.FindProperty("btnContinue").objectReferenceValue = btnContinue;
            so.FindProperty("btnSettings").objectReferenceValue = btnSettings;
            so.FindProperty("btnBackToMap").objectReferenceValue = btnBackToMap;
            so.ApplyModifiedPropertiesWithoutUndo();

            log?.Add("✅ UIPause prefab created");
            return root;
        }

        private static Button MakePauseButton(Transform parent, string name, string label, Color bgColor)
        {
            var btnGo = MakeUI(name, parent);
            var btnRect = btnGo.GetComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(0f, 55f);
            btnGo.AddComponent<LayoutElement>().preferredHeight = 55f;
            btnGo.AddComponent<Image>().color = bgColor;
            var btn = btnGo.AddComponent<Button>();

            var labelGo = MakeUI("Label", btnGo.transform);
            StretchFull(labelGo.GetComponent<RectTransform>());
            var tmp = labelGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 22f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = C_CHOICE_TEXT;
            tmp.raycastTarget = false;

            return btn;
        }

        // ================================================================
        //  LOCK PUZZLE PREFAB
        // ================================================================

        private const string ART_STORAGE = "Assets/Art/SOURCE CHÍNH THỨC/Map Storage/Tách layer két sắt";

        [MenuItem("DoMiTruth/🔄 Create Transition Prefab", false, 107)]
        public static void CreateTransitionPrefab()
        {
            var log = new List<string>();
            var go = CreateTransitionPrefab_Internal(log);
            if (go == null) return;
            string path = PREFAB_ROOT + "/Components/UITransition.prefab";
            EnsureDir(path);
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Done!", string.Join("\n", log) +
                "\n\nKéo prefab UITransition vào field 'Transition Prefab' trên GameBootstrap.", "OK");
        }

        [MenuItem("DoMiTruth/📓 Create Notebook Prefab", false, 106)]
        public static void CreateNotebookPrefab()
        {
            var log = new List<string>();
            var go = CreateNotebookPrefab_Internal(log);
            if (go == null) return;
            string path = PREFAB_ROOT + "/Screens/UINotebook.prefab";
            EnsureDir(path);
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            AssetDatabase.Refresh();

            // Register to UIRegistry
            var registry = AssetDatabase.LoadAssetAtPath<UIRegistrySO>(
                DATA_ROOT + "/Config/UIRegistry.asset");
            if (registry != null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var uiBase = prefab.GetComponent<UIBase>();
                if (uiBase != null)
                {
                    var regSO = new SerializedObject(registry);
                    var entries = regSO.FindProperty("entries");
                    RegisterEntry(entries, UIName.Notebook, uiBase);
                    regSO.ApplyModifiedProperties();
                    EditorUtility.SetDirty(registry);
                }
            }

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Done!", string.Join("\n", log), "OK");
        }

        [MenuItem("DoMiTruth/Create Lock Puzzle Prefab", false, 105)]
        public static void CreateLockPuzzlePrefab()
        {
            var log = new List<string>();
            var go = CreateLockPuzzlePrefab_Internal(log);
            if (go == null) return;
            string path = PREFAB_ROOT + "/Popups/UILockPuzzle.prefab";
            EnsureDir(path);
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);

            var registry = AssetDatabase.LoadAssetAtPath<UIRegistrySO>(
                "Assets/Luzart/DoMiTruth/Data/Config/UIRegistry.asset");
            if (registry != null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var uiBase = prefab.GetComponent<UIBase>();
                if (uiBase != null)
                {
                    var so = new SerializedObject(registry);
                    var entries = so.FindProperty("entries");
                    RegisterEntry(entries, UIName.LockPuzzle, uiBase);
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(registry);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Done!", string.Join("\n", log), "OK");
        }

        private static Sprite EnsureSpriteImport(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        public static GameObject CreateLockPuzzlePrefab_Internal(List<string> log)
        {
            // Load sprites (set default image)
            var safeSprite = EnsureSpriteImport(ART_STORAGE + "/Ket sat tach layer.png");
            var numpadSprite = EnsureSpriteImport(ART_STORAGE + "/nubmer.png");

            var root = new GameObject("UILockPuzzle", typeof(RectTransform), typeof(CanvasGroup));
            StretchFull(root.GetComponent<RectTransform>());

            // Semi-transparent overlay
            var overlay = MakeUI("Overlay", root.transform);
            StretchFull(overlay.GetComponent<RectTransform>());
            overlay.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);

            // ── Safe image (center) ──
            var safeGo = MakeUI("SafeImage", root.transform);
            var safeRect = safeGo.GetComponent<RectTransform>();
            safeRect.anchorMin = new Vector2(0.5f, 0.5f);
            safeRect.anchorMax = new Vector2(0.5f, 0.5f);
            safeRect.pivot = new Vector2(0.5f, 0.5f);
            safeRect.sizeDelta = new Vector2(600f, 700f);
            var safeImg = safeGo.AddComponent<Image>();
            safeImg.preserveAspect = true;
            safeImg.raycastTarget = false;
            if (safeSprite != null) safeImg.sprite = safeSprite;

            // ── Passcode Panel (trên safe) ──
            var passcodePanel = MakeUI("PasscodePanel", root.transform);
            var ppRect = passcodePanel.GetComponent<RectTransform>();
            ppRect.anchorMin = new Vector2(0.5f, 0.5f);
            ppRect.anchorMax = new Vector2(0.5f, 0.5f);
            ppRect.pivot = new Vector2(0.5f, 0.5f);
            ppRect.sizeDelta = new Vector2(350f, 500f);

            // ── Passcode Display (top) — Image bg + child TMP ──
            var displayGo = MakeUI("PasscodeDisplay", passcodePanel.transform);
            var dRect = displayGo.GetComponent<RectTransform>();
            dRect.anchorMin = new Vector2(0.05f, 0.85f);
            dRect.anchorMax = new Vector2(0.95f, 0.95f);
            displayGo.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.05f, 0.9f);
            var displayTextGo = MakeUI("Text", displayGo.transform);
            StretchFull(displayTextGo.GetComponent<RectTransform>());
            var displayTMP = displayTextGo.AddComponent<TextMeshProUGUI>();
            displayTMP.text = "";
            displayTMP.fontSize = 36f;
            displayTMP.fontStyle = FontStyles.Bold;
            displayTMP.alignment = TextAlignmentOptions.Center;
            displayTMP.color = new Color(0.0f, 1f, 0.3f, 1f); // green LED
            displayTMP.raycastTarget = false;

            // ── Hint text ──
            var hintGo = MakeUI("HintText", passcodePanel.transform);
            var hRect = hintGo.GetComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0.05f, 0.78f);
            hRect.anchorMax = new Vector2(0.95f, 0.84f);
            var hintTMP = hintGo.AddComponent<TextMeshProUGUI>();
            hintTMP.text = ""; hintTMP.fontSize = 14f;
            hintTMP.alignment = TextAlignmentOptions.Center;
            hintTMP.color = new Color(0.7f, 0.65f, 0.55f, 1f);
            hintTMP.raycastTarget = false;

            // ── Error text ──
            var errGo = MakeUI("ErrorText", passcodePanel.transform);
            var eRect = errGo.GetComponent<RectTransform>();
            eRect.anchorMin = new Vector2(0.05f, 0.72f);
            eRect.anchorMax = new Vector2(0.95f, 0.78f);
            var errTMP = errGo.AddComponent<TextMeshProUGUI>();
            errTMP.text = "WRONG CODE"; errTMP.fontSize = 14f;
            errTMP.alignment = TextAlignmentOptions.Center;
            errTMP.color = new Color(1f, 0.3f, 0.2f, 1f);
            errTMP.raycastTarget = false;
            errGo.SetActive(false);

            // ── Numpad Grid (4 rows x 3 cols) ──
            // Layout: 1 2 3 / 4 5 6 / 7 8 9 / DEL 0 OK
            var gridGo = MakeUI("NumpadGrid", passcodePanel.transform);
            var gRect = gridGo.GetComponent<RectTransform>();
            gRect.anchorMin = new Vector2(0.05f, 0.05f);
            gRect.anchorMax = new Vector2(0.95f, 0.72f);
            var grid = gridGo.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.cellSize = new Vector2(95f, 70f);
            grid.spacing = new Vector2(10f, 10f);
            grid.childAlignment = TextAnchor.MiddleCenter;

            // Button order: 1-9, DEL, 0, OK
            string[] labels = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "DEL", "0", "OK" };
            Button[] numpadBtns = new Button[10]; // 0-9
            Button btnDelete = null;
            Button btnConfirm = null;

            for (int i = 0; i < labels.Length; i++)
            {
                var btnGo = MakeUI("Btn_" + labels[i], gridGo.transform);
                var btnImg = btnGo.AddComponent<Image>();
                btnImg.preserveAspect = false;

                if (labels[i] == "DEL" || labels[i] == "OK")
                {
                    // DEL and OK use color background
                    btnImg.color = labels[i] == "DEL"
                        ? new Color(0.5f, 0.2f, 0.15f, 0.9f)
                        : new Color(0.15f, 0.4f, 0.2f, 0.9f);
                }
                else
                {
                    // Number buttons use numpad sprite
                    if (numpadSprite != null) btnImg.sprite = numpadSprite;
                    btnImg.color = Color.white;
                }

                var btn = btnGo.AddComponent<Button>();

                var lblGo = MakeUI("Label", btnGo.transform);
                StretchFull(lblGo.GetComponent<RectTransform>());
                var lblTMP = lblGo.AddComponent<TextMeshProUGUI>();
                lblTMP.text = labels[i];
                lblTMP.fontSize = labels[i] == "DEL" || labels[i] == "OK" ? 18f : 28f;
                lblTMP.fontStyle = FontStyles.Bold;
                lblTMP.alignment = TextAlignmentOptions.Center;
                lblTMP.color = Color.white;
                lblTMP.raycastTarget = false;

                if (labels[i] == "DEL")
                {
                    btnDelete = btn;
                }
                else if (labels[i] == "OK")
                {
                    btnConfirm = btn;
                }
                else
                {
                    int digit = int.Parse(labels[i]);
                    numpadBtns[digit] = btn;
                }
            }

            // ── Close button (top right) ──
            var closeGo = MakeUI("BtnClose", root.transform);
            var clRect = closeGo.GetComponent<RectTransform>();
            clRect.anchorMin = clRect.anchorMax = Vector2.one;
            clRect.pivot = Vector2.one;
            clRect.sizeDelta = new Vector2(50f, 50f);
            clRect.anchoredPosition = new Vector2(-20f, -20f);
            closeGo.AddComponent<Image>().color = new Color(0.5f, 0.2f, 0.15f, 0.9f);
            var btnClose = closeGo.AddComponent<Button>();
            var closeLabel = MakeUI("Label", closeGo.transform);
            StretchFull(closeLabel.GetComponent<RectTransform>());
            var closeTMP = closeLabel.AddComponent<TextMeshProUGUI>();
            closeTMP.text = "X"; closeTMP.fontSize = 24f; closeTMP.fontStyle = FontStyles.Bold;
            closeTMP.alignment = TextAlignmentOptions.Center;
            closeTMP.color = Color.white; closeTMP.raycastTarget = false;

            // ── Wire UILockPuzzle ──
            var ui = root.AddComponent<UILockPuzzle>();
            ui.uiName = UIName.LockPuzzle;
            var so = new SerializedObject(ui);
            so.FindProperty("txtPasscodeDisplay").objectReferenceValue = displayTMP;
            so.FindProperty("txtHint").objectReferenceValue = hintTMP;
            so.FindProperty("txtError").objectReferenceValue = errTMP;
            so.FindProperty("btnConfirm").objectReferenceValue = btnConfirm;
            so.FindProperty("btnDelete").objectReferenceValue = btnDelete;

            // Numpad buttons array (0-9)
            var numpadProp = so.FindProperty("numpadButtons");
            numpadProp.arraySize = 10;
            for (int d = 0; d < 10; d++)
            {
                numpadProp.GetArrayElementAtIndex(d).objectReferenceValue = numpadBtns[d];
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            log?.Add("✅ UILockPuzzle prefab created (safe bg + numpad 0-9 + DEL + OK)");
            return root;
        }

        // ================================================================
        //  NOTEBOOK PREFAB
        // ================================================================

        private const string ART_NOTEBOOK = "Assets/Art/SOURCE CHÍNH THỨC/UI/Notebook/Elements";

        public static GameObject CreateNotebookPrefab_Internal(List<string> log)
        {
            // Load sprites
            var spBiaSo = EnsureSpriteImport(ART_NOTEBOOK + "/bia so.png");
            var spTrang1 = EnsureSpriteImport(ART_NOTEBOOK + "/trang 1 ghi thong tin note cua props.png");
            var spPolaroidProp = EnsureSpriteImport(ART_NOTEBOOK + "/anh prop.png");
            var spNameTag = EnsureSpriteImport(ART_NOTEBOOK + "/ten props.png");

            var root = new GameObject("UINotebook", typeof(RectTransform), typeof(CanvasGroup));
            StretchFull(root.GetComponent<RectTransform>());

            // ── Dark overlay background ──
            var overlay = MakeUI("Overlay", root.transform);
            overlay.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);
            StretchFull(overlay.GetComponent<RectTransform>());

            // ── Notebook Background (bìa sổ mở 2 trang, center) ──
            var notebookBg = MakeUI("NotebookBg", root.transform);
            var nbRect = notebookBg.GetComponent<RectTransform>();
            nbRect.anchorMin = new Vector2(0.5f, 0.5f);
            nbRect.anchorMax = new Vector2(0.5f, 0.5f);
            nbRect.pivot = new Vector2(0.5f, 0.5f);
            nbRect.sizeDelta = new Vector2(900f, 600f);
            var nbImg = notebookBg.AddComponent<Image>();
            if (spBiaSo != null) nbImg.sprite = spBiaSo;
            nbImg.preserveAspect = true;

            // ── Trang trái — Description ──
            var leftPage = MakeUI("LeftPage", notebookBg.transform);
            var lpRect = leftPage.GetComponent<RectTransform>();
            lpRect.anchorMin = new Vector2(0.03f, 0.05f);
            lpRect.anchorMax = new Vector2(0.48f, 0.92f);
            var leftPageImg = leftPage.AddComponent<Image>();
            if (spTrang1 != null) leftPageImg.sprite = spTrang1;
            leftPageImg.preserveAspect = false;
            leftPageImg.raycastTarget = false;

            // Description text trên trang trái
            var descGo = MakeUI("TxtDescription", leftPage.transform);
            var descRect = descGo.GetComponent<RectTransform>();
            StretchFull(descRect);
            descRect.offsetMin = new Vector2(25f, 30f);
            descRect.offsetMax = new Vector2(-25f, -30f);
            var descTMP = descGo.AddComponent<TextMeshProUGUI>();
            descTMP.text = "";
            descTMP.fontSize = 16f;
            descTMP.alignment = TextAlignmentOptions.TopLeft;
            descTMP.color = new Color(0.2f, 0.15f, 0.1f, 1f);
            descTMP.raycastTarget = false;

            // ── Trang phải ──
            var rightPage = MakeUI("RightPage", notebookBg.transform);
            var rpRect = rightPage.GetComponent<RectTransform>();
            rpRect.anchorMin = new Vector2(0.52f, 0.05f);
            rpRect.anchorMax = new Vector2(0.97f, 0.92f);
            var rightPageImg = rightPage.AddComponent<Image>();
            rightPageImg.color = new Color(0.75f, 0.73f, 0.7f, 1f);
            rightPageImg.raycastTarget = false;

            // Polaroid frame (trên cùng trang phải)
            var polaroid = MakeUI("PolaroidFrame", rightPage.transform);
            var polRect = polaroid.GetComponent<RectTransform>();
            polRect.anchorMin = new Vector2(0.1f, 0.35f);
            polRect.anchorMax = new Vector2(0.9f, 0.95f);
            var polImg = polaroid.AddComponent<Image>();
            if (spPolaroidProp != null) polImg.sprite = spPolaroidProp;
            polImg.preserveAspect = true;
            polImg.raycastTarget = false;

            // Photo bên trong polaroid
            var photo = MakeUI("CluePhoto", polaroid.transform);
            var phRect = photo.GetComponent<RectTransform>();
            phRect.anchorMin = new Vector2(0.08f, 0.12f);
            phRect.anchorMax = new Vector2(0.92f, 0.88f);
            var phImg = photo.AddComponent<Image>();
            phImg.preserveAspect = true;
            phImg.color = Color.white;
            phImg.raycastTarget = false;

            // Name tag (giấy rách — dưới polaroid)
            var nameTag = MakeUI("NameTag", rightPage.transform);
            var ntRect = nameTag.GetComponent<RectTransform>();
            ntRect.anchorMin = new Vector2(0.1f, 0.08f);
            ntRect.anchorMax = new Vector2(0.9f, 0.32f);
            var ntImg = nameTag.AddComponent<Image>();
            if (spNameTag != null) ntImg.sprite = spNameTag;
            ntImg.preserveAspect = true;
            ntImg.raycastTarget = false;

            // Clue name text trên name tag
            var nameGo = MakeUI("TxtName", nameTag.transform);
            var nameRect = nameGo.GetComponent<RectTransform>();
            StretchFull(nameRect);
            nameRect.offsetMin = new Vector2(10f, 5f);
            nameRect.offsetMax = new Vector2(-10f, -20f);
            var nameTMP = nameGo.AddComponent<TextMeshProUGUI>();
            nameTMP.text = "";
            nameTMP.fontSize = 16f;
            nameTMP.fontStyle = FontStyles.Bold;
            nameTMP.alignment = TextAlignmentOptions.Center;
            nameTMP.color = new Color(0.2f, 0.15f, 0.1f, 1f);
            nameTMP.raycastTarget = false;

            // Category text (nhỏ dưới name)
            var catGo = MakeUI("TxtCategory", nameTag.transform);
            var catRect = catGo.GetComponent<RectTransform>();
            catRect.anchorMin = new Vector2(0.1f, 0f);
            catRect.anchorMax = new Vector2(0.9f, 0.3f);
            var catTMP = catGo.AddComponent<TextMeshProUGUI>();
            catTMP.text = "";
            catTMP.fontSize = 12f;
            catTMP.fontStyle = FontStyles.Italic;
            catTMP.alignment = TextAlignmentOptions.Center;
            catTMP.color = new Color(0.4f, 0.35f, 0.3f, 0.8f);
            catTMP.raycastTarget = false;

            // ── Tab Buttons (trên notebook) ──
            var tabArea = MakeUI("TabButtons", notebookBg.transform);
            var taRect = tabArea.GetComponent<RectTransform>();
            taRect.anchorMin = new Vector2(0.15f, 0.93f);
            taRect.anchorMax = new Vector2(0.85f, 1f);
            var hlg = tabArea.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;

            var btnCluesGo = MakeUI("BtnCluesTab", tabArea.transform);
            btnCluesGo.AddComponent<Image>().color = new Color(0.6f, 0.45f, 0.3f, 0.9f);
            var btnClues = btnCluesGo.AddComponent<Button>();
            var clueTabLabel = MakeUI("Label", btnCluesGo.transform);
            StretchFull(clueTabLabel.GetComponent<RectTransform>());
            var clueTabTMP = clueTabLabel.AddComponent<TextMeshProUGUI>();
            clueTabTMP.text = "EVIDENCE";
            clueTabTMP.fontSize = 14f;
            clueTabTMP.fontStyle = FontStyles.Bold;
            clueTabTMP.alignment = TextAlignmentOptions.Center;
            clueTabTMP.color = Color.white;
            clueTabTMP.raycastTarget = false;

            var btnCharsGo = MakeUI("BtnCharactersTab", tabArea.transform);
            btnCharsGo.AddComponent<Image>().color = new Color(0.4f, 0.35f, 0.3f, 0.9f);
            var btnChars = btnCharsGo.AddComponent<Button>();
            var charTabLabel = MakeUI("Label", btnCharsGo.transform);
            StretchFull(charTabLabel.GetComponent<RectTransform>());
            var charTabTMP = charTabLabel.AddComponent<TextMeshProUGUI>();
            charTabTMP.text = "SUSPECTS";
            charTabTMP.fontSize = 14f;
            charTabTMP.fontStyle = FontStyles.Bold;
            charTabTMP.alignment = TextAlignmentOptions.Center;
            charTabTMP.color = Color.white;
            charTabTMP.raycastTarget = false;

            // ── Navigation Buttons (dưới notebook) ──
            // Prev (trái)
            var prevGo = MakeUI("BtnPrevPage", notebookBg.transform);
            var prevRect = prevGo.GetComponent<RectTransform>();
            prevRect.anchorMin = new Vector2(0f, 0f);
            prevRect.anchorMax = new Vector2(0f, 0f);
            prevRect.pivot = new Vector2(0f, 0f);
            prevRect.sizeDelta = new Vector2(60f, 40f);
            prevRect.anchoredPosition = new Vector2(20f, -10f);
            prevGo.AddComponent<Image>().color = new Color(0.5f, 0.4f, 0.3f, 0.8f);
            var btnPrev = prevGo.AddComponent<Button>();
            var prevLabel = MakeUI("Label", prevGo.transform);
            StretchFull(prevLabel.GetComponent<RectTransform>());
            var prevTMP = prevLabel.AddComponent<TextMeshProUGUI>();
            prevTMP.text = "◀";
            prevTMP.fontSize = 22f;
            prevTMP.alignment = TextAlignmentOptions.Center;
            prevTMP.color = Color.white;
            prevTMP.raycastTarget = false;

            // Next (phải)
            var nextGo = MakeUI("BtnNextPage", notebookBg.transform);
            var nextRect = nextGo.GetComponent<RectTransform>();
            nextRect.anchorMin = new Vector2(1f, 0f);
            nextRect.anchorMax = new Vector2(1f, 0f);
            nextRect.pivot = new Vector2(1f, 0f);
            nextRect.sizeDelta = new Vector2(60f, 40f);
            nextRect.anchoredPosition = new Vector2(-20f, -10f);
            nextGo.AddComponent<Image>().color = new Color(0.5f, 0.4f, 0.3f, 0.8f);
            var btnNext = nextGo.AddComponent<Button>();
            var nextLabel = MakeUI("Label", nextGo.transform);
            StretchFull(nextLabel.GetComponent<RectTransform>());
            var nextTMP = nextLabel.AddComponent<TextMeshProUGUI>();
            nextTMP.text = "▶";
            nextTMP.fontSize = 22f;
            nextTMP.alignment = TextAlignmentOptions.Center;
            nextTMP.color = Color.white;
            nextTMP.raycastTarget = false;

            // Page number (giữa dưới)
            var pageGo = MakeUI("TxtPageNumber", notebookBg.transform);
            var pgRect = pageGo.GetComponent<RectTransform>();
            pgRect.anchorMin = new Vector2(0.5f, 0f);
            pgRect.anchorMax = new Vector2(0.5f, 0f);
            pgRect.pivot = new Vector2(0.5f, 0f);
            pgRect.sizeDelta = new Vector2(100f, 30f);
            pgRect.anchoredPosition = new Vector2(0f, -10f);
            var pgTMP = pageGo.AddComponent<TextMeshProUGUI>();
            pgTMP.text = "1/1";
            pgTMP.fontSize = 14f;
            pgTMP.alignment = TextAlignmentOptions.Center;
            pgTMP.color = new Color(0.4f, 0.35f, 0.3f, 1f);
            pgTMP.raycastTarget = false;

            // ── Close button (top right of overlay) ──
            var closeGo = MakeUI("BtnClose", root.transform);
            var clRect = closeGo.GetComponent<RectTransform>();
            clRect.anchorMin = clRect.anchorMax = Vector2.one;
            clRect.pivot = Vector2.one;
            clRect.sizeDelta = new Vector2(50f, 50f);
            clRect.anchoredPosition = new Vector2(-20f, -20f);
            closeGo.AddComponent<Image>().color = new Color(0.5f, 0.2f, 0.15f, 0.9f);
            var btnClose = closeGo.AddComponent<Button>();
            var closeLbl = MakeUI("Label", closeGo.transform);
            StretchFull(closeLbl.GetComponent<RectTransform>());
            var closeTMP = closeLbl.AddComponent<TextMeshProUGUI>();
            closeTMP.text = "X";
            closeTMP.fontSize = 24f;
            closeTMP.fontStyle = FontStyles.Bold;
            closeTMP.alignment = TextAlignmentOptions.Center;
            closeTMP.color = Color.white;
            closeTMP.raycastTarget = false;

            // ── Wire UINotebook ──
            var ui = root.AddComponent<UINotebook>();
            ui.uiName = UIName.Notebook;
            var so = new SerializedObject(ui);
            so.FindProperty("imgNotebookBg").objectReferenceValue = nbImg;
            so.FindProperty("btnCluesTab").objectReferenceValue = btnClues;
            so.FindProperty("btnCharactersTab").objectReferenceValue = btnChars;
            so.FindProperty("imgLeftPage").objectReferenceValue = leftPageImg;
            so.FindProperty("txtDescription").objectReferenceValue = descTMP;
            so.FindProperty("imgRightPage").objectReferenceValue = rightPageImg;
            so.FindProperty("imgPolaroidFrame").objectReferenceValue = polImg;
            so.FindProperty("imgPhoto").objectReferenceValue = phImg;
            so.FindProperty("imgNameTag").objectReferenceValue = ntImg;
            so.FindProperty("txtName").objectReferenceValue = nameTMP;
            so.FindProperty("txtCategory").objectReferenceValue = catTMP;
            so.FindProperty("btnPrevPage").objectReferenceValue = btnPrev;
            so.FindProperty("btnNextPage").objectReferenceValue = btnNext;
            so.FindProperty("txtPageNumber").objectReferenceValue = pgTMP;
            so.FindProperty("btnClose").objectReferenceValue = btnClose;
            so.ApplyModifiedPropertiesWithoutUndo();

            log?.Add("✅ UINotebook prefab created (2 pages: left desc + right polaroid + nav)");
            return root;
        }

        // ================================================================
        //  TRANSITION PREFAB (Circle Wipe)
        // ================================================================

        public static GameObject CreateTransitionPrefab_Internal(List<string> log)
        {
            // Tìm hoặc tạo material
            string shaderPath = "Assets/Luzart/DoMiTruth/Shaders/CircleWipe.shader";
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
            if (shader == null)
            {
                log?.Add("⚠️ CircleWipe.shader not found at " + shaderPath);
                return null;
            }

            string matPath = "Assets/Luzart/DoMiTruth/Materials/Mat_CircleWipe.mat";
            EnsureDir(matPath);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                mat.SetFloat("_Progress", 0f);
                mat.SetColor("_Color", Color.black);
                mat.SetFloat("_Softness", 0.15f);
                AssetDatabase.CreateAsset(mat, matPath);
            }

            // Root — Canvas overlay riêng (sort order cao nhất)
            var root = new GameObject("UITransition");
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            root.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            root.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920f, 1080f);

            // RawImage full screen với CircleWipe material
            var wipeGo = new GameObject("WipeImage", typeof(RectTransform));
            wipeGo.transform.SetParent(root.transform, false);
            var wipeRect = wipeGo.GetComponent<RectTransform>();
            wipeRect.anchorMin = Vector2.zero;
            wipeRect.anchorMax = Vector2.one;
            wipeRect.offsetMin = Vector2.zero;
            wipeRect.offsetMax = Vector2.zero;
            var wipeImg = wipeGo.AddComponent<RawImage>();
            wipeImg.material = mat;
            wipeImg.color = Color.white;
            wipeImg.raycastTarget = false;
            wipeGo.SetActive(false);

            // UITransition component
            var transition = root.AddComponent<UITransition>();
            var so = new SerializedObject(transition);
            so.FindProperty("wipeImage").objectReferenceValue = wipeImg;
            so.FindProperty("closeDuration").floatValue = 0.6f;
            so.FindProperty("openDuration").floatValue = 0.6f;
            so.FindProperty("holdDuration").floatValue = 0.2f;
            so.ApplyModifiedPropertiesWithoutUndo();

            log?.Add("✅ UITransition prefab created (CircleWipe shader + overlay canvas)");
            return root;
        }

        // ================================================================
        //  SFX AUTO-ASSIGN
        // ================================================================

        private static void AssignSFXToGameConfig(List<string> log)
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfigSO>(
                DATA_ROOT + "/Config/GameConfig.asset");
            if (config == null)
            {
                log?.Add("⚠️ GameConfig.asset not found, skipped SFX assign");
                return;
            }

            config.sfxDialogue = LoadAudioClip("Sound effect dialogue");
            config.sfxMenuClick = LoadAudioClip("Sound lúc chọn các mục ở Menu (Settings Guide, Quitgame)");
            config.sfxInteract = LoadAudioClip("Tuong tac do vat ingame");
            config.sfxPasscodeInput = LoadAudioClip("Nhap mat khau ket sat");
            config.sfxPasscodeWrong = LoadAudioClip("Sai mat khau");
            config.sfxSafeOpen = LoadAudioClip("Sound mo ket sat");

            EditorUtility.SetDirty(config);

            int count = 0;
            if (config.sfxDialogue != null) count++;
            if (config.sfxMenuClick != null) count++;
            if (config.sfxInteract != null) count++;
            if (config.sfxPasscodeInput != null) count++;
            if (config.sfxPasscodeWrong != null) count++;
            if (config.sfxSafeOpen != null) count++;

            log?.Add($"✅ SFX: {count}/6 audio clips assigned to GameConfig");
        }

        private static AudioClip LoadAudioClip(string fileName)
        {
            string path = SFX_ROOT + "/" + fileName + ".mp3";
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip == null)
            {
                // Try wav
                path = SFX_ROOT + "/" + fileName + ".wav";
                clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            }
            return clip;
        }

        // ================================================================
        //  HELPERS
        // ================================================================

        private static T LoadOrCreate<T>(string path, string name) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;

            EnsureDir(path);
            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static RuntimeAnimatorController LoadAnim(string animName)
        {
            string path = ANIM_ROOT + "/" + animName + ".controller";
            return AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path);
        }

        private static void RegisterEntry(SerializedProperty entries, UIName uiName, UIBase prefab)
        {
            for (int i = 0; i < entries.arraySize; i++)
            {
                var entry = entries.GetArrayElementAtIndex(i);
                if (entry.FindPropertyRelative("uiName").intValue == (int)uiName)
                {
                    entry.FindPropertyRelative("prefab").objectReferenceValue = prefab;
                    return;
                }
            }

            entries.InsertArrayElementAtIndex(entries.arraySize);
            var newEntry = entries.GetArrayElementAtIndex(entries.arraySize - 1);
            newEntry.FindPropertyRelative("uiName").intValue = (int)uiName;
            newEntry.FindPropertyRelative("prefab").objectReferenceValue = prefab;
            newEntry.FindPropertyRelative("layerIndex").intValue = 0;
            newEntry.FindPropertyRelative("useCache").boolValue = false;
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

        private static void EnsureDir(string assetPath)
        {
            string dir = System.IO.Path.GetDirectoryName(assetPath);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
        }
    }
}
#endif
