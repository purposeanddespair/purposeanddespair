using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class manuelStandController : MonoBehaviour
{
    public buttonController button;
    public NetGameBehaviour netGame;

    private bool createNewGame = true;

    void Update()
    {
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
}
