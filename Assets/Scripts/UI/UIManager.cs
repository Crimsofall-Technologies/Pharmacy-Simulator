using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[DefaultExecutionOrder(0)]
public class UIManager : MonoBehaviour
{
	#region SINGLETON

	public static UIManager Instance;
	
	private void Awake() {
		Instance = this;
	}

#endregion
	
	public Text currencyText, gemsText, backRoomsAmountText, taskText, taskGetUIText, levelXpText, buyWithGemsText;
	public GameObject bubbleObj;
	public Transform taskParent;
	public Camera cam;

	public GameObject xpFloat;
	public Transform floatUIParent;
	
	public GameObject pauseMenu, settingMenu, shopScreen, warehouseScreen, perksUI, notEnoughGO, timerUIPrefab, shoppingListUI, taskUI, achieveUI;
	public GameObject shoppingListUIPrefab, newResearch, newBuild;
	public Transform timerUIArea, shoppingList;
	public Animator pauseMenuAnimator, settingAnimator, emptyAnimator, shopAnimator, backRoomFillAnimator, levelUpAnimator, perksAnimator, buildAnimator, notEnoughAnimator, shoppingListAnimator;
	public Animator researchUIAnimator, taskAnimator, achieveAnimator, achieveCompAnimator, timerBuyAnimator;
	
	public Slider taskFill;
	public float fillSpeed;
	
	public FillManager groceriesFill, iceCreamFill, pharmacyFill, drinksFill, backRoomsFill, vaccineFill;
	public GameObject groceryButton, iceCreamButton, pharmacyButton, drinksButton, backRoomsButton, vaccineButton, emptyUI, shopUI, backRoomGO, levelUpGO, buildGO, researchUI, achieveCompleteUI, timerBuyUI;
	public Text groceryText, iceCreamText, pharmacyText, drinksText, levelText, backRoomsText, vaccineText, achieveCompText, timerBuyText;
	public Playershop shop;
	public Sprite[] ShopperTypeSprites;
	public Sprite moneySpriteIcon, thiefSpriteIcon;
	public Warehouse wareHouse;
	public CameraManager camManager;
	public CreatableBuildings Buildings;
	public TaskList taskList;
	public AchievementManager achievementManager;

	[Space]
	public Sprite gemSprite;
	public Sprite refillSprite;
	public TutorialManager tutorialManager;
	public Image iceCreamIcon, drinksIcon, pharmacyIcon, groceryIcon, vaccineIcon, backRoomIcon;

    private List<TimerUI> TimerUIs = new List<TimerUI>();
	private ResearchBar[] researchBars;
	
	private void Start() 
	{
		groceryText.text = "";
		iceCreamText.text = "";
		pharmacyText.text = "";
		drinksText.text = "";

		researchBars = researchUI.GetComponentsInChildren<ResearchBar>(true);
		newResearch.SetActive(false);
		newBuild.SetActive(false);

		UpdateUI();
		StartCoroutine(UpdateBarsSmoothly());
	}

    private void Update()
    {
		if (Input.GetKeyDown(KeyCode.Space)) { GameManager.Instance.Level = 10; UpdateUI(); }
		if (Input.GetKeyDown(KeyCode.V)) { GlobalVar.Instance.AddGems(100); GlobalVar.Instance.AddCurrency(100); }

        if (Input.GetKeyDown(KeyCode.N)) {
            GlobalVar.Instance.AddXP(100);
			UpdateUI();
        }
    }

