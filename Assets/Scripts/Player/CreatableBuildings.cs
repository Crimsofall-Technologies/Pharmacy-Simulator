using UnityEngine;
using UnityEngine.UI;

public class CreatableBuildings : MonoBehaviour
{
	[System.Serializable]
	public class Building
	{
		public string Name;
		[Min(1)] public int Level = 1;
		public GameObject Ground;
		public int Cost, Time;
		public GameObject UIButton;
		public Text text, costText, levelText;
		public GameObject[] BuildingUpgrades; //number of upgrades to this building?
		public CameraType cameraType = CameraType.Shop;
		public ShopperType shopperType = ShopperType.None;

		[Space]
		public GameObject myParentUpgradeable; //this will not be upgradeable till the parent is done.
		public GameObject[] uiResearchArea; //the area activated when building is built in Research Window

		public int UpgradesDone { get; set; }
		public bool IsBuilt { get; set; }
		public bool IsBuilding { get; set; }
		public string normalText { get; set; }
	}
	
	public Building[] Buildings = new Building[0];
	public CameraManager camManager;
	public Playershop shop;
	
	private void Awake() {
		for (int i = 0; i < Buildings.Length; i++) 
		{
			if(Buildings[i].Ground != null) Buildings[i].Ground.SetActive(true);
			Buildings[i].UIButton.SetActive(true);
			SetBuildingParts(false, Buildings[i].BuildingUpgrades);
			Buildings[i].normalText = Buildings[i].text.text;
			Buildings[i].UpgradesDone=0;
			Buildings[i].IsBuilt = false;
			Buildings[i].IsBuilding = false;

            if (Buildings[i].uiResearchArea.Length > 0)
            {
                for (int n = 0; n < Buildings[i].uiResearchArea.Length; n++)
                {
                    Buildings[i].uiResearchArea[n].SetActive(false);
                }
            }

            Buildings[i].costText.text = "Cost: " + Buildings[i].Cost;
        }
	}
	
	public void OnLoad(int i, int remainTime, int updateIndex, bool complete)
	{
		if(complete)
		{
			BuildingComplete(Buildings[i].Name);
			return;
		}

		if(remainTime <= 0) return;

		Timer.CustomData data = new Timer.CustomData() { _string = Buildings[i].Name };

				//time will be 120s x 4 = 480s at level 2, 120 x 5 = 600s at level 3
				int time = GameManager.Instance.BaseTime;
				if (Buildings[i].Level > 1)
					time = GameManager.Instance.BaseTime * (Buildings[i].Level + 2);

				//make the time half if double timer is on
				if(GameManager.Instance.perksManager.DoubleSpeed)
					time /= 2;
                
				Timer timer = TimerManager.CreateTimerWithUI(Buildings[i].Name, time, data, Buildings[i].Ground.transform);
				timer.OnCompleteAction += OnRealBuildComplete;
				Buildings[i].IsBuilding = true;
				timer.SkipTimeTo(remainTime);

				Buildings[i].UpgradesDone = updateIndex;
				if(Buildings[i].BuildingUpgrades.Length > 1 && updateIndex - 1 >= 0) {
				for (int j = 0; j < Buildings[i].BuildingUpgrades.Length; j++)
				{
					Buildings[i].BuildingUpgrades[Buildings[i].UpgradesDone - 1].SetActive(true);
				}
				}

                //disable buttons when presses automatically!
                if (Buildings[i].BuildingUpgrades.Length > 1 && Buildings[i].UpgradesDone < Buildings[i].BuildingUpgrades.Length)
                {
                    string t = Buildings[i].normalText;
                    Buildings[i].text.text = t.Replace("Build", "Upgrade") + (Buildings[i].UpgradesDone > 0 ? $"(LVL: {Buildings[i].UpgradesDone})" : "");
                }
                else
                {
                    Buildings[i].UIButton.SetActive(false);
                }
	}

	public void UpdateUI()
	{
		for (int i = 0; i < Buildings.Length; i++) 
		{
			//do not let player upgrade next before parent.
			if (Buildings[i].IsBuilding) 
			{
                Buildings[i].UIButton.GetComponent<Button>().interactable = false;
            }
			else if (Buildings[i].myParentUpgradeable == null)
			{
				Buildings[i].UIButton.GetComponent<Button>().interactable = Buildings[i].Level <= GameManager.Instance.Level;
				Buildings[i].levelText.transform.parent.gameObject.SetActive(Buildings[i].Level > GameManager.Instance.Level);
				Buildings[i].levelText.text = Buildings[i].Level.ToString();
			}
			else if (Buildings[i].myParentUpgradeable.activeSelf)
			{
				Buildings[i].UIButton.GetComponent<Button>().interactable = Buildings[i].Level <= GameManager.Instance.Level;
				Buildings[i].levelText.transform.parent.gameObject.SetActive(Buildings[i].Level > GameManager.Instance.Level);
				Buildings[i].levelText.text = Buildings[i].Level.ToString();
			}
			else 
			{
                //locked.
                Buildings[i].UIButton.GetComponent<Button>().interactable = false;
                Buildings[i].levelText.transform.parent.gameObject.SetActive(true);

                //show no text since player may be above level (or not)
				if(Buildings[i].Level <= GameManager.Instance.Level)
					Buildings[i].levelText.text = ""; 
				else
                    Buildings[i].levelText.text = Buildings[i].Level.ToString();
            }
        }
	}
	
