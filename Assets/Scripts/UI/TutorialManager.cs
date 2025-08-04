using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
	#region Singleton
	
	public static TutorialManager Instance;
	
	private void Awake()
	{
		if (!Application.isEditor)
			NoTutor = false;

		TutorialRunning = true;
		Instance = this;
		
		hasSeenBefore = PlayerPrefs.GetInt("seen_tutor_before", 0) == 1;
	}

	#endregion

	public bool NoTutor = false;
	public bool TutorialRunning { get; private set; }
	
	public float letterAddDelay = 0.15f;
	public GameObject tutorialGO;
	public Animator animator;
	public Text text;
	
	private bool showing;
	private bool a,b,c,d,e,f;
	private string lastMessage;
	private bool hasSeenBefore;
	
	public void ShowTutorMessage(string message)
	{
		text.text = "";
		Time.timeScale = 0f;
		tutorialGO.SetActive(true);
		animator.SetBool("Open", true);
		lastMessage = message;
		StartCoroutine(AddLetters(message));
	}
	
	public void Close()
	{
		if(showing) //first tap to make full message appear!
		{
			showing = false;
			text.text = lastMessage;
			StopAllCoroutines();
			return;
		}
		
		animator.SetBool("Open", false);
		Invoke(nameof(DelayedCloseUI), 0.35f);
		Time.timeScale = GameManager.Instance.timeScale;
		StopAllCoroutines();
	}
	
	private void DelayedCloseUI()
	{
		tutorialGO.SetActive(false);
	}
	
	private IEnumerator AddLetters(string txt)
	{
		yield return new WaitForSecondsRealtime(0.25f);
		
		showing = true;
		char[] chars = txt.ToCharArray();
		int i = 0;
		while(i < chars.Length)
		{
			yield return new WaitForSecondsRealtime(letterAddDelay);
			text.text += chars[i];
			i++;
		}
		
		showing = false;
	}
	
	#region TUTOR_HANDLER
	
	public void OnGameStart()
	{
		if(TutorialRunning && !a && !NoTutor)
			ShowTutorMessage("Welcome to Pharmacy Sim, Let's Get you started to some basic mechnics of this game.");
		a = true;
	}
	
	public void OnFirstNPC()
	{
		if(TutorialRunning && !b && !NoTutor)
			ShowTutorMessage("You now have your first customer, They will stand at a specific region, you will have to serve them (by clicking the balloon above them).");
		b = true;
	}
	
	public void OnNPCPay()
	{
		if(TutorialRunning && !c && !NoTutor)
			ShowTutorMessage("The customer has got what they wanted now they will move to checkout section and pay (here you gain in-game currency which will used later on).");
		c = true;
	}
	
	public void OnRefillRegionShow()
	{
		if(TutorialRunning && !d && !NoTutor)
			ShowTutorMessage("As your customers take things from the shop the aisles get emptier and you will want to refill them from the backrooms, But this takes time to fill.");
		d = true;
	}
	
	public void OnRegionRefilled()
	{
		if(TutorialRunning && !e && !NoTutor)
			ShowTutorMessage("The Aisle is now full, each refill takes some amount from your backrooms (they will get empty then you have to buy stuff with in-game currency).");
		e = true;
	}
	
	public void TutorialComplete()
	{
		if(TutorialRunning && !hasSeenBefore && !NoTutor)
			ShowTutorMessage("You can now play the game free-style as you now know the basics of the game!");
		hasSeenBefore = true;
		PlayerPrefs.SetInt("seen_tutor_before", 1);
	}
	
	#endregion
}
