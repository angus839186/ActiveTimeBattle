using System;
using System.Collections.Generic;
using System.Linq;
using ActiveTimeBattle.Domain;
using ActiveTimeBattle.Domain.Exploration;
using ActiveTimeBattle.Presentation.Core;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ActiveTimeBattle.Presentation.Exploration
{
    public sealed class ExplorationPlaceholderView : MonoBehaviour
    {
        private const float RoomSize = 8f;
        private const float HalfRoom = RoomSize * 0.5f;
        private const float RoomSpacing = 12f;
        private const float InteractDistance = 1.35f;
        private const float WallFadeDistance = 1.8f;
        private const float WallVisibleAlpha = 0.82f;
        private const float WallFadedAlpha = 0.18f;

        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private Camera explorationCamera;
        [SerializeField] private TMP_Text seedText;
        [SerializeField] private TMP_Text buildText;
        [SerializeField] private TMP_Text optionText;
        [SerializeField, Min(0.1f)] private float moveSpeed = 4.5f;
        [SerializeField, Min(0.1f)] private float cameraMoveSpeed = 5f;
        [SerializeField, Min(1f)] private float orthographicSize = 6.2f;
        [SerializeField] private Vector3 cameraOffset =
            new Vector3(7.5f, 8f, -7.5f);
        [SerializeField] private Vector3 cameraEulerAngles =
            new Vector3(35.264f, 315f, 0f);

        private readonly Dictionary<int, NodeRoomView> _rooms =
            new Dictionary<int, NodeRoomView>();
        private readonly Dictionary<int, Vector3> _roomCenters =
            new Dictionary<int, Vector3>();
        private readonly List<OptionItem> _options = new List<OptionItem>();
        private readonly Dictionary<InputAction, int> _choiceActions =
            new Dictionary<InputAction, int>();

        private InputAction _moveAction;
        private InputAction _interactAction;
        private InputAction _cancelAction;
        private InputAction _openBuildAction;
        private GameObject _roomsRoot;
        private GameObject _player;
        private ExplorationMiniMapView _miniMapView;
        private RunSession _session;
        private NodeRoomView _currentRoom;
        private Vector3 _cameraTargetPosition;
        private DoorView _nearDoor;
        private InteractableView _nearInteractable;
        private bool _isOptionOpen;
        private bool _isTransitioning;
        private string _lastNodeResult;
        private int _selectedOptionIndex;

        private void Awake()
        {
            InputActionAsset actions = inputActions != null
                ? inputActions
                : InputSystem.actions;
            _moveAction = actions.FindAction("Exploration/Move", true);
            _interactAction = actions.FindAction("Exploration/Interact", true);
            _cancelAction = actions.FindAction("Exploration/Cancel", true);
            _openBuildAction = actions.FindAction(
                "Exploration/OpenBuild",
                true);

            RegisterChoice(actions, "Exploration/Choice1", 0);
            RegisterChoice(actions, "Exploration/Choice2", 1);
            RegisterChoice(actions, "Exploration/Choice3", 2);
        }

        private void Start()
        {
            _session = GameBootstrapper.Instance?
                .RunFlowController
                .CurrentSession;
            if (_session == null)
            {
                seedText.text = "探索雛型\n目前沒有 Run Session";
                return;
            }

            if (explorationCamera == null)
            {
                explorationCamera = Camera.main;
            }

            ConfigureExplorationCamera();
            BuildRooms(_session);
            CreatePlayer();
            _miniMapView = gameObject.AddComponent<ExplorationMiniMapView>();
            _miniMapView.Initialize(_session, _roomCenters, buildText);
            EnterNode(_session.CurrentNodeId, true);
            RefreshHud();
        }

        private void OnEnable()
        {
            _interactAction.performed += OnInteractPerformed;
            _cancelAction.performed += OnCancelPerformed;
            _openBuildAction.performed += OnOpenBuildPerformed;
            foreach (KeyValuePair<InputAction, int> choice in _choiceActions)
            {
                choice.Key.performed += OnChoicePerformed;
            }
        }

        private void OnDisable()
        {
            _interactAction.performed -= OnInteractPerformed;
            _cancelAction.performed -= OnCancelPerformed;
            _openBuildAction.performed -= OnOpenBuildPerformed;
            foreach (KeyValuePair<InputAction, int> choice in _choiceActions)
            {
                choice.Key.performed -= OnChoicePerformed;
            }
        }

        private void Update()
        {
            if (_session == null)
            {
                return;
            }

            if (!_isOptionOpen && !_isTransitioning)
            {
                MovePlayer();
                FindNearbyInteraction();
            }

            UpdateForegroundWallTransparency();
            MoveCamera();
            RefreshHud();
        }

        private void RegisterChoice(
            InputActionAsset actions,
            string actionName,
            int index)
        {
            InputAction action = actions.FindAction(actionName, false);
            if (action != null)
            {
                _choiceActions[action] = index;
            }
        }

        private void BuildRooms(RunSession session)
        {
            _roomsRoot = new GameObject("Generated Exploration Rooms");
            Dictionary<int, Vector2Int> grid = BuildGrid(session.ExplorationMap);

            foreach (ExplorationNode node in session.ExplorationMap.Nodes)
            {
                _roomCenters[node.Id] = GridToWorld(grid[node.Id]);
            }

            foreach (ExplorationNode node in session.ExplorationMap.Nodes)
            {
                NodeRoomView room = CreateRoom(node, _roomCenters[node.Id]);
                _rooms[node.Id] = room;
            }
        }

        private static Dictionary<int, Vector2Int> BuildGrid(
            ExplorationMap map)
        {
            Dictionary<int, Vector2Int> result = new Dictionary<int, Vector2Int>();
            IGrouping<int, ExplorationNode>[] layers = map.Nodes
                .GroupBy(node => node.Depth)
                .OrderBy(layer => layer.Key)
                .ToArray();

            foreach (IGrouping<int, ExplorationNode> layer in layers)
            {
                ExplorationNode[] nodes = layer.OrderBy(node => node.Id).ToArray();
                for (int index = 0; index < nodes.Length; index++)
                {
                    int y = (nodes.Length - 1) - (index * 2);
                    result[nodes[index].Id] = new Vector2Int(layer.Key, y);
                }
            }

            return result;
        }

        private static Vector3 GridToWorld(Vector2Int grid)
        {
            Vector3 screenUp = new Vector3(-1f, 0f, 1f).normalized;
            Vector3 screenRight = new Vector3(1f, 0f, 1f).normalized;
            return screenUp * (grid.x * RoomSpacing)
                + screenRight * (grid.y * RoomSpacing);
        }

        private NodeRoomView CreateRoom(ExplorationNode node, Vector3 center)
        {
            GameObject root = new GameObject($"Node {node.Id} {node.Type}");
            root.transform.SetParent(_roomsRoot.transform, false);
            root.transform.position = center;

            CreatePrimitive(
                root.transform,
                "Floor",
                PrimitiveType.Cube,
                Vector3.zero,
                new Vector3(RoomSize, 0.25f, RoomSize),
                new Color(0.09f, 0.07f, 0.12f));
            CreateWall(root.transform, "North Wall", new Vector3(0f, 0.75f, HalfRoom),
                new Vector3(RoomSize, 1.5f, 0.25f));
            Renderer southWall = CreateWall(
                root.transform,
                "South Wall",
                new Vector3(0f, 0.75f, -HalfRoom),
                new Vector3(RoomSize, 1.5f, 0.25f),
                true);
            Renderer eastWall = CreateWall(
                root.transform,
                "East Wall",
                new Vector3(HalfRoom, 0.75f, 0f),
                new Vector3(0.25f, 1.5f, RoomSize),
                true);
            CreateWall(root.transform, "West Wall", new Vector3(-HalfRoom, 0.75f, 0f),
                new Vector3(0.25f, 1.5f, RoomSize));

            NodeRoomView room = new NodeRoomView(node, root, center);
            room.ForegroundWalls.Add(
                new ForegroundWallView(southWall, DoorDirection.South));
            room.ForegroundWalls.Add(
                new ForegroundWallView(eastWall, DoorDirection.East));
            CreateNodeInteractable(room);
            CreateDoors(room);
            root.SetActive(false);
            return room;
        }

        private void CreateNodeInteractable(NodeRoomView room)
        {
            ExplorationNode node = room.Node;
            switch (node.Type)
            {
                case ExplorationNodeType.Treasure:
                    room.Interactable = new InteractableView(
                        CreatePrimitive(
                            room.Root.transform,
                            "Treasure Chest",
                            PrimitiveType.Cube,
                            new Vector3(0f, 0.65f, 0f),
                        new Vector3(1.4f, 1f, 1f),
                            new Color(0.82f, 0.46f, 0.08f)),
                        "按 E 開啟寶箱");
                    break;
                case ExplorationNodeType.Rest:
                    room.Interactable = new InteractableView(
                        CreatePrimitive(
                            room.Root.transform,
                            "Campfire",
                            PrimitiveType.Cylinder,
                            new Vector3(0f, 0.35f, 0f),
                            new Vector3(1.2f, 0.45f, 1.2f),
                            new Color(1f, 0.22f, 0.04f)),
                        "按 E 使用營地");
                    break;
                case ExplorationNodeType.Combat:
                case ExplorationNodeType.Boss:
                    room.Interactable = new InteractableView(
                        CreatePrimitive(
                            room.Root.transform,
                            "Node Enemy",
                            PrimitiveType.Capsule,
                            new Vector3(0f, 1f, 0f),
                            Vector3.one,
                            node.Type == ExplorationNodeType.Boss
                                ? new Color(0.7f, 0.05f, 0.7f)
                                : new Color(0.58f, 0.07f, 0.1f)),
                        "按 E 面對敵人");
                    break;
            }
        }

        private void CreateDoors(NodeRoomView room)
        {
            HashSet<DoorDirection> usedDirections =
                new HashSet<DoorDirection>();
            IReadOnlyList<ExplorationNode> connectedNodes =
                _session.ExplorationMap.GetConnectedNodes(room.Node.Id);
            foreach (ExplorationNode connectedNode in connectedNodes)
            {
                Vector3 delta =
                    _roomCenters[connectedNode.Id] - room.Center;
                DoorDirection direction = GetAvailableDoorDirection(
                    delta,
                    usedDirections);
                usedDirections.Add(direction);
                GameObject door = CreatePrimitive(
                    room.Root.transform,
                    $"Door {direction} To {connectedNode.Id}",
                    PrimitiveType.Cube,
                    GetDoorLocalPosition(direction),
                    GetDoorScale(direction),
                    new Color(0.14f, 0.42f, 0.56f));
                room.Doors.Add(
                    new DoorView(
                        door,
                        connectedNode.Id,
                        direction));
            }
        }

        private static DoorDirection GetAvailableDoorDirection(
            Vector3 delta,
            ISet<DoorDirection> usedDirections)
        {
            Vector3 direction = new Vector3(delta.x, 0f, delta.z).normalized;
            return Enum.GetValues(typeof(DoorDirection))
                .Cast<DoorDirection>()
                .Where(candidate => !usedDirections.Contains(candidate))
                .OrderByDescending(
                    candidate => Vector3.Dot(
                        direction,
                        GetDoorWorldDirection(candidate)))
                .First();
        }

        private static Vector3 GetDoorWorldDirection(DoorDirection direction)
        {
            return direction switch
            {
                DoorDirection.North => Vector3.forward,
                DoorDirection.East => Vector3.right,
                DoorDirection.South => Vector3.back,
                _ => Vector3.left
            };
        }

        private static Vector3 GetDoorLocalPosition(DoorDirection direction)
        {
            return direction switch
            {
                DoorDirection.North => new Vector3(0f, 0.75f, HalfRoom),
                DoorDirection.East => new Vector3(HalfRoom, 0.75f, 0f),
                DoorDirection.South => new Vector3(0f, 0.75f, -HalfRoom),
                _ => new Vector3(-HalfRoom, 0.75f, 0f)
            };
        }

        private static Vector3 GetDoorScale(DoorDirection direction)
        {
            return direction == DoorDirection.North
                || direction == DoorDirection.South
                ? new Vector3(1.6f, 1.5f, 0.3f)
                : new Vector3(0.3f, 1.5f, 1.6f);
        }

        private static GameObject CreatePrimitive(
            Transform parent,
            string name,
            PrimitiveType primitive,
            Vector3 localPosition,
            Vector3 scale,
            Color color)
        {
            GameObject obj = GameObject.CreatePrimitive(primitive);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPosition;
            obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().material.color = color;
            return obj;
        }

        private static Renderer CreateWall(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 scale,
            bool transparent = false)
        {
            GameObject wall = CreatePrimitive(
                parent,
                name,
                PrimitiveType.Cube,
                localPosition,
                scale,
                transparent
                    ? new Color(0.22f, 0.07f, 0.1f, 0.32f)
                    : new Color(0.22f, 0.07f, 0.1f));
            if (transparent)
            {
                ConfigureTransparentMaterial(wall.GetComponent<Renderer>());
            }

            return wall.GetComponent<Renderer>();
        }

        private static void ConfigureTransparentMaterial(Renderer renderer)
        {
            Material material = renderer.material;
            material.color = new Color(
                material.color.r,
                material.color.g,
                material.color.b,
                WallVisibleAlpha);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = 3000;
        }

        private static void SetMaterialAlpha(Material material, float alpha)
        {
            Color color = material.color;
            color.a = alpha;
            material.color = color;
        }

        private void CreatePlayer()
        {
            _player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            _player.name = "Exploration Player";
            _player.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);
            _player.GetComponent<Renderer>().material.color =
                new Color(0.18f, 0.36f, 0.72f);
        }

        private void ConfigureExplorationCamera()
        {
            if (explorationCamera == null)
            {
                return;
            }

            explorationCamera.clearFlags = CameraClearFlags.SolidColor;
            explorationCamera.backgroundColor = Color.black;
            explorationCamera.orthographic = true;
            explorationCamera.orthographicSize = orthographicSize;
        }

        private void EnterNode(int nodeId, bool instantCamera)
        {
            foreach (NodeRoomView room in _rooms.Values)
            {
                room.Root.SetActive(room.Node.Id == nodeId);
            }

            _currentRoom = _rooms[nodeId];
            _player.transform.position =
                _currentRoom.Center + new Vector3(0f, 0.9f, -1.5f);
            _cameraTargetPosition = _currentRoom.Center + cameraOffset;
            if (instantCamera && explorationCamera != null)
            {
                explorationCamera.transform.position = _cameraTargetPosition;
                explorationCamera.transform.rotation =
                    Quaternion.Euler(cameraEulerAngles);
            }

            UpdateDoorVisuals();
            RefreshNodeObjectVisibility();
            if (!_session.IsCurrentNodeCompleted
                && (_session.CurrentNode.Type == ExplorationNodeType.Combat
                    || _session.CurrentNode.Type == ExplorationNodeType.Boss))
            {
                OpenCombatOptions();
            }
        }

        private void MovePlayer()
        {
            Vector2 input = _moveAction.ReadValue<Vector2>();
            if (input.sqrMagnitude <= 0.001f)
            {
                return;
            }

            Vector3 cameraForward = Vector3.ProjectOnPlane(
                explorationCamera.transform.forward,
                Vector3.up).normalized;
            Vector3 cameraRight = Vector3.ProjectOnPlane(
                explorationCamera.transform.right,
                Vector3.up).normalized;
            Vector3 move = cameraRight * input.x + cameraForward * input.y;
            if (move.sqrMagnitude > 1f)
            {
                move.Normalize();
            }

            Vector3 nextPosition =
                _player.transform.position + move * moveSpeed * Time.deltaTime;
            Vector3 local = nextPosition - _currentRoom.Center;
            local.x = Mathf.Clamp(local.x, -HalfRoom + 0.65f, HalfRoom - 0.65f);
            local.z = Mathf.Clamp(local.z, -HalfRoom + 0.65f, HalfRoom - 0.65f);
            _player.transform.position =
                _currentRoom.Center + new Vector3(local.x, 0.9f, local.z);
            _player.transform.rotation = Quaternion.LookRotation(move);
        }

        private void UpdateForegroundWallTransparency()
        {
            if (_currentRoom == null || _player == null)
            {
                return;
            }

            Vector3 local = _player.transform.position - _currentRoom.Center;
            foreach (ForegroundWallView wall in _currentRoom.ForegroundWalls)
            {
                float distance = wall.Direction == DoorDirection.South
                    ? Mathf.Abs(local.z + HalfRoom)
                    : Mathf.Abs(local.x - HalfRoom);
                float targetAlpha = distance <= WallFadeDistance
                    ? WallFadedAlpha
                    : WallVisibleAlpha;
                wall.Alpha = Mathf.MoveTowards(
                    wall.Alpha,
                    targetAlpha,
                    Time.deltaTime * 2.5f);
                SetMaterialAlpha(wall.Renderer.material, wall.Alpha);
            }
        }

        private void MoveCamera()
        {
            if (explorationCamera == null)
            {
                return;
            }

            explorationCamera.transform.position = Vector3.Lerp(
                explorationCamera.transform.position,
                _cameraTargetPosition,
                Time.deltaTime * cameraMoveSpeed);
            explorationCamera.transform.rotation = Quaternion.Lerp(
                explorationCamera.transform.rotation,
                Quaternion.Euler(cameraEulerAngles),
                Time.deltaTime * cameraMoveSpeed);
        }

        private void FindNearbyInteraction()
        {
            _nearDoor = null;
            _nearInteractable = null;

            if (!_session.AreCurrentNodeExitsLocked)
            {
                foreach (DoorView door in _currentRoom.Doors)
                {
                    if (Vector3.Distance(
                            _player.transform.position,
                            door.Object.transform.position)
                        <= InteractDistance)
                    {
                        _nearDoor = door;
                        return;
                    }
                }
            }

            if (_currentRoom.Interactable != null
                && _currentRoom.Interactable.Object.activeSelf
                && Vector3.Distance(
                    _player.transform.position,
                    _currentRoom.Interactable.Object.transform.position)
                <= InteractDistance)
            {
                _nearInteractable = _currentRoom.Interactable;
            }
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (_session == null || _isTransitioning)
            {
                return;
            }

            if (_isOptionOpen)
            {
                ChooseOption(_selectedOptionIndex);
                return;
            }

            if (_nearDoor != null)
            {
                MoveThroughDoorAsync(_nearDoor.TargetNodeId).Forget();
                return;
            }

            if (_nearInteractable == null)
            {
                return;
            }

            switch (_session.CurrentNode.Type)
            {
                case ExplorationNodeType.Treasure:
                    _lastNodeResult = _session.OpenTreasure();
                    RefreshNodeObjectVisibility();
                    UpdateDoorVisuals();
                    break;
                case ExplorationNodeType.Rest:
                    OpenCampOptions();
                    break;
                case ExplorationNodeType.Combat:
                case ExplorationNodeType.Boss:
                    OpenCombatOptions();
                    break;
            }
        }

        private async UniTaskVoid MoveThroughDoorAsync(int targetNodeId)
        {
            NodeRoomView sourceRoom = _currentRoom;
            if (!_session.TryMoveToNode(targetNodeId))
            {
                return;
            }

            _isTransitioning = true;
            _currentRoom = _rooms[targetNodeId];
            sourceRoom.Root.SetActive(true);
            _currentRoom.Root.SetActive(true);
            DoorView entryDoor = _currentRoom.Doors.FirstOrDefault(
                door => door.TargetNodeId == sourceRoom.Node.Id);
            _player.transform.position = GetEntryPosition(
                _currentRoom,
                entryDoor);
            _cameraTargetPosition = _currentRoom.Center + cameraOffset;
            UpdateDoorVisuals();
            RefreshNodeObjectVisibility();

            await UniTask.Delay(
                TimeSpan.FromSeconds(0.65f),
                cancellationToken: destroyCancellationToken);
            foreach (NodeRoomView room in _rooms.Values)
            {
                room.Root.SetActive(room == _currentRoom);
            }

            _isTransitioning = false;

            if (!_session.IsCurrentNodeCompleted
                && (_session.CurrentNode.Type == ExplorationNodeType.Combat
                    || _session.CurrentNode.Type == ExplorationNodeType.Boss))
            {
                OpenCombatOptions();
            }
        }

        private static Vector3 GetEntryPosition(
            NodeRoomView room,
            DoorView entryDoor)
        {
            if (entryDoor == null)
            {
                return room.Center + new Vector3(0f, 0.9f, -1.5f);
            }

            Vector3 inward = -GetDoorWorldDirection(entryDoor.Direction);
            Vector3 doorPosition = GetDoorLocalPosition(entryDoor.Direction);
            return room.Center
                + new Vector3(doorPosition.x, 0.9f, doorPosition.z)
                + inward * 1.1f;
        }

        private void OpenCampOptions()
        {
            OpenOptions(
                "營地\n你想怎麼使用這個營地？",
                new OptionItem("1. 回復生命", () =>
                {
                    _lastNodeResult = _session.RestAtCamp(40);
                    CloseOptions();
                    RefreshNodeObjectVisibility();
                    UpdateDoorVisuals();
                }),
                new OptionItem("2. 獲得金錢", () =>
                {
                    _lastNodeResult = _session.TakeCampMoney(25);
                    CloseOptions();
                    RefreshNodeObjectVisibility();
                    UpdateDoorVisuals();
                }),
                new OptionItem("3. 不做任何事", () =>
                {
                    _lastNodeResult = _session.LeaveCamp();
                    CloseOptions();
                    RefreshNodeObjectVisibility();
                    UpdateDoorVisuals();
                }));
        }

        private void OpenCombatOptions()
        {
            string enemyName = _session.CurrentNode.Type == ExplorationNodeType.Boss
                ? "深淵守門者"
                : "巡邏敵人";
            OpenOptions(
                $"遭遇敵人：{enemyName}\n是否進入戰鬥？",
                new OptionItem("1. 是，進入戰鬥", () =>
                {
                    CloseOptions();
                    GameBootstrapper.Instance.RunFlowController
                        .EnterCombatAsync(GameBootstrapper.Instance.LifetimeToken)
                        .Forget();
                }),
                new OptionItem("2. 否，返回上一個節點", () =>
                {
                    CloseOptions();
                    if (_session.TryReturnToPreviousNode())
                    {
                        EnterNode(_session.CurrentNodeId, false);
                    }
                    else
                    {
                        _lastNodeResult = "沒有上一個節點可以返回。";
                    }
                }));
        }

        private void OpenOptions(string title, params OptionItem[] options)
        {
            _options.Clear();
            _options.AddRange(options);
            _selectedOptionIndex = 0;
            _isOptionOpen = true;
            RefreshOptionText(title);
        }

        private void RefreshOptionText(string title)
        {
            if (optionText == null)
            {
                return;
            }

            string[] lines = new string[_options.Count];
            for (int index = 0; index < _options.Count; index++)
            {
                lines[index] =
                    $"{(index == _selectedOptionIndex ? ">" : " ")} "
                    + _options[index].Text;
            }

            optionText.gameObject.SetActive(true);
            optionText.text = title
                + "\n\n"
                + string.Join("\n", lines)
                + "\n\n按 1/2/3 或 E 確認，Esc 取消";
        }

        private void CloseOptions()
        {
            _isOptionOpen = false;
            _options.Clear();
            if (optionText != null)
            {
                optionText.gameObject.SetActive(false);
            }
        }

        private void ChooseOption(int index)
        {
            if (index < 0 || index >= _options.Count)
            {
                return;
            }

            _options[index].Action.Invoke();
        }

        private void OnChoicePerformed(InputAction.CallbackContext context)
        {
            if (!_isOptionOpen)
            {
                return;
            }

            ChooseOption(_choiceActions[context.action]);
        }

        private void OnCancelPerformed(InputAction.CallbackContext context)
        {
            if (_isOptionOpen)
            {
                CloseOptions();
                return;
            }

            GameBootstrapper bootstrapper = GameBootstrapper.Instance;
            bootstrapper.RunFlowController
                .EnterHubAsync(bootstrapper.LifetimeToken)
                .Forget();
        }

        private void OnOpenBuildPerformed(InputAction.CallbackContext context)
        {
            if (_isOptionOpen && _options.Count > 0)
            {
                _selectedOptionIndex =
                    (_selectedOptionIndex + 1)
                    % _options.Count;
                RefreshOptionText("選擇行動");
            }
        }

        private void RefreshNodeObjectVisibility()
        {
            foreach (NodeRoomView room in _rooms.Values)
            {
                if (room.Interactable == null)
                {
                    continue;
                }

                room.Interactable.Object.SetActive(
                    !_session.IsNodeCompleted(room.Node.Id));
            }
        }

        private void UpdateDoorVisuals()
        {
            foreach (DoorView door in _currentRoom.Doors)
            {
                Renderer renderer = door.Object.GetComponent<Renderer>();
                bool open = !_session.AreCurrentNodeExitsLocked;
                renderer.material.color = open
                    ? new Color(0.14f, 0.42f, 0.56f)
                    : new Color(0.25f, 0.25f, 0.3f);
            }
        }

        private void RefreshHud()
        {
            if (_session == null)
            {
                return;
            }

            string prompt = _nearDoor != null
                ? "按 E 通過門"
                : _nearInteractable != null
                    ? _nearInteractable.Prompt
                    : !_session.AreCurrentNodeExitsLocked
                        ? "尋找門前往下一個節點"
                        : "探索當前節點";
            seedText.text =
                $"Run Seed: {_session.Seed}\n"
                + $"生命：{_session.Player.CurrentHealth} / "
                + $"{_session.Player.MaximumHealth}　金錢：{_session.Resources}\n"
                + $"節點 {_session.CurrentNode.Id}｜"
                + $"{GetNodeTypeLabel(_session.CurrentNode.Type)}｜"
                + $"{(_session.IsCurrentNodeCompleted ? "已完成" : "未完成")}\n"
                + $"{prompt}"
                + (string.IsNullOrWhiteSpace(_lastNodeResult)
                    ? string.Empty
                    : $"\n{_lastNodeResult}");

            if (buildText != null)
            {
                buildText.gameObject.SetActive(false);
            }

            _miniMapView?.Refresh();
        }

        private static string GetNodeTypeLabel(ExplorationNodeType type)
        {
            return type switch
            {
                ExplorationNodeType.Entrance => "入口",
                ExplorationNodeType.Combat => "戰鬥",
                ExplorationNodeType.Treasure => "寶箱",
                ExplorationNodeType.Event => "事件",
                ExplorationNodeType.Shop => "商店",
                ExplorationNodeType.Rest => "休息",
                ExplorationNodeType.Boss => "BOSS",
                _ => type.ToString()
            };
        }

        private enum DoorDirection
        {
            North,
            East,
            South,
            West
        }

        private sealed class NodeRoomView
        {
            public NodeRoomView(
                ExplorationNode node,
                GameObject root,
                Vector3 center)
            {
                Node = node;
                Root = root;
                Center = center;
            }

            public ExplorationNode Node { get; }

            public GameObject Root { get; }

            public Vector3 Center { get; }

            public List<DoorView> Doors { get; } = new List<DoorView>();

            public List<ForegroundWallView> ForegroundWalls { get; } =
                new List<ForegroundWallView>();

            public InteractableView Interactable { get; set; }
        }

        private sealed class ForegroundWallView
        {
            public ForegroundWallView(
                Renderer renderer,
                DoorDirection direction)
            {
                Renderer = renderer;
                Direction = direction;
                Alpha = WallVisibleAlpha;
            }

            public Renderer Renderer { get; }

            public DoorDirection Direction { get; }

            public float Alpha { get; set; }
        }

        private sealed class DoorView
        {
            public DoorView(
                GameObject obj,
                int targetNodeId,
                DoorDirection direction)
            {
                Object = obj;
                TargetNodeId = targetNodeId;
                Direction = direction;
            }

            public GameObject Object { get; }

            public int TargetNodeId { get; }

            public DoorDirection Direction { get; }
        }

        private sealed class InteractableView
        {
            public InteractableView(GameObject obj, string prompt)
            {
                Object = obj;
                Prompt = prompt;
            }

            public GameObject Object { get; }

            public string Prompt { get; }
        }

        private sealed class OptionItem
        {
            public OptionItem(string text, Action action)
            {
                Text = text;
                Action = action;
            }

            public string Text { get; }

            public Action Action { get; }
        }
    }
}
