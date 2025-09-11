using UnityEngine;
using System.Collections;

//manages most stuff of game.
public class GameManager : MonoBehaviour
{
	#region SINGLETON
	
	public static GameManager Instance;
	
	private void Awake()
	{
		if(Application.platform == RuntimePlatform.Android) {
			timeScale = 1f;
			NoGameLoad = false;
		}
		
		Instance = this;
		
		Application.targetFrameRate = targetFPS;
		Time.timeScale = timeScale;
		
		GlobalVar.Instance.nextXp = 250;

		if (DebugTime) 
			BaseTime = 5; //for testing game it should be very small delays
		
		if(DebugGems)
			GlobalVar.Instance.SetGems(100000);
	}
	
	#endregion
	
	public bool DebugTime = false; //starts game in debug mode (adds 100,000 gems too)
	public bool DebugGems = false;
	public bool NoGameLoad;
	
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
	public CreatableBuildings creatableBuildings;

	public Sprite lockSprite, searchSprite;

	public ResearchBar[] ResearchBars;
	
	[Space]
	public int maxBoxes = 64;
	public int Level = 1;

	private PlayerData cachedPlayerData;
	
	private IEnumerator Start()
	{
		yield return new WaitForSecondsRealtime(.25f);

        ui.taskFill.minValue = GlobalVar.Instance.currentXP;
        ui.taskFill.maxValue = GlobalVar.Instance.nextXp;

        yield return new WaitForSecondsRealtime(0.5f);
		if(!NoGameLoad)
			LoadPlayer();
		TryEnableExcalmPoint();
    }

    private void OnApplicationQuit()
    {
        SavePlayer();
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
		float xp = 300 * Mathf.Exp(levelExponent * Level);
		GlobalVar.Instance.nextXp = Mathf.Round(xp / 100f) * 100.0f;
		Invoke(nameof(NextLevelUp), 1f);
		TryEnableExcalmPoint();

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

	public void TryEnableExcalmPoint()
	{
		bool researchNew = false;
		//show player can now build or research something new... maybe!
		for(int i = 0; i < ResearchBars.Length; i++) {
			if(ResearchBars[i].UnlockAtCurrentLevel()) 
				researchNew = true;
		}

		//enable excalmation points on buttons showing they can research/build something
		ui.newResearch.SetActive(researchNew);
		ui.newBuild.SetActive(creatableBuildings.CanBuildSomethingThisLevel());
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

	public void SavePlayer()
	{
		PlayerData data = new PlayerData()
		{
			remainVaccine = shop.vaccineAmount, 
			remainBackrooms = shop.backRoomsAmount, 
			remainIcecream = shop.iceCreamAmount, 
			remainGroceries = shop.groceriesAmount,
			remainPharmacy = shop.pharmacyAmount,
			remainDrinks = shop.drinksAmount,

			remainDoubleTime = TimerManager.GetRemainingTime("DOUBLE_TIME"),
			remainDoubleMoney = TimerManager.GetRemainingTime("DOUBLE_MONEY"),
			remainGuard = TimerManager.GetRemainingTime("GUARD"),
			remainHelper = TimerManager.GetRemainingTime("HELPER"),

			currentXp = (int)GlobalVar.Instance.currentXP,
			maxXp = (int)GlobalVar.Instance.nextXp,
			lastXp = (int)ui.taskFill.minValue,
			Level = Level,
			Cash = GlobalVar.Instance.Currency,
			Gems = GlobalVar.Instance.Gems,
			TutorComplete = TutorialManager.Instance.IsTutorialComplete,
		};

		data.AchivementsCollected = new bool[ui.achievementManager.achievementUIOS.Length];
		data.AchivementsProgress = new int[ui.achievementManager.achievementUIOS.Length];
		for(int i = 0; i < data.AchivementsCollected.Length;i++)
		{
			data.AchivementsCollected[i] = ui.achievementManager.achievementUIOS[i].Collected;
			data.AchivementsProgress[i] = ui.achievementManager.achievementUIOS[i].currentValue;
		}

		data.ResearchesComplete = new bool[ResearchBars.Length];
		data.researchRemainTimes = new float[ResearchBars.Length];
		for(int i = 0; i < data.researchRemainTimes.Length;i++)
		{
			data.ResearchesComplete[i] = ResearchBars[i].ResearchComplete;

			if(ResearchBars[i].Researching)
				data.researchRemainTimes[i] = ResearchBars[i].myTimer.remainingTime;
		}

		data.CompletedBuildings = new bool[creatableBuildings.Buildings.Length];
		data.BuildingUpdateIndexes = new int[creatableBuildings.Buildings.Length];
		data.BuildingRemainTime = new float[creatableBuildings.Buildings.Length];
		for(int i = 0; i < data.BuildingUpdateIndexes.Length;i++)
		{
			data.CompletedBuildings[i] = creatableBuildings.Buildings[i].IsBuilt;
			data.BuildingUpdateIndexes[i] = creatableBuildings.Buildings[i].UpgradesDone;
			
			if(creatableBuildings.Buildings[i].IsBuilding)
				data.BuildingRemainTime[i] = TimerManager.GetRemainingTime(creatableBuildings.Buildings[i].Name);
		}

		DataSerializer.SavePlayer(data);
	}

	public void LoadPlayer()
	{
		cachedPlayerData = DataSerializer.LoadData();

		if(cachedPlayerData == null)
			return;

		shop.vaccineAmount = cachedPlayerData.remainVaccine; 
		shop.backRoomsAmount= cachedPlayerData.remainBackrooms; 
		shop.iceCreamAmount= cachedPlayerData.remainIcecream; 
		shop.groceriesAmount= cachedPlayerData.remainGroceries;
		shop.pharmacyAmount= cachedPlayerData.remainPharmacy;
		shop.drinksAmount= cachedPlayerData.remainVaccine;

		TutorialManager.Instance.StartTutorial(cachedPlayerData.TutorComplete);

		GlobalVar.Instance.SetCurrency(cachedPlayerData.Cash);
		GlobalVar.Instance.SetGems(cachedPlayerData.Gems);

		shop.shopCrate.RemoveBoxes((shop.itemsMax - cachedPlayerData.remainGroceries) * GetNumberOfBoxesPerShopper());
		perksManager.OnLoad(cachedPlayerData);

		GlobalVar.Instance.nextXp = cachedPlayerData.maxXp;
		GlobalVar.Instance.SetXp(cachedPlayerData.currentXp);
		ui.taskFill.value = cachedPlayerData.currentXp;
		ui.taskFill.minValue = cachedPlayerData.lastXp;
        ui.taskFill.maxValue = GlobalVar.Instance.nextXp;
		
		Level = cachedPlayerData.Level;

		for(int i = 0; i < ui.achievementManager.achievementUIOS.Length;i++)
		{
			ui.achievementManager.achievementUIOS[i].UpdateSelf(cachedPlayerData.AchivementsCollected[i], cachedPlayerData.AchivementsProgress[i]);
		}

		for(int i = 0; i < ResearchBars.Length; i++)
		{
			ResearchBars[i].OnLoaded(cachedPlayerData.ResearchesComplete[i], (int)cachedPlayerData.researchRemainTimes[i]);
		}

		for(int i = 0; i < creatableBuildings.Buildings.Length; i++)
		{
			creatableBuildings.OnLoad(i, (int)cachedPlayerData.BuildingRemainTime[i], cachedPlayerData.BuildingUpdateIndexes[i], cachedPlayerData.CompletedBuildings[i]);
		}

		ui.UpdateUI();
	}
}
