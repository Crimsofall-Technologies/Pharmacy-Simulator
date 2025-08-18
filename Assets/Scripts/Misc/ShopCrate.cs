using UnityEngine;

public class ShopCrate : MonoBehaviour
{
	public GameObject[] Boxes;
	
	public void RemoveBoxes(int number)
	{
		int m = number + 1;
		for (int i = 0; i < Boxes.Length; i++) 
		{
			if(m <= 0)
			{
				break;
			}
			else if(Boxes[i].activeSelf)
			{
				Boxes[i].SetActive(false);
				m--;
			}
		}	
	}
	
	public void FillUp()
	{
		for (int i = 0; i < Boxes.Length; i++) {
			Boxes[i].SetActive(true);
		}	
	}
}
