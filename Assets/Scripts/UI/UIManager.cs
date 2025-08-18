using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class UIManager : MonoBehaviour
{
	#region SINGLETON

	public static UIManager Instance;
	
	private void Awake() {
		Instance = this;
	}

#endregion
	
	public Text currencyText, gemsText, backRoomsAmountText, taskText, taskGetUIText, levelXpText;
	public GameObject bubble3, bubble2, bubble1;
	public Transform taskParent;
	public Camera cam;
	
	public GameObject pauseMenu, settingMenu, shopScreen, warehouseScreen, perksUI, notEnoughGO, timerUIPrefab, shoppingListUI, taskUI, achieveUI;
	public GameObject shoppingListUIPrefab;
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
		
		UpdateUI();
		StartCoroutine(UpdateBarsSmoothly());
	}

    private void Update()
    {
		if (Input.GetKeyDown(KeyCode.Space)) { GameManager.Instance.Level = 10; UpdateUI(); }
		if (Input.GetKeyDown(KeyCode.V)) { GlobalVar.Instance.AddGems(100); GlobalVar.Instance.AddCurrency(100); }

        if (Input.GetKeyDown(KeyCode.N)) {
            GlobalVar.Instance.currentXP++;

            if (GlobalVar.Instance.currentXP >= GlobalVar.Instance.nextXP)
            {
                GameManager.Instance.LevelUp();
            }
			UpdateUI();
        }
    }

    public void UpdateUI()
	{
		currencyText.text = GlobalVar.Instance.Currency.ToString();
		gemsText.text = GlobalVar.Instance.Gems.ToString();
        levelXpText.text = "Lv. " + GameManager.Instance.Level;
        taskText.text = $"XP: {GlobalVar.Instance.currentXP}/{GlobalVar.Instance.nextXP}";

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

        if (TutorialManager.Instance.TutorialRunning && !TutorialManager.Instance.d && groceryButton.gameObject.activeSelf)
		{
			TutorialManager.Instance.OnRefillRegionShow();
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

        if (TutorialManager.Instance.TutorialRunning && !TutorialManager.Instance.i && backRoomsButton.gameObject.activeSelf)
        {
            TutorialManager.Instance.OnRefillRegionBackrooms();
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
			yield return new WaitForSeconds(0.005f);
			if(taskFill.value != GlobalVar.Instance.currentXP)
			{
				if(taskFill.value < GlobalVar.Instance.currentXP)
					taskFill.value += Time.deltaTime * fillSpeed;
				else
					taskFill.value -= Time.deltaTime * fillSpeed;
					
				if(taskFill.value > GlobalVar.Instance.currentXP) taskFill.value = GlobalVar.Instance.currentXP;
				if(taskFill.value < 0f) taskFill.value = 0f;
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
            Bubble b = Instantiate(bubble1, taskParent).GetComponent<Bubble>();
            b.Init(new ShopperType[0], shopper, new Sprite[1] { moneySpriteIcon }, atCashier);
            shopper.OnBubbleCreated(b);
			return;
        }

        if (isThief)
        {
            Bubble b = Instantiate(bubble1, taskParent).GetComponent<Bubble>();
            b.Init(new ShopperType[0], shopper, new Sprite[1] { thiefSpriteIcon }, false, true);
            shopper.OnBubbleCreated(b);
            return;
        }

        GameObject go = bubble1;
		if (shopper.Types.Length == 2) go = bubble2;
		if (shopper.Types.Length == 3) go = bubble3;

		Bubble bubble = Instantiate(go, taskParent).GetComponent<Bubble>();
		bubble.gameObject.SetActive(false);
		List<Sprite> spr = new List<Sprite>();
		for (int i = 0; i < shopper.Types.Length; i++)
		{
			spr.Add(ShopperTypeSprites[(int)shopper.Types[i]]);
		}
		bubble.Init(shopper.Types, shopper, spr.ToArray(), atCashier);
		shopper.OnBubbleCreated(bubble);
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

		shop.creatableBuilding.UpdateUI();
	}
	
	public void CloseBuildUI()
	{
		buildAnimator.SetBool("Open", false);
		StartCoroutine(DelayedCloseUI(buildGO));
	}

	private Timer timer;
	public void OpenTimerBuyUI(Timer time) 
	{
		Time.timeScale = 0f; //pause.
		timer = time;
		timerBuyText.text = "Cost: "+Mathf.RoundToInt(timer.remainingTime * 1.5f) + "";
		timerBuyUI.SetActive(true);
		timerBuyAnimator.SetBool("Open", true);
	}

	public void BuyAndCompleteTimer()
	{
		int cost = Mathf.RoundToInt(timer.remainingTime * 1.5f);
		if (GlobalVar.Instance.RemoveGems(cost))
		{
			timer.OverrideComplete();
			timerBuyAnimator.SetBool("Open", false);
			StartCoroutine(DelayedCloseUI(timerBuyUI));
		}
		else 
		{
			OpenNotEnoughUI();
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

		achieveCompText.text = "You have completed:\n"+achieveName+"!";
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

	public void OpenNotEnoughUI() 
	{
        Time.timeScale = 0f;
        notEnoughGO.SetActive(true);
        notEnoughAnimator.SetBool("Open", true);
    }

	public void CloseNotEnoughUI()
	{
		notEnoughAnimator.SetBool("Open", false);
		StartCoroutine(DelayedCloseUI(notEnoughGO));
	}
	
	public void OpenResearchUI()
	{
		Time.timeScale = 0f;
		researchUI.SetActive(true);
		researchUIAnimator.SetBool("Open", true);
		
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
			OpenNotEnoughUI();
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
			OpenNotEnoughUI();
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
