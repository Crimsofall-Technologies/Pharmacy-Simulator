using UnityEngine;
using UnityEngine.UI;

public class PerksManager : MonoBehaviour
{
	public bool DoubleSpeed, DoubleMoney, GuardActive, HelperActive;

	public Text doubleSpeedText, moneyText, helperText, guardText;
	
	public Button[] Buttons;
	
	public void UpdateButtons()
	{
		Buttons[0].interactable = !TimerManager.TimerExistsNamed("DOUBLE_TIME");
		Buttons[1].interactable = !TimerManager.TimerExistsNamed("DOUBLE_MONEY");
		Buttons[2].interactable = !TimerManager.TimerExistsNamed("GUARD");
		Buttons[3].interactable = !TimerManager.TimerExistsNamed("HELPER");

		//disable texts:
		doubleSpeedText.transform.parent.gameObject.SetActive(DoubleSpeed);
		moneyText.transform.parent.gameObject.SetActive(DoubleMoney);
		guardText.transform.parent.gameObject.SetActive(GuardActive);
		helperText.transform.parent.gameObject.SetActive(HelperActive);
	}
	
	public void ActivatePerk(string Name, int PerkTime)
	{
		if(Name == "DOUBLE_TIME") { 
			DoubleSpeed = true;
			doubleSpeedText.text = "Time: "+PerkTime+"s";
		}
		if(Name == "DOUBLE_MONEY") { 
			DoubleMoney = true;
			moneyText.text = "Time: "+PerkTime+"s";
		}
		if(Name == "GUARD") { 
			GuardActive = true;
			guardText.text = "Time: "+PerkTime+"s";

			GameManager.Instance.shop.SpawnGuard();
		}
		if(Name == "HELPER") { 
			HelperActive = true;
			helperText.text = "Time: "+PerkTime+"s";
		}
		
		//show on UI and activate a timer!
		Timer timer = TimerManager.CreateTimer(Name, (int)PerkTime, new Timer.CustomData { _string = Name }, true);
		timer.OnCompleteAction += OnTimerComplete;
		timer.OnTickAction += OnTickAction;
		
		Debug.Log("Activated Perk: " + Name);
		UpdateButtons();
		UIManager.Instance.ClosePerkUI();
	}

	private void OnTickAction(Timer.CustomData data, int remiainTime) {
		if(data._string == "DOUBLE_TIME") doubleSpeedText.text = "Time: "+remiainTime+"s";
		if(data._string == "DOUBLE_MONEY") moneyText.text = "Time: "+remiainTime+"s";
		if(data._string == "GUARD") guardText.text = "Time: "+remiainTime+"s";
		if(data._string == "HELPER") helperText.text = "Time: "+remiainTime+"s";
	}
	
	private void OnTimerComplete(Timer.CustomData data)
	{
		if(data._string == "DOUBLE_TIME") { DoubleSpeed = false; doubleSpeedText.text = ""; }
		if(data._string == "DOUBLE_MONEY") { DoubleMoney = false; moneyText.text = ""; }
		if(data._string == "GUARD") { 
			GuardActive = false; 
			guardText.text = ""; 

			//disable guard object!
			GameManager.Instance.shop.RemoveGuard();
		}
		if(data._string == "HELPER") { HelperActive = false; helperText.text = ""; }

		UpdateButtons();
	}
}
