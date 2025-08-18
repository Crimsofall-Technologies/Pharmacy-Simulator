using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

//The NPC that will buy items from Shop/Pharmacy
public class Shopper : MonoBehaviour
{
	public List<SellableItem> Order = new();
	public ShopperType[] Types = new ShopperType[0];
	public float orderCompleteDelay = 3f;
	public float rotationSpeed;
	public bool IsThief;
	
	private Playershop shop;
	private NPCAreasManager areasManager;
	private NavMeshAgent agent;
	private Transform target = null;
	private bool OrderComplete, hasTakenSomething, completingOrder, waitingAtCashier;

	public bool hasPaid { get; private set; }

    public bool waitingForOrder { get; private set; }
	public bool leavingShop { get; private set; }

    private float remainingDistance;
	private Bubble myBubble;
	private UIManager UI;
	private Animator animator;
	private float lastCheckTime;
	private float orderCompleteTimeLeft = 0f;
	public int shopperId { get; private set; } //0 will later mean this is not inited successfully!

    public int currentIndex = 0;
	
	//just spawned?
	public void Init(SellableItem[] order, ShopperType[] shopperTypes, Playershop s, NPCAreasManager a)
	{
		animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
		UI = UIManager.Instance;
		shopperId = GameManager.Instance.GetNewShopperId();
		
		currentIndex = 0;
		Order = order.ToList();
		shop = s;
		areasManager = a;
		completingOrder = false;
		Types = shopperTypes;

		target = shop.GetClosePosition(Types[currentIndex], this);
		shop.AreaOccupied(this, target);
	}
	
	public void OnBubbleCreated(Bubble bub)
	{
		myBubble = bub;
		
		//automatically complete orders for player by pressing automatically after 2 seconds:
		if(GameManager.Instance.perksManager.AutoFill) 
		{
			Invoke(nameof(OnBubbleClicked), 2f);
		}
	}
	
	public void OnBubbleClicked()
	{	
		//It takes time to fill stock for player:
		completingOrder = true;
		orderCompleteTimeLeft = orderCompleteDelay * (GameManager.Instance.perksManager.DoubleSpeed ? 0.5f:1f);
	}
	
	private void Delayed_CompletedOrder()
	{
        //remove everything from UI as well!
        if (myBubble != null)
            Destroy(myBubble.gameObject);

        //means shopper has brought something (avoid leaving shop without paying)
        hasTakenSomething = true;
        completingOrder = false;

		if (leavingShop)
			return;

		if (IsThief) 
		{
			OnCaughtStealing();
            return;
		}

        //paid at cashier? make player move away and get destroyed!
        if (waitingAtCashier)
        {
            DelayedPay();
            return;
        }

        //move away and pay at cashier table.
        shop.OnShopperOrderCompleted(this, target);
		
		//has shopper shopped all items they want?
		if(currentIndex + 1 < Types.Length)
		{
            //task completed? but make sure this is the current item this npc is buying
            UI.taskList.OnNPCTookShopItem(Types[currentIndex]);

            //achievements:
            TryProgressAchievementsForCurrentType(Types[currentIndex]);

            currentIndex++;
            //is the next place still active? not empty i mean?
            if (shop.ShopHasType(Types[currentIndex]))
			{
                target = shop.GetClosePosition(Types[currentIndex], this); //move to next thing this shopper wants.
				
				OrderComplete = false;
				waitingForOrder = false;
			}
			else //pay at cashier and move away.
			{
				target = areasManager.GetCashierPosition();
				OrderComplete = true;
			}
		}
		else
		{
			//means shopper has got all they needed and are ready to pay & leave the shop:
            TutorialManager.Instance.OnNPCPay();
            target = areasManager.GetCashierPosition();
			OrderComplete = true;

            //task completed? but make sure this is the current item this npc is buying
            UI.taskList.OnNPCTookShopItem(Types[currentIndex]);

            //achievements:
            TryProgressAchievementsForCurrentType(Types[currentIndex]);
        }
	}

