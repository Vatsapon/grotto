using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LilypadScript : MonoBehaviour
{
    public GameObject BaseObject;
    private PlacePlantInteractive PPI;
    private GameObject PlayerPos;
    public Transform LilypadSpawnPoint;
    public GameObject LilypadObject;
    public Item LilyPadItem;
    private Vector3 FinalLilyPadScale = new Vector3(4.65f, 3, 4.35f);
    private Vector3 StartLilyPadScale = new Vector3(0.2f, 0.2f, 0.2f);
    private float LilypadLerp = 1;
    private float StartTimer;
    private float EndTimer;
    private bool LilyPadPlaced = false;
    private bool KillObject = false;


    private GameObject Obj;
    private void Start()
    {
        PlayerPos = GameObject.FindGameObjectWithTag("Player"); ;
        PPI = GetComponentInParent<PlacePlantInteractive>();
        Obj = Instantiate(LilypadObject, PlayerPos.transform.position, PlayerPos.transform.rotation);
        Obj.transform.parent = this.gameObject.transform;
        Obj.transform.localScale = StartLilyPadScale;


    }
    private void Update()
    {
        StartTimer += Time.deltaTime * 1.5f;
        if(StartTimer <= LilypadLerp)
        {
            Obj.transform.position = Vector3.Lerp(PlayerPos.transform.position, LilypadSpawnPoint.position, StartTimer / LilypadLerp);
            Obj.transform.localScale = Vector3.Lerp(StartLilyPadScale, FinalLilyPadScale, StartTimer / LilypadLerp);
        }
        if(KillObject)
        {
            EndTimer += Time.deltaTime * 1.5f;
            Obj.transform.position = Vector3.Lerp(LilypadSpawnPoint.position, PlayerPos.transform.position, EndTimer / LilypadLerp);
            Obj.transform.localScale = Vector3.Lerp(FinalLilyPadScale, StartLilyPadScale, EndTimer / LilypadLerp);
            if(EndTimer >= LilypadLerp)
            {
                DestroyLilypad();
            }
        }

    }
    public void Activate()
    {
        PlayerHolder holder = Player.instance.Holder;

        if (holder.IsHolding())
        {
            AudioManager.instance.PlaySound("NoPlant");
            return;
        }

        KillObject = true;
    }

    public void DestroyLilypad()
    {
        PPI.PlantPlaced = false;

        // PPI.IM.AddItem(LilyPadItem, 1);
        FindObjectOfType<Player>().Holder.HoldingPlant(LilyPadItem);

        Destroy(BaseObject);
    }



}