    public void UpdateUI()
	{
		currencyText.text = GlobalVar.Instance.Currency.ToString();
		gemsText.text = GlobalVar.Instance.Gems.ToString();
        levelXpText.text = "Lv. " + GameManager.Instance.Level;
        taskText.text = $"XP: {GlobalVar.Instance.currentXP}/{GlobalVar.Instance.nextXp}";

        //activate stuffs
        float Half = shop.itemsMax * 0.5f;
		pharmacyFill.gameObject.SetActive(shop.pharmacyAmount <= Half);
		drinksFill.gameObject.SetActive(shop.drinksAmount <= Half);
		groceriesFill.gameObject.SetActive(shop.groceriesAmount <= Half);
		iceCreamFill.gameObject.SetActive(shop.iceCreamAmount <= Half);
        vaccineFill.gameObject.SetActive(shop.vaccineAmount <= Half);

        pharmacyButton.SetActive(shop.pharmacyAmount <= 0);
		drinksButton.SetActive(shop.drinksAmount <= 0);
		groceryButton.SetActive(shop.groceriesAmount <= 0);
		iceCreamButton.SetActive(shop.iceCreamAmount <= 0);
        vaccineButton.SetActive(shop.vaccineAmount <= 0);

		iceCreamIcon.sprite = shop.refillingIceCream ? gemSprite : refillSprite;
        groceryIcon.sprite = shop.refillingGroceries ? gemSprite : refillSprite;
        vaccineIcon.sprite = shop.refillingVaccines ? gemSprite : refillSprite;
        drinksIcon.sprite = shop.refillingDrinks ? gemSprite : refillSprite;
        pharmacyIcon.sprite = shop.refillingPharmacy ? gemSprite : refillSprite;
		backRoomIcon.sprite = shop.refillingBackrooms ? gemSprite : refillSprite;

        if (tutorialManager.TutorialRunning && !tutorialManager.d && groceryButton.gameObject.activeSelf)
		{
			tutorialManager.OnRefillRegionShow();
		}

        pharmacyText.gameObject.SetActive(shop.pharmacyAmount <= 0);
		drinksText.gameObject.SetActive(shop.drinksAmount <= 0);
		groceryText.gameObject.SetActive(shop.groceriesAmount <= 0);
		iceCreamText.gameObject.SetActive(shop.iceCreamAmount <= 0);
		vaccineText.gameObject.SetActive(shop.vaccineAmount <= 0);
		
		//Backroom filling
		bool less30Remaining = shop.backRoomsAmount<=(shop.backRoomsAmount*0.3f);
		backRoomsFill.gameObject.SetActive(less30Remaining && !shop.refillingBackrooms);
		backRoomsFill.UpdateSelf((float)shop.backRoomsAmount, (float)shop.maxBackroomsAmount);
		backRoomsButton.gameObject.SetActive(shop.backRoomsAmount <= 0 && !shop.refillingBackrooms);

        if (tutorialManager.TutorialRunning && !tutorialManager.i && backRoomsButton.gameObject.activeSelf)
        {
            tutorialManager.OnRefillRegionBackrooms();
        }

        //update areas
        pharmacyFill.UpdateSelf(shop.pharmacyAmount,(float)shop.itemsMax);
		drinksFill.UpdateSelf(shop.drinksAmount,(float)shop.itemsMax);
		groceriesFill.UpdateSelf(shop.groceriesAmount,(float)shop.itemsMax);
		iceCreamFill.UpdateSelf(shop.iceCreamAmount,(float)shop.itemsMax);
		vaccineFill.UpdateSelf(shop.vaccineAmount, (float)shop.itemsMax);
	}
	
	private IEnumerator UpdateBarsSmoothly()
	{
		while(true)
		{
			yield return new WaitForSeconds(0.002f);
			if(taskFill.value != GlobalVar.Instance.currentXP)
			{
				if(taskFill.value < GlobalVar.Instance.currentXP)
					taskFill.value += 5;
				else
					taskFill.value -= 10;
					
				if(taskFill.value > GlobalVar.Instance.currentXP) 
					taskFill.value = GlobalVar.Instance.currentXP;
				if(taskFill.value < 0f) 
					taskFill.value = 0f;
			}
		}
	}
	
	public void Refill(int index)
	{
		GameManager.Instance.Refill(index);
		UpdateUI();
	}

	public void CreateBubble(Shopper shopper, bool atCashier = false, bool isThief=false)
	{
		if (atCashier) 
		{
            Bubble b = Instantiate(bubbleObj, taskParent).GetComponent<Bubble>();
            b.Init(new ShopperType[0], shopper, moneySpriteIcon, atCashier);
            shopper.OnBubbleCreated(b);
			return;
        }

        if (isThief)
        {
            Bubble b = Instantiate(bubbleObj, taskParent).GetComponent<Bubble>();
            b.Init(new ShopperType[0], shopper, thiefSpriteIcon, false, true);
            shopper.OnBubbleCreated(b);
            return;
        }

        /*GameObject go = bubble1;
		if (shopper.Types.Length == 2) go = bubble2;
		if (shopper.Types.Length == 3) go = bubble3;*/

		Bubble bubble = Instantiate(bubbleObj, taskParent).GetComponent<Bubble>();
		bubble.gameObject.SetActive(false);
		/*List<Sprite> spr = new List<Sprite>();
		for (int i = 0; i < shopper.Types.Length; i++)
		{
			spr.Add(ShopperTypeSprites[(int)shopper.Types[i]]);
		}*/
		bubble.Init(shopper.Types, shopper, ShopperTypeSprites[(int)shopper.Types[shopper.currentIndex]], atCashier);
		shopper.OnBubbleCreated(bubble);
	}

