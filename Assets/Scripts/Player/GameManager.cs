using UnityEngine;
using System.Collections;

//manages most stuff of game.
public class GameManager : MonoBehaviour
{
	#region SINGLETON
	
	public static GameManager Instance;
	
	private void Awake()
	{
		if(Application.platform == RuntimePlatform.Android)
			timeScale = 1f;
		
		Instance = this;
		
		Application.targetFrameRate = targetFPS;
		Time.timeScale = timeScale;
		
		GlobalVar.Instance.currentXP = 0;
		GlobalVar.Instance.nextXP = 2;

		if (DebugTime) 
			BaseTime = 5; //for testing game it should be very small delays
		
		if(DebugGems)
			GlobalVar.Instance.SetGems(100000);
	}
	
	#endregion
	
	public bool DebugTime = false; //starts game in debug mode (adds 100,000 gems too)
	public bool DebugGems = false;
	
	[Min(1f), Space] public float timeScale = 1f;
	public int targetFPS = 45;

	[Space]
	public int BaseTime = 120; //this will be added everywhere!
	
	[Space]
	public Playershop shop;
	public UIManager ui;
	public PerksManager perksManager;
	public NPCSpawner npcSpawner;

	public Sprite lockSprite, searchSprite;
	
	[Space]
	public int maxBoxes = 64;
	public int Level = 1;
	
	private IEnumerator Start()
	{
		yield return new WaitForSeconds(.25f);
		TutorialManager.Instance.OnGameStart();

        ui.taskFill.minValue = GlobalVar.Instance.currentXP;
        ui.taskFill.maxValue = GlobalVar.Instance.nextXP;
    }
	
	//this calculates and removes X number of boxes from shop!
	public int GetNumberOfBoxesPerShopper()
	{
		int n = Mathf.RoundToInt(((float)maxBoxes / (float)shop.itemsMax));
		if(n <= 0) 
			n = 1;
		return n;
	}
	
	public void LevelUp()
	{
		Level++;
		shop.TryUnlockShopAreas();

		GlobalVar.Instance.currentXP = 0;
		GlobalVar.Instance.nextXP = (int)(6f * Level);
		Invoke(nameof(NextLevelUp), 1f);

		//achievements!
		if (Level == 10) 
		{
            ui.achievementManager.ProgressAchievement("Reach level 10");
        }

		if (Level == 20) 
		{
			ui.achievementManager.ProgressAchievement("Reach level 20");
		}

	}
	
	private void NextLevelUp()
	{
		ui.levelText.text = "You have reached Level " + Level + "!";
		ui.UpdateUI();
		ui.OpenLevelUpUI();
		
		ui.taskFill.minValue = GlobalVar.Instance.currentXP;
		ui.taskFill.maxValue = GlobalVar.Instance.nextXP;
	}
	
	private string GetSortedTimeFromSeconds(int time)
	{
		int totalSec = Mathf.FloorToInt((float)time);
		int hours = totalSec / 3600;
		int minutes = (totalSec % 3600) / 60;
		int seconds = totalSec % 60;

		string result = "";

		if (hours > 0) result += $"{hours}h";
		if (minutes > 0 || hours > 0) result += $" {minutes}m";
		result += $" {seconds}s";

		//returns in format: 1h 2m 5s
		return result.Trim();
	}
	
	public void Refill(int index)
	{
		//do not fill at all if backrooms are empty!
		if(shop.backRoomsAmount <= 0) {
			ui.OpenEmptyUI();	
			return;
		}
		
		string timerName = "Unknown_Timer";
		int time = BaseTime;
		
		if(index == 0) { 
			timerName = "G_Timer"; 
			shop.refillingGroceries = true;
		}
		if(index == 1) { 
			timerName = "P_Timer"; 
			shop.refillingPharmacy = true;
		}
		if(index == 2) { 
			timerName = "I_Timer"; 
			shop.refillingIceCream = true;
		}
		if(index == 3) { 
			timerName = "D_Timer"; 
			shop.refillingDrinks = true;
		}
        if (index == 3)
        {
            timerName = "V_Timer";
            shop.refillingVaccines = true;
        }

        //if timer exists means the player wants to refill using gems!
        Timer oldTimer = TimerManager.GetTimerNamed(timerName);
        if (oldTimer != null)
        {
            UIManager.Instance.OpenTimerBuyUI(oldTimer);
            return;
        }

        //fill delayed based on timing.
        Timer timer = TimerManager.CreateTimer(timerName, time, new Timer.CustomData(){ _int = index });
		timer.OnTickAction += OnRefillTimerMoved;
		timer.OnCompleteAction += OnRefillTimerCompleted;

		//show on UI as soon player presses fill.
        if (index == 0) ui.groceryText.text = GetSortedTimeFromSeconds(time);
        if (index == 1) ui.pharmacyText.text = GetSortedTimeFromSeconds(time);
        if (index == 2) ui.iceCreamText.text = GetSortedTimeFromSeconds(time);
        if (index == 3) ui.drinksText.text = GetSortedTimeFromSeconds(time);
        if (index == 4) ui.vaccineText.text = GetSortedTimeFromSeconds(time);

        ui.UpdateUI();
	}
	
	private void OnRefillTimerMoved(Timer.CustomData data, int remainingTime)
	{
		//show remaining time on UI:
		if (data._int == 0) ui.groceryText.text = GetSortedTimeFromSeconds(remainingTime);
		if(data._int == 1) ui.pharmacyText.text = GetSortedTimeFromSeconds(remainingTime);
		if(data._int == 2) ui.iceCreamText.text = GetSortedTimeFromSeconds(remainingTime);
		if(data._int == 3) ui.drinksText.text = GetSortedTimeFromSeconds(remainingTime);
		if (data._int == 4) ui.vaccineText.text = GetSortedTimeFromSeconds(remainingTime);
	}
	
	private void OnRefillTimerCompleted(Timer.CustomData data)
	{
		//remove from backrooms
		shop.backRoomsAmount--;
		TutorialManager.Instance.OnRegionRefilled();
		
		if(data._int == 0) { 
			shop.groceriesAmount = shop.itemsMax; 
			shop.refillingGroceries = false;
			ui.groceryText.text = "";
			shop.shopCrate.FillUp();
		}
		if(data._int == 1) { 
			shop.pharmacyAmount = shop.itemsMax; 
			shop.refillingPharmacy = false;
			ui.pharmacyText.text = "";
		}
		if(data._int == 2) { 
			shop.iceCreamAmount = shop.itemsMax; 
			shop.refillingIceCream = false;
			ui.iceCreamText.text = "";
		}
		if(data._int == 3) { 
			shop.drinksAmount = shop.itemsMax;
			shop.refillingDrinks = false;
			ui.drinksText.text = "";
		}
        if (data._int == 3)
        {
            shop.vaccineAmount = shop.itemsMax;
            shop.refillingVaccines = false;
            ui.vaccineText.text = "";
        }
        ui.UpdateUI();
	}

	private int lastShopperIndex = 0; //will run up-to *int.Max*
	public int GetNewShopperId()
	{
		lastShopperIndex++;
		return lastShopperIndex;
	}
}
