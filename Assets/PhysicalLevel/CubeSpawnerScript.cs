using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSpawnerScript : MonoBehaviour
{
    public ButtonController button;

    private bool createNewCube = true;

    void Update()
    {
        if (createNewCube && button.isActive)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.tag = "pushable";
            cube.transform.localScale = new Vector3(3, 3, 3);
            cube.layer = 12;
            cube.GetComponent<Renderer>().material.color = Color.green;
            Vector3 cubePosition = new Vector3(gameObject.transform.Find("Pipe (3)").transform.position.x, gameObject.transform.Find("SpawnerHeadBottom").transform.position.y, gameObject.transform.Find("Pipe (3)").transform.position.z);
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
