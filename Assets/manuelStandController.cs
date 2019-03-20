using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManuelStandController : MonoBehaviour
{
    public ButtonController button;
    public NetGameBehaviour netGame;
    public UpgradeController upgrade;
    public PlayerAbilities abilities;

    private bool createNewGame = true;

    private void Start()
    {
        button.isEnabled = false;
        button.GetComponent<Renderer>().material.color = Color.red;

        if (upgrade == null)
        {
            button.isEnabled = true;
            button.GetComponent<Renderer>().material.color = Color.green;
        }
    }

    void Update()
    {
        if (upgrade != null && upgrade.isPickedUp)
        {
            button.isEnabled = true;
            button.GetComponent<Renderer>().material.color = Color.green;
        }
        if (createNewGame && button.isActive)
        {
            netGame.GenerateNewPuzzle();
            netGame.ResetPuzzle();

            createNewGame = false;
        }
        else if(!button.isActive)
        {
            createNewGame = true;
        }
    }

    private void success()
    {
        abilities.canPickup = true;
    }
}
