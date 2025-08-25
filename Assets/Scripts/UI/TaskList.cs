using UnityEngine;
using UnityEngine.UI;

public class TaskList : MonoBehaviour
{
    public GameObject taskPrefab;
    public Transform taskList;
    public Playershop shop;

    [Space]
    public Sprite moneySprite;
    public Sprite npcSprite;

    public TutorialManager tutorialManager;

    //current task stuff:
    private ShopperType ShopperTypeToTask = ShopperType.None;
    private int TaskGold, currentTaskGold;
    private int serveNpcAmount, currentServedNpc;
    private int thiefToCatchCurrent, thiefToCatchMax;
    private int shopperTypeCurrent, shopperTypeMax;

    private Text currentTaskText;
    private GameObject currentTaskGO;

    private bool TaskGoldB, TaskNpc, TaskThief, TaskShop;
    private bool isFirstTask;
    public bool doneTutorTask { get; set; }

    public void GenerateNewRandomTask() 
    {
        //do not create any tasks before tutor!
        if (!doneTutorTask) 
        {
            return;
        }

        //generate a task & add to UI!
        string N = "";
        Sprite s = null;
        float rnd = Random.value;

        //default all other values
        shopperTypeCurrent = 0;
        shopperTypeMax = 0;
        currentServedNpc = 0;
        serveNpcAmount = 0;
        thiefToCatchCurrent = 0;
        thiefToCatchMax = 0;
        TaskGold = 0;
        currentTaskGold = 0;
        ShopperTypeToTask = ShopperType.None;

        if (rnd <= 0.45f) //took shop item
        {
            shopperTypeMax = Random.Range(5, 10);

            //task depending on what player has!
            if (Random.value <= 0.35f && shop.creatableBuilding.IsActive(ShopperType.IceCream)) 
            {
                ShopperTypeToTask = ShopperType.IceCream;
            }
            else if (Random.value <= 0.35f && shop.creatableBuilding.IsActive(ShopperType.Drinks))
            {
                ShopperTypeToTask = ShopperType.Drinks;
            }
            else
                ShopperTypeToTask = ShopperType.Groceries;

            N = $"Sell: '{ShopperTypeToTask}' - {shopperTypeCurrent}/{shopperTypeMax}";
            s = UIManager.Instance.ShopperTypeSprites[(int)ShopperTypeToTask];
            TaskShop = true;
        }
        else if (rnd <= 0.65f) //npc served
        {
            serveNpcAmount = Random.Range(5, 10) * GameManager.Instance.Level;
            N = $"Serve {currentServedNpc}/{serveNpcAmount} Customers";
            s = npcSprite;
            TaskNpc = true;
        }
        else if (rnd <= 0.8f && !isFirstTask) //thieves (not for the first random task player gets)
        {
            thiefToCatchMax = Random.Range(3, 6);
            N = $"Catch {thiefToCatchCurrent}/{thiefToCatchMax} Thieves";
            s = npcSprite;
            TaskThief = true;
        }
        else if (rnd <= 1f) //gold 
        {
            TaskGold = Mathf.RoundToInt(GameManager.Instance.Level / 2f * Random.Range(1, 5) * 150);
            N = $"Collect {currentTaskGold}/{TaskGold} Cash";
            s = moneySprite;
            TaskGoldB = true;
        }

        isFirstTask = true;

        if(s != null)
            AddTask(N,s);
    }

    public void TestTask() 
    {
        serveNpcAmount = 5;
        TaskNpc = true;
        AddTask($"Serve {currentServedNpc}/{5} Customers", npcSprite);

        doneTutorTask = true;
        Invoke(nameof(ShowTutorTask), 0.25f);
    }

    private void ShowTutorTask() { tutorialManager.OnGetTask(); }

    //show at UI
    private void AddTask(string taskName, Sprite Icon) 
    {
        currentTaskGO = Instantiate(taskPrefab, taskList);
        currentTaskGO.transform.GetChild(2).GetComponent<Image>().sprite = Icon;
        currentTaskText = currentTaskGO.transform.GetChild(1).GetComponent<Text>();
        
        currentTaskText.text = taskName;

        UIManager.Instance.taskGetUIText.text = "You now have a new Task:\n" + taskName + "!";
        UIManager.Instance.OpenTaskUI();
    }

    public void OnCollectedGold(int gold) 
    {
        if (!TaskGoldB) 
            return;

        currentTaskGold += gold;
        currentTaskText.text = $"Collect {currentTaskGold}/{TaskGold} Cash";
        if (currentTaskGold >= TaskGold) 
        {
            //Completed Task? Gain XP
            GlobalVar.Instance.AddXP(GameManager.Instance.taskXP);

            TaskGoldB = false;
            Destroy(currentTaskGO);

            GenerateNewRandomTask();
            UIManager.Instance.UpdateUI();
        }
    }

    public void OnNpcServed() 
    {
        if (!TaskNpc) return;

        currentServedNpc++;
        currentTaskText.text = $"Serve {currentServedNpc}/{serveNpcAmount} Customers";
        if (currentServedNpc >= serveNpcAmount) 
        {
            //Completed Task? Gain XP
            GlobalVar.Instance.AddXP(GameManager.Instance.taskXP);

            TaskNpc = false;
            Destroy(currentTaskGO);

            GenerateNewRandomTask();
            UIManager.Instance.UpdateUI();
        }
    }

    public void OnNPCTookShopItem(ShopperType type) 
    {
        if (!TaskShop) return;

        if (ShopperTypeToTask == type) 
        {
            shopperTypeCurrent++;
            currentTaskText.text = $"Sell: '{ShopperTypeToTask}' - {shopperTypeCurrent}/{shopperTypeMax}";
            
            if (shopperTypeCurrent >= shopperTypeMax) 
            {
                //Completed Task? Gain XP
                GlobalVar.Instance.AddXP(GameManager.Instance.taskXP);

                TaskShop = false;
                Destroy(currentTaskGO);

                GenerateNewRandomTask();
                UIManager.Instance.UpdateUI();
            }
        }
    }

    public void OnCaughtThief() 
    {
        if (!TaskThief) return;

        thiefToCatchCurrent++;
        currentTaskText.text = $"Catch {thiefToCatchCurrent}/{thiefToCatchMax} Thieves";
        if (thiefToCatchCurrent >= thiefToCatchMax)
        {
            //Completed Task? Gain XP
            GlobalVar.Instance.AddXP(GameManager.Instance.taskXP);

            TaskThief = false;
            Destroy(currentTaskGO);

            GenerateNewRandomTask();
            UIManager.Instance.UpdateUI();
        }
    }
}
