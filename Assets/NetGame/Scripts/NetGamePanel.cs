using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetGamePanel : MonoBehaviour
{
    public NetGameBehaviour netGameBehaviour;

    void Start()
    {
        var camera = netGameBehaviour.GetComponent<Camera>();
        var renderer = GetComponent<Renderer>();
        var material = renderer.material;
        var targetTexture = new RenderTexture(256, 256, 16);
        material.mainTexture = camera.targetTexture = targetTexture;
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;
        RaycastHit hitInfo;
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
            return;
        if (hitInfo.transform != transform)
            return;
        netGameBehaviour.OnClickAt(hitInfo.textureCoord);
    }
}
