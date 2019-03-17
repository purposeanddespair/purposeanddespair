using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cubeSpawnerScript : MonoBehaviour
{
    public buttonController button;

    private bool createNewCube = true;

    void Update()
    {
        if (createNewCube && button.isActive)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale = new Vector3(3, 3, 3);
            cube.layer = 12;
            cube.GetComponent<Renderer>().material.color = Color.green;
            Vector3 cubePosition = new Vector3(gameObject.transform.position.x + 0.5f, gameObject.transform.GetChild(2).transform.position.y, gameObject.transform.GetChild(5).transform.position.z);
            cube.transform.position = cubePosition;
            Rigidbody rb = cube.AddComponent<Rigidbody>();
            rb.mass = 1;

            createNewCube = false;
        }
        else if (!button.isActive)
        {
            createNewCube = true;
        }
    }
}
