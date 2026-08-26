using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.EditorTools
{
    public static class WorldGeneratorMenu
    {
        [MenuItem("Tools/Create World Generator")]
        public static void CreateWorldGenerator()
        {
            GameObject go = GameObject.Find("WorldGenerator");
            if (go == null)
            {
                go = new GameObject("WorldGenerator");
                Undo.RegisterCreatedObjectUndo(go, "Create World Generator");
            }

            var gen = go.GetComponent<Game.World.WorldGenerator>();
            if (gen == null)
                gen = Undo.AddComponent<Game.World.WorldGenerator>(go);

            string[] treeGuids = AssetDatabase.FindAssets("t:Prefab",
                new[] { "Assets/Low_Poly_Nature_Pack_Lite/Prefabs/Trees" });
            string[] rockGuids = AssetDatabase.FindAssets("t:Prefab",
                new[] { "Assets/Low_Poly_Nature_Pack_Lite/Prefabs/Stones" });
            string[] bushGuids = AssetDatabase.FindAssets("t:Prefab",
                new[] { "Assets/Low_Poly_Nature_Pack_Lite/Prefabs/Bushes" });

            var trees = FilterPrefabs(treeGuids,
                name => name.StartsWith("Tree_") &&
                        !name.Contains("gradient") && !name.Contains("winter") && !name.Contains("snow"),
                "Trees");

            var rocks = FilterPrefabs(rockGuids,
                name => name.StartsWith("Stone_") && !name.Contains("snow"),
                "Stones");

            var bushes = FilterPrefabs(bushGuids,
                name => name.StartsWith("Bush_") &&
                        !name.Contains("gradient") && !name.Contains("winter") && !name.Contains("snow"),
                "Bushes");

            var so = new SerializedObject(gen);
            AssignArray(so, "trees", trees);
            AssignArray(so, "rocks", rocks);
            AssignArray(so, "bushes", bushes);
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(go.scene);

            Debug.Log($"[WorldGenerator] Assigned {trees.Count} tree, {rocks.Count} rock, {bushes.Count} bush prefabs");
        }

        private static List<GameObject> FilterPrefabs(string[] guids,
            System.Func<string, bool> predicate, string folder)
        {
            var result = new List<GameObject>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string name = Path.GetFileNameWithoutExtension(path);
                if (predicate(name))
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab != null)
                        result.Add(prefab);
                }
            }
            if (result.Count == 0)
                Debug.LogWarning($"[WorldGenerator] No prefabs matched in {folder}");
            return result;
        }

        private static void AssignArray(SerializedObject so, string propertyName, List<GameObject> prefabs)
        {
            var prop = so.FindProperty(propertyName);
            prop.arraySize = prefabs.Count;
            for (int i = 0; i < prefabs.Count; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = prefabs[i];
        }
    }
}
