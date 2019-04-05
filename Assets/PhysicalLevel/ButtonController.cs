using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class ButtonController : MonoBehaviour
{
    public bool isActive = false;
    public bool isEnabled = true;

    private void OnCollisionEnter(Collision collision)
    {
        if (isEnabled)
        {
            isActive = true;
            AnalyticsEvent.Custom("ButtonsPressed", new Dictionary<string, object>
            {
                { "name", "Button pressed"}
            });
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isActive = false;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (isEnabled)
            isActive = true;
    }
}