	public void CreateXpFloat(int xp) {
		GameObject g = Instantiate(xpFloat, floatUIParent);
		g.GetComponent<Text>().text = "+"+xp;
		Destroy(g, 5f);
	}
	
	public void Resume()
	{
		pauseMenuAnimator.SetBool("Open", false);
		StartCoroutine(DelayedCloseUI(pauseMenu));
	}
	
	public void Pause()
	{
		Time.timeScale = 0.0f;
		pauseMenu.SetActive(true);
		pauseMenuAnimator.SetBool("Open", true);
	}
	
	public void OpenSettingsUI()
	{
		settingMenu.SetActive(true);
		settingAnimator.SetBool("Open", true);
	}
	
	public void Back()
	{
		settingAnimator.SetBool("Open", false);
		StartCoroutine(DelayedCloseUI(settingMenu));
	}
	
	private IEnumerator DelayedCloseUI(GameObject UI)
	{
		yield return new WaitForSecondsRealtime(0.55f);

		UI.SetActive(false);
		if(UI != notEnoughGO && !pauseMenu.activeSelf)
			Time.timeScale = GameManager.Instance.timeScale;

		if (UI == levelUpGO && TutorialManager.Instance.TutorialRunning) 
		{
            TutorialManager.Instance.OnLevelUp();
        }
	}
	
	public void Exit()
	{
		Debug.Log("Quitting...");

#if UNITY_EDITOR
		EditorApplication.ExitPlaymode();
#endif
		Application.Quit();
	}

	public void CreateTimerUI(Timer timer, Transform T, int time, Sprite spr)
	{
		TimerUI ui = Instantiate(timerUIPrefab, timerUIArea).GetComponent<TimerUI>();

		if (spr != null) 
			ui.Icon.sprite = spr;

		ui.Init(T, time, camManager.CamType, timer);
		timer.OnTickAction += ui.UpdateSelf;
		timer.myUIO = ui.gameObject;
		
		TimerUIs.Add(ui);
	}

	public void UpdateTimersOnCameraChange(CameraType camType) 
	{
		for (int i = 0; i < TimerUIs.Count; i++)
		{
			TimerUIs[i].SetActiveUnactive(camType);
		}
	}
	
	#region OTHER_WINDOWS
	
	public void OpenBuildUI()
	{
		Time.timeScale = 0f;
		buildGO.SetActive(true);
		buildAnimator.SetBool("Open", true);

		newBuild.SetActive(false);
		shop.creatableBuilding.UpdateUI();
	}
	
	public void CloseBuildUI()
	{
		buildAnimator.SetBool("Open", false);
		StartCoroutine(DelayedCloseUI(buildGO));
	}

	private Timer timer;
	private int constantGemsAmount;
	public void OpenTimerBuyUI(Timer _timer, int _constantGemsAmount = 0) 
	{
		Time.timeScale = 0f; //pause.
		timer = _timer;
		constantGemsAmount = _constantGemsAmount;

		if(constantGemsAmount <= 0)
			timerBuyText.text = "Cost: "+Mathf.RoundToInt(timer.remainingTime * 1.5f) + "";
		else
			timerBuyText.text = "Cost: "+constantGemsAmount + "";
		timerBuyUI.SetActive(true);
		timerBuyAnimator.SetBool("Open", true);
	}

	public void BuyAndCompleteTimer()
	{
		int cost;
		if(constantGemsAmount <= 0)
			cost = Mathf.RoundToInt(timer.remainingTime * 1.5f);
		else
			cost = constantGemsAmount;
		if (GlobalVar.Instance.RemoveGems(cost))
		{
			timer.OverrideComplete();
			timerBuyAnimator.SetBool("Open", false);
			StartCoroutine(DelayedCloseUI(timerBuyUI));
		}
		else 
		{
			OpenShopUI();
			//OpenNotEnoughUI();
		}

		UpdateUI();
    }

	public void CloseTimerBuyUI() 
	{
		timer = null;
        timerBuyAnimator.SetBool("Open", false);
		StartCoroutine(DelayedCloseUI(timerBuyUI));
    }

    public void OpenAchieveUI(string achieveName)
    {
        Time.timeScale = 0f;
        achieveCompleteUI.SetActive(true);
        achieveCompAnimator.SetBool("Open", true);

		achieveCompText.text = "You unlocked achivement:\n"+achieveName+"!";
    }

