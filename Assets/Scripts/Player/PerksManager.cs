using UnityEngine;
using UnityEngine.UI;

public class PerksManager : MonoBehaviour
{
	public bool DoubleSpeed, DoubleMoney, AutoFill;
	
	public Button[] Buttons;
	
	public void UpdateButtons()
	{
		Buttons[0].interactable = !TimerManager.TimerExistsNamed("DOUBLE_TIME");
		Buttons[1].interactable = !TimerManager.TimerExistsNamed("DOUBLE_MONEY");
		Buttons[2].interactable = !TimerManager.TimerExistsNamed("AUTO_COLLECT");
		Buttons[3].interactable = !GameManager.Instance.shop.HasGuard;
	}
	
	public void ActivatePerk(string Name)
	{
		if(Name == "DOUBLE_TIME") DoubleSpeed = true;
		if(Name == "DOUBLE_MONEY") DoubleMoney = true;
		if(Name == "AUTO_COLLECT") AutoFill = true;
		
		//show on UI and activate a timer!
		Timer timer = TimerManager.CreateTimer(Name, 15 * 60, new Timer.CustomData { _string = Name });
		timer.OnCompleteAction += OnTimerComplete;
		
		Debug.Log("Activated Perk: " + Name);
		UIManager.Instance.ClosePerkUI();
	}
	
	private void OnTimerComplete(Timer.CustomData data)
	{
		DeactivatePerk(data._string);
	}
	
	public void DeactivatePerk(string Name)
	{
		if(Name == "DOUBLE_TIME") DoubleSpeed = false;
		if(Name == "DOUBLE_MONEY") DoubleMoney = false;
		if(Name == "AUTO_COLLECT") AutoFill = false;
		
		//remove from UI
		
	}
}
