using UnityEngine;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class Playershop : MonoBehaviour
{
	[System.Serializable]
	public class ShopPosition
	{
		public ShopperType shopperType;
		public Transform[] positions;
		
		public int[] areasRemaining;
		
		public void Init()
		{
			areasRemaining = new int[positions.Length];
		}
		
		public bool HasEmptyArea()
		{
			for (int i = 0; i < areasRemaining.Length; i++) {
				if(areasRemaining[i] == 0) return true;
			}
			return false;
		}
		
		public Transform GetNPCPosition()
		{
			for (int i = 0; i < 5; i++) //try doing a random for 5 times before giving up.
			{
				int m = Random.Range(0, areasRemaining.Length);
				if(areasRemaining[m] == 0 && positions[m].GetComponent<TargetPointer>().CanStandHere())
				{
					return positions[m];
				}
			}
			
			//fallback - return nearest open one:
			for (int i = 0; i < areasRemaining.Length; i++) 
			{
				if(areasRemaining[i] == 0 && positions[i].GetComponent<TargetPointer>().CanStandHere())
				{
					return positions[i];
				}
			}
			
			//completely fallback (this is possible only if there are no more places to stand around)
			return null;
		}
		
		public void OnAreaOccupationChanged(Transform T, int shopperId)
		{
			for (int i = 0; i < positions.Length; i++) {
				if(positions[i] == T)
				{
					areasRemaining[i] = shopperId;
				}
			}
		}
		
		public int GetAreaOccupiedInt(Transform T)
		{
			for (int i = 0; i < positions.Length; i++) {
				if(positions[i] == T && areasRemaining[i] > 0) //area equals?
				{
					return areasRemaining[i];
				}
			}
			
			//fallback:
			return 0; //just return the area is empty.
		}
	}
	
	public CameraManager camManager;
	public CreatableBuildings creatableBuilding;
	public ShopPosition[] shopPositions;
	public SellableItem[] ThingsSold;
	public FillerMan[] fillerMen;
	public ShopCrate shopCrate;

	public Transform guardSpawnPoint;
	public GameObject guardPrefab;
	
	public float cashierPayDelay;
	public float backRoomFillDelay = 5f;
	public int itemsMax = 100;
	public Animator gateAnimator;
	public WarehouseTruck truck;
	
	public int groceriesAmount, pharmacyAmount, iceCreamAmount, drinksAmount, backRoomsAmount, vaccineAmount;
	public bool refillingPharmacy, refillingGroceries, refillingIceCream, refillingDrinks, refillingBackrooms, refillingVaccines;

	[Space, Header("Unlockable Areas:")]
	public GameObject[] PharmacyAreasToDisable;
    public GameObject[] PharmacyAreasToEnable;
	public int pharmacyUnlockLevel = 2;
	public bool IsPharmacyUnlocked = false;

    [Space]
    public GameObject[] consultationAreasToDisable;
    public GameObject[] consultationAreasToEnable;
    public int consultationUnlockLevel = 5;
	public bool IsConsultationUnlocked = false;
	public bool HasGuard=false;

    public int maxBackroomsAmount { get; set; }
	public bool IsFocused { get; private set; }
	
	private void Awake() 
	{
		PlayerFocus(true);
		maxBackroomsAmount = 5; //as default!
		
		groceriesAmount = itemsMax;
		pharmacyAmount = itemsMax;
		iceCreamAmount = itemsMax;
		drinksAmount = itemsMax;
		vaccineAmount = itemsMax;
		
		UIManager.Instance.UpdateUI();
		
		for (int i = 0; i < shopPositions.Length; i++) {
			shopPositions[i].Init();
		}
	}

	private void Update() 
	{
		if (Input.GetKeyDown(KeyCode.P)) 
		{
			UnlockPharmacy();
        }
    }

	public void SpawnGuard() 
	{
		Instantiate(guardPrefab, guardSpawnPoint.position, Quaternion.identity);
		HasGuard = true;
	}

	public void ResearchedItem(string Type) 
	{
		for (int i = 0; i < ThingsSold.Length; i++)
		{
			if (ThingsSold[i].Name == Type) 
			{
				ThingsSold[i].Researched = true;
			}
		}
	}
	
	public float GetBackRoomFillCost()
	{
		return (int)((maxBackroomsAmount - backRoomsAmount) * 8f);
	}
	
	//can new NPC enter the shop and shop around?
	public bool CanSpawnNPC()
	{
		for (int i = 0; i < shopPositions.Length; i++) {
			if(shopPositions[i].HasEmptyArea())
			{
				return true;
			}
		}
		
		return false;
	}
	
	public void PlayerFocus(bool value)
	{
		IsFocused = value;
		if(value) camManager.SwitchCamera(CameraType.Shop);
	}
	
	public void OnShopperPaid(Shopper shopper)
	{
		//give player currency:
		int total = 0;
		for (int i = 0; i < shopper.Order.Count; i++) 
		{
			total += shopper.Order[i].Cost;
		}

		GlobalVar.Instance.AddCurrency(total * (GameManager.Instance.perksManager.DoubleMoney ? 2 : 1));
        
		//Update for Task!
        UIManager.Instance.taskList.OnCollectedGold(total);
        UIManager.Instance.UpdateUI();
	}
	
	public void OnShopperOrderCompleted(Shopper shopper, Transform T)
	{
		//deplete resources when shopper leaves shop.
		if(shopper.Types[shopper.currentIndex] == ShopperType.Groceries) {
			groceriesAmount--;
			shopCrate.RemoveBoxes(GameManager.Instance.GetNumberOfBoxesPerShopper());
		}
		
		if(shopper.Types[shopper.currentIndex] == ShopperType.IceCream) iceCreamAmount --;
		if(shopper.Types[shopper.currentIndex] == ShopperType.Pharmacy) pharmacyAmount --;
		if(shopper.Types[shopper.currentIndex] == ShopperType.Drinks) drinksAmount --;
		
		UIManager.Instance.UpdateUI();
		
		for (int i = 0; i < shopPositions.Length; i++) {
			if(shopPositions[i].shopperType == shopper.Types[shopper.currentIndex])
			{
				shopPositions[i].OnAreaOccupationChanged(T, 0); //release area for other shoppers to take place
				return;
			}
		}
	}
	
	public SellableItem[] GetRandomShopperItems(ShopperType[] types)
	{
		List<SellableItem> items = new List<SellableItem>();
		
		//only make shopper choose items they have the ability to take (e.g. no point in taking pharma stuff unless pharmacy is involved)
		for (int i = 0; i < types.Length; i++) {
			for (int n = 0; n < ThingsSold.Length; n++) 
			{
                //this is something shopper can take? make it random too & Researched too?
                if (ThingsSold[n].shopperType == types[i] && Random.value <= 0.6f && ThingsSold[n].Researched) 
				{
					items.Add(ThingsSold[n]);
				}
			}
		}
		
		return items.ToArray();
	}
	
	public Transform GetClosePosition(ShopperType Type, Shopper shopper)
	{
		for (int i = 0; i < shopPositions.Length; i++) 
		{
			if(shopPositions[i].shopperType == Type)
			{
				return shopPositions[i].GetNPCPosition();
			}
		}
		
		//fallback
		return null;
	}
	
	//this checks what type of shopper to spawn and returns a type only if that type is not empty in shop.
	public ShopperType[] GetShopperTypes(bool isThief)
	{
		if (isThief)
		{
			//just return a random point!
			if(Random.value <= 0.45f && creatableBuilding.IsActive(ShopperType.IceCream) && shopPositions[1].GetNPCPosition() != null) 
				return new ShopperType[] { ShopperType.IceCream };
            if (Random.value <= 0.5f && creatableBuilding.IsActive(ShopperType.Drinks) && shopPositions[1].GetNPCPosition() != null) 
				return new ShopperType[] { ShopperType.Drinks };

            if (shopPositions[4].GetNPCPosition() != null) return new ShopperType[] { ShopperType.Groceries };
        }

		List<ShopperType> shopperTypes = new List<ShopperType>();
		
		//45% chance to choose ice creams - also make sure npc can even go there?
		if(Random.value <= 0.45f && iceCreamAmount > 0 && creatableBuilding.IsActive(ShopperType.IceCream) && shopperTypes.Count < 3) {
			if (shopPositions[1].GetNPCPosition() != null)
			{
				shopperTypes.Add(ShopperType.IceCream);
			}
		}

        //60% chance to choose drinks - also make sure npc can even go there?
        if (Random.value <= 0.6f && drinksAmount > 0 && creatableBuilding.IsActive(ShopperType.Drinks) && shopperTypes.Count < 3)  {
            if (shopPositions[0].GetNPCPosition() != null)
                shopperTypes.Add(ShopperType.Drinks);
		}

		//65% chance to get a medicine!
		if (Random.value <= 0.65f && pharmacyAmount > 0 && IsPharmacyUnlocked && shopperTypes.Count < 3) 
		{
            if (shopPositions[2].GetNPCPosition() != null)
                shopperTypes.Add(ShopperType.Pharmacy);
        }

        //25% chance to get a vaccine!
        if (Random.value <= 0.25f && vaccineAmount > 0 && IsConsultationUnlocked && shopperTypes.Count < 3)
        {
            if (shopPositions[3].GetNPCPosition() != null)
                shopperTypes.Add(ShopperType.Consultation);
        }
		
		if(groceriesAmount > 0 && shopperTypes.Count < 3) //groceries exist?
			shopperTypes.Add(ShopperType.Groceries);
		else if(shopperTypes.Count == 0 && shopperTypes.Count < 3) //fallback if this is empty only.
			shopperTypes.Add(ShopperType.None);
		
		//never return more than 3!
		return shopperTypes.ToArray();
	}
	
	public void AreaOccupied(Shopper shopper, Transform T)
	{
		for (int i = 0; i < shopPositions.Length; i++) {
			if(shopPositions[i].shopperType == shopper.Types[shopper.currentIndex])
			{
				shopPositions[i].OnAreaOccupationChanged(T, shopper.shopperId); //take this place so no other NPC takes this area!
				break;
			}
		}
	}
	
	public bool ShopHasType(ShopperType shopperType)
	{
		if(shopperType == ShopperType.Drinks) return drinksAmount > 0;
		if(shopperType == ShopperType.Groceries) return groceriesAmount > 0;
		if(shopperType == ShopperType.IceCream) return iceCreamAmount > 0;
		if(shopperType == ShopperType.Pharmacy) return pharmacyAmount > 0;
		if (shopperType == ShopperType.Consultation) return vaccineAmount > 0;
		
		return false;
	}
	
	public int IsAreaOccupied(Transform T, ShopperType shopperType)
	{
		for (int i = 0; i < shopPositions.Length; i++) {
			if(shopPositions[i].shopperType == shopperType && shopPositions[i].HasEmptyArea())
			{
				return shopPositions[i].GetAreaOccupiedInt(T);
			}
		}
		
		return 0; //means empty.
	}

	public void ClearArea(Transform T, ShopperType shopperType) 
	{
        for (int i = 0; i < shopPositions.Length; i++)
        {
            if (shopPositions[i].shopperType == shopperType)
            {
				shopPositions[i].OnAreaOccupationChanged(T, 0);
            }
        }
    }
	
	public void OpenGateAndTakeStuff(Transform[] T)
	{
		for (int i = 0; i < fillerMen.Length; i++) {
			fillerMen[i].SetTarget(T[i]);
		}

		gateAnimator.SetBool("Open", true);
		Invoke(nameof(OnBackroomsFilled), backRoomFillDelay * (GameManager.Instance.perksManager.DoubleSpeed ? 0.5f : 1f)); //speed this up too!
		UIManager.Instance.UpdateUI();
	}
	
	private void OnBackroomsFilled()
	{
		refillingBackrooms = false;
		gateAnimator.SetBool("Open", false);
		truck.ReturnToWarehouse();

		//each truck fills the backrooms by '5'
        backRoomsAmount += 5;
        if (backRoomsAmount > maxBackroomsAmount)
            backRoomsAmount = maxBackroomsAmount; //clamp.
        UIManager.Instance.UpdateUI();
	}

	public void TryUnlockShopAreas() 
	{
		if (GameManager.Instance.Level >= pharmacyUnlockLevel) 
		{
			UnlockPharmacy();
        }

        if (GameManager.Instance.Level >= consultationUnlockLevel)
        {
			UnlockVaccine();
        }
    }

	private void UnlockPharmacy() 
	{
        IsPharmacyUnlocked = true;
        for (int i = 0; i < PharmacyAreasToDisable.Length; i++) PharmacyAreasToDisable[i].SetActive(false);
        for (int i = 0; i < PharmacyAreasToEnable.Length; i++) PharmacyAreasToEnable[i].SetActive(true);

		//default items:
		ResearchedItem("Pain Medicine");
		ResearchedItem("Allergy Medicine");
    }

	private void UnlockVaccine() 
	{
        IsConsultationUnlocked = true;
        for (int i = 0; i < consultationAreasToDisable.Length; i++) consultationAreasToDisable[i].SetActive(false);
        for (int i = 0; i < consultationAreasToEnable.Length; i++) consultationAreasToEnable[i].SetActive(true);

        //default items:
        ResearchedItem("Covid Vaccine");
    }
}
