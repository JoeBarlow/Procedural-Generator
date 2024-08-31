using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_PlayCameraAnim : MonoBehaviour
{
    Animation anim;
    private void Start()
    {
        anim = GetComponent<Animation>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            anim.Play();
        }
    }
}
