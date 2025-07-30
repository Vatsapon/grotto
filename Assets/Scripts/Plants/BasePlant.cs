using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UnityEngine.Rendering.DebugUI.Table;

public class BasePlant : MonoBehaviour
{
    [Tooltip("Item for this plant")]
    public Item plantItem;

    [Header("Base Plant Settings")]

    [Tooltip("Name of the plant")]
    public string PlantName;

    [Tooltip("Time taken to grow (in minutes)")]
    public float GrowTime = 0.2f;

    [Tooltip("If true, seed will start growing when game starts")]
    public bool GrowOnSpawn;

    [Tooltip("If true, seed will also grow at night.")]
    public bool GrowAtNight;

    [Space(5)]

    public GameObject seedObject;
    public GameObject middleGrowthPhase;
    public GameObject grownPlant;

    [Header("UI Feedback")]
    [Tooltip("Offset of popup")]
    [SerializeField] private float popupOffset = 50f;
    [Tooltip("Popup to show plant growth progress")]
    [SerializeField] private GameObject plantGrowingPopupPrefab;

    [SerializeField] private GameObject plantGrowingAnimation;

    [Space(5f)]

    public bool hideUIText;
    public TMP_Text currentState;
    public Slider plantGrowthPercentage;

    public Animator plantAnim;

    public Image plantDoneImage;

    public bool UseAnimation;

    //to know the state if needed
    private GameObject currentStateObject;
    private GameObject currentPopup; // Current plant growing popup.

    //full growth time and time left (in seconds)
    private float GrowthTime;
    private float GrowthTimeLeft;

    private bool isGrowing;
    private bool doneGrowing;

    private bool isPaused;
    private float savedSpeedMultiplier;

    private void Awake()
    {
        // Start the plant as Seed object.
        seedObject.SetActive(true);
        middleGrowthPhase.SetActive(false);
        grownPlant.SetActive(false);

        currentStateObject = seedObject;

        isGrowing = false;
        doneGrowing = false;
        GrowthTime = GrowTime * 60f; // Convert Grow time from minute to second.

        UpdateUIState("Idle");

        // Hide growth percentage.
        if (plantGrowthPercentage)
        {
            plantGrowthPercentage.gameObject.SetActive(false);
        }

        plantDoneImage.gameObject.SetActive(false);

        // Grow plant on spawn, if specified.
        if (GrowOnSpawn)
        {
            PlantSeed();
        }
    }

    private void Update()
    {
        if (isPaused)
        {
            return;
        } 

        if (doneGrowing)
        {
            UpdateUIState("Pickable");

            //Set interactive to true for the pickup icon to display
            Interactive inter = GetComponent<Interactive>();

            if (inter && inter.IsConditionalIcon)
            {
                inter.ActivateConditional();
            }

            if (inter)
            {
                plantDoneImage.gameObject.SetActive(!inter.IsShowingInteractKey());
            }

            return;
        }

        if (isGrowing)
        {
            DayNightCycleManager manager = GameObject.FindGameObjectWithTag("DayNightCycle").GetComponent<DayNightCycleManager>();

            // If night, only grow if the plant can grow at night
            if (!manager.IsDay())
            {
                if (GrowAtNight)
                {
                    UpdatePlant();
                }
                else
                {
                    plantAnim.SetFloat("Speed multiplier", 0f);
                }

                UpdateUIState("Not growing (night)");
            }
            else
            {
                if (plantAnim.GetFloat("Speed multiplier") == 0f)
                {
                    plantAnim.SetFloat("Speed multiplier", savedSpeedMultiplier);
                }

                UpdatePlant();
            }
        }
    }
    
    // Function to plant a seed.
    public void PlantSeed()
    {
        isGrowing = true;
        GrowthTimeLeft = GrowthTime;

        if (plantGrowingPopupPrefab)
        {
            // Edied some thing here, NEED TO BE TIDIED UP
            // -Yeen

            if (!UseAnimation)
            {
                Transform parentUI = GameObject.Find("UI Canvas").transform;
                currentPopup = Instantiate(plantGrowingPopupPrefab, parentUI);
                currentPopup.transform.SetAsFirstSibling();

                currentState = currentPopup.transform.Find("Label").GetComponent<TextMeshProUGUI>();
                plantGrowthPercentage = currentPopup.transform.Find("Slider").GetComponent<Slider>();
                plantDoneImage = currentPopup.transform.Find("Pickable").GetComponent<Image>();
            }
            else
            {
                Transform parentUI = GameObject.Find("UI Canvas").transform;
                currentPopup = Instantiate(plantGrowingAnimation, parentUI);
                currentPopup.transform.SetAsFirstSibling();

                plantAnim = currentPopup.transform.Find("GrowthBar").GetComponent<Animator>();
                plantDoneImage = currentPopup.transform.Find("Pickable").GetComponent<Image>();

                savedSpeedMultiplier = plantAnim.GetCurrentAnimatorStateInfo(0).length / GrowthTime;
                plantAnim.SetFloat("Speed multiplier", savedSpeedMultiplier);
                plantAnim.Play("Growth", 0, 1f - (GrowthTimeLeft / GrowthTime));
            }
        }
        /*
        //show percentage progress slider
        if (plantGrowthPercentage)
            plantGrowthPercentage.gameObject.SetActive(true);
        */
    }

