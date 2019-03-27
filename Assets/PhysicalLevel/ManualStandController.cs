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
    
    private void Start()
    {
        button.isEnabled = false;
        button.GetComponent<Renderer>().material.color = Color.red;
    }

    void Update()
    {
        if (upgrade == null || upgrade.isPickedUp)
        {
            button.isEnabled = true;
            button.GetComponent<Renderer>().material.color = Color.green;
        }
        if (createNewGame && button.isActive)
        {
            if (!isSolved)
            {
                netGame.GenerateNewPuzzle();
                netGame.ResetPuzzle();
            }

            netGame.GameWasCompleted += successaction.success;

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
