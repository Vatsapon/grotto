using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using static SceneTransitionController;

public class DungeonDoor : MonoBehaviour
{
    public enum DungeonDoorType
    {
        Dungeon_Door, Level_Door
    }

    [Header("Dungeon Door Settings")]

    [Tooltip("Type of this dungeon door")]
    [SerializeField] private DungeonDoorType _doorType = DungeonDoorType.Level_Door;
    [Tooltip("Determine to use interact or not")]
    [SerializeField] private bool _interactMode = true;

    [ShowIf("_doorType", DungeonDoorType.Level_Door, DisableType.Hide)]
    [Tooltip("Determine to lock the door on start")]
    [SerializeField] private bool _lockOnStart = false;
    [ShowIf("_doorType", DungeonDoorType.Level_Door, DisableType.Hide)]
    [Tooltip("Determine if this door is normal door (e.g. warp to different scene without level requirement)")]
    [SerializeField] private bool _isNormalDoor = false;
    [ShowIf("_doorType", DungeonDoorType.Level_Door, DisableType.Hide)]
    [Tooltip("The name of the scene to load when the interact or enter")]
    public string _sceneToLoad;
    [ShowIf("_doorType", DungeonDoorType.Level_Door, DisableType.Hide)]
    [Tooltip("Select destination's scene type")]
    [SerializeField] private SceneType _targetSceneType;

    [ShowIf("_doorType", DungeonDoorType.Dungeon_Door, DisableType.Hide)]
    [Tooltip("Pool for this door")]
    [SerializeField] private DungeonPool _doorPool;
    [ShowIf("_doorType", DungeonDoorType.Dungeon_Door, DisableType.Hide)]
    [Tooltip("Requirement to unlock this door")]
    [SerializeField] private DungeonDoor _requiredDoor;

    public DungeonPool DoorPool => _doorPool;
    public DungeonDoorType DoorType => _doorType;
    public bool IsLock => isLock;
    
    private Animator doorAnimator;
    private bool isLock = true; // Determine if this door is lock or not.
    [HideInInspector] public int sceneToLoadIndex = 0; // Scene to load, but as index instead of name.

    private DungeonPoolManager dungeonPoolM;

    private void Start()
    {
        doorAnimator = GetComponent<Animator>();
        dungeonPoolM = DungeonPoolManager.instance;

        // If there's a pool that game is progressing and this door has no pool assign, assign pool to this door.
        if (dungeonPoolM.CurrentDungeonPool && !_doorPool)
        {
            _doorPool = dungeonPoolM.CurrentDungeonPool;
        }

        DoorUpdater();

        if (_doorType == DungeonDoorType.Level_Door)
        {
            SetLock(_lockOnStart);
        }

        doorAnimator.Play(isLock ? "Close" : "Open", -1, 0.99f);
        
        if (!_interactMode)
        {
            try
            {
                GetComponent<Interactive>().enabled = false;
                GetComponent<PopupManager>().enabled = false;
            }
            catch (NullReferenceException) { }
        }
    }

    private void Update()
    {
        DoorUpdater();
    }
    
    // Function to update door.
    private void DoorUpdater()
    {
        // If required door is complete, then unlock this door.
        if (_doorType == DungeonDoorType.Dungeon_Door)
        {
            // If this dungeon pool is complete, set to complete.
            if (dungeonPoolM.IsPoolComplete(_doorPool))
            {
                _doorPool = Instantiate(_doorPool);
                _doorPool.Complete();
                SetLock(true);
            }
            else
            {
                if (_requiredDoor)
                {
                    SetLock(!_requiredDoor.DoorPool.IsComplete);
                }
                else
                {
                    SetLock(false);
                }
            }
        }
    }

    // Function to open prompt when player interact with this door.
    public void OpenPrompt()
    {
        HUDManager hudM = HUDManager.instance;
        hudM.DungeonDoorPanel.SetDoor(this);
        hudM.DungeonDoorPanel.Refresh();

        PauseManager.instance.PauseGame(hudM.DungeonDoorPanel.gameObject);
    }

