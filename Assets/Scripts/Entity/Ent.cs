using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ent : MonoBehaviour
{
    public enum EntState
    {
        IDLE = 0,
        DIALOG,
        GIVE_RESOURCES,
        MIRROR,
        FOLLOW,
        PLANTING
    }

    public enum EntType
    {
        PROTOTYPE = 0,
        SUMMER = 1,
        WINTER
    }

    [Tooltip("The type of ent determines the model and dialogue")]
    public EntType TypeOfEnt;

    [Tooltip("Items the ent gives you after the first interaction")]
    public Item[] ItemsToGive;
    public int[] ItemAmountsToGive;

    [Space]
    [Header("Movement Settings")]
    public bool FollowPlayer;
    public bool PlantAtClosestTileOnStart;
    public float EntSpeed;
    [Tooltip("The ent will not move when it is within this range of the player")]
    public float acceptableRange;

    [Space]
    [Header("Restrict Movement")]
    public bool canMirror;
    public bool canFollow;

    [Space]
    [Header("References")]
    [SerializeField] private GameObject[] leavesObjects;
    [SerializeField] private Material _idleMaterial;
    [SerializeField] private Material _followMaterial;

    private EntState CurrentState = EntState.IDLE;
    private EntState NextState = EntState.IDLE;

    //this is for the Ent moving around when the player is not interacting with it
    private Vector3 MoveToPosition;

    //CUSTOM pathfinding for Ent variables

    //plantable tiles
    private GameObject[] plantableTiles;
    private GameObject customTarget; //this is where the ent moves to, if not moving towards the player
    private float acceptableRangePlant = 2.5f;
    private float acceptableRangePlayer;

    private bool EntCanPlant; //whether the ent is close enough to a plantable spot or not
    private PlacePlantInteractive closestPlantable;

    private bool PlayerCanSwitchState;

    // Start is called before the first frame update
    void Start()
    {
        //SetNextState(StartingState);

        MoveToPosition = transform.position;

        //Find all the plantable spots for searching
        plantableTiles = GameObject.FindGameObjectsWithTag("PlantableSpot");

        //set custom target to null
        customTarget = null;

        if (PlantAtClosestTileOnStart)
        {
            SetTargetAsClosestWaterTile();
            FollowPlayer = true; //move back to the player after tile is reached
        }

        //get value from editor
        acceptableRangePlayer = acceptableRange;

        switch (TypeOfEnt)
        {
            case EntType.SUMMER:
                break;
        }
    }

    public void SetPlantAtStart(bool shouldPlant)
    {
        PlantAtClosestTileOnStart = shouldPlant;

        if (shouldPlant)
        {
            if (plantableTiles == null)
            {
                plantableTiles = GameObject.FindGameObjectsWithTag("PlantableSpot");
            }

            SetTargetAsClosestWaterTile();
        }
    }

    public void SetPlayerCanSwitchState(bool state)
    {
        PlayerCanSwitchState = state;
    }

    public void SetCanMirrorOrFollow(bool canMirr, bool canFoll)
    {
        canMirror = canMirr;
        canFollow = canFoll;
    }

    private void SwitchCurrentState(EntState newState)
    {
        //state has not changed
        if (CurrentState == newState)
            return;

        CurrentState = newState;
    }

    //Update target to move to (the player)
    private void UpdateTarget()
    {
        if (customTarget)
        {
            MoveToPosition = customTarget.transform.position;
            acceptableRange = acceptableRangePlant;
        }

        else if (GameObject.FindGameObjectWithTag("Player"))
        {
            MoveToPosition = GameObject.FindGameObjectWithTag("Player").gameObject.transform.position;
            acceptableRange = acceptableRangePlayer;
        }
    }

    private void RotateToFaceCurrentTarget()
    {
        //rotate to face opposite of player when mirrored
        if (CurrentState == EntState.MIRROR)
        {
            Quaternion playerRotation = GameObject.FindGameObjectWithTag("Player").transform.rotation;

            Vector3 reverseRot;
            float x = -Input.GetAxis("Horizontal");

            //when moving up and down, keep same rotation as player
            if (x == 0)
                reverseRot = new Vector3(0, playerRotation.eulerAngles.y, 0);
            //otherwise must be opposite
            else
                reverseRot = new Vector3(0, playerRotation.eulerAngles.y + 180, 0);

            transform.rotation = Quaternion.Euler(reverseRot);
        }
        else if (customTarget)
        {
            transform.LookAt(customTarget.transform);
        }
        else
        {
            transform.LookAt(GameObject.FindGameObjectWithTag("Player").transform);
        }
    }

    private void MoveToTarget()
    {
        //make sure the target's position is updated
        UpdateTarget();

        //conserve y (cannot move up and down)
        MoveToPosition.y = transform.position.y;

        float distance = Vector3.Distance(transform.position, MoveToPosition);

        //means ent is within an acceptable range of the target, so no need to move it
        if (distance < acceptableRange)
        {
            return;
        }

        //means ent has reached the target directly
        if (transform.position == MoveToPosition)
            return;
         
        //update position
        transform.position = Vector3.Lerp(transform.position, MoveToPosition, Time.deltaTime * EntSpeed);
    }

    private void MirrorPlayerMovement()
    {
        float x = - Input.GetAxisRaw("Horizontal");
        float z =  Input.GetAxisRaw("Vertical");

        float playSpeed = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().GetSpeed();

        // from player script
        Vector3 dir = Vector3.zero;
        Rigidbody rigid = GetComponent<Rigidbody>();
        dir += new Vector3(x, 0, z).normalized;
        rigid.velocity = new Vector3(dir.x * playSpeed, 0, dir.z * playSpeed);

        /*
        MoveToPosition = transform.position;
        MoveToPosition.x = MoveToPosition.x + (x * playSpeed);
        MoveToPosition.z = MoveToPosition.z + (z * playSpeed);

        transform.position = Vector3.Lerp(transform.position, MoveToPosition, Time.deltaTime * EntSpeed);
        */
    }

    //switch from idle, to follow, to mirror each time the whistle is used.
    public void OnWhistleToggle()
    {
        switch (CurrentState)
        {
            case EntState.IDLE:
                if (canFollow)
                {
                    FollowPlayer = true;
                    SetNextState(EntState.FOLLOW);
                }
                else if (canMirror)
                {
                    SetNextState(EntState.MIRROR);
                }
                break;

            case EntState.FOLLOW:
                FollowPlayer = false;
                if (canMirror)
                {
                    SetNextState(EntState.MIRROR);
                }
                else
                    SetNextState(EntState.IDLE);
                break;

            case EntState.MIRROR:
                SetNextState(EntState.IDLE);
                break;
        }
    }

    private void PerformCurrentState()
    {
        //make sure time scale is 1 when not in dialog
        if (CurrentState != EntState.DIALOG)
        {
            Time.timeScale = 1;

            //face the player or current target
            RotateToFaceCurrentTarget();

            //Make next state planting a tile if a custom target exists
            if (customTarget)
                SetNextState(EntState.PLANTING);
        }

        //if ent can plant, check 'G' key
        if (EntCanPlant)
        {
            if (Input.GetKeyUp(KeyCode.G))
            {
                closestPlantable.EntSpawnPlant();
            }
        }

        switch (CurrentState)
        {
            case EntState.IDLE:
                break;

            case EntState.FOLLOW:
                MoveToTarget();
                break;

            case EntState.MIRROR:
                //to do: mirror movement
                MirrorPlayerMovement();
                break;

            case EntState.PLANTING:
                MoveToTarget();

                if (customTarget == null)
                {
                    SetNextState(EntState.FOLLOW);
                }
                break;

            case EntState.DIALOG:
                //pause the game, only resume when dialog is done
                Time.timeScale = 0;

                //print("ent in dialog");
                break;

            case EntState.GIVE_RESOURCES:

                //give resources to player
                for (int i = 0; i < ItemsToGive.Length; i++)
                {
                    InventoryManager.instance.AddItem(ItemsToGive[i], ItemAmountsToGive[i]);
                }

                SetNextState(EntState.IDLE);
                break;
        }
    }

    public void SetNextState(EntState nextState)
    {
        NextState = nextState;
        Material leaveColor = _idleMaterial;

        switch (nextState)
        {
            case EntState.IDLE:
            leaveColor = _idleMaterial;
            break;

            case EntState.FOLLOW:
            leaveColor = _followMaterial;
            break;
        }

        for (int i = 0; i < leavesObjects.Length; i++)
        {
            MeshRenderer renderer = leavesObjects[i].GetComponent<MeshRenderer>();
            renderer.material = leaveColor;
        }
    }

    public void ForceState(EntState nextState)
    {
        CurrentState = nextState;
    }

    public void SetEntType(EntType type)
    {
        TypeOfEnt = type;

        //To do: change the MODEL of the prefab to match summer or winter
    }

    //Update whether an ent should follow the player
    public void UpdateEntFollowPlayer(bool shouldFollowPlayer, float acceptableRadius, float speed)
    {
        FollowPlayer = shouldFollowPlayer;
        acceptableRange = acceptableRadius;
        EntSpeed = speed;
    }

    private void Update()
    {
        //Assume this key is for whistling (switching state)
        if (!PlayerCanSwitchState)
            return;

         if (Input.GetKeyUp(KeyCode.F))
        {
            OnWhistleToggle();
        }
          
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //update current state to next state if needed
        if (CurrentState != NextState)
            SwitchCurrentState(NextState);

        PerformCurrentState();

        //only allow rotation on y axis
        //transform.rotation = Quaternion.Euler(0.0f, transform.rotation.y, 0.0f);
    }

    private void SetTargetAsClosestWaterTile()
    {
        float closestDist = 100000;

        GameObject closestTile = plantableTiles[0];

        for (int i = 0; i < plantableTiles.Length; i++)
        {
            PlacePlantInteractive pin = plantableTiles[i].GetComponentInChildren<PlacePlantInteractive>();

            //SKIP all plantable locations which already have plants
            if (pin.PlantPlaced)
                continue;

            float dist = Vector3.Distance(transform.position, plantableTiles[i].transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                closestTile = plantableTiles[i];
            }
        }

        customTarget = closestTile;
    }

    private void OnTriggerEnter(Collider other)
    {
        //print("collision with ent");

        Player player = other.transform.GetComponent<Player>();
        if (player != null)
        {
            
        }

        closestPlantable = other.transform.GetComponent<PlacePlantInteractive>();
        if (closestPlantable)
        {
            //print("ent found a plantable spot");

            EntCanPlant = true;
            
            //reached custom target (plantable spot), no need to keep using it. if it was walked over accidentally it wont plant
            if (customTarget)
            {
                closestPlantable.EntSpawnPlant();
                customTarget = null;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //print("collision with ent");

        PlacePlantInteractive pin = other.transform.GetComponent<PlacePlantInteractive>();
        if (pin)
        {
            closestPlantable = null;
           // print("ent MOVED AWAY from plantable spot");

            EntCanPlant = false;
        }
    }
}
