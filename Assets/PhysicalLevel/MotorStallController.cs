using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotorStallController : MonoBehaviour
{
    private readonly float maxDistance = 3f;

    [HideInInspector]
    public Transform playerTransform;

    private bool isDying = false;
    private new ParticleSystem particleSystem;
    private Vector3 startPosition;

    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
        startPosition = playerTransform.position;
    }

    void Update()
    {
        if (isDying)
            return;

        float curDistance = Vector3.Distance(playerTransform.position, startPosition);
        if (curDistance > maxDistance || Input.GetAxis("Vertical") <= 0.0f)
        {
            isDying = true;
            transform.parent = playerTransform.parent;

            var emission = particleSystem.emission;
            emission.rateOverTime = 0.0f;

            var main = particleSystem.main;
            particleSystem.time = 0.0f;
            main.loop = false;
        }
    }
}
