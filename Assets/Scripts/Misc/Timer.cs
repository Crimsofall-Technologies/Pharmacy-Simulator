using UnityEngine;
using System.Collections;

public class Timer : MonoBehaviour
{
	[System.Serializable]
	public class CustomData
	{
		public int _int;
		public string _string;
	}
	
	public System.Action<CustomData, int> OnTickAction;
	public System.Action<CustomData> OnCompleteAction;
	
	private string myID;
	private CustomData data;
	public int remainingTime, maxTime;
	public GameObject myUIO;
	private bool useRealTime;
	
	public void Init(int time, string ID, CustomData _data, bool _useRealTime)
	{
		remainingTime = time;
		maxTime = time;
		myID = ID;
		data = _data;
		useRealTime = _useRealTime;

        OnTickAction?.Invoke(data, remainingTime); //initial update (mostly for showing the time to player on UI)
        StartCoroutine(Run());
	}
	
	private IEnumerator Run()
	{
		while(remainingTime > 0)
		{
			if(!useRealTime)
				yield return new WaitForSeconds(1.0f * (GameManager.Instance.perksManager.DoubleSpeed ? 0.5f : 1f));
            else
                yield return new WaitForSecondsRealtime(1.0f * (GameManager.Instance.perksManager.DoubleSpeed ? 0.5f : 1f)); //goes on regardless of game is paused or not.
            remainingTime--;
			OnTickAction?.Invoke(data, remainingTime);
		}
		
		OnCompleteAction?.Invoke(data);
		TimerManager.RemoveTimer(myID); //it will destroy this object automatically
	}

	public void OverrideComplete() 
	{
		remainingTime = 0;
        OnCompleteAction?.Invoke(data);
        TimerManager.RemoveTimer(myID); //it will destroy this object automatically
    }
	
	//called by TimerManager.cs
	public void DestroySelf()
	{
		if(myUIO != null) Destroy(myUIO);
		Destroy(gameObject);
	}
}
