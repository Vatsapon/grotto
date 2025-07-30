using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntStateTrigger : MonoBehaviour
{
    public Ent.EntState StateToSwitchTo;

    [Tooltip("If true, player entering the trigger will switch the only Ent in scene's state")]
    public bool TriggerIsPlayer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (TriggerIsPlayer)
        {
            if (other.tag == "Player")
            {
                GameObject entObj = GameObject.FindGameObjectWithTag("Ent");

                if (entObj)
                {
                    Ent ent = entObj.GetComponent<Ent>();

                    if (ent)
                    {
                        ent.SetNextState(StateToSwitchTo);
                    }
                }
            }
        }

        //double verify it is an Ent to avoid errors
        else if (other.tag == "Ent")
        {
            Ent ent = other.GetComponent<Ent>();

            if (ent)
            {
                ent.SetNextState(StateToSwitchTo);
            }
        }
    }
}
