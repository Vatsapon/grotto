using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public static HUDManager instance;

    [Tooltip("Player's Health fill image")]
    public Image healthFill;

    [Space(10f)]

    [Tooltip("Text to display amouny of gold coin")]
    [SerializeField] private TextMeshProUGUI _goldCoinText;

    [Header("Fast Travel")]

    [Tooltip("Duration to hold to activate the warp")]
    [SerializeField] private float _warpDuration = 1.5f;
    [Tooltip("Name of waypoint's scene to warp back to Farm")]
    [SerializeField] private string _sceneName;

    [Space(5f)]

    [Tooltip("Warp Prompt object")]
    [SerializeField] private GameObject _warpPromptObject;
    [Tooltip("Fill image for fast travel")]
    [SerializeField] private Image _warpImage;

    [Space(5f)]

    [Tooltip("Dungeon Door UI Panel")]
    [SerializeField] private DungeonDoorUI _dungeonDoorPanel;

    [Header("Settings Panel")]
    [Tooltip("Image to update Music volume")]
    [SerializeField] private Image musicImage;
    [Tooltip("Image to update SFX volume")]
    [SerializeField] private Image soundImage;

    public DungeonDoorUI DungeonDoorPanel => _dungeonDoorPanel;

    private float currentWarpDuration;

    private WaypointManager waypointM;

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        waypointM = WaypointManager.instance;
    }

    private void Update()
    {
        _goldCoinText.text = GameManager.instance.GetGoldCoin().ToString();

        string currentScene = SceneManager.GetActiveScene().name;
        _warpPromptObject.SetActive(!currentScene.Equals(_sceneName));

        musicImage.fillAmount = GameManager.instance.GetGameData().musicVolume / 100f;
        soundImage.fillAmount = GameManager.instance.GetGameData().soundVolume / 100f;

        if (!currentScene.Equals(_sceneName))
        {
            // [Q] - Use Fast Travel to teleport back to Farm.
            if (Input.GetKey(KeyCode.Q) && !waypointM.IsTeleporting)
            {
                currentWarpDuration += Time.deltaTime;

                if (currentWarpDuration >= _warpDuration)
                {
                    DungeonPoolManager.instance.SetPool(null);
                    waypointM.Teleport(_sceneName);
                }
            }
            else
            {
                currentWarpDuration = 0f;
            }

            _warpImage.fillAmount = currentWarpDuration / _warpDuration;
        }
    }
}