    public void PauseGrowth()
    {
        isPaused = true;

        plantAnim.enabled = false;
        //to do: change color to orange / not moving
        //if (plantGrowthPercentage)
        //  plantGrowthPercentage.colors.
    }

    public void ResumeGrowth()
    {
        isPaused = false;
        plantAnim.enabled = true;

        //to do: change color to green / moving
        //if (plantGrowthPercentage)
        //  plantGrowthPercentage.colors.
    }

    public void HideAllUIAndShowDoneSymbol()
    {

        if (!UseAnimation)
        {
            if (plantGrowthPercentage)
            plantGrowthPercentage.gameObject.SetActive(false);
        }
        else
        {
            if (plantAnim)
                plantAnim.gameObject.SetActive(false);
        }

        if (currentState)
        currentState.gameObject.SetActive(false);

        plantDoneImage.gameObject.SetActive(true);
    }

    // Function to grow the plant
    private void UpdatePlant()
    {
        GrowthSpeedUpgrade upgrade = UpgradeManager.instance.GetUpgrade("Plant Growth") as GrowthSpeedUpgrade;
        float additionalTime = upgrade.GetLevel() == 0 ? 1f : 1f + (upgrade.GetSpeedIncrease() / 100f);

        GrowthTimeLeft -= Time.deltaTime * additionalTime;

        if (plantGrowthPercentage)
        {
            plantGrowthPercentage.value = GetPercentageOfPlantComplete() / 100f;
        }

        plantAnim.enabled = !FindObjectOfType<DayNightCycleManager>().IsNight();

        if (GrowthTimeLeft <= 0f)
        {
            doneGrowing = true;
            seedObject.SetActive(false);
            middleGrowthPhase.SetActive(false);
            grownPlant.SetActive(true);

            UpdateUIState("Complete");

            //hide and display the done icon
            HideAllUIAndShowDoneSymbol();

            currentStateObject = grownPlant;

            //for testing, make the seed automatically go into inventory
            //PickupSeed();
        }

        else if (GrowthTimeLeft <= GrowthTime / 2.0f)
        {
            seedObject.SetActive(false);
            middleGrowthPhase.SetActive(true);
            grownPlant.SetActive(false);

            UpdateUIState("Half Growing..");

            currentStateObject = middleGrowthPhase;
        }

        else
        {
            seedObject.SetActive(true);
            middleGrowthPhase.SetActive(false);
            grownPlant.SetActive(false);

            UpdateUIState("Growing..");

            currentStateObject = seedObject;
        }
    }
    public float GetPercentageOfPlantComplete()
    {
        if (!isGrowing)
        {
            return 0;
        }

        return (1.0f - (GrowthTimeLeft / GrowthTime)) * 100.0f;
    }

    public void PickupPlant()
    {
        if (doneGrowing)
        {
            //add to the inventory
            InventoryManager.instance.AddItem(plantItem, 3);
            UpdateUIState("Picked up");

            print(PlantName + " was picked up");

            //invisible till repositioned
            seedObject.SetActive(false);
            middleGrowthPhase.SetActive(false);
            grownPlant.SetActive(false);

            //hide all the UI
            HideAllUIAndShowDoneSymbol();
            plantDoneImage.gameObject.SetActive(false);

            //destroy the plant
            Destroy(gameObject);
        }
    }

    private void UpdateUIState(string text)
    {
        /*
        if (currentState)
        {
            if (!hideUIText)
            currentState.text = text;
        }
        */

        if (currentPopup)
        {
            // Update position.
            Vector2 position = Camera.main.WorldToScreenPoint(transform.position);
            position.y += popupOffset * 2f;
            currentPopup.transform.position = position;

            // Update information.
            currentState.text = text;
        }
    }

    // Function to return growth time left.
    public float GetTimeLeft()
    {
        return GrowthTimeLeft;
    }

    // Function to set grownth time left.
    public void SetTimeLeft(float timeLeft)
    {
        GrowthTimeLeft = timeLeft;

        if (GrowthTimeLeft > 0f)
        {
            plantAnim.Play("Growth", 0, 1f - (GrowthTimeLeft / GrowthTime));
        }
    }

    // Function to return plant game object state.
    public GameObject GetPlantStateObject()
    {
        if (seedObject.activeSelf)
        {
            return seedObject;
        }

        if (middleGrowthPhase.activeSelf)
        {
            return middleGrowthPhase;
        }

        if (grownPlant.activeSelf)
        {
            return grownPlant;
        }

        return null;
    }

    // Function to set plant game object state.
    public void SetPlantStateObject(string name)
    {
        seedObject.SetActive(false);

        if (seedObject.name.Equals(name))
        {
            seedObject.SetActive(true);
        }

        if (middleGrowthPhase.name.Equals(name))
        {
            middleGrowthPhase.SetActive(true);
        }

        if (grownPlant.name.Equals(name))
        {
            grownPlant.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        if (currentPopup)
        {
            Destroy(currentPopup);
        }
    }
}
