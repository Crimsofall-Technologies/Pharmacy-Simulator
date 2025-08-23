using UnityEngine;
using UnityEngine.UI;

public class ShopUIO : MonoBehaviour
{
    public string ID = ""; //shop item ID
    public int Cost = 10; //in gems
    public int PerkTime = 600; //10 minutes

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
            GameManager.Instance.perksManager.ActivatePerk(ID, PerkTime);
        }
        else 
        {
            UIManager.Instance.OpenNotEnoughUI();
        }
    }
}
