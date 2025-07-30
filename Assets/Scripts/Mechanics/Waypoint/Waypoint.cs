using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Waypoint : MonoBehaviour, IGameData
{
    [Tooltip("Name of this waypoint")]
    [SerializeField] private string waypointName = "";

    [Tooltip("Determine to unlock this waypoint automatically or not")]
    [SerializeField] private bool autoUnlock = false;

    [ReadOnly]
    [Tooltip("Determine if this waypoint had unlocked or not.")]
    [SerializeField] private bool isUnlock = false;

    [Tooltip("Determine to show spawnpoint as Gizmo on Sceneview or not")]
    [SerializeField] private bool showSpawn = true;

    [ShowIf("showSpawn", true, DisableType.Hide)]
    [Tooltip("Offset position from waypoint's position for player's spawnpoint")]
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;

    [Space(10f)]
    [Tooltip("Event that will execute when player unlock the waypoint for the first time")]
    [SerializeField] private UnityEvent OnUnlock;

    [Header("Additional settings")]
    [Tooltip("Mesh of this waypoint")]
    [SerializeField] private MeshRenderer runeMesh;
    [SerializeField] private Material activatedMaterial;

    private Vector3 position;
    private Interactive interactive;

    private void Start()
    {
        if (autoUnlock)
        {
            Activate();
        }

        position = transform.position;
        interactive = GetComponent<Interactive>();

        string tempTeleportData = PlayerPrefs.GetString("Waypoint_Teleport", "");
        
        // If saved teleport data doesn't have name or :, cancelled.
        if (tempTeleportData.Equals("") || !tempTeleportData.Contains(":"))
        {
            PlayerPrefs.DeleteKey("Waypoint_Teleport");
            return;
        }

        string sceneName = tempTeleportData.Split(":")[0];
        string waypointName = tempTeleportData.Split(":")[1];

        // If the scene and waypoint name matches, teleport player to this waypoint.
        if (SceneManager.GetActiveScene().name.Equals(sceneName) && this.waypointName.Equals(waypointName))
        {
            Teleport();
            PlayerPrefs.DeleteKey("Waypoint_Teleport");
        }
    }

    // Function to activate this waypoint.
    public void Activate()
    {
        // If this is the first time unlock this waypoint, invoke event.
        if (!isUnlock)
        {
            AudioManager.instance.PlaySound("RuneActivate");

            isUnlock = true;
            OnUnlock.Invoke();

            // Save data after unlock the waypoint.
            GameData gameData = GameManager.instance.GetGameData();
            SaveData(ref gameData);
            GameManager.instance.SetGameData(gameData);
        }

        runeMesh.material = activatedMaterial;
        isUnlock = true;

        // Just in case, the object hasn't loaded property yet when loading data.
        if (!interactive)
        {
            TryGetComponent(out interactive);
        }

        interactive.enabled = false;
    }

    // Function to teleport player to this waypoint.
    public void Teleport()
    {
        FindObjectOfType<Player>().transform.position = transform.position + spawnOffset;
    }

    // Function to load data.
    public void LoadData(GameData gameData)
    {
        string sceneName = SceneManager.GetActiveScene().name;

        foreach (KeyValuePair<string, WaypointPosition> wayPoint in gameData.wayPoints)
        {
            // Find waypoint within same scene.
            if (wayPoint.Key.Equals(sceneName))
            {
                foreach (KeyValuePair<string, Vector3> positions in wayPoint.Value.positions)
                {
                    // If the name and position is the same, set unlock to true.
                    if (waypointName.Equals(positions.Key) && transform.position.Equals(positions.Value))
                    {
                        isUnlock = true;
                        Activate();
                        break;
                    }
                }

                break;
            }
        }
    }

    // Function to save data.
    public void SaveData(ref GameData gameData)
    {
        // If this waypoint isn't unlock yet, don't save data.
        if (!isUnlock)
        {
            return;
        }

        string sceneName = SceneManager.GetActiveScene().name;

        // If there's no save data for this scene yet, create new one.
        if (!gameData.wayPoints.ContainsKey(sceneName))
        {
            gameData.wayPoints.Add(sceneName, new WaypointPosition(new SerializableDictionary<string, Vector3>()));
        }

        // If in scene data, there's no this waypoint position yet, add into list.
        if (!gameData.wayPoints[sceneName].positions.ContainsKey(waypointName))
        {
            gameData.wayPoints[sceneName].positions.Add(waypointName, position);
        }
    }

    // Function to determine if this waypoint is already unlock or not.
    public bool IsUnlock()
    {
        return isUnlock;
    }

    // Function to fetch waypoint name.
    public string GetName()
    {
        return waypointName;
    }

    private void OnDrawGizmos()
    {
        if (showSpawn)
        {
            Vector3 position = transform.position + spawnOffset;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(position, Vector3.one);
            Gizmos.DrawLine(transform.position, position);
        }
    }
}

[System.Serializable]
public struct WaypointPosition
{
    public SerializableDictionary<string, Vector3> positions;

    public WaypointPosition(SerializableDictionary<string, Vector3> positions)
    {
        this.positions = positions;
    }
}