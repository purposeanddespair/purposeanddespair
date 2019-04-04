using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AchievementManager : Singleton<AchievementManager>
{
    private ISet<Achievement> _achievements = null;
    public ISet<Achievement> Achievements
    {
        get
        {
            if (_achievements != null)
                return _achievements;
            var array = Resources.LoadAll<Achievement>("");
            Debug.Log($"Loaded {array.Length} achievements");
            return _achievements = new HashSet<Achievement>(array);
        }
    }

    public bool HasAchievement(Achievement a)
    {
        if (!Achievements.Contains(a))
            throw new System.ArgumentException("Invalid achievement instance");
        return false;
        //return PlayerPrefs.GetInt("AchievementHas_" + a.name, 0) > 0;
    }

    public void GotAchievement(Achievement a)
    {
        if (HasAchievement(a))
            return;
        PlayerPrefs.SetInt("AchievementHas_" + a.name, 1);

        var uiAchievement = Instantiate<GameObject>(Resources.Load<GameObject>("UIAchievement"));
        var uiCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
        uiAchievement.transform.SetParent(uiCanvas.transform, false);

        Vector3 targetPosition = uiAchievement.transform.localPosition;
        var uiTitle = uiAchievement.transform.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        uiTitle.text = uiTitle.text.Replace("$NAME", a.DisplayName);
        var uiIcon = uiAchievement.transform.Find("Image").GetComponent<UnityEngine.UI.Image>();
        uiIcon.sprite = a.Sprite;

        iTween.MoveFrom(uiAchievement, new Hashtable
        {
            { "position", targetPosition - new Vector3(0, -80.0f, 0.0f) },
            { "islocal", true },
            { "easetype", iTween.EaseType.easeOutBounce },
            { "time", 0.4f }
        });
        iTween.MoveTo(uiAchievement, new Hashtable
        {
            { "position", targetPosition - new Vector3(0, -80.0f, 0.0f) },
            { "islocal", true },
            { "time", 0.3f },
            { "delay", 2.5f },
            { "oncomplete", "OnUIAchievementAnimationDone" },
            { "oncompletetarget", gameObject },
            { "oncompleteparams", uiAchievement }
        });
    }

    private void OnUIAchievementAnimationDone(GameObject uiAchievement)
    {
        Destroy(uiAchievement);
    }
}
