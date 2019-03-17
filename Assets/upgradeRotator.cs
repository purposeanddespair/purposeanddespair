using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class upgradeRotator : MonoBehaviour
{
    private int sign = 1;
    private int signCount = 0;
    void Update()
    {
        if(signCount == 80)
        {
            sign = sign * -1;
            signCount = 0;
        }

        gameObject.transform.Rotate(new Vector3(0, 30, 0) * Time.deltaTime);
        Vector3 pickupPosition = gameObject.transform.position;
        pickupPosition.y += sign * Time.deltaTime; 
        gameObject.transform.position = pickupPosition;

        signCount++;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            gameObject.SetActive(false);
        }
    }
}
