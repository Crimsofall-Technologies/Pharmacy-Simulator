using UnityEngine;
using UnityEngine.UI;

public class PerksManager : MonoBehaviour
{
	public bool DoubleSpeed, DoubleMoney, GuardActive, HelperActive;

	public Text doubleSpeedText, moneyText, helperText, guardText;
	public int PerkTime;
	
	public ShopUIO shopUIO;

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

    public void OnLoad(PlayerData data)
    {
		ActivatePerk("DOUBLE_TIME", data.remainDoubleTime > 0 ? PerkTime : 0, (int)data.remainDoubleTime);
		ActivatePerk("DOUBLE_MONEY", data.remainDoubleMoney > 0 ? PerkTime : 0, (int)data.remainDoubleMoney);
		ActivatePerk("GUARD", data.remainGuard > 0 ? PerkTime : 0, (int)data.remainGuard);
		ActivatePerk("HELPER", data.remainHelper > 0 ? PerkTime : 0, (int)data.remainHelper);
    }

	public void ActivatePerk(string Name)
	{
		ActivatePerk(Name, PerkTime);
	}

    public void ActivatePerk(string Name, int PerkTime, int remainTime = 0)
	{
		if(PerkTime <= 0)
			return;

		if(Name == "DOUBLE_TIME") { 
			DoubleSpeed = true;
			doubleSpeedText.text = "Time: "+StringSimplifier.GetSortedTimeFromSeconds(PerkTime);
		}
		if(Name == "DOUBLE_MONEY") { 
			DoubleMoney = true;
			moneyText.text = "Time: "+StringSimplifier.GetSortedTimeFromSeconds(PerkTime);
		}
		if(Name == "GUARD") { 
			GuardActive = true;
			guardText.text = "Time: "+StringSimplifier.GetSortedTimeFromSeconds(PerkTime);

			GameManager.Instance.shop.SpawnGuard();
		}
		if(Name == "HELPER") { 
			HelperActive = true;
			helperText.text = "Time: "+StringSimplifier.GetSortedTimeFromSeconds(PerkTime);
		}
		
		//show on UI and activate a timer!
		Timer timer = TimerManager.CreateTimer(Name, (int)PerkTime, new Timer.CustomData { _string = Name }, true);
		timer.OnCompleteAction += OnTimerComplete;
		timer.OnTickAction += OnTickAction;
		
		if(remainTime > 0)
			timer.SkipTimeTo(remainTime);
		
		UpdateButtons();
		UIManager.Instance.ClosePerkUI();
	}

	private void OnTickAction(Timer.CustomData data, int remiainTime) {
		if(data._string == "DOUBLE_TIME") doubleSpeedText.text = "Time: "+StringSimplifier.GetSortedTimeFromSeconds(remiainTime);
		if(data._string == "DOUBLE_MONEY") moneyText.text = "Time: "+StringSimplifier.GetSortedTimeFromSeconds(remiainTime);
		if(data._string == "GUARD") guardText.text = "Time: "+StringSimplifier.GetSortedTimeFromSeconds(remiainTime);
		if(data._string == "HELPER") helperText.text = "Time: "+StringSimplifier.GetSortedTimeFromSeconds(remiainTime);
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
