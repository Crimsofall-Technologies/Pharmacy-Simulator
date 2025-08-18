using System.Collections;
using UnityEngine;

//track the total play time (not paused for) and progress achievements when they exit!
public class AutoPlayTimeDetector : MonoBehaviour
{
    private int secondsPlayed = 0;

    private void Start()
    {
        StartCoroutine(Loop());
    }

    private IEnumerator Loop() 
    {
        while (true) 
        {
            yield return new WaitForSeconds(1f);
            secondsPlayed++;
        }
    }

    private void OnApplicationQuit()
    {
        UIManager.Instance.achievementManager.ProgressAchievement("Play 1 hour", secondsPlayed);
        UIManager.Instance.achievementManager.ProgressAchievement("Play 5 hours", secondsPlayed);
        UIManager.Instance.achievementManager.ProgressAchievement("Play 8 hours", secondsPlayed);
    }
}
