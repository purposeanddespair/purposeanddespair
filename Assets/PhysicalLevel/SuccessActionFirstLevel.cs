using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuccessActionFirstLevel : SuccessAction
{
    public override void success()
    {
        abilities.canPickup = true;

        manualStandController.isSolved = true;
    }
}
