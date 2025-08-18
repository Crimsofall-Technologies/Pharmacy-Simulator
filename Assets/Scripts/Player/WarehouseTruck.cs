using UnityEngine;
using UnityEngine.AI;

public class WarehouseTruck : MonoBehaviour
{
	public GameObject[] Meshes;
	public Transform[] pickPoints;
	public Transform shopPoint;
	public Transform warehousePoint;
	public Playershop shop;
	public Warehouse warehouse;
	
	public bool MovingToShop;
	private NavMeshAgent agent;
	private Transform area;
	private Quaternion originalRotation;
	private Vector3 originalPosition;
	
	private bool IsActive, TimerComplete;
	private float distance;
	
	private void Start() 
	{
		originalPosition = transform.position;
		originalRotation = transform.rotation;
		agent = GetComponent<NavMeshAgent>();
		IsActive = false;
		TimerComplete = false;
		
		Meshes[0].SetActive(true);
		Meshes[1].SetActive(false);
	}
	
	private void Update() 
	{
		if(!IsActive)
			return;
		
		if(MovingToShop)
		{
			distance = Vector3.Distance(transform.position, shopPoint.position);
			if(!TimerComplete)
			{
				//stop now till timer is completed!
				agent.SetDestination(transform.position);
				return;
			}
			
			if(TimerComplete)
			{
				if(distance <= 1f) //went to shop already?
				{
					agent.SetDestination(transform.position);
					shop.OpenGateAndTakeStuff(pickPoints);
					IsActive = false;
				}
				else
				{
					agent.SetDestination(shopPoint.position);
				}
			}
		}
		else
		{
			distance = Vector3.Distance(transform.position, warehousePoint.position);
			if(distance <= 30f) //went to warehouse already?
			{
				warehouse.TruckReturned();
				IsActive = false;
				ResetSelf();
			}
		}
	}
	
	//after timer truck continues to the shop:
	public void ContinueWay()
	{
		TimerComplete = true;
	}
	
	public void GoToShop()
	{
		MovingToShop = true;
		IsActive = true;
		agent.SetDestination(shopPoint.position);
		TimerComplete = true;
		
		Meshes[0].SetActive(false);
		Meshes[1].SetActive(true);
		Invoke(nameof(PauseOnRoad), 3f);
	}
	
	private void PauseOnRoad()
	{
		//make sure Timer exists - it means if timer is not present player has brought it with Diamonds, if it's present it's normal
		//(i.e. player is waiting for it to complete!).
		if(TimerManager.TimerExistsNamed("BackRoomsFiller"))
			TimerComplete = false;
	}
	
	public void ReturnToWarehouse()
	{
		IsActive = true;
		MovingToShop = false;
		agent.SetDestination(warehousePoint.position);
		
		Meshes[0].SetActive(true);
		Meshes[1].SetActive(false);
	}
	
	private void ResetSelf()
	{
		TimerComplete = false;
		agent.Warp(originalPosition);
		transform.rotation = originalRotation;
	}
}
