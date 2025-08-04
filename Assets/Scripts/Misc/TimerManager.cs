using UnityEngine;
using System.Collections.Generic;

public static class TimerManager
{
	private static Dictionary<string, Timer> RunningTimers = new Dictionary<string, Timer>();
	
	public static Timer CreateTimer(string ID, int time, Timer.CustomData data, float timerMultiplier = 1f)
	{
		if(RunningTimers.ContainsKey(ID))
		{
			Debug.Log("Timer with ID (" + ID + ") Already exsists!");
			return null;
		}
		
		Timer T = new GameObject($"Timer {ID}").AddComponent<Timer>();
		T.Init(time, ID, data, timerMultiplier);
		RunningTimers.Add(ID, T);
		return T;
	}
	
	public static Timer CreateTimerWithUI(string ID, int time, Timer.CustomData data, float timerMultiplier, Vector3 screenPos, Sprite sprite = null)
	{
		if(RunningTimers.ContainsKey(ID))
		{
			Debug.Log("Timer with ID (" + ID + ") Already exsists!");
		}
		
		Timer T = new GameObject($"Timer {ID}").AddComponent<Timer>();
		T.Init(time, ID, data, timerMultiplier);
		UIManager.Instance.CreateTimerUI(T, screenPos, time, sprite);
		RunningTimers.Add(ID, T);
		return T;
	}
	
	public static bool TimerExsistsNamed(string ID)
	{
		return RunningTimers.ContainsKey(ID);
	}
	
	public static void RemoveTimer(string ID)
	{
		if(RunningTimers[ID] != null)
		{
			RunningTimers[ID].DestroySelf();
		}
		
		RunningTimers.Remove(ID);
	}
}
