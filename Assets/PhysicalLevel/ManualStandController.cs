using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualStandController : MonoBehaviour
{
    public ButtonController button;
    public NetGameBehaviour netGame;
    public UpgradeController upgrade;
    public SuccessAction successaction;
    public NetGamePanel netGamePanel;

    public bool isSolved = false;

    private bool createNewGame = true;
    private GameObject uiUpgrade = null;
    
    private void Start()
    {
        button.isEnabled = false;
        button.GetComponent<Renderer>().material.color = Color.red;
        if (upgrade == null)
            OnUpgradePickedUp();
        else
            upgrade.OnPickedUp += OnUpgradePickedUp;
    }

    private void OnUpgradePickedUp()
    {
        button.isEnabled = true;
        button.GetComponent<Renderer>().material.color = Color.green;

        uiUpgrade = Instantiate<GameObject>(Resources.Load<GameObject>("UIUpgrade"));
        var uiCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
        uiUpgrade.transform.SetParent(uiCanvas.transform, false);
        uiUpgrade.transform.Find("Image").GetComponent<UnityEngine.UI.Image>().sprite = upgrade.icon;
        iTween.MoveFrom(uiUpgrade, new Hashtable
        {
            { "position", uiUpgrade.transform.localPosition - new Vector3(0, -100.0f, 0.0f) },
            { "islocal", true },
            { "easetype", iTween.EaseType.easeOutBounce },
            { "time", 0.4f }
        });
    }

    private void OnUpgradeFinished()
    {
        iTween.MoveTo(uiUpgrade, new Hashtable
        {
            { "position", uiUpgrade.transform.localPosition - new Vector3(0, -100.0f, 0.0f) },
            { "islocal", true },
            { "time", 0.3f },
            { "oncomplete", "OnUIUpgradeAnimationDone" },
            { "oncompletetarget", gameObject },
            { "oncompleteparams", uiUpgrade }
        });
    }

    private void OnUIAchievementAnimationDone(GameObject uiUpgrade)
    {
        Destroy(uiUpgrade);
    }

    void Update()
    {
        if (createNewGame && button.isActive)
        {
            if (!isSolved)
            {
                netGame.GenerateNewPuzzle();
                netGame.ResetPuzzle();
            }

            netGame.GameWasCompleted += successaction.success;
            netGame.GameWasCompleted += OnUpgradeFinished;

            createNewGame = false;
            netGamePanel.gameObject.SetActive(true);
        }
        else if(!button.isActive)
        {
            createNewGame = true;
            netGamePanel.gameObject.SetActive(false);
        }
    }
}
