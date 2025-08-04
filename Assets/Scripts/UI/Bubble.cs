using System.Collections;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Bubble : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	public Image[] fillImages;
	public Image[] icons;
	
	[Space]
	public Color green;
	public Color red;
	
	private Shopper shopper;
	private ShopperType[] Type;
	private RectTransform rectT;
	
	private bool done;
	private bool ClickHeld;
	private bool cashierBubble, IsThief;
	private float holdTime;

	public void Init(ShopperType[] _Types, Shopper _shopper, Sprite[] sprites, bool isCashier, bool isThief = false)
	{
        for (int i = 0; i < sprites.Length; i++)
		{
			icons[i].sprite = sprites[i];
			fillImages[i].gameObject.SetActive(true);
			Fill(0f, 1f, i);
		}

		rectT = GetComponent<RectTransform>();
		Type = _Types;
		IsThief = isThief;
		shopper = _shopper;
		cashierBubble = isCashier;

		Invoke(nameof(DelayedEnable), 0.5f);
	}

	private void DelayedEnable() 
	{
		GetComponent<CanvasGroup>().alpha = 1f;
	}

    private void Update()
	{
		if (ClickHeld && Time.time >= holdTime && !cashierBubble && !IsThief)
		{
			ClickHeld = false;

			//open the shopping list for this NPC
			UIManager.Instance.OpenShoppingList(shopper);
		}
	}

    public void Fill(float current, float max, int index)
    {
		if (fillImages.Length == 0)
			return;

        fillImages[index].color = green;
        fillImages[index].fillAmount = current / max;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			holdTime = Time.time + 1f;
			ClickHeld = true;
		}
    }

    public void OnPointerUp(PointerEventData eventData)
    {
		if (eventData.button == PointerEventData.InputButton.Left) 
		{
            //complete the Task!
            if (!done && ClickHeld)
            {
                shopper.OnBubbleClicked();
                done = true;
            }

			holdTime = 0f;
            ClickHeld = false;
        }
    }

    public void SetPosition(Vector3 pos)
	{
		rectT.position = pos;
		gameObject.SetActive(true);
	}
}
