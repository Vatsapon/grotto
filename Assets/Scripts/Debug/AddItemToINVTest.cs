using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddItemToINVTest : MonoBehaviour
{
    public Item Lilypad;
    public Item Whirlybird;
    public Item Launcher;
    public bool AddLilypad;
    public bool AddWhirlybird;
    public bool AddLauncher;
    public int AmountToAdd;
    private InventoryManager IM;

    private void Start()
    {
        IM = InventoryManager.instance;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            Activate();
        }
    }

    public void Activate()
    {
        if(AddWhirlybird)
        {
            IM.AddItem(Whirlybird, AmountToAdd);
        }
        if (AddLilypad)
        {
            IM.AddItem(Lilypad, AmountToAdd);
        }
        if (AddLauncher)
        {
            IM.AddItem(Launcher, AmountToAdd);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Activate();
        }
    }
}
