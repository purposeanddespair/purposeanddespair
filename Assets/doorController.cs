using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class doorController : MonoBehaviour
{
    public buttonController input;

    // Update is called once per frame
    void Update()
    {
        Transform door = gameObject.transform.GetChild(0);
        Vector3 closePosition = door.transform.position;
        Vector3 openPosition = closePosition;
        openPosition.x += 20;
        if (input.isActive)
            door.transform.position = openPosition;
        else
            door.transform.position = closePosition;
    }
}
