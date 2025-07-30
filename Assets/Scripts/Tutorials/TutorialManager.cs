using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TutorialManager : MonoBehaviour, IGameData
{
    public static TutorialManager instance;

    [Tooltip("Determine to start tutorial on load or not")]
    [SerializeField] private bool _startOnLoad = true;
    [Tooltip("Offset of waypoint from target position")]
    [SerializeField] private float _waypointOffset = 50f;

    [Space(10f)]
    [Tooltip("Prefab of waypoint")]
    [SerializeField] private GameObject _waypointPrefab;
    [Tooltip("UI of waypoint icon")]
    [SerializeField] private GameObject _waypointUIPrefab;

    [Tooltip("List of all tutorials")]
    [SerializeField] private Tutorial[] tutorials;

    private GameObject waypointObject;
    private GameObject waypointUIObject;
    private Transform waypointTarget;

    private bool isTutorialize;
    private string currentTutorial;

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Update()
    {
        if (waypointTarget)
        {
            if (waypointObject)
            {
                waypointObject.transform.position = waypointTarget.transform.position;
            }

            if (waypointUIObject)
            {
                Vector2 position = Camera.main.WorldToScreenPoint(waypointTarget.position);
                waypointUIObject.transform.position = position + (Vector2.up * _waypointOffset);
            }
        }
    }

    // Function to complete current tutorial.
    public void CompleteTutorial()
    {
        if (currentTutorial != null)
        {
            Tutorial tutorial = GetTutorial(currentTutorial);

            if (tutorial != null)
            {
                tutorial.OnEnd.Invoke();
            }
        }
    }

    // Function to start tutorial.
    public void StartTutorial(string tutorialName)
    {
        Tutorial tutorial = GetTutorial(tutorialName);
        currentTutorial = tutorialName;

        if (tutorial != null)
        {
            tutorial.OnStart.Invoke();
        }
    }

    // Function to execute when tutorial is finish.
    public void Finish()
    {
        isTutorialize = true;
        GameManager.instance.GetGameData().isTutorialize = true;
    }

    // Function to freeze the game or not.
    public void SetFreeze(bool value) => Time.timeScale = value ? 0f : 1f;

    // Function to display waypoint at certain position.
    public void DisplayWaypoint(Transform transformPosition)
    {
        waypointTarget = transformPosition;

        if (!waypointObject)
        {
            waypointObject = Instantiate(_waypointPrefab, waypointTarget.position, Quaternion.identity);
        }

        if (!waypointUIObject)
        {
            Vector2 position = Camera.main.WorldToScreenPoint(waypointTarget.position);
            waypointUIObject = Instantiate(_waypointUIPrefab, HUDManager.instance.transform);
            waypointUIObject.transform.position = position + (Vector2.up * _waypointOffset);
            waypointUIObject.transform.SetAsFirstSibling();
        }
    }

    // Function to hide waypoint.
    public void HideWaypoint()
    {
        if (waypointObject)
        {
            Destroy(waypointObject);
        }

        if (waypointUIObject)
        {
            Destroy(waypointUIObject);
        }

        waypointTarget = null;
    }

    // Function to find tutorial by name.
    private Tutorial GetTutorial(string name)
    {
        for (int i = 0; i < tutorials.Length; i++)
        {
            if (tutorials[i].Name.Equals(name))
            {
                return tutorials[i];
            }
        }

        return null;
    }

    public void LoadData(GameData gameData)
    {
        isTutorialize = gameData.isTutorialize;

        if (isTutorialize)
        {
            return;
        }

        if (gameData.tutorialName == null && _startOnLoad)
        {
            if (tutorials.Length > 0)
            {
                currentTutorial = tutorials[0].Name;
                tutorials[0].OnStart.Invoke();
            }
            
            return;
        }

        Tutorial tutorial = GetTutorial(gameData.tutorialName);

        if (tutorial != null && _startOnLoad)
        {
            currentTutorial = tutorial.Name;
            tutorial.OnStart.Invoke();
        }
    }

    public void SaveData(ref GameData gameData)
    {
        if (currentTutorial != null)
        {
            gameData.tutorialName = currentTutorial;
        }
    }
}

[Serializable]
public class Tutorial
{
    [Tooltip("Name of this tutorial")]
    [SerializeField] private string _name;
    [Tooltip("Event to execute when tutorial started")]
    [SerializeField] private UnityEvent _onStart;
    [Tooltip("Event to eecute when tutorial ended")]
    [SerializeField] private UnityEvent _onEnd;

    public string Name => _name;
    public UnityEvent OnStart => _onStart;
    public UnityEvent OnEnd => _onEnd;
}
