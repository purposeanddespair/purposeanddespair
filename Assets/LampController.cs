using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LampController : MonoBehaviour
{
    public buttonController input;
    
    void Update()
    {
        if (input.isActive)
            gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
        else
            gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
    }
}
