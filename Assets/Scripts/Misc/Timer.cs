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
	private float timerMultiplier;
	public GameObject myUIO;
	
	public void Init(int time, string ID, CustomData _data, float _timerMultiplier)
	{
		remainingTime = time;
		maxTime = time;
		myID = ID;
		data = _data;
		timerMultiplier = _timerMultiplier;
		StartCoroutine(Run());
	}
	
	private IEnumerator Run()
	{
		while(remainingTime > 0)
		{
			yield return new WaitForSeconds(1.0f / timerMultiplier);
			remainingTime--;
			OnTickAction?.Invoke(data, remainingTime);
		}
		
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
