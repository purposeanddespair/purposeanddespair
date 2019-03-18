using UnityEngine;

public class ButtonController : MonoBehaviour
{
    public bool isActive = false;
    public bool isEnabled = true;

    private void OnCollisionEnter(Collision collision)
    {
        if(isEnabled)
            isActive = true;
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
