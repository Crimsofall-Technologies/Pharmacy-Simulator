using UnityEngine;
using UnityEngine.UI;

public class ResearchBar : MonoBehaviour
{
	public string Name;
	public string ShopItemName;
	public bool isFlavor;
	public int ResearchTime = 10;
	public int Cost;
	[Min(1)] public int Level = 1; //the level at which this research will unlock!

	public Slider bar;
	public Text text;

	private Button button;
	private Image icon;
	private Timer myTimer;

	public void UpdateSelf() 
	{
		if (button == null)
		{
			button = transform.GetChild(2).GetComponent<Button>();
			icon = button.transform.GetChild(0).GetComponent<Image>();
		}

		//not yet done?
		if (PlayerPrefs.GetInt("RES_" + Name, 0) == 0)
		{
			text.text = $"[Not Researched] {Name}{(isFlavor ? " Flavor" : "")}";
			bar.value = 0f;
		}
		else 
		{
			//done previously?
            text.text = $"[Researched] {Name}{(isFlavor ? " Flavor" : "")}";
            bar.value = bar.maxValue;
        }

        //player of same or higher level than this?
        if (Level > GameManager.Instance.Level)
		{
			text.text += $" (Lv. {Level})";
			button.interactable = false;
			icon.sprite = GameManager.Instance.lockSprite;
		}
		else 
		{
            button.interactable = true;
            icon.sprite = GameManager.Instance.searchSprite;
        }
    }

	public void OnClickResearch()
	{
		if (!GlobalVar.Instance.RemoveCurrency(Cost)) 
		{
			UIManager.Instance.OpenNotEnoughUI();
			return;
		}

		//start researching
		Timer.CustomData data = new Timer.CustomData(){ _string = Name };
		Timer timer = TimerManager.CreateTimer(Name, ResearchTime, data);
		timer.OnTickAction += OnTick;
		timer.OnCompleteAction += OnComplete;
		myTimer = timer;
		
		//show on UI as well.
		bar.value = 0f;
		text.text = $"[Researching {(int)bar.value}%] {Name}{(isFlavor ? " Flavor" : "")}";
	}
	
	public void OnTick(Timer.CustomData data, int time)
	{
		bar.value = (1 - ((float)time / (float)myTimer.maxTime)) * 100.0f;
		text.text = $"[Researching {(int)bar.value}%] {Name}{(isFlavor ? " Flavor" : "")}";
	}
	
	public void OnComplete(Timer.CustomData data)
	{
		UIManager.Instance.shop.ResearchedItem(ShopItemName);
		text.text = $"[Researched] {Name}{(isFlavor ? " Flavor" : "")}";
		bar.value = 100f;

		//achievements:
		UIManager.Instance.achievementManager.ProgressAchievement("Complete 30 researches");
        UIManager.Instance.achievementManager.ProgressAchievement("Complete 10 researches");

		PlayerPrefs.SetInt("RES_"+Name, 1);
    }
}
