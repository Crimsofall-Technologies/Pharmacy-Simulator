using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DailLoginReward : MonoBehaviour
{
	public GameObject rewardUI;
	public Animator rewardAnimator;
	
	private void Start()
	{
		//can player get reward now?
		int lastDayOfYear = PlayerPrefs.GetInt("last_reward_day", System.DateTime.UtcNow.DayOfYear);
		if(lastDayOfYear < System.DateTime.UtcNow.DayOfYear) //last reward was some day before today?
		{
			//Give reward
			Debug.Log("Daily reward given.");
			PlayerPrefs.SetInt("last_reward_day", System.DateTime.UtcNow.DayOfYear);
			OpenRewardUI();
		}
	}
	
	public void OpenRewardUI()
	{
		rewardUI.SetActive(true);
		rewardAnimator.SetBool("Open", true);
	}
	
	public void CloseRewardUI()
	{
		rewardAnimator.SetBool("Open", false);
		StartCoroutine(DelayedCloseRewardUI());
	}
	
	private IEnumerator DelayedCloseRewardUI()
	{
		yield return new WaitForSeconds(0.35f);
		rewardUI.SetActive(false);
	}
}
