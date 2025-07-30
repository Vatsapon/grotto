using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static SceneTransitionController;

public class WaypointUI : MonoBehaviour
{
    [Tooltip("Name of the scene")]
    [SerializeField] private string sceneName = "";
    [Tooltip("Name of the waypoint in certain scene")]
    [SerializeField] private string waypointName = "";

    [Space(10f)]

    [Tooltip("Type of scene to load to (affect with UI transition)")]
    [SerializeField] private SceneType sceneType = SceneType.Farm;

    [Header("References")]
    [Tooltip("Animator for waypoint icon on the right")]
    [SerializeField] private Animator waypointIconAnimator;

    [HideInInspector] public int sceneIndex = 0;

    private Button button;
    private GameManager gameManager;
    private WaypointManager waypointManager;

    private void Start()
    {
        button = GetComponent<Button>();
        waypointManager = FindObjectOfType<WaypointManager>();

        gameManager = GameManager.instance;
    }

    private void Update()
    {
        GameData gameData = gameManager.GetGameData();

        // Check if that there's scene with that name for waypoint or not.
        if (gameData.wayPoints.ContainsKey(sceneName))
        {
            WaypointPosition waypoints = gameData.wayPoints[sceneName];
            
            // Check if that waypoint has been saved to data already or not.
            if (waypoints.positions.ContainsKey(waypointName))
            {
                button.interactable = true;
            }
            else
            {
                button.interactable = false;
            }
        }
        else
        {
            button.interactable = false;
        }

        transform.Find("Label").GetComponent<TextMeshProUGUI>().text = waypointName;
        waypointIconAnimator.SetBool("Hovered", waypointManager.GetSelected() && waypointManager.GetSelected().Equals(this));
    }

    // Function to select this waypoint UI.
    public void Select()
    {
        button.Select();
        waypointManager.Select(this);
    }

    // Function to fetch scene name.
    public string GetSceneName()
    {
        return sceneName;
    }

    // Function to set scene name.
    public void SetSceneName(string name)
    {
        sceneName = name;
    }

    // Function to fetch waypoint name.
    public string GetWaypointName()
    {
        return waypointName;
    }

    // Function to set waypoint name.
    public void SetWaypointName(string name)
    {
        waypointName = name;
    }

    // Function to fetch scene type.
    public SceneType GetSceneType()
    {
        return sceneType;
    }

    // Function to set scene type.
    public void SetSceneType(SceneType type)
    {
        sceneType = type;
    }

    // Function to icon animator of this waypoint.
    public void SetAnimator(Animator animator)
    {
        waypointIconAnimator = animator;
    }
}

#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(WaypointUI))]
public class WaypointUIEditor : Editor
{
    private WaypointUI waypointUI;

    private void OnEnable()
    {
        waypointUI = target as WaypointUI;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        string[] scenes = new string[SceneManager.sceneCountInBuildSettings];

        for (int i = 0; i < scenes.Length; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = Path.GetFileNameWithoutExtension(path);
            scenes[i] = name;
        }

        waypointUI.sceneIndex = EditorGUILayout.Popup("Scene", waypointUI.sceneIndex, scenes);
        waypointUI.SetSceneName(scenes[waypointUI.sceneIndex]);

        DrawPropertiesExcluding(serializedObject, "m_Script", "sceneName");
        serializedObject.ApplyModifiedProperties();
    }
}
#endif