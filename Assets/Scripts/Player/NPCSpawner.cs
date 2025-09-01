using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCSpawner : MonoBehaviour
{
	public GameObject[] NPCPrefabs; 
	public Transform[] NPCSpawnArea;
	public Playershop playerShop;
	public NPCAreasManager areaManager;
	public Vector2 randomTime;
	public int maxNpcObjects = 12;

	[Range(0.05f, 1f)] public float thiefSpawnChance = 0.15f;

	public List<Shopper> Shoppers = new List<Shopper>();
	private int lastAreaIndex = 0;
	
	private void Start() 
	{
		StartCoroutine(SpawnNPC());
	}
	
	public IEnumerator SpawnNPC()
	{
		while(true)
		{
			//make sure there is enough space before creating new NPC
			if(playerShop.CanSpawnNPC()) {
				SpawnOneNPC();
			}
			yield return new WaitForSeconds(Random.Range(randomTime.x, randomTime.y));
		}
	}
	
	private void SpawnOneNPC()
	{
		//spawn at starting point.
		lastAreaIndex++;
		if(lastAreaIndex >= NPCSpawnArea.Length)
			lastAreaIndex = 0;
		
		bool isThief = Random.value <= thiefSpawnChance && GlobalVar.Instance.Currency > 0; //chance to spawn an NPC as a thief
		ShopperType[] shopperTypes = playerShop.GetShopperTypes(isThief);
		SellableItem[] items = playerShop.GetRandomShopperItems(shopperTypes);

		//no thieves during tutorial - execpt when needed!
		if (TutorialManager.Instance.TutorialRunning)
		{
			if(TutorialManager.Instance.d && !TutorialManager.Instance.k)
				isThief = true;
			else 
				isThief = false;

			if (!TutorialManager.Instance.b) 
			{
                //first NPC must do a shopping of 50 or more!
                SellableItem[] i = new SellableItem[]
                {
                    playerShop.ThingsSold[0],
                    playerShop.ThingsSold[1],
                    playerShop.ThingsSold[2],
                    playerShop.ThingsSold[10]
                };
                items = i;
            }
		}

		//spawn if this is a valid shopper:
		if(shopperTypes[0] != ShopperType.None && Shoppers.Count < maxNpcObjects) 
		{
			Shopper s = Instantiate(NPCPrefabs[Random.Range(0, NPCPrefabs.Length)], NPCSpawnArea[lastAreaIndex].position, Quaternion.identity).GetComponent<Shopper>();
			s.IsThief = isThief;
			s.Init(items, shopperTypes, playerShop, areaManager);

			Shoppers.Add(s);
		}
	}
}
