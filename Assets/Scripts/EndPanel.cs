using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class EndPanel : MonoBehaviour, IPointerClickHandler
{
    public PlayerController player;
    public SphereCollider triggerSphere;

    private TMP_Text textMeshPro;

    void Start()
    {
        textMeshPro = transform.GetComponentInChildren<TMP_Text>();
    }

    void Update()
    {
        if (Vector3.Distance(player.transform.position, triggerSphere.transform.position) <= triggerSphere.radius)
        {
            player.lockMovement = true;
            transform.localScale = Vector3.one;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMeshPro, Input.mousePosition, null);
        if (linkIndex != -1)
        { // was a link clicked?
            TMP_LinkInfo linkInfo = textMeshPro.textInfo.linkInfo[linkIndex];

            // open the link id as a url, which is the metadata we added in the text field
            Application.OpenURL(linkInfo.GetLinkID());
        }
    }
}
