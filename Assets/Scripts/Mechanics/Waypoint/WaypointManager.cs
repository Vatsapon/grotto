using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static SceneTransitionController;

public class WaypointManager : MonoBehaviour
{
    public static WaypointManager instance;

    [Header("References")]
    [Tooltip("Parent of all waypoints")]
    [SerializeField] private Transform waypointParent;
    [Tooltip("Parent of waypoint label to spawn button")]
    [SerializeField] private Transform waypointButtonParent;

    [Space(10f)]

    [Tooltip("Button to teleport")]
    [SerializeField] private Button teleportButton;

    [Header("Prefabs")]
    [Tooltip("Button for waypoint selection")]
    [SerializeField] private GameObject waypointButtonPrefab;
    
    public bool IsTeleporting => isTeleporting;

    private WaypointUI selectedWaypointUI; // Selected waypoint UI.
    private bool isTeleporting = false; // Determine if player is teleporting or not.

    private SceneTransitionController sceneTransition;

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        Setup();

        sceneTransition = FindObjectOfType<SceneTransitionController>();
    }

    private void Update()
    {
        // Check if player is currently has select any waypoint UI.
        if (selectedWaypointUI)
        {
            EventSystem e = EventSystem.current;
            GameObject selectedUI = e.currentSelectedGameObject;

            teleportButton.gameObject.SetActive(true);
            
            // Check if current selected UI isn't empty and it's current waypoint UI or teleport button, then enable, otherwise, disable.
            if (selectedUI && (selectedUI.GetComponent<WaypointUI>() || selectedUI.Equals(teleportButton.gameObject)))
            {
                teleportButton.gameObject.SetActive(true);
            }
            else
            {
                selectedWaypointUI = null;
                teleportButton.gameObject.SetActive(false);
            }
        }
        else
        {
            try
            {
                // Disable since there's no waypoint has been selected yet.
                teleportButton.gameObject.SetActive(false);
            }
            catch (NullReferenceException) { }
        }
    }

    // Function to setup the waypoint UI.
    private void Setup()
    {
        if (waypointButtonParent)
        {
            for (int i = 0; i < waypointParent.childCount; i++)
            {
                GameObject waypointButton = Instantiate(waypointButtonPrefab, waypointButtonParent);
                WaypointUI waypointUI = waypointButton.GetComponent<WaypointUI>();

                Transform waypointIcon = waypointParent.GetChild(i);
                WaypointUI waypointIconUI = waypointIcon.GetComponent<WaypointUI>();

                waypointUI.SetSceneName(waypointIconUI.GetSceneName());
                waypointUI.SetWaypointName(waypointIconUI.GetWaypointName());
                waypointUI.SetSceneType(waypointIconUI.GetSceneType());
                waypointUI.SetAnimator(waypointIconUI.GetComponent<Animator>());

                Destroy(waypointIconUI);
            }
        }
    }

    // Function to select this waypoint UI.
    public void Select(WaypointUI waypointUI)
    {
        selectedWaypointUI = waypointUI;
    }

    // Function to get selected waypoint UI.
    public WaypointUI GetSelected()
    {
        return selectedWaypointUI;
    }

    // Function to fetch all waypoints.
    public List<WaypointUI> GetAllWaypoint()
    {
        List<WaypointUI> waypoints = new List<WaypointUI>();

        for (int i = 0; i < waypointButtonParent.childCount; i++)
        {
            waypoints.Add(waypointButtonParent.GetChild(i).GetComponent<WaypointUI>());
        }

        return waypoints;
    }

    // Function to execute to save waypoint teleport to teleport when reach the certain scene.
    public void Teleport()
    {
        // If player isn't teleporting, return.
        if (isTeleporting)
        {
            return;
        }

        // If player hasn't select waypoint yet, cancelled.
        if (!selectedWaypointUI)
        {
            Debug.LogError("You haven't select any waypoint yet!");
            return;
        }

        string sceneName = selectedWaypointUI.GetSceneName();
        string waypointName = selectedWaypointUI.GetWaypointName();

        Teleport(sceneName, waypointName);
    }

    // Function to execute to save waypoint teleport to teleport when reach the certain scene.
    public void Teleport(string sceneName, string waypointName)
    {
        // If player isn't teleporting, return.
        if (isTeleporting)
        {
            return;
        }

        isTeleporting = true;

        GameData gameData = GameManager.instance.GetGameData();

        // If there's no game scene with that name in game data, cancelled.
        if (!gameData.wayPoints.ContainsKey(sceneName))
        {
            Debug.LogError("There's no scene name '" + sceneName + "' in waypoint data!");
            return;
        }

        WaypointPosition waypoint = gameData.wayPoints[sceneName];

        // If there's no game scene with that name in game data, cancelled.
        if (!waypoint.positions.ContainsKey(waypointName))
        {
            Debug.LogError("There's no waypoint name '" + waypointName + "' in scene '" + sceneName + "' in waypoint data!");
            return;
        }

        float delayTeleportTime = 2f;

        StartCoroutine(DelayFadeIn(delayTeleportTime));

        // Find the particle system in the player and plays it - Yeen
        Player.instance.GetComponentInChildren<ParticleSystem>().Play();

        // If it's the same scene, teleport instantly, or else, load scene and save teleport data.
        // Changed it to plays a particle - yeen
        if (SceneManager.GetActiveScene().name.Equals(sceneName))
        {
            //StartCoroutine(DelayTeleport(waypointName, 0.5f)); 
            StartCoroutine(DelayTeleport(waypointName, delayTeleportTime + 0.5f));
        }
        else
        {
            PlayerPrefs.SetString("Waypoint_Teleport", sceneName + ":" + waypointName);
            //StartCoroutine(DelayLoadScene(sceneName, 0.5f));
            StartCoroutine(DelayLoadScene(sceneName, delayTeleportTime + 0.5f));
        }
    }

    // Function to execute to teleport to the certain scene.
    public void Teleport(string sceneName)
    {
        // If player isn't teleporting, return.
        if (isTeleporting)
        {
            return;
        }

        isTeleporting = true;
        float delayTeleportTime = 2f;

        StartCoroutine(DelayFadeIn(delayTeleportTime));

        // Find the particle system in the player and plays it - Yeen
        Player.instance.GetComponentInChildren<ParticleSystem>().Play();

        // If it's the same scene, teleport instantly, or else, load scene and save teleport data.
        // Changed it to plays a particle - yeen
        if (SceneManager.GetActiveScene().name.Equals(sceneName))
        {
            StartCoroutine(DelayTeleport(delayTeleportTime + 0.5f));
        }
        else
        {
            StartCoroutine(DelayLoadScene(sceneName, delayTeleportTime + 0.5f));
        }
    }

    // Function to load scene by name with delay.
    private IEnumerator DelayLoadScene(string sceneName, float delayTimer)
    {
        yield return new WaitForSeconds(delayTimer);
        DungeonPoolManager.instance.SetPool(null);

        GameManager.instance.SaveGameData();
        SceneManager.LoadScene(sceneName);
    }

    // Function to teleport to waypoint by name with delay.
    private IEnumerator DelayTeleport(string waypointName, float delayTimer)
    {
        yield return new WaitForSeconds(delayTimer);

        Waypoint[] waypoints = FindObjectsOfType<Waypoint>();

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i].GetName().Equals(waypointName))
            {
                waypoints[i].Teleport();
                break;
            }
        }

        sceneTransition.FadeOut();
    }

    // Function to teleport to default position with delay.
    private IEnumerator DelayTeleport(float delayTimer)
    {
        yield return new WaitForSeconds(delayTimer);
        Player.instance.ResetPlayerPos();

        sceneTransition.FadeOut();
    }

    private void OnApplicationQuit() => PlayerPrefs.DeleteKey("Waypoint_Teleport");

    // Function to Fade in after delay -Yeen
    private IEnumerator DelayFadeIn(float delayTimer)
    {
        yield return new WaitForSeconds(delayTimer);
        sceneTransition.FadeIn(selectedWaypointUI ? selectedWaypointUI.GetSceneType() : SceneType.Farm);
    }
}