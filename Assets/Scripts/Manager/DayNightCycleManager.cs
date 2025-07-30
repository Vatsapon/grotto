using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

[ExecuteAlways]
public class DayNightCycleManager : MonoBehaviour, IGameData
{
    [Header("General Settings")]
    
    [Tooltip("If true, will begin the cycle when the scene opens")]
    public bool StartCycleOnBegin = true;
    [Tooltip("Full Day and night cycle length in minutes. Half this time will result in day, and the other half night.")]
    public float dayLength = 5f;

    [Header("Lighting Settings")]

    [Tooltip("Color for world ambient")]
    [SerializeField] private Gradient ambientColor;
    [Tooltip("Color for the sun light")]
    [SerializeField] private Gradient lightColor;
    [Tooltip("Color for ambient fog")]
    [SerializeField] private Gradient fogColor;
    [Tooltip("Bloom Intensity in daynight cycle")]
    [SerializeField] private AnimationCurve bloomIntensity;

    [Space(5f)]

    [Tooltip("Whether user can edit day night time during edit-mode")]
    [SerializeField] private bool editMode = false;
    [ShowIf("editMode", true, DisableType.ReadOnly)]
    [Tooltip("Update your sun value while in edit-mode")]
    [Range(0f, 1f)]
    [SerializeField] private float dayNightValue;

    [Header("HUD Settings")]

    [Tooltip("Will just be set to the day number")]
    [SerializeField] private TMP_Text dayText;
    [Tooltip("Animator to show day/night cycle transition")]
    [SerializeField] private Animator animator;

    [Header("Lighting References")]

    [Tooltip("Directional light in the game scene (Leave empty for auto search in run-time)")]
    [SerializeField] private Light directionalLight;

    [Tooltip("Post-Processing for each time in day night cycle (Leave empty for auto search in run-time)")]
    [SerializeField] private Volume postProcessing;

    private int dayNumber = 0;
    private bool isCycling = false;
    private bool isPaused = false; // Determine if cycle is pause or not.

    private float cycleTime; // Cycle time (after convert from minute to second)
    private float currentCycleTime;

    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.instance;

        // Day and night are equal length. Multiply by 60 to get total number of seconds.
        cycleTime = (dayLength) * 60f;

        // Check and search for directional light.
        if (!directionalLight)
        {
            if (RenderSettings.sun)
            {
                directionalLight = RenderSettings.sun;
            }
            else
            {
                Light[] sceneLights = FindObjectsOfType<Light>();

                for (int i = 0; i < sceneLights.Length; i++)
                {
                    if (sceneLights[i].type == LightType.Directional)
                    {
                        directionalLight = sceneLights[i];
                        RenderSettings.sun = directionalLight;

                        break;
                    }
                }
            }
        }
        else
        {
            RenderSettings.sun = directionalLight;
        }

        RenderSettings.ambientMode = AmbientMode.Flat;

        // Check for bloom post-processing
        if (!postProcessing)
        {
            Volume[] volumes = FindObjectsOfType<Volume>();

            for (int i = 0; i < volumes.Length; i++)
            {
                if (volumes[i].isGlobal && volumes[i].profile)
                {
                    postProcessing = volumes[i];
                    break;
                }
            }
        }

        // If the game isn't playing, return.
        if (!Application.isPlaying)
        {
            return;
        }

        bloomIntensity.keys[0].time = 0f;
        bloomIntensity.keys[bloomIntensity.keys.Length - 1].time = 1f;

        GameData gameData = gameManager.GetGameData();
        LoadData(gameData);

        SceneManager.sceneUnloaded += OnSceneUnload;

