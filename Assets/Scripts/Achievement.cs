using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AchievementTriggerType
{
    GotUpgrade
}

[CreateAssetMenu(fileName = "NewAchievement", menuName = "PurposeAndDespair/Achievement")]
public class Achievement : ScriptableObject
{
    public string DisplayName;
    public Sprite Sprite;
    public AchievementTriggerType TriggerType;
    public string TriggerParameter;
}
