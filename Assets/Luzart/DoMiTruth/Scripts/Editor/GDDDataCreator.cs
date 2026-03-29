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

        // Colors matching briefing style
        private static readonly Color C_BOARD_BG = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        private static readonly Color C_LABEL_BG = new Color(0.25f, 0.35f, 0.3f, 0.85f);
        private static readonly Color C_LABEL_TEXT = new Color(0.7f, 0.82f, 0.72f, 1f);
        private static readonly Color C_BOARD_TEXT = new Color(0.7f, 0.65f, 0.55f, 1f);
        private static readonly Color C_CHOICE_BG = new Color(0.16f, 0.13f, 0.1f, 0.92f);
        private static readonly Color C_CHOICE_TEXT = new Color(0.92f, 0.88f, 0.8f, 1f);
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
            var rooms = CreateRooms(interactables, log);
            AssignRoomsToMaps(rooms, log);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("✅ GDD Data Created!",
                string.Join("\n", log), "OK");
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
                ClueCategory.Document);

            r.debitNote = MakeClue("Clue_DebitNote", "debit_note",
                "Debit note",
                "A debit note showing a large outstanding debt.",
                ClueCategory.Document);

            r.contactInfo = MakeClue("Clue_ContactInfo", "contact_info",
                "Contact Information",
                "A list of phone numbers and addresses.",
                ClueCategory.Document);

            // Living Room
            r.brokenCabinet = MakeClue("Clue_BrokenCabinet", "broken_cabinet",
                "The broken cabinet door",
                "A cabinet door that has been smashed open with force.",
                ClueCategory.Physical);

            r.godOfWealth = MakeClue("Clue_GodOfWealth", "god_of_wealth",
                "The statue of the God of Wealth",
                "A golden statue of the God of Wealth. Seems important to the victim.",
                ClueCategory.Physical);

            // Master Bedroom
            r.brokenPhoto = MakeClue("Clue_BrokenPhoto", "broken_photo",
                "Broken photo portrait",
                "A family portrait that has been smashed. Glass shards everywhere.",
                ClueCategory.Photo);

            r.bloodShawl = MakeClue("Clue_BloodShawl", "blood_shawl",
                "Blood-stained shawl",
                "A silk shawl with visible bloodstains.",
                ClueCategory.Physical);

            r.sleepingPills = MakeClue("Clue_SleepingPills", "sleeping_pills",
                "Sleeping pills",
                "A bottle of prescription sleeping pills. Nearly empty.",
                ClueCategory.Physical);

            // Storage
            r.frenchMaterials = MakeClue("Clue_FrenchMaterials", "french_materials",
                "French learning materials",
                "Textbooks and notes for learning French.",
                ClueCategory.Document);

            r.storagePhotos = MakeClue("Clue_StoragePhotos", "storage_photos",
                "Photos",
                "A collection of photographs found in the storage room.",
                ClueCategory.Photo);

            log?.Add("✅ Clues: 10 clues created");
            return r;
        }

        private static ClueSO MakeClue(string fileName, string clueId, string clueName, string desc, ClueCategory cat)
        {
            string path = DATA_ROOT + "/Clues/" + fileName + ".asset";
            var clue = LoadOrCreate<ClueSO>(path, fileName);
            clue.clueId = clueId;
            clue.clueName = clueName;
            clue.description = desc;
            clue.category = cat;
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

            // Create unlock success actions: collect 2 clues
            var actCollectFrench = LoadOrCreate<CollectClueConfig>(
                DATA_ROOT + "/Actions/Act_Collect_FrenchMaterials.asset", "Act_Collect_FrenchMaterials");
            var soFrench = new SerializedObject(actCollectFrench);
            soFrench.FindProperty("clue").objectReferenceValue = clues.frenchMaterials;
            soFrench.ApplyModifiedProperties();
            EditorUtility.SetDirty(actCollectFrench);

            var actCollectPhotos = LoadOrCreate<CollectClueConfig>(
                DATA_ROOT + "/Actions/Act_Collect_StoragePhotos.asset", "Act_Collect_StoragePhotos");
            var soPhotos = new SerializedObject(actCollectPhotos);
            soPhotos.FindProperty("clue").objectReferenceValue = clues.storagePhotos;
            soPhotos.ApplyModifiedProperties();
            EditorUtility.SetDirty(actCollectPhotos);

            var actToast = LoadOrCreate<ShowToastConfig>(
                DATA_ROOT + "/Actions/Act_Toast_Unlocked.asset", "Act_Toast_Unlocked");
            var soToast = new SerializedObject(actToast);
            soToast.FindProperty("message").stringValue = "Unlocked successfully";
            soToast.ApplyModifiedProperties();
            EditorUtility.SetDirty(actToast);

            r.storageLock.onUnlockSuccess = new List<ActionStepConfig>
            {
                actToast, actCollectFrench, actCollectPhotos
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
        //  ROOMS
        // ================================================================

        private struct RoomRefs
        {
            public RoomSO bathroom, diningRoom, courtyard, subBedroom;
            public RoomSO homeOffice, livingRoom, masterBedroom, storage, hallway;
        }

        private static RoomRefs CreateRooms(InteractableRefs io, List<string> log)
        {
            var r = new RoomRefs();
            string dir = DATA_ROOT + "/Rooms/";

            // Background-only rooms
            r.bathroom = MakeRoom(dir, "Room_Bathroom", "bathroom", "BATHROOM");
            r.diningRoom = MakeRoom(dir, "Room_DiningRoom", "dining_room", "DINING ROOM");
            r.courtyard = MakeRoom(dir, "Room_Courtyard", "courtyard", "COURTYARD");
            r.subBedroom = MakeRoom(dir, "Room_SubBedroom", "sub_bedroom", "SUB BEDROOM");

            // Home Office: Props + NPC
            r.homeOffice = MakeRoom(dir, "Room_HomeOffice", "home_office", "HOME OFFICE");
            r.homeOffice.interactables = new List<RoomInteractable>
            {
                new RoomInteractable { data = io.contract, positionOnBackground = new Vector2(300f, 400f) },
                new RoomInteractable { data = io.debitNote, positionOnBackground = new Vector2(600f, 350f) },
                new RoomInteractable { data = io.contactInfo, positionOnBackground = new Vector2(900f, 400f) },
                new RoomInteractable { data = io.doctor, positionOnBackground = new Vector2(500f, 300f) },
            };
            EditorUtility.SetDirty(r.homeOffice);

            // Living Room: Props + NPC
            r.livingRoom = MakeRoom(dir, "Room_LivingRoom", "living_room", "LIVING ROOM");
            r.livingRoom.interactables = new List<RoomInteractable>
            {
                new RoomInteractable { data = io.brokenCabinet, positionOnBackground = new Vector2(200f, 400f) },
                new RoomInteractable { data = io.godOfWealth, positionOnBackground = new Vector2(800f, 350f) },
                new RoomInteractable { data = io.debtCollector, positionOnBackground = new Vector2(500f, 300f) },
            };
            EditorUtility.SetDirty(r.livingRoom);

            // Master Bedroom: Props only
            r.masterBedroom = MakeRoom(dir, "Room_MasterBedroom", "master_bedroom", "MASTER BEDROOM");
            r.masterBedroom.interactables = new List<RoomInteractable>
            {
                new RoomInteractable { data = io.brokenPhoto, positionOnBackground = new Vector2(300f, 300f) },
                new RoomInteractable { data = io.bloodShawl, positionOnBackground = new Vector2(600f, 500f) },
                new RoomInteractable { data = io.sleepingPills, positionOnBackground = new Vector2(900f, 400f) },
            };
            EditorUtility.SetDirty(r.masterBedroom);

            // Storage: Lock
            r.storage = MakeRoom(dir, "Room_Storage", "storage", "STORAGE");
            r.storage.interactables = new List<RoomInteractable>
            {
                new RoomInteractable { data = io.storageLock, positionOnBackground = new Vector2(500f, 400f) },
            };
            EditorUtility.SetDirty(r.storage);

            // Hallway: NPC only
            r.hallway = MakeRoom(dir, "Room_Hallway", "hallway", "HALLWAY");
            r.hallway.interactables = new List<RoomInteractable>
            {
                new RoomInteractable { data = io.hallwayPolice, positionOnBackground = new Vector2(500f, 350f) },
            };
            EditorUtility.SetDirty(r.hallway);

            log?.Add("✅ Rooms: 9 rooms created (4 bg-only, 2 props+NPC, 1 props, 1 lock, 1 NPC)");
            return r;
        }

        private static RoomSO MakeRoom(string dir, string fileName, string roomId, string roomName)
        {
            var room = LoadOrCreate<RoomSO>(dir + fileName + ".asset", fileName);
            room.roomId = roomId;
            room.roomName = roomName;
            room.backgroundSize = new Vector2(1920f, 1080f);
            room.interactables = new List<RoomInteractable>();
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

            // ── Detective Portrait (bottom right) ──
            var detPort = MakeUI("DetectivePortrait", root.transform);
            var dpRect = detPort.GetComponent<RectTransform>();
            dpRect.anchorMin = dpRect.anchorMax = new Vector2(1f, 0f);
            dpRect.pivot = new Vector2(1f, 0f);
            dpRect.sizeDelta = new Vector2(100f, 120f);
            dpRect.anchoredPosition = new Vector2(-15f, 15f);
            var detImg = detPort.AddComponent<Image>();
            detImg.preserveAspect = true; detImg.raycastTarget = false;
            var detAnim = detPort.AddComponent<Animator>();
            detAnim.enabled = false;

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
            so.FindProperty("choiceContainer").objectReferenceValue = choiceCont.transform;
            so.FindProperty("choiceButtonPrefab").objectReferenceValue = choicePfItem;
            so.FindProperty("imgDetectivePortrait").objectReferenceValue = detImg;
            so.FindProperty("animDetectivePortrait").objectReferenceValue = detAnim;
            so.FindProperty("btnNext").objectReferenceValue = btnNext;
            so.FindProperty("btnSettings").objectReferenceValue = btnSettings;
            so.ApplyModifiedPropertiesWithoutUndo();

            log?.Add("✅ UINPCDialogue prefab created");
            return root;
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
