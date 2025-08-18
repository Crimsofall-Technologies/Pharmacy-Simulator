using UnityEngine;

public class TargetPointer : MonoBehaviour
{
	public Transform IntrestPoint;

	//can the player stand near this area?
	public bool CanStandHere() 
	{
		return IntrestPoint.gameObject.activeSelf;
	}
}
