using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
	public GameObject settingUI, achieveUI;
	public Animator achieveAnimator, settingAnimator;
	public Text versionText;
	
	private void Start()
	{
		versionText.text = "v."+Application.version;
	}
	
	private void Update() {
		if(Input.GetKeyDown(KeyCode.Return))
		{
			Application.Quit();
		}
	}
	
	public void Play()
	{
		SceneManager.LoadScene("Game");		
	}

    #region BUTTONS

    public void OpenAchievements() 
	{
		achieveUI.SetActive(true);
		achieveAnimator.SetBool("Open", true);
	}

	public void CloseAchievements() 
	{
        achieveAnimator.SetBool("Open", false);
		StartCoroutine(DelayedCloseUI(achieveUI));
    }

    public void OpenSettings()
    {
        settingUI.SetActive(true);
        settingAnimator.SetBool("Open", true);
    }

	public void CloseSettings() 
	{
		settingAnimator.SetBool("Open", false);
		StartCoroutine(DelayedCloseUI(settingUI));
	}

    #endregion

    private IEnumerator DelayedCloseUI(GameObject panel) 
	{
		yield return new WaitForSeconds(0.5f);
		panel.SetActive(false);
	}
}
