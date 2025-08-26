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
		
		GlobalVar.Instance.nextXp = 500;

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

	public int researchXP = 75, taskXP = 50, shopperXp = 10;
	public float levelExponent = 1f;
	
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
        ui.taskFill.maxValue = GlobalVar.Instance.nextXp;
    }
	
	//this calculates and removes X number of boxes from shop!
	public int GetNumberOfBoxesPerShopper()
	{
		int n = Mathf.RoundToInt((float)maxBoxes / (float)shop.itemsMax);
		if(n <= 0) 
			n = 1;
		return n;
	}
	
	public void LevelUp()
	{
		Level++;

		//GlobalVar.Instance.nextXp = 500 * (1.1f * Level);
		float xp = 500 * Mathf.Exp(levelExponent * Level);
		GlobalVar.Instance.nextXp = Mathf.Round(xp / 100f) * 100.0f;
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
		ui.taskFill.maxValue = GlobalVar.Instance.nextXp;
	}
	
	public void Refill(int index)
	{
		//do not fill at all if backrooms are empty!
		if(shop.backRoomsAmount <= 0) {
			ui.OpenEmptyUI();	
			return;
		}
		
		string timerName = "Unknown_Timer";
		int time = 60; //basically one min!

		//make the time half if double timer is on
		if(perksManager.DoubleSpeed)
			time /= 2;
		
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
        if (index == 0) ui.groceryText.text = StringSimplifier.GetSortedTimeFromSeconds(time);
        if (index == 1) ui.pharmacyText.text = StringSimplifier.GetSortedTimeFromSeconds(time);
        if (index == 2) ui.iceCreamText.text = StringSimplifier.GetSortedTimeFromSeconds(time);
        if (index == 3) ui.drinksText.text = StringSimplifier.GetSortedTimeFromSeconds(time);
        if (index == 4) ui.vaccineText.text = StringSimplifier.GetSortedTimeFromSeconds(time);

        ui.UpdateUI();
	}
	
	private void OnRefillTimerMoved(Timer.CustomData data, int remainingTime)
	{
		//show remaining time on UI:
		if (data._int == 0) ui.groceryText.text = StringSimplifier.GetSortedTimeFromSeconds(remainingTime);
		if(data._int == 1) ui.pharmacyText.text = StringSimplifier.GetSortedTimeFromSeconds(remainingTime);
		if(data._int == 2) ui.iceCreamText.text = StringSimplifier.GetSortedTimeFromSeconds(remainingTime);
		if(data._int == 3) ui.drinksText.text = StringSimplifier.GetSortedTimeFromSeconds(remainingTime);
		if (data._int == 4) ui.vaccineText.text = StringSimplifier.GetSortedTimeFromSeconds(remainingTime);
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
