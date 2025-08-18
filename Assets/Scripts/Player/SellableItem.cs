using UnityEngine;

[System.Serializable]
public class SellableItem 
{
	public string Name = "Item name";
	public ShopperType shopperType = ShopperType.Groceries;
	public int Cost = 1;

	[Space]
	public bool Researched = false;
}