    public void CloseAchieveUI()
    {
        achieveCompAnimator.SetBool("Open", false);
        StartCoroutine(DelayedCloseUI(achieveCompleteUI));
    }

    public void OpenAchievements() 
	{
		achieveUI.SetActive(true);
		achieveAnimator.SetBool("Open", true);
	}

	public void CloseAchievements() 
	{
        achieveAnimator.SetBool("Open", false);
		StartCoroutine(DelayedCloseUI(achieveUI));
    }

	public void OpenTaskUI() 
	{
		Time.timeScale = 0f; //pause game for player!
		taskUI.SetActive(true);
		taskAnimator.SetBool("Open", true);
	}

	public void CloseTaskUI() 
	{
		StartCoroutine(DelayedCloseUI(taskUI));
        taskAnimator.SetBool("Open", false);
    }
	
	public void OpenEmptyUI()
	{
		Time.timeScale = 0f;
		emptyUI.SetActive(true);
		emptyAnimator.SetBool("Open", true);
	}
	
	public void CloseEmptyUI()
	{
		emptyAnimator.SetBool("Open", false);
		StartCoroutine(DelayedCloseUI(emptyUI));
	}
	
	public void OpenShopUI()
	{
		Time.timeScale = 0f;
		shopUI.SetActive(true);
		shopAnimator.SetBool("Open", true);
	}
	
	public void CloseShopUI()
	{
		shopAnimator.SetBool("Open", false);
		StartCoroutine(DelayedCloseUI(shopUI));
	}
	
	public void OpenBackroomsUI()
	{
		Time.timeScale = 0f;
		backRoomGO.SetActive(true);
		backRoomFillAnimator.SetBool("Open", true);
		
		//calculate the amount of money player needs to fill the backrooms thing!
		backRoomsAmountText.text = "Funds Needed to fill: $" + shop.GetBackRoomFillCost().ToString();
	}
	
	public void CloseBackroomsUI()
	{
		backRoomFillAnimator.SetBool("Open", false);
		StartCoroutine(DelayedCloseUI(backRoomGO));
	}
	
	public void OpenLevelUpUI()
	{
		Time.timeScale = 0f;
		levelUpGO.SetActive(true);
		levelUpAnimator.SetBool("Open", true);
	}
	
	public void CloseLevelUPUI()
	{
		levelUpAnimator.SetBool("Open", false);
		StartCoroutine(DelayedCloseUI(levelUpGO));
	}
	
	public void OpenPerkUI()
	{
		Time.timeScale = 0f;
		perksUI.SetActive(true);
		GameManager.Instance.perksManager.UpdateButtons();
		perksAnimator.SetBool("Open", true);
	}
	
	public void ClosePerkUI()
	{
		perksAnimator.SetBool("Open", false);
		StartCoroutine(DelayedCloseUI(perksUI));
	}

	private string NotEnoughID = "";
	private int buyWithGemsCost = 0;
	public void OpenNotEnoughUI(string type, int originalCost) 
	{
		NotEnoughID = type;

        Time.timeScale = 0f;
        notEnoughGO.SetActive(true);
        notEnoughAnimator.SetBool("Open", true);

		//pre-calculate cost in gems!
		if(originalCost > 0)
		{
			buyWithGemsCost = Mathf.RoundToInt(originalCost * 0.75f);
			buyWithGemsText.text = "Buy with " + buyWithGemsCost + " Gems!";
		}
    }

	public void CloseNotEnoughUI()
	{
		buyWithGemsCost = 0;
		NotEnoughID = "";
		notEnoughAnimator.SetBool("Open", false);
		StartCoroutine(DelayedCloseUI(notEnoughGO));
	}
	
	public void OpenResearchUI()
	{
		Time.timeScale = 0f;
		researchUI.SetActive(true);
		researchUIAnimator.SetBool("Open", true);
		newResearch.SetActive(false);
		
		//update all bars depending on player level!
		for (int i = 0; i < researchBars.Length; i++)
		{
			researchBars[i].UpdateSelf();
		}
	}
	
	public void CloseResearchUI()
	{
		researchUIAnimator.SetBool("Open", false);
		StartCoroutine(DelayedCloseUI(researchUI));
	}

	public void OpenShoppingList(Shopper shopper) 
	{
		Time.timeScale = 0f;
		shoppingListUI.SetActive(true);
		shoppingListAnimator.SetBool("Open", true);

        //Update depending on NPC selected
        for (int i = 0; i < shopper.Order.Count; i++)
        {
			GameObject g = Instantiate(shoppingListUIPrefab, shoppingList);
			g.GetComponentInChildren<Text>().text = $"{shopper.Order[i].Name}, Cost: {shopper.Order[i].Cost}";
			g.GetComponent<Image>().sprite = ShopperTypeSprites[(int)shopper.Order[i].shopperType];
        }
    }

