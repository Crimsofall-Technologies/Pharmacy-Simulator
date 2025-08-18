using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchieveUIO : MonoBehaviour
{
    public int MaxValue = 100;
    public int gemsReward = 10;

    private Text titleText;
    private Slider bar;
    private Button button;
    public bool IsComplete { get; private set; }

    private string baseText;

    public void TryLoad() 
    {
        titleText = transform.GetChild(1).GetComponent<Text>();
        bar = transform.GetChild(0).GetComponent<Slider>();
        button = transform.GetChild(3).GetComponent<Button>();
        button.onClick.AddListener(CollectGems);
        button.gameObject.SetActive(false);

        baseText = titleText.text;

        int value = PlayerPrefs.GetInt(transform.name, 0);
        if (value < MaxValue) //means this is not yet complete!
        {
            bar.maxValue = MaxValue;
            bar.minValue = 0;
            bar.value = value;

            titleText.text = baseText + " - [In Progress]";
            transform.SetAsFirstSibling(); //make completed achievements at bottom
            IsComplete = false;
        }
        else //completed?
        {
            IsComplete = true;
            titleText.text = baseText + $" - [Complete] (+{gemsReward}Gems)";
            transform.SetAsLastSibling(); //make completed achievements at bottom

            //complete but player has not collected reward yet?
            if (PlayerPrefs.GetInt("", 0) == 0) 
            {
                button.gameObject.SetActive(true);
            }
        }
    }

    public void ProgressAchievement(int amount)
    {
        int value = PlayerPrefs.GetInt(transform.name, 0);
        value+=amount;
        bar.value = value;

        if (value >= MaxValue) 
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

        PlayerPrefs.SetInt(transform.name, value);
    }

    public void CollectGems() 
    {
        PlayerPrefs.SetInt(transform.name + "-comp", 1);
        GlobalVar.Instance.AddGems(gemsReward);
        button.gameObject.SetActive(false);
    }
}
