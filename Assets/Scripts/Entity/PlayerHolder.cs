using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHolder : MonoBehaviour
{
    [Tooltip("Determine to show debug gizmos of offset or not")]
    [SerializeField] private bool showDebug = false;
    [Tooltip("Offset to spawn the holding object")]
    [SerializeField] private Vector3 holdOffset = Vector3.zero;

    private Item holdingPlant; // Plant that player is currently holding.
    private GameObject holdingObject; // Object of plant that currently holding.

    private Item visualHoldingPlant; // Plant that player is currently holding as visual.
    private GameObject visualHoldingObject; // Object of plant that current holding (only for visual, not actual plant)
    
    private Player player;
    private Animator playerAnimator;

    private void Start()
    {
        player = FindObjectOfType<Player>();
        playerAnimator = player.GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        playerAnimator.SetBool("IsHolding", IsHolding() || IsVisualHolding());
        
        if (holdingObject)
        {
            holdingObject.transform.position = GetHoldingPosition();
        }
    }

    // Function to execute when player start holding a plant.
    public void HoldingPlant(Item item)
    {
        if (IsHolding())
        {
            return;
        }

        if (IsVisualHolding())
        {
            visualHoldingPlant = null;

            if (IsVisualHolding())
            {
                Destroy(visualHoldingObject);
            }
        }

        holdingPlant = item;

        holdingObject = Instantiate(item.holdingPrefab, GetHoldingPosition(), Quaternion.identity);
        holdingObject.transform.localScale = Vector3.one;
        holdingObject.transform.SetParent(transform);
    }

    // Function to execute when player placing a plant.
    public Item PlacePlant()
    {
        Item placePlant = holdingPlant;
        holdingPlant = null;
        Destroy(transform.GetChild(0).gameObject);

        return placePlant;
    }

    // Function to determine if player is holding a plant or not.
    public bool IsHolding() => holdingPlant;

    public bool IsVisualHolding() => visualHoldingObject;

    // Function to get holding plant.
    public Item GetHolding() => holdingPlant;

    // Function to set holding plant as visual. (Not plant that pick up.)
    public void HoldingVisualPlant(Item item)
    {
        if (IsHolding())
        {
            visualHoldingPlant = null;

            if (IsVisualHolding())
            {
                Destroy(visualHoldingObject);
            }

            return;
        }

        if (!item || (item && visualHoldingPlant && !item.Equals(visualHoldingPlant)))
        {
            visualHoldingPlant = null;

            if (visualHoldingObject)
            {
                Destroy(visualHoldingObject);
            }

            return;
        }

        if (!IsVisualHolding() && item.holdingPrefab)
        {
            visualHoldingObject = Instantiate(item.holdingPrefab, GetHoldingPosition(), Quaternion.identity);
            visualHoldingObject.transform.localScale = Vector3.one;
            visualHoldingObject.transform.SetParent(transform);

            visualHoldingPlant = item;
        }
    }
    
    // Function to get holding position.
    private Vector3 GetHoldingPosition()
    {
       return transform.position + (player.transform.right * holdOffset.x) + (player.transform.up * holdOffset.y) + (player.transform.forward * holdOffset.z);
    }

    private void OnDrawGizmos()
    {
        if (showDebug)
        {
            player = FindObjectOfType<Player>();
            Gizmos.DrawWireSphere(GetHoldingPosition(), 0.5f);
        }
    }
}
