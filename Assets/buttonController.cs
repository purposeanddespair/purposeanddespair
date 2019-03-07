using UnityEngine;

public class buttonController : MonoBehaviour
{
    public bool isActive = false;

    private void OnCollisionEnter(Collision collision)
    {
        isActive = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        isActive = false;
    }

    private void OnCollisionStay(Collision collision)
    {
        isActive = true;
    }
}
