using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchieveUIO : MonoBehaviour
{
    public int MaxValue = 100;
    public int currentValue=0;
    public int gemsReward = 10;

    private Text titleText;
    private Slider bar;
    private Button button;
    public bool IsComplete { get; private set; }
    public bool Collected { get; private set; }

    private string baseText;

    public void UpdateSelf(bool collected, int progress)
    {
        Collected = collected;
        currentValue = progress;

        if(progress >= MaxValue) 
            IsComplete = true;

        button.gameObject.SetActive(!Collected);
        bar.value = progress;
    }

    public void TryLoad() 
    {
        titleText = transform.GetChild(1).GetComponent<Text>();
        bar = transform.GetChild(0).GetComponent<Slider>();
        button = transform.GetChild(3).GetComponent<Button>();
        button.onClick.AddListener(CollectGems);
        button.gameObject.SetActive(false);

        baseText = titleText.text;
        if (currentValue < MaxValue) //means this is not yet complete!
        {
            bar.maxValue = MaxValue;
            bar.minValue = 0;
            bar.value = currentValue;

            titleText.text = baseText + " - [In Progress]";
            transform.SetAsFirstSibling(); //make completed achievements at bottom
            IsComplete = false;
        }
        else //completed?
        {
            IsComplete = true;
            titleText.text = baseText + $" - [Complete] (+{gemsReward}Gems)";
            transform.SetAsLastSibling(); //make completed achievements at bottom
        }
    }

    public void ProgressAchievement(int amount)
    {
        if(IsComplete)
            return;

        currentValue += amount;
        bar.value = currentValue;

        if (currentValue >= MaxValue) 
        {
            Debug.Log("Completed achivement: " + titleText);
            transform.SetAsLastSibling(); //complete? set as last and let player view uncomplete achievements first!

            //show something to player on completing!
            UIManager.Instance.OpenAchieveUI(baseText);

            titleText.text = baseText + $" - [Complete] (+{gemsReward}Gems)";
            button.gameObject.SetActive(true);

            //otherwise this will cause a StackOverflowException!
            if (transform.name != "Complete All") 
            {
                UIManager.Instance.achievementManager.ProgressAchievement("Complete All");
            }
            IsComplete = true;
        }
    }

    public void CollectGems() 
    {
        GlobalVar.Instance.AddGems(gemsReward);
        button.gameObject.SetActive(false);
        Collected = true;
    }
}
