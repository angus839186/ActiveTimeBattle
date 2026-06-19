using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ActiveTimeBattle.Domain.Combat;
using ActiveTimeBattle.Presentation.Core;
using ActiveTimeBattle.Presentation.Combat;
using ActiveTimeBattle.Presentation.Exploration;
using ActiveTimeBattle.Presentation.Hub;
using ActiveTimeBattle.Presentation.Narrative;
using ActiveTimeBattle.Presentation.UI;
using TMPro;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ActiveTimeBattle.Editor
{
    public static class PrototypeSceneBuilder
    {
        private const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";
        private const string SceneFolder = "Assets/_Project/Scenes";

        [MenuItem("ActiveTimeBattle/Build Phase 1 Prototype")]
        public static void BuildPhaseOnePrototype()
        {
            EnsureProjectFolders();
            EnsureTextMeshProResources();
            EnsureInputActions();
            EnsureInteractableLayer();

            InputActionAsset inputActions =
                AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (inputActions == null)
            {
                throw new System.InvalidOperationException(
                    $"Unable to load Input Action Asset at {InputActionsPath}.");
            }

            EnsureSkillDataAssets();
            EnsureDialogueDataAssets();
            BuildBootstrapScene();
            BuildHubScene(inputActions);
            BuildExplorationScene(inputActions);
            BuildCombatScene(inputActions);
            ConfigureBuildSettings();

            AssetDatabase.SaveAssets();
            NormalizeGeneratedTextEncoding();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene($"{SceneFolder}/00_Bootstrap.unity");
            Debug.Log("Phase 1 and 2 prototype scenes were built successfully.");
        }

        private static void EnsureProjectFolders()
        {
            Directory.CreateDirectory(SceneFolder);
            Directory.CreateDirectory("Assets/_Project/Data");
            Directory.CreateDirectory("Assets/_Project/Data/Materials");
            Directory.CreateDirectory("Assets/_Project/Data/Narrative");
            Directory.CreateDirectory("Assets/_Project/Prefabs");
            Directory.CreateDirectory("Assets/_Project/Tests");
            AssetDatabase.Refresh();
        }

        private static void EnsureTextMeshProResources()
        {
            if (TMP_Settings.defaultFontAsset != null)
            {
                return;
            }

            UnityEditor.PackageManager.PackageInfo packageInfo =
                UnityEditor.PackageManager.PackageInfo.FindForAssembly(
                    typeof(TMP_Text).Assembly);
            string packagePath = Path.Combine(
                packageInfo.resolvedPath,
                "Package Resources",
                "TMP Essential Resources.unitypackage");

            if (File.Exists(packagePath))
            {
                AssetDatabase.ImportPackage(packagePath, false);
                AssetDatabase.Refresh();
            }
        }

        private static void EnsureInputActions()
        {
            InputActionAsset inputActions =
                AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);

            ReplaceMap(inputActions, "Hub", map =>
            {
                AddMoveAction(map);
                InputAction look = map.AddAction("Look", InputActionType.Value);
                look.expectedControlType = "Vector2";
                look.AddBinding("<Mouse>/delta");
                AddButton(map, "Interact", "<Mouse>/leftButton", "<Keyboard>/e");
                AddButton(map, "OpenStatus", "<Keyboard>/tab");
                AddButton(map, "Cancel", "<Keyboard>/escape");
                AddButton(map, "Pause", "<Keyboard>/escape");
            });

            ReplaceMap(inputActions, "Exploration", map =>
            {
                AddMoveAction(map);
                AddButton(map, "Interact", "<Keyboard>/e");
                AddButton(map, "SelectTarget", "<Mouse>/leftButton");
                AddButton(map, "Confirm", "<Keyboard>/enter", "<Keyboard>/space");
                AddButton(map, "Choice1", "<Keyboard>/1");
                AddButton(map, "Choice2", "<Keyboard>/2");
                AddButton(map, "Choice3", "<Keyboard>/3");
                AddButton(map, "Cancel", "<Keyboard>/escape");
                AddButton(map, "OpenBuild", "<Keyboard>/tab");
                AddButton(map, "Pause", "<Keyboard>/escape");
            });

            ReplaceMap(inputActions, "Combat", map =>
            {
                AddButton(map, "DirectionUp", "<Keyboard>/upArrow");
                AddButton(map, "DirectionDown", "<Keyboard>/downArrow");
                AddButton(map, "DirectionLeft", "<Keyboard>/leftArrow");
                AddButton(map, "DirectionRight", "<Keyboard>/rightArrow");
                AddButton(map, "SkillQ", "<Keyboard>/q");
                AddButton(map, "SkillW", "<Keyboard>/w");
                AddButton(map, "SkillE", "<Keyboard>/e");
                AddButton(map, "UltimateR", "<Keyboard>/r");
                AddButton(map, "Defense", "<Keyboard>/space");
                AddButton(map, "Potion", "<Keyboard>/f");
                AddButton(map, "Pause", "<Keyboard>/escape");
            });

            ReplaceMap(inputActions, "Dialogue", map =>
            {
                AddButton(
                    map,
                    "Advance",
                    "<Mouse>/leftButton",
                    "<Keyboard>/space",
                    "<Keyboard>/enter");
                InputAction selectChoice = map.AddAction(
                    "SelectChoice",
                    InputActionType.Value);
                selectChoice.expectedControlType = "Vector2";
                selectChoice.AddCompositeBinding("2DVector")
                    .With("Up", "<Keyboard>/upArrow")
                    .With("Down", "<Keyboard>/downArrow");
                AddButton(map, "Cancel", "<Keyboard>/escape");
            });

            string absolutePath = Path.GetFullPath(InputActionsPath);
            File.WriteAllText(
                absolutePath,
                inputActions.ToJson(),
                new UTF8Encoding(false));
            AssetDatabase.ImportAsset(
                InputActionsPath,
                ImportAssetOptions.ForceUpdate
                | ImportAssetOptions.ForceSynchronousImport);
            InputSystem.actions =
                AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
        }

        private static void ReplaceMap(
            InputActionAsset inputActions,
            string mapName,
            System.Action<InputActionMap> configure)
        {
            InputActionMap existingMap =
                inputActions.FindActionMap(mapName, false);
            if (existingMap != null)
            {
                inputActions.RemoveActionMap(existingMap);
            }

            InputActionMap map = inputActions.AddActionMap(mapName);
            configure(map);
        }

        private static void AddMoveAction(InputActionMap map)
        {
            InputAction move = map.AddAction(
                "Move",
                InputActionType.Value);
            move.expectedControlType = "Vector2";
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
        }

        private static void AddButton(
            InputActionMap map,
            string actionName,
            params string[] bindings)
        {
            InputAction action = map.AddAction(
                actionName,
                InputActionType.Button);
            action.expectedControlType = "Button";

            foreach (string binding in bindings)
            {
                action.AddBinding(binding);
            }
        }

        private static void EnsureInteractableLayer()
        {
            SerializedObject tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath(
                    "ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");

            for (int index = 8; index < layers.arraySize; index++)
            {
                SerializedProperty layer = layers.GetArrayElementAtIndex(index);
                if (layer.stringValue == "Interactable")
                {
                    return;
                }
            }

            for (int index = 8; index < layers.arraySize; index++)
            {
                SerializedProperty layer = layers.GetArrayElementAtIndex(index);
                if (string.IsNullOrEmpty(layer.stringValue))
                {
                    layer.stringValue = "Interactable";
                    tagManager.ApplyModifiedProperties();
                    return;
                }
            }

            throw new System.InvalidOperationException(
                "No free user layer is available for Interactable.");
        }

        private static void BuildBootstrapScene()
        {
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            new GameObject(
                "GameBootstrapper",
                typeof(GameBootstrapper));
            CreateCamera("Bootstrap Camera", new Vector3(0f, 2f, -6f));
            CreateDirectionalLight();

            EditorSceneManager.SaveScene(
                scene,
                $"{SceneFolder}/00_Bootstrap.unity");
        }

        private static void BuildHubScene(InputActionAsset inputActions)
        {
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            CreateDirectionalLight();
            CreateRoom();

            Canvas hudCanvas = CreateScreenCanvas("Hub HUD");
            DialogueRunner dialogueRunner =
                CreateDialogueRunner(hudCanvas, inputActions);
            CreateText(
                hudCanvas.transform,
                "Crosshair",
                "+",
                30f,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(60f, 60f));
            TMP_Text targetName = CreateText(
                hudCanvas.transform,
                "Target Name",
                string.Empty,
                30f,
                TextAlignmentOptions.Center,
                new Vector2(0.25f, 0.18f),
                new Vector2(0.75f, 0.26f),
                Vector2.zero);
            TMP_Text prompt = CreateText(
                hudCanvas.transform,
                "Interaction Prompt",
                string.Empty,
                22f,
                TextAlignmentOptions.Center,
                new Vector2(0.2f, 0.1f),
                new Vector2(0.8f, 0.18f),
                Vector2.zero);

            InteractionPromptView promptView =
                hudCanvas.gameObject.AddComponent<InteractionPromptView>();
            SetObjectReference(promptView, "targetNameText", targetName);
            SetObjectReference(promptView, "promptText", prompt);

            GameObject player = new GameObject(
                "First Person Player",
                typeof(CharacterController),
                typeof(FirstPersonController),
                typeof(HubInteractionDetector),
                typeof(SceneInputMapController));
            player.transform.position = new Vector3(0f, 0.05f, -4f);

            CharacterController characterController =
                player.GetComponent<CharacterController>();
            characterController.height = 1.8f;
            characterController.center = new Vector3(0f, 0.9f, 0f);
            characterController.radius = 0.35f;

            GameObject cameraObject = CreateCamera(
                "Main Camera",
                Vector3.zero,
                player.transform);
            cameraObject.tag = "MainCamera";
            cameraObject.transform.localPosition = new Vector3(0f, 1.6f, 0f);

            FirstPersonController firstPersonController =
                player.GetComponent<FirstPersonController>();
            SetObjectReference(firstPersonController, "inputActions", inputActions);
            SetObjectReference(
                firstPersonController,
                "cameraPivot",
                cameraObject.transform);

            SceneInputMapController inputMapController =
                player.GetComponent<SceneInputMapController>();
            SetObjectReference(inputMapController, "inputActions", inputActions);
            SetString(inputMapController, "actionMapName", "Hub");

            HubInteractionDetector detector =
                player.GetComponent<HubInteractionDetector>();
            SetObjectReference(
                detector,
                "raycastCamera",
                cameraObject.GetComponent<Camera>());
            SetObjectReference(detector, "inputActions", inputActions);
            SetObjectReference(detector, "promptView", promptView);
            SetInteger(
                detector,
                "interactableLayers",
                LayerMask.GetMask("Interactable"));
            SetFloat(detector, "interactionDistance", 4f);

            CreateAdventureEntrance(cameraObject.GetComponent<Camera>());
            CreateDialogueInteractable(
                "Archive Keeper",
                PrimitiveType.Capsule,
                new Vector3(-2.8f, 1f, 2.2f),
                new Vector3(1f, 2f, 1f),
                new Color(0.24f, 0.42f, 0.58f),
                "檔案守望者",
                "交談",
                LoadRequiredAsset<DialogueData>(
                    "Assets/_Project/Data/Narrative/HubKeeper.asset"),
                dialogueRunner,
                cameraObject.GetComponent<Camera>());
            CreateDialogueInteractable(
                "Old Journal",
                PrimitiveType.Cube,
                new Vector3(2.8f, 0.45f, 2.2f),
                new Vector3(1.4f, 0.25f, 1f),
                new Color(0.38f, 0.23f, 0.12f),
                "殘破日誌",
                "調查",
                LoadRequiredAsset<DialogueData>(
                    "Assets/_Project/Data/Narrative/OldJournal.asset"),
                dialogueRunner,
                cameraObject.GetComponent<Camera>());

            EditorSceneManager.SaveScene(
                scene,
                $"{SceneFolder}/01_Hub.unity");
        }

        private static void BuildExplorationScene(InputActionAsset inputActions)
        {
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            CreateDirectionalLight();
            GameObject cameraObject = CreateCamera(
                "Main Camera",
                new Vector3(7.5f, 8f, -7.5f));
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.orthographic = true;
            camera.orthographicSize = 6.2f;
            cameraObject.transform.rotation =
                Quaternion.Euler(35.264f, 315f, 0f);

            Canvas canvas = CreateScreenCanvas("Exploration HUD");
            TMP_Text seedText = CreateText(
                canvas.transform,
                "Exploration Status",
                string.Empty,
                25f,
                TextAlignmentOptions.TopLeft,
                new Vector2(0.03f, 0.72f),
                new Vector2(0.36f, 0.96f),
                Vector2.zero);
            seedText.text =
                "探索路線會在執行時依 Run Seed 產生";
            TMP_Text buildText = CreateText(
                canvas.transform,
                "Mini Map",
                string.Empty,
                22f,
                TextAlignmentOptions.TopLeft,
                new Vector2(0.68f, 0.68f),
                new Vector2(0.96f, 0.96f),
                Vector2.zero);
            buildText.color = new Color(0.78f, 0.9f, 1f);
            TMP_Text optionText = CreateText(
                canvas.transform,
                "Exploration Options",
                string.Empty,
                28f,
                TextAlignmentOptions.Center,
                new Vector2(0.25f, 0.08f),
                new Vector2(0.75f, 0.34f),
                Vector2.zero);
            optionText.color = new Color(1f, 0.92f, 0.65f);
            optionText.gameObject.SetActive(false);

            GameObject controller = new GameObject(
                "Exploration Controller",
                typeof(SceneInputMapController),
                typeof(ExplorationPlaceholderView));
            SceneInputMapController inputMapController =
                controller.GetComponent<SceneInputMapController>();
            SetObjectReference(inputMapController, "inputActions", inputActions);
            SetString(inputMapController, "actionMapName", "Exploration");

            ExplorationPlaceholderView placeholder =
                controller.GetComponent<ExplorationPlaceholderView>();
            SetObjectReference(placeholder, "inputActions", inputActions);
            SetObjectReference(
                placeholder,
                "explorationCamera",
                cameraObject.GetComponent<Camera>());
            SetObjectReference(placeholder, "seedText", seedText);
            SetObjectReference(placeholder, "buildText", buildText);
            SetObjectReference(placeholder, "optionText", optionText);

            EditorSceneManager.SaveScene(
                scene,
                $"{SceneFolder}/02_Exploration.unity");
        }

        private static void BuildCombatScene(InputActionAsset inputActions)
        {
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            CreateDirectionalLight();
            CreatePrimitive(
                "Combat Floor",
                PrimitiveType.Plane,
                Vector3.zero,
                new Vector3(2.5f, 1f, 2.5f),
                new Color(0.08f, 0.09f, 0.12f));

            GameObject enemy = CreatePrimitive(
                "Enemy Combatant",
                PrimitiveType.Capsule,
                new Vector3(0f, 1f, 3.5f),
                new Vector3(1.4f, 1.4f, 1.4f),
                new Color(0.65f, 0.12f, 0.16f));

            GameObject cameraObject = CreateCamera(
                "Main Camera",
                new Vector3(0f, 1.65f, -4.5f));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.rotation = Quaternion.identity;

            Canvas canvas = CreateScreenCanvas("Combat HUD");
            Slider playerHealth = CreateSlider(
                canvas.transform,
                "Player Health",
                new Vector2(0.05f, 0.88f),
                new Vector2(0.4f, 0.93f),
                new Color(0.15f, 0.65f, 1f));
            TMP_Text playerHealthText = CreateText(
                canvas.transform,
                "Player Health Text",
                "100 / 100",
                20f,
                TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.88f),
                new Vector2(0.4f, 0.93f),
                Vector2.zero);
            Slider enemyHealth = CreateSlider(
                canvas.transform,
                "Enemy Health",
                new Vector2(0.6f, 0.88f),
                new Vector2(0.95f, 0.93f),
                new Color(0.9f, 0.15f, 0.18f));
            TMP_Text enemyHealthText = CreateText(
                canvas.transform,
                "Enemy Health Text",
                "120 / 120",
                20f,
                TextAlignmentOptions.Center,
                new Vector2(0.6f, 0.88f),
                new Vector2(0.95f, 0.93f),
                Vector2.zero);
            Slider enemyAction = CreateSlider(
                canvas.transform,
                "Enemy Action CD",
                new Vector2(0.3f, 0.79f),
                new Vector2(0.7f, 0.83f),
                new Color(1f, 0.55f, 0.1f));
            TMP_Text actionLabel = CreateText(
                canvas.transform,
                "Enemy Action Label",
                "敵人行動 CD",
                18f,
                TextAlignmentOptions.Center,
                new Vector2(0.3f, 0.83f),
                new Vector2(0.7f, 0.87f),
                Vector2.zero);
            Slider qteWindow = CreateSlider(
                canvas.transform,
                "Defense QTE",
                new Vector2(0.35f, 0.5f),
                new Vector2(0.65f, 0.56f),
                new Color(0.25f, 0.9f, 1f));
            qteWindow.maxValue = 1f;
            CreateQteZone(
                qteWindow.transform,
                "Perfect Zone",
                0f,
                0.25f,
                new Color(0.2f, 0.9f, 1f, 0.5f));
            CreateQteZone(
                qteWindow.transform,
                "Normal Zone",
                0.25f,
                0.667f,
                new Color(1f, 0.75f, 0.1f, 0.5f));
            CreateQteZone(
                qteWindow.transform,
                "Break Zone",
                0.667f,
                1f,
                new Color(0.95f, 0.15f, 0.18f, 0.5f));
            TMP_Text statusText = CreateText(
                canvas.transform,
                "Combat Status",
                "輸入方向後按技能鍵",
                28f,
                TextAlignmentOptions.Center,
                new Vector2(0.2f, 0.62f),
                new Vector2(0.8f, 0.72f),
                Vector2.zero);
            TMP_Text commandText = CreateText(
                canvas.transform,
                "Direction Command",
                "方向指令：等待輸入",
                28f,
                TextAlignmentOptions.Center,
                new Vector2(0.2f, 0.12f),
                new Vector2(0.8f, 0.2f),
                Vector2.zero);
            TMP_Text skillsText = CreateText(
                canvas.transform,
                "Skill Cooldowns",
                "Q / W / E / R",
                20f,
                TextAlignmentOptions.Left,
                new Vector2(0.04f, 0.25f),
                new Vector2(0.35f, 0.48f),
                Vector2.zero);
            CreateText(
                canvas.transform,
                "Controls",
                "方向鍵輸入指令  |  Q/W/E/R 施放  |  Space 防禦（紅/黃/藍）",
                18f,
                TextAlignmentOptions.Center,
                new Vector2(0.15f, 0.03f),
                new Vector2(0.85f, 0.09f),
                Vector2.zero);

            CombatHudView hud = canvas.gameObject.AddComponent<CombatHudView>();
            SetObjectReference(hud, "playerHealth", playerHealth);
            SetObjectReference(hud, "enemyHealth", enemyHealth);
            SetObjectReference(hud, "enemyAction", enemyAction);
            SetObjectReference(hud, "qteWindow", qteWindow);
            SetObjectReference(hud, "playerHealthText", playerHealthText);
            SetObjectReference(hud, "enemyHealthText", enemyHealthText);
            SetObjectReference(hud, "commandText", commandText);
            SetObjectReference(hud, "statusText", statusText);
            SetObjectReference(hud, "skillsText", skillsText);

            GameObject controller = new GameObject(
                "Combat Controller",
                typeof(SceneInputMapController),
                typeof(CombatController));
            SceneInputMapController inputMap =
                controller.GetComponent<SceneInputMapController>();
            SetObjectReference(inputMap, "inputActions", inputActions);
            SetString(inputMap, "actionMapName", "Combat");

            CombatController combatController =
                controller.GetComponent<CombatController>();
            SetObjectReference(combatController, "inputActions", inputActions);
            SetObjectReference(
                combatController,
                "skillQ",
                LoadRequiredAsset<SkillData>(
                    "Assets/_Project/Data/Skills/Skill_Q_Slash.asset"));
            SetObjectReference(
                combatController,
                "skillW",
                LoadRequiredAsset<SkillData>(
                    "Assets/_Project/Data/Skills/Skill_W_Break.asset"));
            SetObjectReference(
                combatController,
                "skillE",
                LoadRequiredAsset<SkillData>(
                    "Assets/_Project/Data/Skills/Skill_E_Pierce.asset"));
            SetObjectReference(
                combatController,
                "ultimateR",
                LoadRequiredAsset<SkillData>(
                    "Assets/_Project/Data/Skills/Skill_R_Ultimate.asset"));
            SetObjectReference(combatController, "hudView", hud);
            SetObjectReference(
                combatController,
                "enemyRenderer",
                enemy.GetComponent<Renderer>());

            EditorSceneManager.SaveScene(
                scene,
                $"{SceneFolder}/03_Combat.unity");
        }

        private static SkillData[] EnsureSkillDataAssets()
        {
            Directory.CreateDirectory("Assets/_Project/Data/Skills");
            string[] paths =
            {
                "Assets/_Project/Data/Skills/Skill_Q_Slash.asset",
                "Assets/_Project/Data/Skills/Skill_W_Break.asset",
                "Assets/_Project/Data/Skills/Skill_E_Pierce.asset",
                "Assets/_Project/Data/Skills/Skill_R_Ultimate.asset"
            };
            EnsureSkillData(
                    "Skill_Q_Slash",
                    "q",
                    "迅擊",
                    new[] { CombatDirection.Up, CombatDirection.Right },
                    18,
                    3f,
                    0.4f);
            EnsureSkillData(
                    "Skill_W_Break",
                    "w",
                    "破勢",
                    new[]
                    {
                        CombatDirection.Left,
                        CombatDirection.Right,
                        CombatDirection.Up
                    },
                    30,
                    5f,
                    0.65f);
            EnsureSkillData(
                    "Skill_E_Pierce",
                    "e",
                    "穿刺",
                    new[] { CombatDirection.Down, CombatDirection.Up },
                    24,
                    4f,
                    0.5f);
            EnsureSkillData(
                    "Skill_R_Ultimate",
                    "r",
                    "終結",
                    new[]
                    {
                        CombatDirection.Up,
                        CombatDirection.Up,
                        CombatDirection.Down
                    },
                    60,
                    10f,
                    0.9f);

            AssetDatabase.SaveAssets();
            foreach (string path in paths)
            {
                AssetDatabase.ImportAsset(
                    path,
                    ImportAssetOptions.ForceUpdate
                    | ImportAssetOptions.ForceSynchronousImport);
            }

            SkillData[] result = new SkillData[paths.Length];
            for (int index = 0; index < paths.Length; index++)
            {
                result[index] = AssetDatabase.LoadAssetAtPath<SkillData>(
                    paths[index]);
                if (result[index] == null)
                {
                    throw new System.InvalidOperationException(
                        $"Unable to load SkillData at {paths[index]}.");
                }
            }

            return result;
        }

        private static SkillData EnsureSkillData(
            string assetName,
            string id,
            string displayName,
            CombatDirection[] command,
            int damage,
            float cooldown,
            float executionDuration)
        {
            string path = $"Assets/_Project/Data/Skills/{assetName}.asset";
            SkillData data = AssetDatabase.LoadAssetAtPath<SkillData>(path);
            if (data != null)
            {
                return data;
            }

            data = ScriptableObject.CreateInstance<SkillData>();
            AssetDatabase.CreateAsset(data, path);
            SerializedObject serialized = new SerializedObject(data);
            serialized.Update();
            serialized.FindProperty("skillId").stringValue = id;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("damage").intValue = damage;
            serialized.FindProperty("cooldownDuration").floatValue = cooldown;
            serialized.FindProperty("executionDuration").floatValue =
                executionDuration;

            SerializedProperty commandProperty =
                serialized.FindProperty("command");
            commandProperty.arraySize = command.Length;
            for (int index = 0; index < command.Length; index++)
            {
                commandProperty.GetArrayElementAtIndex(index).enumValueIndex =
                    (int)command[index];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(data);
            return data;
        }

        private static void EnsureDialogueDataAssets()
        {
            EnsureDialogueData(
                "Assets/_Project/Data/Narrative/HubKeeper.asset",
                new DialogueSeedLine(
                    "檔案守望者",
                    "每次踏入深淵，路線都會依新的種子重新排列。"),
                new DialogueSeedLine(
                    "檔案守望者",
                    "死亡或完成冒險後，你仍會回到這個房間。"));
            EnsureDialogueData(
                "Assets/_Project/Data/Narrative/OldJournal.asset",
                new DialogueSeedLine(
                    "殘破日誌",
                    "紙頁記著相同種子會重現相同道路。"),
                new DialogueSeedLine(
                    "殘破日誌",
                    "最後一頁只寫著：先觀察，再決定方向。"));
            EnsureDialogueData(
                "Assets/_Project/Data/Narrative/ExplorationEvent.asset",
                new DialogueSeedLine(
                    "探索",
                    "牆面的裂縫傳來低語，像是在記錄你的選擇。"),
                new DialogueSeedLine(
                    "探索",
                    "你確認周圍安全，繼續向下一個節點前進。"));
            EnsureDialogueData(
                "Assets/_Project/Data/Narrative/BossIntro.asset",
                new DialogueSeedLine(
                    "深淵核心",
                    "這條路線的盡頭正在等待挑戰者。"),
                new DialogueSeedLine(
                    "深淵核心",
                    "擊敗守衛，才能完成這一層的冒險。"));
        }

        private static DialogueData EnsureDialogueData(
            string path,
            params DialogueSeedLine[] lines)
        {
            DialogueData data =
                AssetDatabase.LoadAssetAtPath<DialogueData>(path);
            if (data != null)
            {
                return data;
            }

            data = ScriptableObject.CreateInstance<DialogueData>();
            AssetDatabase.CreateAsset(data, path);
            SerializedObject serialized = new SerializedObject(data);
            SerializedProperty lineArray = serialized.FindProperty("lines");
            lineArray.arraySize = lines.Length;
            for (int index = 0; index < lines.Length; index++)
            {
                SerializedProperty line =
                    lineArray.GetArrayElementAtIndex(index);
                line.FindPropertyRelative("speaker").stringValue =
                    lines[index].Speaker;
                line.FindPropertyRelative("body").stringValue =
                    lines[index].Body;
                line.FindPropertyRelative("choices").arraySize = 0;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(data);
            return data;
        }

        private static void CreateRoom()
        {
            CreatePrimitive(
                "Floor",
                PrimitiveType.Cube,
                new Vector3(0f, -0.15f, 0f),
                new Vector3(10f, 0.3f, 12f),
                new Color(0.28f, 0.24f, 0.2f));
            CreatePrimitive(
                "Back Wall",
                PrimitiveType.Cube,
                new Vector3(0f, 2.5f, 6f),
                new Vector3(10f, 5f, 0.3f),
                new Color(0.24f, 0.25f, 0.29f));
            CreatePrimitive(
                "Front Wall",
                PrimitiveType.Cube,
                new Vector3(0f, 2.5f, -6f),
                new Vector3(10f, 5f, 0.3f),
                new Color(0.24f, 0.25f, 0.29f));
            CreatePrimitive(
                "Left Wall",
                PrimitiveType.Cube,
                new Vector3(-5f, 2.5f, 0f),
                new Vector3(0.3f, 5f, 12f),
                new Color(0.2f, 0.21f, 0.25f));
            CreatePrimitive(
                "Right Wall",
                PrimitiveType.Cube,
                new Vector3(5f, 2.5f, 0f),
                new Vector3(0.3f, 5f, 12f),
                new Color(0.2f, 0.21f, 0.25f));
        }

        private static void CreateAdventureEntrance(Camera targetCamera)
        {
            GameObject entrance = CreatePrimitive(
                "Abyss Entrance",
                PrimitiveType.Cube,
                new Vector3(0f, 1.5f, 5.7f),
                new Vector3(2.2f, 3f, 0.35f),
                new Color(0.16f, 0.06f, 0.24f));
            entrance.layer = LayerMask.NameToLayer("Interactable");
            entrance.AddComponent<StartRunInteraction>();

            GameObject nameCanvasObject = new GameObject(
                "Name Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(RuntimeChineseFontProvider),
                typeof(BillboardView));
            nameCanvasObject.transform.SetParent(entrance.transform, false);
            nameCanvasObject.transform.localPosition = new Vector3(0f, 0.75f, -0.55f);
            nameCanvasObject.transform.localScale = Vector3.one * 0.003f;

            Canvas nameCanvas = nameCanvasObject.GetComponent<Canvas>();
            nameCanvas.renderMode = RenderMode.WorldSpace;
            RectTransform canvasRect =
                nameCanvasObject.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(400f, 80f);

            TMP_Text nameText = CreateText(
                nameCanvasObject.transform,
                "Name",
                "深淵入口",
                36f,
                TextAlignmentOptions.Center,
                Vector2.zero,
                Vector2.one,
                Vector2.zero);
            nameText.color = new Color(0.82f, 0.66f, 1f);

            BillboardView billboard =
                nameCanvasObject.GetComponent<BillboardView>();
            SetObjectReference(billboard, "targetCamera", targetCamera);
        }

        private static void CreateDialogueInteractable(
            string objectName,
            PrimitiveType primitiveType,
            Vector3 position,
            Vector3 scale,
            Color color,
            string displayName,
            string interactionPrompt,
            DialogueData dialogue,
            DialogueRunner dialogueRunner,
            Camera targetCamera)
        {
            GameObject interactable = CreatePrimitive(
                objectName,
                primitiveType,
                position,
                scale,
                color);
            interactable.layer = LayerMask.NameToLayer("Interactable");
            DialogueInteraction interaction =
                interactable.AddComponent<DialogueInteraction>();
            SetString(interaction, "displayName", displayName);
            SetString(interaction, "interactionPrompt", interactionPrompt);
            SetObjectReference(interaction, "dialogue", dialogue);
            SetObjectReference(interaction, "dialogueRunner", dialogueRunner);
            TMP_Text label = CreateWorldLabel(
                interactable,
                targetCamera,
                $"{objectName} Label",
                displayName);
            label.color = new Color(0.85f, 0.9f, 1f);
        }

        private static Canvas CreateScreenCanvas(string name)
        {
            GameObject canvasObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(RuntimeChineseFontProvider));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(InputSystemUIInputModule));
            return canvas;
        }

        private static DialogueRunner CreateDialogueRunner(
            Canvas canvas,
            InputActionAsset inputActions)
        {
            GameObject panel = new GameObject(
                "Dialogue Panel",
                typeof(RectTransform),
                typeof(Image));
            panel.transform.SetParent(canvas.transform, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.12f, 0.06f);
            panelRect.anchorMax = new Vector2(0.88f, 0.38f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color =
                new Color(0.035f, 0.045f, 0.07f, 0.94f);

            TMP_Text speaker = CreateText(
                panel.transform,
                "Speaker",
                string.Empty,
                30f,
                TextAlignmentOptions.Left,
                new Vector2(0.04f, 0.72f),
                new Vector2(0.96f, 0.94f),
                Vector2.zero);
            speaker.color = new Color(0.55f, 0.85f, 1f);
            TMP_Text body = CreateText(
                panel.transform,
                "Body",
                string.Empty,
                30f,
                TextAlignmentOptions.TopLeft,
                new Vector2(0.04f, 0.28f),
                new Vector2(0.96f, 0.72f),
                Vector2.zero);
            TMP_Text choices = CreateText(
                panel.transform,
                "Choices",
                string.Empty,
                22f,
                TextAlignmentOptions.BottomRight,
                new Vector2(0.04f, 0.05f),
                new Vector2(0.96f, 0.28f),
                Vector2.zero);

            DialogueRunner runner =
                canvas.gameObject.AddComponent<DialogueRunner>();
            SetObjectReference(runner, "inputActions", inputActions);
            SetObjectReference(runner, "panel", panel);
            SetObjectReference(runner, "speakerText", speaker);
            SetObjectReference(runner, "bodyText", body);
            SetObjectReference(runner, "choiceText", choices);
            panel.SetActive(false);
            return runner;
        }

        private static TMP_Text CreateWorldLabel(
            GameObject parent,
            Camera targetCamera,
            string name,
            string content)
        {
            GameObject canvasObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(RuntimeChineseFontProvider),
                typeof(BillboardView));
            canvasObject.transform.SetParent(parent.transform, false);
            canvasObject.transform.localPosition = new Vector3(0f, 0.8f, -0.6f);
            canvasObject.transform.localScale = Vector3.one * 0.003f;

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasObject.GetComponent<RectTransform>().sizeDelta =
                new Vector2(420f, 120f);

            TMP_Text text = CreateText(
                canvasObject.transform,
                "Label",
                content,
                34f,
                TextAlignmentOptions.Center,
                Vector2.zero,
                Vector2.one,
                Vector2.zero);
            BillboardView billboard = canvasObject.GetComponent<BillboardView>();
            SetObjectReference(billboard, "targetCamera", targetCamera);
            return text;
        }

        private static Slider CreateSlider(
            Transform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color fillColor)
        {
            GameObject sliderObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Slider));
            sliderObject.transform.SetParent(parent, false);
            RectTransform sliderRect =
                sliderObject.GetComponent<RectTransform>();
            sliderRect.anchorMin = anchorMin;
            sliderRect.anchorMax = anchorMax;
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = Vector2.zero;

            GameObject background = new GameObject(
                "Background",
                typeof(RectTransform),
                typeof(Image));
            background.transform.SetParent(sliderObject.transform, false);
            RectTransform backgroundRect =
                background.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            background.GetComponent<Image>().color =
                new Color(0.05f, 0.05f, 0.07f, 0.85f);

            GameObject fillArea = new GameObject(
                "Fill Area",
                typeof(RectTransform));
            fillArea.transform.SetParent(sliderObject.transform, false);
            RectTransform fillAreaRect =
                fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = new Vector2(3f, 3f);
            fillAreaRect.offsetMax = new Vector2(-3f, -3f);

            GameObject fill = new GameObject(
                "Fill",
                typeof(RectTransform),
                typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            fill.GetComponent<Image>().color = fillColor;

            Slider slider = sliderObject.GetComponent<Slider>();
            slider.fillRect = fillRect;
            slider.targetGraphic = fill.GetComponent<Image>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            return slider;
        }

        private static void CreateQteZone(
            Transform parent,
            string name,
            float anchorMinX,
            float anchorMaxX,
            Color color)
        {
            GameObject zone = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image));
            zone.transform.SetParent(parent, false);
            RectTransform rect = zone.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(anchorMinX, 0f);
            rect.anchorMax = new Vector2(anchorMaxX, 1f);
            rect.offsetMin = new Vector2(3f, 3f);
            rect.offsetMax = new Vector2(-3f, -3f);
            Image image = zone.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }

        private static TMP_Text CreateText(
            Transform parent,
            string name,
            string content,
            float fontSize,
            TextAlignmentOptions alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 sizeDelta)
        {
            GameObject textObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);

            RectTransform rectTransform =
                textObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.sizeDelta = sizeDelta;

            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static GameObject CreateCamera(
            string name,
            Vector3 position,
            Transform parent = null)
        {
            GameObject cameraObject = new GameObject(
                name,
                typeof(Camera),
                typeof(AudioListener));
            cameraObject.transform.SetParent(parent, false);
            cameraObject.transform.position = position;
            return cameraObject;
        }

        private static void CreateDirectionalLight()
        {
            GameObject lightObject = new GameObject(
                "Directional Light",
                typeof(Light));
            Light light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            lightObject.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
        }

        private static GameObject CreatePrimitive(
            string name,
            PrimitiveType primitiveType,
            Vector3 position,
            Vector3 scale,
            Color color)
        {
            GameObject gameObject =
                GameObject.CreatePrimitive(primitiveType);
            gameObject.name = name;
            gameObject.transform.position = position;
            gameObject.transform.localScale = scale;
            gameObject.GetComponent<Renderer>().sharedMaterial =
                EnsureMaterial(name, color);
            return gameObject;
        }

        private static Material EnsureMaterial(string objectName, Color color)
        {
            string safeName = objectName.Replace(' ', '_');
            string path =
                $"Assets/_Project/Data/Materials/{safeName}.mat";
            Shader shader = GetPrototypeShader();
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                material.shader = shader;
                material.color = color;
                EditorUtility.SetDirty(material);
                return material;
            }

            material = new Material(shader)
            {
                name = $"{objectName} Material",
                color = color
            };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Shader GetPrototypeShader()
        {
            string shaderName = GraphicsSettings.currentRenderPipeline != null
                ? "Universal Render Pipeline/Lit"
                : "Standard";
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
            {
                throw new System.InvalidOperationException(
                    $"Unable to find the prototype shader '{shaderName}'.");
            }

            return shader;
        }

        private static T LoadRequiredAsset<T>(string path)
            where T : Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                throw new System.InvalidOperationException(
                    $"Unable to load {typeof(T).Name} at {path}.");
            }

            return asset;
        }

        private static void ConfigureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(
                    $"{SceneFolder}/00_Bootstrap.unity",
                    true),
                new EditorBuildSettingsScene(
                    $"{SceneFolder}/01_Hub.unity",
                    true),
                new EditorBuildSettingsScene(
                    $"{SceneFolder}/02_Exploration.unity",
                    true),
                new EditorBuildSettingsScene(
                    $"{SceneFolder}/03_Combat.unity",
                    true)
            };
        }

        private static void NormalizeGeneratedTextEncoding()
        {
            string[] paths =
            {
                $"{SceneFolder}/00_Bootstrap.unity",
                $"{SceneFolder}/01_Hub.unity",
                $"{SceneFolder}/02_Exploration.unity",
                $"{SceneFolder}/03_Combat.unity",
                "Assets/_Project/Data/Narrative/HubKeeper.asset",
                "Assets/_Project/Data/Narrative/OldJournal.asset",
                "Assets/_Project/Data/Narrative/ExplorationEvent.asset",
                "Assets/_Project/Data/Narrative/BossIntro.asset"
            };

            foreach (string path in paths)
            {
                string absolutePath = Path.GetFullPath(path);
                if (!File.Exists(absolutePath))
                {
                    continue;
                }

                string content = File.ReadAllText(absolutePath, Encoding.UTF8);
                string normalized = Regex.Replace(
                    content,
                    @"\\u([0-9A-Fa-f]{4})",
                    match => ((char)System.Convert.ToInt32(
                        match.Groups[1].Value,
                        16)).ToString());
                if (normalized == content)
                {
                    continue;
                }

                File.WriteAllText(
                    absolutePath,
                    normalized,
                    new UTF8Encoding(false));
            }
        }

        private static void SetObjectReference(
            Object target,
            string propertyName,
            Object value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            serializedObject.Update();
            SerializedProperty property =
                serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new System.InvalidOperationException(
                    $"Serialized property '{propertyName}' was not found on "
                    + target.GetType().Name);
            }

            if (value == null)
            {
                throw new System.InvalidOperationException(
                    $"Cannot assign null to '{propertyName}' on "
                    + target.GetType().Name);
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetObjectReferenceArray(
            Object target,
            string propertyName,
            Object[] values)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            serializedObject.Update();
            SerializedProperty property =
                serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new System.InvalidOperationException(
                    $"Serialized property '{propertyName}' was not found on "
                    + target.GetType().Name);
            }

            property.arraySize = values.Length;
            for (int index = 0; index < values.Length; index++)
            {
                if (values[index] == null)
                {
                    throw new System.InvalidOperationException(
                        $"Cannot assign null at index {index} to "
                        + $"'{propertyName}' on {target.GetType().Name}.");
                }

                property.GetArrayElementAtIndex(index).objectReferenceValue =
                    values[index];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetString(
            Object target,
            string propertyName,
            string value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).stringValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetInteger(
            Object target,
            string propertyName,
            int value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).intValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetFloat(
            Object target,
            string propertyName,
            float value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).floatValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private readonly struct DialogueSeedLine
        {
            public DialogueSeedLine(string speaker, string body)
            {
                Speaker = speaker;
                Body = body;
            }

            public string Speaker { get; }

            public string Body { get; }
        }
    }
}
