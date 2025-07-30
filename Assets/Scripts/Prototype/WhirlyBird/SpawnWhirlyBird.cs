using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnWhirlyBird : MonoBehaviour
{
    public GameObject BaseObject;
    public GameObject WhirlyBirdPrefab;
    public Item WhirlyBirdItem;
    public Transform[] SpawnPositions;
    private int CurrentPostion;
    private GameObject SpawnedWhirlyBird = null;
    public PlacePlantInteractive PPI;

    private Interactive interactive;

    private void Start()
    {
        PPI = GetComponentInParent<PlacePlantInteractive>();
        interactive = GetComponent<Interactive>();
        
        Activate();
    }

    public void Activate()
    {
        if(CurrentPostion == 0)
        {
            interactive.ActivateConditional();

            SpawnedWhirlyBird = Instantiate(WhirlyBirdPrefab, SpawnPositions[CurrentPostion]);
            CurrentPostion++;
        }
        else if(CurrentPostion == 4)
        {
            PlayerHolder holder = Player.instance.Holder;

            // If player is holding plant, make it reset rotation instead of pick up.
            if (holder.IsHolding())
            {
                Destroy(SpawnedWhirlyBird);
                CurrentPostion = 0;
                Activate();
                return;
            }

            interactive.DeActivateConditional();

            PPI.PlantPlaced = false;
            holder.HoldingPlant(WhirlyBirdItem);

            Destroy(BaseObject);
            CurrentPostion = 0;
        }
        else
        {
            MoveWhirlyBird();
            CurrentPostion++;
        }
    }

    public void MoveWhirlyBird()
    {
        Destroy(SpawnedWhirlyBird);
        SpawnedWhirlyBird = Instantiate(WhirlyBirdPrefab, SpawnPositions[CurrentPostion]);
    }
}
