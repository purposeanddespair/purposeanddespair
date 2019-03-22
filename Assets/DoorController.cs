using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public ButtonController input;

    private Vector3 closePosition;
    private Transform door;

    public void Start()
    {
        door = gameObject.transform.GetChild(0);
        closePosition = door.transform.position;
    }

    void Update()
    {
        Vector3 openPosition = closePosition;
        Vector3 doorMovement = new Vector3(9, 0, 0);
        openPosition += door.rotation * doorMovement;
        if (input.isActive)
            door.transform.position = openPosition;
        else
            door.transform.position = closePosition;
    }
}
