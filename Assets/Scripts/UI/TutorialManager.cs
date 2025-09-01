using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[DefaultExecutionOrder(0)]
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

		//disabled tutorial for editor session? debug.
		if (NoTutor && Application.isEditor) 
		{ 
			IsTutorialComplete = true; 
			TutorialRunning = false; 
			UIManager.Instance.taskList.doneTutorTask = true;
			UIManager.Instance.taskList.GenerateNewRandomTask();
			return;
		}

		NPCMax = GameManager.Instance.npcSpawner.maxNpcObjects;
		//GameManager.Instance.npcSpawner.maxNpcObjects = 2; //just 2 for now!
        IsTutorialComplete = PlayerPrefs.GetInt("seen_tutor_before", 0) == 1;
	}

	#endregion

	public bool NoTutor = false;
	public bool TutorialRunning { get; private set; }
	
	public float letterAddDelay = 0.15f;
	public GameObject tutorialGO;
	public Animator animator;
	public Text text;
	public TaskList taskList;
	public Playershop shop;
	public CameraManager camManager;
	
	private bool showing;
	public bool d, i, b, k;
	private bool a,c,e,f, g, h, j;
	private bool addTestTaskOnClose = false;
    private string lastMessage;
	public bool IsTutorialComplete { get; private set; }
	private int NPCMax;

	public void ShowTutorMessage(string message)
	{
		StopAllCoroutines();
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

		if (addTestTaskOnClose) { 
			taskList.TestTask(); 
			addTestTaskOnClose = false;
		}

		//shown all tutorials? complete it.
		if(a&&b&&c&&d&&e&&f&&g&&h&&i&&j&&k) {
			TutorialComplete();
		}
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
			ShowTutorMessage("Welcome to Pharmacy Sim, Let's Get you started to some basic mechanics of this game.");
		a = true;
	}
	
	public void OnFirstNPC()
	{
		if(TutorialRunning && !b && !NoTutor)
			ShowTutorMessage("You now have your first customer, They will stand at a specific region, you will have to serve them (by clicking the icon above them, hold the icon and a popup explains what the customer needs).");
		b = true;
	}
	
	public void OnNPCPay()
	{
		if (TutorialRunning && !c && !NoTutor)
		{
			ShowTutorMessage("The customer has got what they wanted now they will move to checkout section and pay you will have to click the icon above their head again.");
			shop.groceriesAmount = 2; //one more use and it triggers next tutor stage!
			shop.shopCrate.RemoveBoxes((shop.itemsMax - 2) * GameManager.Instance.GetNumberOfBoxesPerShopper());
			UIManager.Instance.UpdateUI();
		}
		c = true;
	}
	
	public void OnRefillRegionShow()
	{
		if (TutorialRunning && !d && !NoTutor)
		{
			ShowTutorMessage("As your customers take things from the shop the aisles get emptier and you will want to refill them from the backrooms, But this takes time to fill.");
			shop.backRoomsAmount = 1; //on this refill it triggers next tutorial stage!
			d = true;
            UIManager.Instance.UpdateUI();
        }
	}
	
	public void OnRegionRefilled()
	{
		if (TutorialRunning && !e && !NoTutor)
		{
			ShowTutorMessage("The Aisle is now full, each refill takes some amount from your backrooms (they will get empty over time then you will have to refill them).");
		}

		e = true;
	}

    public void OnGetTask()
    {
		if (TutorialRunning && !g && !NoTutor)
		{
			ShowTutorMessage("You also got a new Task: Serve 5 Customers to gain XP!");
        }
        g = true;
    }

	public void OnFirstThief()
	{
		if(TutorialRunning && !k && !NoTutor)
			ShowTutorMessage("People with the *?* icon above them indicates they are thieves and steal money if you do not catch them quickly!");
		k = true;
	}

    public void OnLevelUp()
    {
		if (TutorialRunning && !f && !NoTutor)
		{
			ShowTutorMessage("You have now gained a level you can now upgrade/research things & you can also create new departments.");

			Invoke(nameof(OpenBuildUI), 2f);
			Invoke(nameof(OnBuilding), 1f);
        }
        f = true;
    }

	private void OpenBuildUI() { UIManager.Instance.OpenBuildUI(); }

    public void OnBuilding()
    {
        if (TutorialRunning && !h && !NoTutor)
            ShowTutorMessage("You will now have to build the Ice-Cream department to gain more upgrades for Ice-Creams (scroll down & tap the icon to build).");
        h = true;
    }

    public void OnBuildingCreated()
    {
		if (TutorialRunning && !j && !NoTutor)
		{
			ShowTutorMessage("Congrats! You have now created the Ice Cream Department! now you can research more tastier Ice-Cream flavors (the same happens with all other buildings).");
			j = true;
		}
	}

	private IEnumerator DelayedOpenTutorUI(){
		yield return new WaitForSecondsRealtime(3f);
		TutorialComplete();
	}

    public void OnRefillRegionBackrooms()
    {
		if (TutorialRunning && !i && !NoTutor)
		{
			ShowTutorMessage("The backrooms are now empty, you will have to fill them in order to fill aisles.");
            GameManager.Instance.npcSpawner.maxNpcObjects = NPCMax;
			i = true;
			UIManager.Instance.UpdateUI();
			addTestTaskOnClose = true;
        }
    }

    public void TutorialComplete()
	{
		Debug.Log("Completed tutor!");

		if (TutorialRunning && !IsTutorialComplete)
		{
			ShowTutorMessage("You can now play the game free-style as you now know the basics of the game!");
			camManager.SwitchCamera(CameraType.Shop);
		}

		IsTutorialComplete = true;
		TutorialRunning = false;
		PlayerPrefs.SetInt("seen_tutor_before", 1);
	}
	
	#endregion
}