    // Function to set door lock value.
    public void SetLock(bool value)
    {
        isLock = value;
        doorAnimator.SetBool("isLock", value);

        Interactive interactive = GetComponent<Interactive>();

        if (interactive)
        {
            if (value)
            {
                interactive.DeActivateConditional();
            }
            else
            {
                interactive.ActivateConditional();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !_interactMode)
        {
            Interact();
        }
    }

    // Function to execute when player interact with the door.
    public void Interact()
    {
        PlayerHolder holder = FindObjectOfType<Player>().Holder;

        // If player is holding plant, can't leave the level.
        if (holder.IsHolding())
        {
            return;
        }

        // If this is level door, show level complete/clear prompt.
        if (_doorType == DungeonDoorType.Level_Door && !_isNormalDoor)
        {
            LevelManager.instance.Complete();
        }
        else
        {
            LoadScene();
        }
    }

    // Function to load next scene.
    public void LoadScene()
    {
        SceneTransitionController transition = FindObjectOfType<SceneTransitionController>();

        // If it using Pool system, choose fade type based on current dungeon pool.
        if (dungeonPoolM)
        {
            // If this is dungeon door, set transition for door pool level.
            if (_doorType == DungeonDoorType.Dungeon_Door)
            {
                transition.FadeIn(_doorPool.CurrentLevel.SceneType);
            }
            else
            {
                // If there's no pool, set to default from this door.
                if (!dungeonPoolM.CurrentDungeonPool)
                {
                    transition.FadeIn(_targetSceneType);
                }
                else
                {
                    transition.FadeIn(_doorPool.CurrentLevel.SceneType);
                }
            }
        }
        else
        {
            transition.FadeIn(_targetSceneType);
        }

        StartCoroutine(DelayLoadScene());
    }

    private IEnumerator DelayLoadScene()
    {
        yield return new WaitForSeconds(0.5f);

        // If it using Pool system, choose scene based on current dungeon pool.
        if (dungeonPoolM)
        {
            // If this is dungeon door, set current dungeon to new one.
            if (_doorType == DungeonDoorType.Dungeon_Door)
            {
                dungeonPoolM.SetPool(_doorPool);
                GameManager.instance.SaveGameData();
                SceneManager.LoadScene(dungeonPoolM.CurrentDungeonPool.CurrentLevel.SceneName, LoadSceneMode.Single);
            }
            else
            {
                // If there's no pool. load default scene.
                if (!dungeonPoolM.CurrentDungeonPool)
                {
                    GameManager.instance.SaveGameData();
                    SceneManager.LoadScene(_sceneToLoad, LoadSceneMode.Single);
                }
                else
                {
                    dungeonPoolM.LoadNextLevel();
                }
            }
        }
        else
        {
            GameManager.instance.SaveGameData();
            SceneManager.LoadScene(_sceneToLoad, LoadSceneMode.Single);
        }
    }
}

#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(DungeonDoor))]
public class DungeonDoorEditor : Editor
{
    private DungeonDoor dungeonDoor;

    private void OnEnable()
    {
        dungeonDoor = target as DungeonDoor;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (dungeonDoor.DoorType == DungeonDoor.DungeonDoorType.Level_Door)
        {
            DrawPropertiesExcluding(serializedObject, "m_Script", "_sceneToLoad");

            string[] scenes = new string[SceneManager.sceneCountInBuildSettings];

            for (int i = 0; i < scenes.Length; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string name = Path.GetFileNameWithoutExtension(path);
                scenes[i] = name;
            }

            dungeonDoor.sceneToLoadIndex = EditorGUILayout.Popup("Target Scene", dungeonDoor.sceneToLoadIndex, scenes);
            dungeonDoor._sceneToLoad = scenes[dungeonDoor.sceneToLoadIndex];
        }
        else
        {
            base.OnInspectorGUI();
        }

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(dungeonDoor);
            EditorSceneManager.MarkSceneDirty(dungeonDoor.gameObject.scene);
        }
    }
}
#endif