	public bool IsActive(ShopperType shopperType)
	{
		for (int i = 0; i < Buildings.Length; i++) {
			if(Buildings[i].shopperType == shopperType && Buildings[i].IsBuilt)
			{
				return true;
			}
		}
		
		return false;
	}
	
	public bool CanBuildSomethingThisLevel()
	{
		for(int i = 0; i < Buildings.Length; i++) {
			if(!Buildings[i].IsBuilt && !Buildings[i].IsBuilding && Buildings[i].Level == GameManager.Instance.Level)
				return true;
		}

		return false;
	}

	public int GetBuildCost(string Name)
	{
		for (int i = 0; i < Buildings.Length; i++) {
			if(Name == Buildings[i].Name)
			{
				if(!Buildings[i].IsBuilt || !Buildings[i].IsBuilding)
					return Buildings[i].Cost;
				else
					return 0;
			}
		}
		
		return 0;
	}
	
	public void Build(string Name)
	{
		for (int i = 0; i < Buildings.Length; i++) {
			if(Buildings[i].Name == Name)
			{
				Timer.CustomData data = new Timer.CustomData() { _string = Name };

				//time will be 120s x 4 = 480s at level 2, 120 x 5 = 600s at level 3
				int time = GameManager.Instance.BaseTime;
				if (Buildings[i].Level > 1)
					time = GameManager.Instance.BaseTime * (Buildings[i].Level + 2);

				//make the time half if double timer is on
				if(GameManager.Instance.perksManager.DoubleSpeed)
					time /= 2;

				camManager.SwitchCamera(Buildings[i].cameraType);
                
				Timer timer = TimerManager.CreateTimerWithUI(Name, time, data, Buildings[i].Ground.transform);
				timer.OnCompleteAction += OnRealBuildComplete;
				Buildings[i].IsBuilding = true;

                //disable buttons when presses automatically!
                if (Buildings[i].BuildingUpgrades.Length > 1 && Buildings[i].UpgradesDone < Buildings[i].BuildingUpgrades.Length)
                {
                    string t = Buildings[i].normalText;
                    Buildings[i].text.text = t.Replace("Build", "Upgrade") + (Buildings[i].UpgradesDone > 0 ? $"(LVL: {Buildings[i].UpgradesDone})" : "");
                }
                else
                {
                    Buildings[i].UIButton.SetActive(false);
                }
                break;
			}
		}
	}
	
	public void OnRealBuildComplete(Timer.CustomData data)
	{
		BuildingComplete(data._string);
	}

	private void BuildingComplete(string Name)
	{
		for (int i = 0; i < Buildings.Length; i++) {
			if (Name == Buildings[i].Name)
			{
				Buildings[i].IsBuilt = true;
				Buildings[i].IsBuilding = false;
				if (Buildings[i].Ground != null) Buildings[i].Ground.SetActive(false);
				if (Buildings[i].uiResearchArea.Length > 0) 
				{
					for (int n = 0; n < Buildings[i].uiResearchArea.Length; n++)
					{
						Buildings[i].uiResearchArea[n].SetActive(true);
					}
				}
				Buildings[i].UpgradesDone++;
                Buildings[i].UIButton.GetComponent<Button>().interactable = true;

                if (Buildings[i].UpgradesDone < Buildings[i].BuildingUpgrades.Length)
				{
					if(Buildings[i].normalText == "Drinks Aisle")
						Buildings[i].text.text = "Upgrade Drinks Dept." + $" [LVL: {Buildings[i].UpgradesDone}]";
					if(Buildings[i].normalText == "Ice Cream Aisle")
						Buildings[i].text.text = "Upgrade Ice Cream Dept." + $" [LVL: {Buildings[i].UpgradesDone}]";

					//increase cost by 100 per level upgrade needed!
					Buildings[i].Cost += 100;
					Buildings[i].costText.text = "Cost: " + Buildings[i].Cost;
				}
				else
				{
					Buildings[i].UIButton.SetActive(false);
				}

				Buildings[i].BuildingUpgrades[Buildings[i].UpgradesDone - 1].SetActive(true);

				//give defaults:

				//give choc flavor
				if (Buildings[i].shopperType == ShopperType.IceCream) { shop.ResearchedItem("Choc Ice Cream"); }

				//give water
				if (Buildings[i].shopperType == ShopperType.Drinks) { shop.ResearchedItem("Water Cola"); }

				//give covid vaccination
				if (Buildings[i].shopperType == ShopperType.Consultation) { shop.ResearchedItem("Covid Vaccine"); }

				//give pain medicene, allergy medicene
				if (Buildings[i].shopperType == ShopperType.Pharmacy) { 
					shop.ResearchedItem("Pain Medicine"); 
					shop.ResearchedItem("Allergy Medicine");
				}

				//for tutorials
				if (TutorialManager.Instance.TutorialRunning)
				{
					TutorialManager.Instance.OnBuildingCreated();
				}
			}
		}

		UpdateUI();
	}
	
	public void SetBuildingParts(bool active, GameObject[] array)
	{
		for (int i = 0; i < array.Length; i++) {
			array[i].SetActive(active);
		}
	}
}
