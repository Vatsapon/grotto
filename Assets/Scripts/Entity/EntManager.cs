using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntManager : MonoBehaviour
{
    //places where the Ent can be put when the player walks into the room. Right now just one point
    public Transform EntSpawnpoint;

    public Ent.EntType TypeOfEnt;

    public Ent.EntState StartingState = Ent.EntState.IDLE;

    public bool PlayerCanSwitchState;

    [Tooltip("Items the ent gives you after the first interaction")]
    public Item[] ItemsToGive;
    public int[] ItemAmountsToGive;

    [Space]
    [Header("Movement Settings")]
    public bool FollowPlayerAfterInteraction;
    public bool PlacePlantNearbyOnStart;
    public float EntSpeed;

    [Tooltip("The ent will not move when it is within this range of the player")]
    public float acceptableRange;

    [Header("Restrict Movement")]
    public bool canMirror = false;
    public bool canFollow = true;

    [Space]
    public GameObject entPrefab;

    // Start is called before the first frame update
    void Start()
    {      
       print("Ent manager spawning ent");
       GameObject newEnt = Instantiate(entPrefab, EntSpawnpoint.position, Quaternion.identity);

        Ent ent = newEnt.GetComponent<Ent>();
       ent.ForceState(StartingState);
       ent.SetNextState(StartingState);
       ent.SetEntType(TypeOfEnt);
       ent.ItemsToGive = ItemsToGive;
       ent.ItemAmountsToGive = ItemAmountsToGive;

       ent.UpdateEntFollowPlayer(FollowPlayerAfterInteraction, acceptableRange, EntSpeed);
       ent.SetPlantAtStart(PlacePlantNearbyOnStart);

       ent.SetPlayerCanSwitchState(PlayerCanSwitchState);
       ent.SetCanMirrorOrFollow(canMirror, canFollow);

           
    }

    public void SpawnNewEntWithoutStartingDialogue()
    {
        print("Ent manager spawning ent without starting dialogue");
        GameObject newEnt = Instantiate(entPrefab, EntSpawnpoint.position, Quaternion.identity);

        Ent ent = newEnt.GetComponent<Ent>();

        ent.ForceState(StartingState);
        ent.SetNextState(StartingState);
        ent.SetEntType(TypeOfEnt);
        ent.ItemsToGive = ItemsToGive;
        ent.ItemAmountsToGive = ItemAmountsToGive;

        ent.UpdateEntFollowPlayer(FollowPlayerAfterInteraction, acceptableRange, EntSpeed);

        ent.SetPlayerCanSwitchState(PlayerCanSwitchState);
        ent.SetCanMirrorOrFollow(canMirror, canFollow);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
