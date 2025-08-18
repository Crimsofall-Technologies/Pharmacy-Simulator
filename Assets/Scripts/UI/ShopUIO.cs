using UnityEngine;
using UnityEngine.UI;

public class ShopUIO : MonoBehaviour
{
    public string ID = ""; //shop item ID
    public int Cost = 10; //in gems

    [Space]
    public Text costText;

    private void Start()
    {
        costText.text = "Cost: "+Cost.ToString();
    }

    public void Buy() 
    {
        if (GlobalVar.Instance.RemoveGems(Cost))
        {
            if (ID == "GUARD") 
            {
                UIManager.Instance.shop.SpawnGuard();
            }
            GameManager.Instance.perksManager.ActivatePerk(ID);
        }
        else 
        {
            UIManager.Instance.OpenNotEnoughUI();
        }
    }
}
