using UnityEngine;

public class NPCAreasManager : MonoBehaviour
{
	public Transform[] awayAreas;
	public Transform[] cashierAreas;

	private int lastCashierIndex = 0;
	
	public Transform GetAwayAreaPosition()
	{
		return awayAreas[Random.Range(0, awayAreas.Length)];
	}
	
	public Transform GetCashierPosition()
	{
		lastCashierIndex++;
		if (lastCashierIndex >= cashierAreas.Length)
			lastCashierIndex = 0;

		return cashierAreas[lastCashierIndex];
	}
}
