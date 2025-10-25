using UnityEngine;

public class DisableConcaveCollidersOnArticulationBodies : MonoBehaviour
{
    [Tooltip("If true, the script will run automatically when the game starts.")]
    public bool runOnStart = true;

    void Start()
    {
        if (runOnStart)
        {
            DisableConcaveColliders();
        }
    }

    [ContextMenu("Disable Concave Colliders")]
    public void DisableConcaveColliders()
    {
        Debug.Log("Searching for Articulation Bodies and disabling concave Mesh Colliders...");

        ArticulationBody[] articulationBodies = FindObjectsOfType<ArticulationBody>();

        if (articulationBodies.Length == 0)
        {
            Debug.Log("No Articulation Bodies found in the scene.");
            return;
        }

        int collidersDisabledCount = 0;

        foreach (ArticulationBody body in articulationBodies)
        {
            MeshCollider[] meshColliders = body.GetComponentsInChildren<MeshCollider>();

            foreach (MeshCollider meshCollider in meshColliders)
            {
                if (!meshCollider.convex)
                {
                    meshCollider.enabled = false;
                    collidersDisabledCount++;
                    Debug.LogWarning($"Disabled concave Mesh Collider on GameObject: {meshCollider.gameObject.name}", meshCollider.gameObject);
                }
            }
        }

        if (collidersDisabledCount > 0)
        {
            Debug.Log($"{collidersDisabledCount} concave Mesh Collider(s) have been disabled.");
        }
        else
        {
            Debug.Log("No concave Mesh Colliders were found on any Articulation Bodies.");
        }
    }
}