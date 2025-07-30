using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

public class TileCombiner : MonoBehaviour
{
    [Tooltip("Determine to show log when combine or clear meshes")]
    public bool showLog = true;
    [Tooltip("Determine to generate collider when combine or not")]
    public bool generateCollider = true;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        Setup();

        if (!meshFilter.sharedMesh)
        {
            Combine();
        }
    }

    // Function to combine all children's meshs into a single mesh.
    public void Combine()
    {
        Setup();

        // If there's no tilemap, don't do anything.
        if (transform.childCount == 0)
        {
            if (showLog)
            {
                Debug.LogWarning("[Tile Combiner] There's no tilemap in '" + name + "' to combine, operation cancelled.");
            }

            return;
        }

        List<Material> materials = new List<Material>();

        List<MeshFilter> meshFilters = new List<MeshFilter>();
        List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
        
        // Find all MeshFilter in children object and add them into a list.
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            foreach (MeshFilter childFilter in child.GetComponentsInChildren<MeshFilter>())
            {
                meshFilters.Add(childFilter);
            }

            foreach (MeshRenderer childRenderer in child.GetComponentsInChildren<MeshRenderer>())
            {
                meshRenderers.Add(childRenderer);
            }
        }

        // If there's no child with MeshFilter, return 0.
        if (meshFilters.Count == 0)
        {
            if (showLog)
            {
                Debug.LogError("[Tile Combiner] Unable to combine mesh! There's no mesh in each child object");
            }

            return;
        }

        // Add materials from child into list.
        foreach (MeshRenderer childRenderer in meshRenderers)
        {
            // Check if it's not this object.
            if (childRenderer.transform == transform)
            {
                continue;
            }

            Material[] localMaterials = childRenderer.sharedMaterials;

            foreach (Material localMaterial in localMaterials)
            {
                // If material list doesn't contain this material, add it.
                if (!materials.Contains(localMaterial))
                {
                    materials.Add(localMaterial);
                }
            }
        }

        List<Mesh> subMeshes = new List<Mesh>();

        foreach (Material material in materials)
        {
            List<CombineInstance> combiners = new List<CombineInstance>();

            for (int i = 0; i < meshFilters.Count; i++)
            {
                // If the mesh is the same as the parent, ignore.
                if (meshFilters[i].gameObject.Equals(gameObject))
                {
                    continue;
                }

                MeshRenderer meshRenderer = meshFilters[i].GetComponent<MeshRenderer>();

                // If child doesn't have mesh renderer, skip to next one.
                if (!meshRenderer)
                {
                    continue;
                }

                Material[] localMaterials = meshRenderer.sharedMaterials;

                for (int j = 0; j < localMaterials.Length; j++)
                {
                    // If material isn't the same one, skip.
                    if (localMaterials[j] != material)
                    {
                        continue;
                    }

                    CombineInstance combine = new CombineInstance();
                    combine.mesh = meshFilters[i].sharedMesh;
                    combine.subMeshIndex = j;
                    combine.transform = meshFilters[i].transform.localToWorldMatrix;

                    combiners.Add(combine);
                }
            }

            Mesh mesh = new Mesh { indexFormat = IndexFormat.UInt32 };
            mesh.CombineMeshes(combiners.ToArray(), true);
            subMeshes.Add(mesh);
        }

        // Combine the all submeshes into one single mesh.
        List<CombineInstance> finalCombiners = new List<CombineInstance>();

        foreach (Mesh mesh in subMeshes)
        {
            CombineInstance combiner = new CombineInstance();
            combiner.mesh = mesh;
            combiner.subMeshIndex = 0;
            combiner.transform = Matrix4x4.identity;

            finalCombiners.Add(combiner);
        }

        Mesh finalMesh = new Mesh { indexFormat = IndexFormat.UInt32 };
        finalMesh.name = name;
        finalMesh.CombineMeshes(finalCombiners.ToArray(), false);

        meshFilter.sharedMesh = finalMesh;

        if (generateCollider && meshCollider)
        {
            meshCollider.sharedMesh = finalMesh;
        }
        
        meshRenderer.sharedMaterials = materials.ToArray();
        
        // Disable all child game object.
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        // Check if user need to show on log after operation or not.
        if (showLog)
        {
            Debug.Log("[Tile Combiner] Combined " + meshFilters.Count + " meshes successfully!");
        }
    }

    // Function to clear mesh from the meshfilter.
    public void Clear()
    {
        Setup();

        meshFilter.sharedMesh = null;

        // If there's mesh collider, destroy it.
        if (GetComponent<Collider>())
        {
            DestroyImmediate(GetComponent<Collider>());
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }

        meshRenderer.sharedMaterials = new Material[0];

        if (showLog)
        {
            Debug.Log("[Tile Combiner] Successfully cleared mesh!");
        }
    }

    // Function to setup the components of game object.
    private void Setup()
    {
        // If there's tilemap renderer, destroy it. (It auto-generate along with Tilemap)
        if (GetComponent<TilemapRenderer>())
        {
            DestroyImmediate(GetComponent<TilemapRenderer>());
        }

        // If mesh filter hasn't been setup, set references.
        if (!meshFilter)
        {
            // If there's no mesh filter, create one.
            if (!GetComponent<MeshFilter>())
            {
                meshFilter = this.AddComponent<MeshFilter>();
            }

            meshFilter = GetComponent<MeshFilter>();
        }

        // If mesh renderer hasn't been setup, set references.
        if (!meshRenderer)
        {
            // If there's no mesh collider, create one.
            if (!GetComponent<MeshRenderer>())
            {
                meshRenderer = this.AddComponent<MeshRenderer>();
            }

            meshRenderer = GetComponent<MeshRenderer>();
        }

        // If mesh collider hasn't been setup, set references.
        if (generateCollider)
        {
            // If there's no mesh collider, create one.
            if (!GetComponent<MeshCollider>())
            {
                meshCollider = this.AddComponent<MeshCollider>();
            }

            meshCollider = GetComponent<MeshCollider>();
        }
        else
        {
            if (meshCollider)
            {
                DestroyImmediate(meshCollider);
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TileCombiner))]
public class TileCombinerEditor : Editor
{
    private TileCombiner meshCombiner;

    public void OnEnable()
    {
        meshCombiner = target as TileCombiner;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        serializedObject.Update();

        // Combine + Clear button
        EditorGUILayout.BeginHorizontal();

        GUIContent combineContent = new GUIContent("Combine");
        combineContent.tooltip = "Combine all child's meshs into a single mesh";

        if (GUILayout.Button(combineContent, GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.5f)))
        {
            meshCombiner.Combine();
        }

        GUIContent clearContent = new GUIContent("Clear");
        clearContent.tooltip = "Clear mesh from Mesh Filter";

        if (GUILayout.Button(clearContent, GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.5f)))
        {
            meshCombiner.Clear();
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(10f);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif