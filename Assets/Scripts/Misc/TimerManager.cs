using UnityEngine;
using System.Collections.Generic;
using System;

public static class TimerManager
{
	private static Dictionary<string, Timer> RunningTimers = new Dictionary<string, Timer>();
	
	public static Timer CreateTimer(string ID, int time, Timer.CustomData data, bool useRealTime = false)
	{
		if(RunningTimers.ContainsKey(ID))
		{
			Debug.Log("Timer with ID (" + ID + ") Already exsists!");
			return null;
		}
		
		Timer T = new GameObject($"Timer {ID}").AddComponent<Timer>();
		T.Init(time, ID, data, useRealTime);
		RunningTimers.Add(ID, T);
		return T;
	}
	
	public static Timer CreateTimerWithUI(string ID, int time, Timer.CustomData data, Transform worldT, Sprite sprite = null, bool useRealTime = false)
	{
		if(RunningTimers.ContainsKey(ID))
		{
			Debug.Log("Timer with ID (" + ID + ") Already exists!");
		}
		
		Timer T = new GameObject($"Timer {ID}").AddComponent<Timer>();
		T.Init(time, ID, data, useRealTime);
		UIManager.Instance.CreateTimerUI(T, worldT, time, sprite);
		RunningTimers.Add(ID, T);
		return T;
	}
	
	public static bool TimerExistsNamed(string ID)
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

    public static Timer GetTimerNamed(string Id)
    {
		if (RunningTimers.ContainsKey(Id))
			return RunningTimers[Id];
		else
			return null;
    }

	public static int GetRemainingTime(string Id)
	{
		if (RunningTimers.ContainsKey(Id))
			return RunningTimers[Id].remainingTime;
		else
			return 0;
	}
}
