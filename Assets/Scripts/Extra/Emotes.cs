using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Emotes : MonoBehaviour
{
    public Animator Anim;
    public KeyCode SquatTriggerKey = KeyCode.Alpha1;
    public string SquatTriggerString;

    private void Update()
    {
        if(Input.GetKeyDown(SquatTriggerKey))
        {
            Anim.SetTrigger(SquatTriggerString);
        }
    }

}