    public void CloseShoppingList()
    {
		shoppingListAnimator.SetBool("Open", false);
        StartCoroutine(DelayedCloseUI(shoppingListUI));

		for (int i = 0; i < shoppingList.childCount; i++)
		{
			Destroy(shoppingList.GetChild(i).gameObject);
		}
    }

    #endregion

    #region BUTTONS

    public void Add_Gems()
	{
		OpenShopUI();
	}
	
	public void Add_Money()
	{
		OpenShopUI();
	}

	//buy stuff with gems if there is no currency avail
	public void BuyWithGems()
	{
		bool openShop=false;
		if(NotEnoughID == "Warehouse")
		{
			//fill the backrooms
			if(GlobalVar.Instance.RemoveGems(buyWithGemsCost))
			{
				//really fill it.
				wareHouse.SendStuffToShop();
			}
			else openShop = true;
		}

		if(NotEnoughID.Contains("Research"))
		{
			if(GlobalVar.Instance.RemoveGems(buyWithGemsCost))
			{
				string Name = NotEnoughID.Split(':')[1];
				CloseResearchUI();
				for(int i = 0; i < researchBars.Length; i++)
				{
					if(researchBars[i].Name == Name)
					{
						researchBars[i].StartResearch();
						break;
					}
				}
				UpdateUI();
			}
			else openShop = true;
		}

		if(NotEnoughID.Contains("BuildBuilding"))
		{
			//build the building with gems
			if(GlobalVar.Instance.RemoveGems(buyWithGemsCost))
			{
				string Name = NotEnoughID.Split(':')[1];
				CloseBuildUI();
				Buildings.Build(Name);

				//for backrooms also increase so there will be 2 & 3 trucks respectively coming for each refill!
				if (Name == "Backrooms 10") { wareHouse.TimesRefill = 2; shop.maxBackroomsAmount = 10; }
				if (Name == "Backrooms 15") { wareHouse.TimesRefill = 3; shop.maxBackroomsAmount = 15; }
				UpdateUI();
			}
			else openShop = true;
		}

		CloseNotEnoughUI();
		if(openShop) OpenShopUI();
	}
	
	public void GoToWarehouse()
	{
		if(shop.IsFocused) {
			wareHouse.PlayerFocus(true);
			shop.PlayerFocus(false);
			shopScreen.SetActive(false);
			warehouseScreen.SetActive(true);
		}
		else {
			shop.PlayerFocus(true);
			wareHouse.PlayerFocus(false);
			shopScreen.SetActive(true);
			warehouseScreen.SetActive(false);
		}
	}
	
	public void TryBackroomFill()
	{
		int cost = (int)shop.GetBackRoomFillCost();
		
		if(cost <= 0) //it's full already?
			return;
		
		if(GlobalVar.Instance.RemoveCurrency(cost))
		{
			//really fill it.
			wareHouse.SendStuffToShop();
		}
		else
		{
			OpenNotEnoughUI("Warehouse", cost);
        }
		
		CloseBackroomsUI();
	}

	public void BuildBuilding(string Name)
	{
		int cost = Buildings.GetBuildCost(Name);

		if (cost <= 0)
		{
			return;
		}

		if (GlobalVar.Instance.RemoveCurrency(cost))
		{
			CloseBuildUI();
			Buildings.Build(Name);

			//for backrooms also increase so there will be 2 & 3 trucks respectively coming for each refill!
			if (Name == "Backrooms 10") { wareHouse.TimesRefill = 2; shop.maxBackroomsAmount = 10; }
			if (Name == "Backrooms 15") { wareHouse.TimesRefill = 3; shop.maxBackroomsAmount = 15; }
			UpdateUI();
        }
		else
		{
			OpenNotEnoughUI("BuildBuilding:" + Name, cost);
		}
	}
	
	public void SwitchCamera()
	{
		if(camManager.CamType == CameraType.Shop) 
			camManager.SwitchCamera(CameraType.Warehouse);
		else if(camManager.CamType == CameraType.Warehouse) 
			camManager.SwitchCamera(CameraType.Buildings);
		else if(camManager.CamType == CameraType.Buildings) 
			camManager.SwitchCamera(CameraType.Shop);
	}
	
	#endregion
}
