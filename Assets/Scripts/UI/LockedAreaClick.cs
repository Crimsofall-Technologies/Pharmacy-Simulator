using UnityEngine;

public class LockedAreaClick : MonoBehaviour
{
	public Animator clickAnimator;
	
	private void OnMouseDown()
	{
		clickAnimator.SetTrigger("Click");
	}
	
	public void DisableIt()
	{
		clickAnimator.gameObject.SetActive(false);
		gameObject.SetActive(false);
	}
}
