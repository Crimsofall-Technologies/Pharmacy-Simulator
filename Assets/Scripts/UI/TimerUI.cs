using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TimerUI : MonoBehaviour
{
	public Image Icon;
	public Image fillImage;

	private CameraType camType;
	private float maxTime, currentTime;
	private CanvasGroup cg;

	private Timer timer;
	private RectTransform rect;

    public void Init(Transform T, int t, CameraType ct, Timer time)
	{
        cg = GetComponent<CanvasGroup>();
		camType = ct;
		timer = time;

		GetComponent<RectTransform>().position = UIManager.Instance.camManager.Camera.WorldToScreenPoint(T.position);

		maxTime = t;
		currentTime = t;
	}

    public void UpdateSelf(Timer.CustomData data, int t)
	{
		currentTime = t;
		fillImage.fillAmount = (float)t / maxTime;
	}

	public void SetActiveUnactive(CameraType type) 
	{
		if (cg == null) return;

		if (camType == type) 
			cg.alpha = 1f;
		else 
			cg.alpha = 0f;
	}

	public void OnClick() 
	{
		UIManager.Instance.OpenTimerBuyUI(timer);
	}
}
