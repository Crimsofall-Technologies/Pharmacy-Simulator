using UnityEngine;

public class ResearchBuilding : MonoBehaviour
{
	public bool IceCreamResearchActive, PharmacyResearchActive;
	
	public GameObject[] IceCreamObjects, PharmacyObjects;
	
	//update UI to show current researches
	public void OnActivate()
	{
		//Enable/Disable objects
		for (int i = 0; i < IceCreamObjects.Length; i++) {
			IceCreamObjects[i].SetActive(IceCreamResearchActive);
		}
		for (int i = 0; i < PharmacyObjects.Length; i++) {
			PharmacyObjects[i].SetActive(PharmacyResearchActive);
		}
	}
}
