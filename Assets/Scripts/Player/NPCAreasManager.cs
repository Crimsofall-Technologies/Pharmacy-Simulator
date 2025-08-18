using UnityEngine;

public class NPCAreasManager : MonoBehaviour
{
	public Transform[] awayAreas;
	public Transform[] cashierAreas;
	public Transform thiefAwayArea;

    private int lastCashierIndex = 0;
	
	public Transform GetAwayAreaPosition(bool isThief)
	{
		if (!isThief)
			return awayAreas[Random.Range(0, awayAreas.Length)];
		else
			return thiefAwayArea;
	}
	
	public Transform GetCashierPosition()
	{
		lastCashierIndex++;
		if (lastCashierIndex >= cashierAreas.Length)
			lastCashierIndex = 0;

		return cashierAreas[lastCashierIndex];
	}
}
