using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SuccessAction : MonoBehaviour
{
    public PlayerAbilities abilities;
    public ManualStandController manualStandController;

    public abstract void success();
}
