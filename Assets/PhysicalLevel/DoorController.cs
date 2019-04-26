using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class DoorController : MonoBehaviour
{
    public ButtonController input;

    private Vector3 closePosition;
    private Transform door;
    private bool lastInputActive;

    public void Start()
    {
        door = gameObject.transform.GetChild(0);
        closePosition = door.transform.position;
        lastInputActive = input.isActive;
    }

    void Update()
    {
        Vector3 openPosition = closePosition;
        Vector3 doorMovement = new Vector3(9, 0, 0);
        openPosition += door.rotation * doorMovement;
        if (input.isActive)
        {
            if (input.isActive != lastInputActive)
            {
                lastInputActive = input.isActive;
                AnalyticsEvent.Custom("DoorOpened", new Dictionary<string, object>
                {
                    { "name", "Door opened"}
                });
            }
            iTween.MoveTo(door.gameObject, new Hashtable
            {
                { "position", openPosition },
                { "islocal", false },
                { "easetype", iTween.EaseType.linear },
                { "time", 0.4f }
            });
        }
        else
        {
            iTween.MoveTo(door.gameObject, new Hashtable
            {
                { "position", closePosition},
                { "islocal", false },
                { "easetype", iTween.EaseType.linear },
                { "time", 0.4f }
            });
        }

    }
}
