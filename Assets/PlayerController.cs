using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 0.1f;
    public float rotationSpeed = 1f;
    public float maxStepHeight = 1.2f;
    public PlayerAbilities abilities;

    private List<ContactPoint> allContactPoints = new List<ContactPoint>();
    private GameObject holdingObject = null;
    Vector3 relativeCubeLocation;

    private List<Vector3> drawPoint = new List<Vector3>();
    private Vector3 drawDirection;
    
    private void Update()
    {
        Move();

        //RotateAdjustingToGround();

        ContactPoint groundContactPoint = findGround();

        float stepHeight;
        if (findStep(groundContactPoint, out stepHeight))
        {
            MoveUp(stepHeight);
        }

        if (Input.GetKeyDown("e"))
            pickupOrDrop();

        allContactPoints.Clear();
    }
    
    private void Move()
    {
        Vector3 forward = gameObject.transform.TransformDirection(Vector3.forward);
        Vector3 previousPosition = gameObject.transform.position;
        
        gameObject.transform.position = previousPosition + forward * speed * Input.GetAxis("Vertical");
        gameObject.transform.Rotate(new Vector3(0, rotationSpeed * Input.GetAxis("Horizontal"), 0));
        
        if (holdingObject != null)
        {
            relativeCubeLocation = Quaternion.Euler(0, rotationSpeed * Input.GetAxis("Horizontal"), 0) * relativeCubeLocation;
            holdingObject.transform.position = gameObject.transform.position + relativeCubeLocation;
        }
    }

    private void MoveUp(float height)
    {
        Vector3 forward = gameObject.transform.forward;
        Vector3 position = gameObject.transform.position + forward * speed * Input.GetAxis("Vertical");
        position.y += height;
        gameObject.transform.position = position;
        gameObject.transform.Rotate(new Vector3(0, rotationSpeed * Input.GetAxis("Horizontal"), 0));
    }

    private void RotateAdjustingToGround()
    {
        RaycastHit hit;
        int layerMask = LayerMask.GetMask("Ground"); //select only "Ground" layer
        Vector3 direction = gameObject.transform.TransformDirection(Vector3.down);
        Physics.Raycast(gameObject.transform.position, direction, out hit, layerMask);
        gameObject.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(gameObject.transform.forward, Vector3.Lerp(gameObject.transform.up, hit.normal, 0.05f)), hit.normal);
    }

    private void OnCollisionEnter(Collision collision)
    {
        allContactPoints.AddRange(collision.contacts);
    }

    private void OnCollisionStay(Collision collition)
    {
        allContactPoints.AddRange(collition.contacts);
    }

    private ContactPoint findGround()
    {
        ContactPoint groundCollisionPoint = default(ContactPoint);
        bool found = false;
        foreach (ContactPoint contactPoint in allContactPoints)
        {
            if (contactPoint.normal.y == gameObject.transform.up.y && (found == false || contactPoint.normal.y > groundCollisionPoint.normal.y))
            {
                groundCollisionPoint = contactPoint;
                found = true;
            }
        }

        return groundCollisionPoint;
    }

    private bool findStep(ContactPoint groundContactPoint, out float stepHeight)
    {
        stepHeight = 0;
        foreach (ContactPoint contactPoint in allContactPoints)
        {
            float contactPointHeight = Math.Abs(contactPoint.point.y) - Math.Abs(groundContactPoint.point.y);
            if (contactPointHeight < maxStepHeight && contactPointHeight > 0.0001f)
            {
                RaycastHit stepHit;
                int layerMask = LayerMask.GetMask("Ground");
                Vector3 direction = contactPoint.point - gameObject.transform.position;
                direction.y = 0;
                float maxDistance = 0.01f;
                Vector3 stepPoint = contactPoint.point;
                stepPoint.y = groundContactPoint.point.y + maxStepHeight;
                stepPoint -= direction * 0.001f;
                
                Physics.Raycast(stepPoint, direction, out stepHit, maxDistance, layerMask);

                stepHeight = findStepHeight(groundContactPoint, contactPoint);

                if (stepHit.collider == null && contactPoint.otherCollider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                    return true;
            }
        }
        return false;
    }

    private float findStepHeight(ContactPoint groundContactPoint, ContactPoint contactPoint)
    {
        RaycastHit hit;
        float stepHeight = groundContactPoint.point.y + maxStepHeight + 0.0001f;
        Vector3 stepTestInvDir = new Vector3(-contactPoint.normal.x, 0, -contactPoint.normal.z).normalized;
        Vector3 origin = new Vector3(contactPoint.point.x, stepHeight, contactPoint.point.z) + (stepTestInvDir);
        Vector3 direction = - gameObject.transform.up;
        if (!(Physics.Raycast(new Ray(origin, direction), out hit, maxStepHeight)))
        {
            return 0;
        }

        return Math.Min(hit.point.y - groundContactPoint.point.y + 0.0001f, maxStepHeight);
    }

    private void pickupOrDrop()
    {
        if (holdingObject == null)
            pickup();
        else
            drop();
            
    }


    private bool pickup()
    {
        if (holdingObject != null || !abilities.canPickup)
            return true;

        //pickup object
        GameObject go = objectIsInfront();
        if (go != null)
            return pickupObject(go);

        return false;
    }

    private GameObject objectIsInfront()
    {
        Transform body = gameObject.transform.GetChild(0).GetChild(0);

        Vector3 origin = body.position;
        Vector3 direction = gameObject.transform.forward;
        float maxDistance = 6.5f;
        LayerMask layerMask = LayerMask.GetMask("Pickupable");

        RaycastHit hit;
        Physics.Raycast(origin, direction, out hit, maxDistance, layerMask);

        if (hit.collider != null)
            return hit.collider.gameObject;
        return null;
    }

    private bool pickupObject(GameObject go)
    {
        go.GetComponent<Rigidbody>().useGravity = false;
        go.GetComponent<Rigidbody>().isKinematic = true;

        Vector3 objectPosition = go.transform.position;
        objectPosition.y = go.transform.position.y + 2;
        go.transform.position = objectPosition;

        holdingObject = go;
        relativeCubeLocation = holdingObject.transform.position - gameObject.transform.position;
        return holdingObject;
    }

    private void drop()
    {
        if (holdingObject == null)
            return;

        holdingObject.GetComponent<Rigidbody>().useGravity = true;
        holdingObject.GetComponent<Rigidbody>().isKinematic = false;

        holdingObject = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if(drawPoint.Count != 0)
        {
            foreach(Vector3 point in drawPoint)
            {
                Gizmos.DrawSphere(point, 0.2f);
                Gizmos.DrawLine(point, point + drawDirection);
            }
        }
    }
}