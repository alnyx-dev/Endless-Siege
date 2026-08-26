using System.Collections.Generic;
using UnityEngine;

namespace Game.World
{
    public class WorldGenerator : MonoBehaviour
    {
        private enum PropKind { Tree, Rock, Bush }

        [Header("Seed")]
        [Tooltip("Fixed seed for reproducible worlds. Negative = new random seed every run")]
        [SerializeField] private int seed = -1;

        [Header("Area")]
        [Min(5f)]
        [SerializeField] private float areaHalfSize = 45f;
        [Min(0f)]
        [SerializeField] private float clearRadius = 8f;
        [Min(0.5f)]
        [SerializeField] private float minSpacing = 2.5f;

        [Header("Density")]
        [Min(0)]
        [SerializeField] private int treeCount = 90;
        [Min(0)]
        [SerializeField] private int rockCount = 50;
        [Min(0)]
        [SerializeField] private int bushCount = 70;

        [Header("Scale Variation")]
        [SerializeField] private float minScale = 0.85f;
        [SerializeField] private float maxScale = 1.25f;

        [Header("Prefabs")]
        [SerializeField] private GameObject[] trees;
        [SerializeField] private GameObject[] rocks;
        [SerializeField] private GameObject[] bushes;

        private System.Random _rng;
        private Transform _worldRoot;

        private void Awake()
        {
            if (seed < 0) seed = new System.Random().Next();
            _rng = new System.Random(seed);
            Debug.Log($"[WorldGenerator] Seed: {seed}");
            Generate();
        }

        public void Generate()
        {
            Clear();
            _worldRoot = new GameObject("Generated World").transform;
            _worldRoot.SetParent(transform);

            var placed = new List<Vector2>();
            SpawnGroup(trees, treeCount, placed, PropKind.Tree);
            SpawnGroup(rocks, rockCount, placed, PropKind.Rock);
            SpawnGroup(bushes, bushCount, placed, PropKind.Bush);
        }

        // ponytail: O(n^2) spacing check, fine for a few hundred props; swap to a grid if counts grow 10x
        private void SpawnGroup(GameObject[] prefabs, int count, List<Vector2> placed, PropKind kind)
        {
            if (prefabs == null || prefabs.Length == 0) return;

            float clearRadiusSqr = clearRadius * clearRadius;
            float minSpacingSqr = minSpacing * minSpacing;

            for (int i = 0; i < count; i++)
            {
                for (int attempt = 0; attempt < 30; attempt++)
                {
                    float px = (float)(_rng.NextDouble() * 2 - 1) * areaHalfSize;
                    float py = (float)(_rng.NextDouble() * 2 - 1) * areaHalfSize;
                    Vector2 p = new Vector2(px, py);

                    if (p.sqrMagnitude < clearRadiusSqr) continue;

                    bool tooClose = false;
                    for (int j = 0; j < placed.Count; j++)
                    {
                        if ((placed[j] - p).sqrMagnitude < minSpacingSqr)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                    if (tooClose) continue;

                    placed.Add(p);

                    GameObject prefab = prefabs[_rng.Next(prefabs.Length)];
                    GameObject obj = Instantiate(prefab,
                        new Vector3(p.x, 0f, p.y),
                        Quaternion.Euler(0f, (float)_rng.NextDouble() * 360f, 0f),
                        _worldRoot);
                    obj.transform.localScale = Vector3.one * Mathf.Lerp(minScale, maxScale, (float)_rng.NextDouble());
                    AddPropComponents(obj, kind);
                    break;
                }
            }
        }

        private void AddPropComponents(GameObject obj, PropKind kind)
        {
            // pack prefabs ship their own colliders; strip them so each prop ends up with exactly one
            foreach (Collider extra in obj.GetComponentsInChildren<Collider>(true))
            {
                if (Application.isPlaying)
                    Destroy(extra);
                else
                    DestroyImmediate(extra);
            }

            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            Vector3 localCenter = obj.transform.InverseTransformPoint(bounds.center);
            float maxExtent = Mathf.Max(bounds.extents.x, bounds.extents.z);

            switch (kind)
            {
                case PropKind.Tree:
                {
                    CapsuleCollider trunk = obj.AddComponent<CapsuleCollider>();
                    trunk.center = localCenter;
                    trunk.radius = Mathf.Max(0.3f, maxExtent * 0.25f); // trunk width, not canopy
                    trunk.height = Mathf.Max(bounds.size.y, trunk.radius * 2f);
                    break;
                }
                case PropKind.Rock:
                {
                    SphereCollider rock = obj.AddComponent<SphereCollider>();
                    rock.center = localCenter;
                    rock.radius = maxExtent * 0.8f;
                    break;
                }
                case PropKind.Bush:
                {
                    SphereCollider bush = obj.AddComponent<SphereCollider>();
                    bush.center = localCenter;
                    bush.radius = maxExtent * 0.7f;
                    bush.isTrigger = true;
                    break;
                }
            }
        }

        private void Clear()
        {
            if (_worldRoot != null)
            {
                if (Application.isPlaying)
                    Destroy(_worldRoot.gameObject);
                else
                    DestroyImmediate(_worldRoot.gameObject);
            }
        }

        [ContextMenu("Regenerate")]
        public void Regenerate()
        {
            Clear();
            Generate();
        }
    }
}
