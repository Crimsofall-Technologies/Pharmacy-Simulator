using UnityEngine;
using UnityEngine.UI;

public class TimerUI : MonoBehaviour
{
	public Image Icon;
	public Image fillImage;
	
	private float maxTime;
	
	public void Init(Vector3 screenPos, int t)
	{
		RectTransform rect = GetComponent<RectTransform>();
		rect.position = screenPos;
		maxTime = t;
	}
	
	public void UpdateSelf(Timer.CustomData data, int t)
	{
		fillImage.fillAmount = (float)t / maxTime;
	}
}