        isCycling = StartCycleOnBegin;
        animator.Play(IsDay() ? "Day" : "Night", -1, 0.99f);
    }

    private void Update()
    {
        // If it's cycling and not pause, update cycle.
        if (isCycling && !isPaused)
        {
            CycleUpdate();
        }

        UpdateUI();
    }

    public bool IsDay() => currentCycleTime / cycleTime < 0.5f;
    public bool IsNight() => currentCycleTime / cycleTime >= 0.5f;

    // Function to pause the cycle.
    public void PauseCycle() => isPaused = true;

    // Functon to resume the cycle.
    public void ResumeCycle() => isPaused = false;

    // Function to forces to day time.
    public void ForceDay() => currentCycleTime = cycleTime * 0.25f;

    // Function to forces to night time.
    public void ForceNight() => currentCycleTime = cycleTime * 0.75f;

    // Function to forces a time skip. (Day -> Night / Night -> Day)
    public void SleepForceTimeSkip()
    {
        if (IsNight())
        {
            dayNumber++;
            ForceDay();
        }
        else
        {
            ForceNight();
        }
    }

    /*
     * 0% means day just started
     * 50% means it is night
     * 100% means the night completed and it is time for the next day.
     * Returns 0 if no cycling is occuring.
     */
    
    // Function to get percentage of day complete.
    private float GetPercentage()
    {
        if (isCycling && cycleTime > 0f)
        {
            return (currentCycleTime / cycleTime) * 100f;
        }

        return 0f;
    }

    // Function to udate cycle during Edit mode.
    private void CycleEditorUpdate()
    {
        float cycleTime = (dayLength) * 60f;
        currentCycleTime = dayNightValue * cycleTime;

        LightingUpdate();
    }

    // Function to update cycle during Run-time.
    private void CycleUpdate()
    {
        currentCycleTime += Time.deltaTime;
        LightingUpdate();

        // Add day number and reset timer when day finishes.
        if (currentCycleTime >= cycleTime)
        {
            dayNumber++;
            currentCycleTime = 0f;
        }
    }

    // Function to update UI.
    private void UpdateUI()
    {
        if (dayText)
        {
            dayText.text = "Day " + dayNumber;
        }

        if (Application.isPlaying)
        {
            animator.SetBool("IsDay", IsDay());
        }
    }

    // Function to update light cycle system.
    private void LightingUpdate()
    {
        if (!directionalLight)
        {
            return;
        }

        float percent = Application.isPlaying ? GetPercentage() / 100f : dayNightValue;

        RenderSettings.ambientLight = ambientColor.Evaluate(percent);
        RenderSettings.fogColor = fogColor.Evaluate(percent);

        Vector3 sunRotation = new Vector3(percent * 360f, 170f, 0f);

        directionalLight.color = lightColor.Evaluate(percent);

        if (directionalLight.transform.parent)
        {
            directionalLight.transform.localRotation = Quaternion.Euler(sunRotation);
        }
        else
        {
            directionalLight.transform.rotation = Quaternion.Euler(sunRotation);
        }

        if (postProcessing && postProcessing.profile)
        {
            Bloom bloom;
            postProcessing.profile.TryGet(out bloom);

            if (bloom)
            {
                bloom.intensity.value = bloomIntensity.Evaluate(percent);
            }
        }
    }

    // Function to load cycle data.
    public void LoadData(GameData gameData)
    {
        CycleVariableData data = gameData.cycleData;

        if (data != null)
        {
            dayNumber = data.currentDay;
            dayLength = data.dayLength;
            isCycling = data.isCycle;
            currentCycleTime = data.currentPercentage * (dayLength * 60);
        }
        else
        {
            dayNumber = 1;
            isCycling = false;
            isPaused = false;
        }
    }

    // Function when current scene is change/unload
    private void OnSceneUnload(Scene current)
    {
        GameManager gameM = GameManager.instance;

        GameData gameData = gameM.GetGameData();
        SaveData(ref gameData);
        gameM.SetGameData(gameData);
    }

    // Function to save cycle data.
    public void SaveData(ref GameData gameData)
    {
        CycleVariableData data = new CycleVariableData();

        data.currentDay = dayNumber;
        data.dayLength = dayLength;
        data.isCycle = isCycling;
        data.currentPercentage = currentCycleTime / cycleTime;

        gameData.cycleData = data;
    }

    private void OnGUI()
    {
        // If application isn't playing, use cycle editor.
        if (!Application.isPlaying && editMode)
        {
            CycleEditorUpdate();
            UpdateUI();
        }
    }
}

[System.Serializable]
public class CycleVariableData
{
    public int currentDay;
    public float dayLength;

    public bool isCycle;
    public float currentPercentage;
}