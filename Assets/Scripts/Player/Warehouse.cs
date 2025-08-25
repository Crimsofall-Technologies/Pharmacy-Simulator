using UnityEngine;

//sends a truck to backrooms, truck is unloaded, truck returns - costs player money.
public class Warehouse : MonoBehaviour
{
	public WarehouseTruck truck;
	public Playershop shop;
	public Animator gateAnimator;
	public CameraManager camManager;
	
	[Space]
	public Transform backroomsT;
	public Sprite backroomFillSprite;

    [Range(1, 3)] public int TimesRefill = 1;

    private int fillTimes = 0;
    public bool IsFocused { get; private set; }
	
	private void Start()
	{
		PlayerFocus(false);
	}
	
	public void PlayerFocus(bool value)
	{
		IsFocused = value;
		if(value) camManager.SwitchCamera(CameraType.Warehouse);
	}
	
	public void SendStuffToShop(bool setupFillTimes = true)
	{
		shop.refillingBackrooms = true;
		if(setupFillTimes) fillTimes = TimesRefill;

		//each time show the timer on UI for player
		int time = GameManager.Instance.BaseTime;
		if(GameManager.Instance.perksManager.DoubleSpeed) time /= 2;
        Timer timer = TimerManager.CreateTimerWithUI("BackRoomsFiller", time, new Timer.CustomData(), backroomsT, backroomFillSprite);
		
		if(timer == null)
			return;

		timer.OnCompleteAction += OnBackroomsContinue;
		
		gateAnimator.SetBool("Open", true);
		UIManager.Instance.UpdateUI();
		Invoke(nameof(MoveTruck), 1.35f);
	}
	
	private void OnBackroomsContinue(Timer.CustomData data) { 
		truck.ContinueWay();
	}
	
	private void MoveTruck()
	{
		truck.GoToShop();
	}
	
	public void TruckReturned()
	{
		gateAnimator.SetBool("Open", false);

        fillTimes -= 1;
        if (fillTimes <= 0) fillTimes = 0;

        //if times-to-refill is still left?
        if (fillTimes != 0)
        {
			SendStuffToShop(false); //auto send again.
        }
    }
}
