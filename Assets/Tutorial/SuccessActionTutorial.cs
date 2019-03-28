using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuccessActionTutorial : SuccessAction
{
    public override void success()
    {
        abilities.gameObject.GetComponent<Rigidbody>().mass = 10000;
        abilities.canPush = true;

        manualStandController.isSolved = true;
        AchievementManager.Instance.GotAchievement(achievement);
    }
}
