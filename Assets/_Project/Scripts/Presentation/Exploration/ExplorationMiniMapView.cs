using System;
using System.Collections.Generic;
using System.Linq;
using ActiveTimeBattle.Domain;
using ActiveTimeBattle.Domain.Exploration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ActiveTimeBattle.Presentation.Exploration
{
    public sealed class ExplorationMiniMapView : MonoBehaviour
    {
        private const float MapHeight = 100f;
        private const int TextureWidth = 512;
        private const int TextureHeight = 384;

        private readonly Dictionary<int, NodeMarkerView> _markers =
            new Dictionary<int, NodeMarkerView>();
        private readonly List<LinkView> _links = new List<LinkView>();

        private RunSession _session;
        private GameObject _mapRoot;
        private Camera _mapCamera;
        private RenderTexture _renderTexture;

        public void Initialize(
            RunSession session,
            IReadOnlyDictionary<int, Vector3> roomCenters,
            TMP_Text textPlaceholder)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            if (roomCenters == null)
            {
                throw new ArgumentNullException(nameof(roomCenters));
            }

            CreateRenderTextureUi(textPlaceholder);
            CreateMapGeometry(roomCenters);
            ConfigureMapCamera(roomCenters.Values);
            Refresh();
        }

        public void Refresh()
        {
            if (_session == null)
            {
                return;
            }

            foreach (KeyValuePair<int, NodeMarkerView> pair in _markers)
            {
                bool visited = _session.IsNodeVisited(pair.Key);
                pair.Value.Root.SetActive(visited);
                if (!visited)
                {
                    continue;
                }

                bool completed = _session.IsNodeCompleted(pair.Key);
                bool current = pair.Key == _session.CurrentNodeId;
                pair.Value.CompletedBadge.SetActive(completed);
                pair.Value.Root.transform.localScale = current
                    ? pair.Value.BaseScale * 1.35f
                    : pair.Value.BaseScale;
                pair.Value.Renderer.material.color = current
                    ? Color.Lerp(pair.Value.BaseColor, Color.white, 0.35f)
                    : pair.Value.BaseColor;
            }

            foreach (LinkView link in _links)
            {
                link.Root.SetActive(
                    _session.IsNodeVisited(link.FromNodeId)
                    && _session.IsNodeVisited(link.ToNodeId));
            }
        }

        private void CreateRenderTextureUi(TMP_Text textPlaceholder)
        {
            if (textPlaceholder == null)
            {
                return;
            }

            textPlaceholder.gameObject.SetActive(false);
            _renderTexture = new RenderTexture(
                TextureWidth,
                TextureHeight,
                16,
                RenderTextureFormat.ARGB32)
            {
                name = "Exploration Mini Map Render Texture"
            };
            _renderTexture.Create();

            GameObject imageObject = new GameObject(
                "Rendered Mini Map",
                typeof(RectTransform),
                typeof(RawImage));
            imageObject.transform.SetParent(textPlaceholder.transform.parent, false);
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.one;
            rect.anchorMax = Vector2.one;
            rect.pivot = Vector2.one;
            rect.sizeDelta = new Vector2(300f, 225f);
            rect.anchoredPosition = new Vector2(-36f, -36f);
            RawImage image = imageObject.GetComponent<RawImage>();
            image.texture = _renderTexture;
            image.color = Color.white;
            image.raycastTarget = false;
        }

        private void CreateMapGeometry(
            IReadOnlyDictionary<int, Vector3> roomCenters)
        {
            _mapRoot = new GameObject("Exploration Mini Map Geometry");
            foreach (ExplorationNode node in _session.ExplorationMap.Nodes)
            {
                foreach (int nextNodeId in node.NextNodeIds)
                {
                    CreateLink(
                        node.Id,
                        nextNodeId,
                        roomCenters[node.Id],
                        roomCenters[nextNodeId]);
                }
            }

            foreach (ExplorationNode node in _session.ExplorationMap.Nodes)
            {
                CreateMarker(node, roomCenters[node.Id]);
            }
        }

        private void CreateLink(
            int fromNodeId,
            int toNodeId,
            Vector3 from,
            Vector3 to)
        {
            Vector3 start = ToMapPosition(from);
            Vector3 end = ToMapPosition(to);
            Vector3 delta = end - start;
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            root.name = $"Map Link {fromNodeId} To {toNodeId}";
            root.transform.SetParent(_mapRoot.transform, false);
            root.transform.position = (start + end) * 0.5f;
            root.transform.rotation = Quaternion.LookRotation(
                delta.normalized,
                Vector3.up);
            root.transform.localScale = new Vector3(0.35f, 0.08f, delta.magnitude);
            root.GetComponent<Renderer>().material.color =
                new Color(0.28f, 0.32f, 0.4f);
            Destroy(root.GetComponent<Collider>());
            _links.Add(new LinkView(root, fromNodeId, toNodeId));
        }

        private void CreateMarker(ExplorationNode node, Vector3 roomCenter)
        {
            PrimitiveType primitive = GetMarkerPrimitive(node.Type);
            GameObject root = GameObject.CreatePrimitive(primitive);
            root.name = $"Map Node {node.Id} {node.Type}";
            root.transform.SetParent(_mapRoot.transform, false);
            root.transform.position = ToMapPosition(roomCenter)
                + Vector3.up * 0.25f;
            Vector3 baseScale = GetMarkerScale(node.Type);
            root.transform.localScale = baseScale;
            Renderer renderer = root.GetComponent<Renderer>();
            Color baseColor = GetMarkerColor(node.Type);
            renderer.material.color = baseColor;
            Destroy(root.GetComponent<Collider>());

            GameObject badge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            badge.name = "Completed Badge";
            badge.transform.SetParent(root.transform, false);
            badge.transform.localPosition = new Vector3(0.55f, 0.65f, 0.55f);
            badge.transform.localScale = Vector3.one * 0.28f;
            badge.GetComponent<Renderer>().material.color = Color.white;
            Destroy(badge.GetComponent<Collider>());

            _markers[node.Id] = new NodeMarkerView(
                root,
                renderer,
                badge,
                baseScale,
                baseColor);
        }

        private void ConfigureMapCamera(IEnumerable<Vector3> roomCenters)
        {
            Vector3[] centers = roomCenters.ToArray();
            float minX = centers.Min(center => center.x);
            float maxX = centers.Max(center => center.x);
            float minZ = centers.Min(center => center.z);
            float maxZ = centers.Max(center => center.z);
            Vector3 mapCenter = new Vector3(
                (minX + maxX) * 0.5f,
                MapHeight + 10f,
                (minZ + maxZ) * 0.5f);

            GameObject cameraObject = new GameObject(
                "Exploration Mini Map Camera",
                typeof(Camera));
            _mapCamera = cameraObject.GetComponent<Camera>();
            _mapCamera.transform.position = mapCenter;
            _mapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            _mapCamera.orthographic = true;
            float width = maxX - minX + 6f;
            float height = maxZ - minZ + 6f;
            float aspect = (float)TextureWidth / TextureHeight;
            _mapCamera.orthographicSize = Mathf.Max(
                height * 0.5f,
                width / (2f * aspect));
            _mapCamera.nearClipPlane = 0.1f;
            _mapCamera.farClipPlane = 20f;
            _mapCamera.clearFlags = CameraClearFlags.SolidColor;
            _mapCamera.backgroundColor = new Color(0.015f, 0.02f, 0.03f);
            _mapCamera.targetTexture = _renderTexture;
        }

        private static Vector3 ToMapPosition(Vector3 worldPosition)
        {
            return new Vector3(worldPosition.x, MapHeight, worldPosition.z);
        }

        private static PrimitiveType GetMarkerPrimitive(
            ExplorationNodeType type)
        {
            return type switch
            {
                ExplorationNodeType.Treasure => PrimitiveType.Cube,
                ExplorationNodeType.Rest => PrimitiveType.Cylinder,
                ExplorationNodeType.Entrance => PrimitiveType.Cylinder,
                ExplorationNodeType.Boss => PrimitiveType.Capsule,
                _ => PrimitiveType.Sphere
            };
        }

        private static Vector3 GetMarkerScale(ExplorationNodeType type)
        {
            return type switch
            {
                ExplorationNodeType.Boss => new Vector3(1.6f, 0.45f, 1.6f),
                ExplorationNodeType.Treasure => new Vector3(1.3f, 0.35f, 1.3f),
                _ => new Vector3(1.2f, 0.35f, 1.2f)
            };
        }

        private static Color GetMarkerColor(ExplorationNodeType type)
        {
            return type switch
            {
                ExplorationNodeType.Entrance => new Color(0.2f, 0.75f, 1f),
                ExplorationNodeType.Combat => new Color(0.85f, 0.12f, 0.16f),
                ExplorationNodeType.Treasure => new Color(1f, 0.65f, 0.08f),
                ExplorationNodeType.Rest => new Color(0.2f, 0.85f, 0.4f),
                ExplorationNodeType.Boss => new Color(0.75f, 0.12f, 0.85f),
                _ => new Color(0.65f, 0.7f, 0.8f)
            };
        }

        private void OnDestroy()
        {
            if (_mapCamera != null)
            {
                _mapCamera.targetTexture = null;
                Destroy(_mapCamera.gameObject);
            }

            if (_mapRoot != null)
            {
                Destroy(_mapRoot);
            }

            if (_renderTexture != null)
            {
                _renderTexture.Release();
                Destroy(_renderTexture);
            }
        }

        private sealed class NodeMarkerView
        {
            public NodeMarkerView(
                GameObject root,
                Renderer renderer,
                GameObject completedBadge,
                Vector3 baseScale,
                Color baseColor)
            {
                Root = root;
                Renderer = renderer;
                CompletedBadge = completedBadge;
                BaseScale = baseScale;
                BaseColor = baseColor;
            }

            public GameObject Root { get; }

            public Renderer Renderer { get; }

            public GameObject CompletedBadge { get; }

            public Vector3 BaseScale { get; }

            public Color BaseColor { get; }
        }

        private sealed class LinkView
        {
            public LinkView(GameObject root, int fromNodeId, int toNodeId)
            {
                Root = root;
                FromNodeId = fromNodeId;
                ToNodeId = toNodeId;
            }

            public GameObject Root { get; }

            public int FromNodeId { get; }

            public int ToNodeId { get; }
        }
    }
}
