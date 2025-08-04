using UnityEngine;
using UnityEngine.UI;

public class FillManager : MonoBehaviour
{
	public Image fillImage;
	public Transform FollowArea; //world space object to follow on UI
	public Camera cam;
	
	private RectTransform rectTransform;
	
	private void Start() {
		rectTransform = GetComponent<RectTransform>();
	}
	
	public void UpdateSelf(float current, float max)
	{
		fillImage.fillAmount = current / max;
	}
	
	private void OnEnable()
	{
		if(rectTransform == null)
			rectTransform = GetComponent<RectTransform>();
		
		if(FollowArea != null)
			rectTransform.position = cam.WorldToScreenPoint(FollowArea.position);
	}
}
