using UnityEngine;
using UnityEngine.UI;

public class ResearchBar : MonoBehaviour
{
	public string Name;
	public string ShopItemName;
	public bool isFlavor;
	public int Cost;
	[Min(1)] public int Level = 1; //the level at which this research will unlock!

	public Slider bar;
	public Text text;

	private Button button;
	private Image icon;

	public Timer myTimer { get; private set; }
	public bool Researching { get; private set; }
	public bool ResearchComplete { get; private set; }

    private void Start()
	{
        bar.interactable = false;
		transform.GetChild(3).GetComponent<Text>().text = "Cost: " + Cost;
    }

	public void OnLoaded(bool complete, int remainTime)
	{
		ResearchComplete = complete;
		
		//make research progress from where player left off!
		if(remainTime > 0)
		{
			//start researching
			Timer.CustomData data = new Timer.CustomData(){ _string = Name };

			//time will be 120s x 4 = 480s at level 2, 120 x 5 = 600s at level 3
			int time = GameManager.Instance.BaseTime;
			if (Level > 1)
				time = GameManager.Instance.BaseTime * (Level + 2);

			if(GameManager.Instance.perksManager.DoubleSpeed)
				time /= 2;

			Timer timer = TimerManager.CreateTimer(Name, time, data, true);
			timer.OnTickAction += OnTick;
			timer.OnCompleteAction += OnComplete;
			myTimer = timer;
			myTimer.SkipTimeTo(remainTime);

			Researching = true;
			
			//show on UI as well.
			bar.value = 0f;
			text.text = $"[Researching {(int)bar.value}%] {Name}{(isFlavor ? " Flavor" : "")}";

			//update Sprite
			icon.sprite = UIManager.Instance.gemSprite;
		}

		UpdateSelf();
	}

    public void UpdateSelf() 
	{
		if (button == null)
		{
			button = transform.GetChild(2).GetComponent<Button>();
			icon = button.transform.GetChild(0).GetComponent<Image>();
		}

		//not yet done?
		if (!ResearchComplete)
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
            icon.sprite = !Researching ? GameManager.Instance.searchSprite : UIManager.Instance.gemSprite;
        }
    }

	public void OnClickResearch()
	{
		if(ResearchComplete)
			return;

		//clicked next time? Buy timer with gems!
		if (Researching) 
		{
			Timer t = TimerManager.GetTimerNamed(Name);

			if(t!=null)
				UIManager.Instance.OpenTimerBuyUI(t);
			return;
		}

		if (!GlobalVar.Instance.RemoveCurrency(Cost)) 
		{
			UIManager.Instance.OpenNotEnoughUI();
			return;
		}

		//start researching
		Timer.CustomData data = new Timer.CustomData(){ _string = Name };

        //time will be 120s x 4 = 480s at level 2, 120 x 5 = 600s at level 3
        int time = GameManager.Instance.BaseTime;
        if (Level > 1)
            time = GameManager.Instance.BaseTime * (Level + 2);

		if(GameManager.Instance.perksManager.DoubleSpeed)
			time /= 2;

        Timer timer = TimerManager.CreateTimer(Name, time, data, true);
		timer.OnTickAction += OnTick;
		timer.OnCompleteAction += OnComplete;
		myTimer = timer;

		Researching = true;
		
		//show on UI as well.
		bar.value = 0f;
		text.text = $"[Researching {(int)bar.value}%] {Name}{(isFlavor ? " Flavor" : "")}";

		//update Sprite
		icon.sprite = UIManager.Instance.gemSprite;
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

		button.gameObject.SetActive(false);

		//achievements:
		UIManager.Instance.achievementManager.ProgressAchievement("Complete 30 researches");
        UIManager.Instance.achievementManager.ProgressAchievement("Complete 10 researches");

		Researching = false;

		//give EXP (75 per research)
		GlobalVar.Instance.AddXP(GameManager.Instance.researchXP);
		ResearchComplete = true;
    }
}
