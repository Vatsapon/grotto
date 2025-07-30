using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class HotbarManager : MonoBehaviour
{
    [Tooltip("Speed to rotate the hotbar")]
    [SerializeField] private float rotateSpeed = 5f;

    [Header("References")]

    [Tooltip("Radial Layout of Hotbar")]
    [SerializeField] private RadialLayout _radial;
    [Tooltip("Panel of Hotbar")]
    [SerializeField] private Transform _radialPanel;
    [Tooltip("Parent of all items")]
    [SerializeField] private Transform _itemsParent;
    [Tooltip("Image of selected item")]
    [SerializeField] private Image _selectedImage;
    [Tooltip("Image of carrying item")]
    [SerializeField] private Image _carryingImage;

    [Space(10f)]

    [Tooltip("Material to fade-in/out for hotbar")]
    [SerializeField] private Material _hotbarMaterial;
    [Tooltip("Color to set on hotbar material")]
    [SerializeField] private Color _hotbarColor = Color.white;

    [Space(10f)]

    [Tooltip("Animator of hotbar")]
    [SerializeField] private Animator _animator;

    private int hotbarIndex;
    private bool isRotating = false;

    public int HotbarIndex => hotbarIndex;

    private Player player;
    private InventoryManager invM;

    private void Start()
    {
        invM = InventoryManager.instance;
        player = FindObjectOfType<Player>();

        _animator.Play("Hotbar", -1, 0.99f);
    }

    private void Update()
    {
        _hotbarMaterial.color = _hotbarColor;

        if (player.Holder.IsHolding())
        {
            _carryingImage.sprite = player.Holder.GetHolding().itemTexture;
        }

        _animator.SetBool("IsCarrying", player.Holder.IsHolding());

        HotbarUpdater();
    }

    // Function to update Hotbar HUD.
    private void HotbarUpdater()
    {
        if (Input.mouseScrollDelta.y < 0f && !isRotating && !player.Holder.IsHolding())
        {
            if (hotbarIndex > 0)
            {
                hotbarIndex--;
            }
            else
            {
                hotbarIndex = invM.GetAllSlots().Count - 1;
            }

            float targetAngle = _radial.StartAngle - 60f;
            Rotate(targetAngle);
        }

        if (Input.mouseScrollDelta.y > 0f && !isRotating && !player.Holder.IsHolding())
        {
            if (hotbarIndex < invM.GetAllSlots().Count - 1)
            {
                hotbarIndex++;
            }
            else
            {
                hotbarIndex = 0;
            }

            float targetAngle = _radial.StartAngle + 60f;
            Rotate(targetAngle);
        }

        // Update selected hotbar slot.
        InventorySlot selectedSlot = invM.GetAllSlots()[hotbarIndex];
        Image icon = _selectedImage.GetComponent<Image>();
        TMP_Text amount = _selectedImage.transform.Find("Amount").GetComponent<TMP_Text>();

        // If there's item in this slot, show data.
        if (selectedSlot.GetItem())
        {
            // If this slot has remove when empty value, remove slot.
            if (selectedSlot.IsRemoveWhenEmpty() && selectedSlot.GetAmount() == 0)
            {
                icon.enabled = false;
                amount.enabled = false;

                selectedSlot.SetItem(null);
            }
            else
            {
                icon.sprite = selectedSlot.GetItem().itemTexture;
                amount.text = "x" + selectedSlot.GetAmount();

                icon.enabled = true;
                amount.enabled = true;
            }
        }
        else
        {
            icon.enabled = false;
            amount.enabled = false;

            selectedSlot.SetItem(null);
        }

        // Update visual holder.
        InventorySlot slot = invM.GetAllSlots()[hotbarIndex];
        Item item = invM.GetAllSlots()[hotbarIndex].GetItem();

        // If it's plant and has more than 0, show holding visual plant.
        if (slot.GetItem() && slot.GetAmount() > 0 && slot.GetItem().itemType == Item.ItemType.Plant)
        {
            player.Holder.HoldingVisualPlant(item);
        }
        else
        {
            player.Holder.HoldingVisualPlant(null);
        }
    }

    public void ContentUpdate()
    {
        if (!invM)
        {
            invM = InventoryManager.instance;
        }

        for (int i = 0; i < _itemsParent.childCount; i++)
        {
            UpdateSlot(i);
        }
    }

    // Function to update slot.
    private void UpdateSlot(int index)
    {
        InventorySlot slot = invM.GetAllSlots()[index];
        Image icon = _itemsParent.GetChild(index).GetComponent<Image>();
        TMP_Text amount = _itemsParent.GetChild(index).Find("Amount").GetComponent<TMP_Text>();

        // If there's item in this slot, show data.
        if (slot.GetItem())
        {
            // If this slot has remove when empty value, remove slot.
            if (slot.IsRemoveWhenEmpty() && slot.GetAmount() == 0)
            {
                icon.enabled = false;
                amount.enabled = false;

                slot.SetItem(null);
            }
            else
            {
                icon.sprite = slot.GetItem().itemTexture;
                amount.text = "x" + slot.GetAmount();

                icon.enabled = true;
                amount.enabled = true;
            }
        }
        else
        {
            icon.enabled = false;
            amount.enabled = false;

            slot.SetItem(null);
        }
    }

    // Function to rotate the hotbar wheel.
    public async void Rotate(float targetRotation)
    {
        isRotating = true;

        while(_radial.StartAngle != targetRotation)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            _radial.StartAngle = Mathf.Lerp(_radial.StartAngle, targetRotation, rotateSpeed * Time.deltaTime);

            if (Mathf.Abs(_radial.StartAngle - targetRotation) < 0.1f)
            {
                _radial.StartAngle = targetRotation;
            }

            await Task.Yield();
        }

        if (_radial.StartAngle < 0f)
        {
            _radial.StartAngle += 360f;
        }

        if (_radial.StartAngle > 360f)
        {
            _radial.StartAngle -= 360f;
        }

        isRotating = false;
    }
}
