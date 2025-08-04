using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public Transform achieveList;
    private AchieveUIO[] achievementUIOS;

    private void Start()
    {
        achievementUIOS = achieveList.GetComponentsInChildren<AchieveUIO>();
        LoadAchievements();
    }

    private void LoadAchievements() 
    {
        for (int i = 0; i < achievementUIOS.Length; i++)
        {
            achievementUIOS[i].TryLoad();
        }
    }

    public void ProgressAchievement(string aName, int amount = 1) 
    {
        //load, edit and save the achievement after progression!
        for (int i = 0; i < achievementUIOS.Length; i++)
        {
            if (aName == achievementUIOS[i].name && !achievementUIOS[i].IsComplete) 
            {
                achievementUIOS[i].ProgressAchievement(amount);
                break;
            }
        }
    }
}