    private void Update()
	{
		//update animations
		if(animator != null)
			animator.SetBool("walk", agent.velocity.magnitude > 0f);

        if (myBubble != null)
            myBubble.SetPosition(UI.cam.WorldToScreenPoint(transform.position));
			
		if(completingOrder)
		{
			orderCompleteTimeLeft -= Time.deltaTime;
			if(myBubble != null) 
			{
				if (!waitingAtCashier)
					myBubble.Fill(orderCompleteTimeLeft, orderCompleteDelay, currentIndex); //at index now shopper is on
				else
					myBubble.Fill(orderCompleteTimeLeft, orderCompleteDelay, 0); //always at default starting index!
			}
			if(orderCompleteTimeLeft <= 0f)
			{
				orderCompleteTimeLeft = 0f;
				Delayed_CompletedOrder();
			}

			//while this player was standing the shelves emptied? leave.
			if (!shop.ShopHasType(Types[currentIndex]) && !hasTakenSomething) 
			{
                OrderComplete = true;
                hasPaid = true;
                leavingShop = true;
                target = areasManager.GetAwayAreaPosition(IsThief);
                if (myBubble != null)
                    Destroy(myBubble.gameObject);
            }

			return;
		}

        if (target == null)
            return;

        //remove after reaching the end point after shopping.
        if (leavingShop)
		{
			agent.updateRotation = true;
			agent.SetDestination(target.position);
			CalculateRemainingDistance();

			if(remainingDistance <= 2f) //destroy
			{
				GameManager.Instance.npcSpawner.Shoppers.Remove(this);
				Destroy(gameObject);
			}
			return;
		}
		
		//check if the area is not empty every 1 sec? be sure not to check once shopper has grabbed stuff from shop.
		if(Time.time >= lastCheckTime && !OrderComplete)
		{
			lastCheckTime = Time.time + 1f;
			
			//just move out if the thing shopper is looking for does not exist!
			if(!shop.ShopHasType(Types[currentIndex]) && !hasTakenSomething) 
			{
				OrderComplete = true;
				hasPaid = true;
				leavingShop = true;

				//release area when shopper moves away from it.
				shop.ClearArea(target, Types[currentIndex]);

				target = areasManager.GetAwayAreaPosition(IsThief);
				if(myBubble != null)
					Destroy(myBubble.gameObject);
				return;
			}

			//is my area taken? no worries take another - ignore if taken by myself!
			int o = shop.IsAreaOccupied(target, Types[currentIndex]);
            if (o > 0 && o != shopperId)
			{
				//take another area!
				target = shop.GetClosePosition(Types[currentIndex], this);
				return;
			}
		}

        agent.updateRotation = true;
        agent.SetDestination(target.position);
		CalculateRemainingDistance();

        if (remainingDistance <= 2f)
        {
        	//make sure the agent is stopped before facing the region
        	if(agent.velocity.magnitude <= 0.25f)
        	{
        		TargetPointer pointer = target.GetComponent<TargetPointer>();
	        	if(pointer != null)
		        	FaceTarget(pointer.IntrestPoint.position);
        	}
        	
	        if (OrderComplete)
	        {
				if (hasPaid)
				{
					//has this shopper paid already? destroy the object.
					GameManager.Instance.npcSpawner.Shoppers.Remove(this);
					Destroy(gameObject);
				}
				else
				{
					//pay at cashier
					target = null;
					waitingAtCashier = true;

					//create a bubble player clicks to get money!
					UI.CreateBubble(this, true);
				}
			}
	        else if (!waitingForOrder) //means shopper is close to area they need to shop at?
	        {
	        	//wait until player clicks to complete the order.
				UI.CreateBubble(this, false, IsThief);
				
				if(IsThief) //just watch an area and leave shop immediately taking money from player
                    Invoke(nameof(DelayedPay), 8.5f); 
				else
					TutorialManager.Instance.OnFirstNPC();

		        waitingForOrder = true;

                //this area is occupied make it be done right when NPC sets target, so no other NPC will take it's place.
                shop.AreaOccupied(this, target);
            }
        }
        else 
			agent.updateRotation = true;
	}
	
	private void DelayedPay()
	{
		//player caught the thief?
		if (IsThief && completingOrder)
			return;

		if (myBubble != null) //called when player actually clicked on it
			Destroy(myBubble.gameObject);

		//make shopper pay up!
		if (!IsThief)
		{
			shop.OnShopperPaid(this);
		}
		else if (!leavingShop)
		{
			//steal if not caught in time.
			GlobalVar.Instance.RemoveCurrencyOnZero(Random.Range(50, 100));

			shop.ClearArea(target, Types[currentIndex]);
		}

		//task completed?
		if (hasPaid == false)
		{
			UI.taskList.OnNpcServed();

			//achievements:
			UI.achievementManager.ProgressAchievement("Serve 100 Npc");
            UI.achievementManager.ProgressAchievement("Serve 50 Npc");
        }

		OrderComplete = true; //make it sure.
		target = areasManager.GetAwayAreaPosition(IsThief);
		hasPaid = true;
    }

    private void CalculateRemainingDistance()
	{
	    Vector3 b = target.position;
		b.y = transform.position.y;
		remainingDistance = Vector3.Distance(transform.position,b);
    }

    public string GetTaskName()
	{
		string s = "";
		int cost = 0;
		for (int i = 0; i < Order.Count; i++) {
			s += Order[i].Name + " | ";
			cost += Order[i].Cost;
		}
		return s + "\nPrice: $"+cost;
	}
	
	public void FaceTarget(Vector3 TargetPos)
	{
		agent.updateRotation = false;
		Vector3 angle = (TargetPos - transform.position).normalized;
		Quaternion lookRotation = Quaternion.LookRotation(new Vector3(angle.x, 0, angle.z));
		transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
	}

	public void OnCaughtStealing() 
	{
        UI.taskList.OnCaughtThief();

        //also progress achievements:
        UI.achievementManager.ProgressAchievement("Catch 15 thieves");
        UI.achievementManager.ProgressAchievement("Catch 40 thieves");

		shop.ClearArea(target, Types[currentIndex]);

        target = areasManager.GetAwayAreaPosition(IsThief);
        hasPaid = true;
		leavingShop = true;
        agent.SetDestination(target.position);
    }

    private void TryProgressAchievementsForCurrentType(ShopperType Type)
    {
		if (Type == ShopperType.Consultation) UI.achievementManager.ProgressAchievement("Sell 50 Consultation");
        if (Type == ShopperType.Pharmacy) UI.achievementManager.ProgressAchievement("Sell 50 Pharmacy");
        if (Type == ShopperType.Groceries) UI.achievementManager.ProgressAchievement("Sell 50 Groceries");
        if (Type == ShopperType.Drinks) UI.achievementManager.ProgressAchievement("Sell 50 Drinks");
        if (Type == ShopperType.IceCream) UI.achievementManager.ProgressAchievement("Sell 50 Ice Creams");
    }
}

public enum ShopperType
{
	Consultation, IceCream, Pharmacy, Drinks, Groceries, 
	None //just null!
}
