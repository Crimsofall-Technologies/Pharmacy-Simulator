using UnityEngine;
using UnityEngine.AI;

public class FillerMan : MonoBehaviour
{
	public Transform[] wanderPoints;
	public Transform shelfTarget;
	public float minWanderTime, maxWanderTime;
	
	private NavMeshAgent agent;
	private Animator animator;
	private Vector3 startPoint;
	private float lastWanderTime;
	private Vector3 wanderPoint;
	
	private Playershop shop;
	private Transform target, truckT;
	private bool PickedFromTruck;
	
	private void Start()
	{
		shop = GetComponentInParent<Playershop>();
		startPoint = transform.position;
		agent = GetComponent<NavMeshAgent>();
		animator = GetComponent<Animator>();
	}
	
	private void Update()
	{
		//update simple animations
		animator.SetBool("walk", agent.velocity.magnitude > 0);
		
		//fill backrooms!
		if(target != null)
		{
			float distance = Vector3.Distance(transform.position, target.position);
			
			if(distance <= 0.5f)
			{
				if(!PickedFromTruck)
				{
					target = shelfTarget;
					PickedFromTruck = true;
				}
				else
				{
					target = truckT;
					PickedFromTruck = false;
				}
			}
			
			agent.SetDestination(target.position);
			
			if(shop.refillingBackrooms==false) //go back to wandering.
			{
				target = null;
				wanderPoint = wanderPoints[Random.Range(0, wanderPoints.Length)].position;
				lastWanderTime = Time.time + Random.Range(minWanderTime, maxWanderTime);
			}
			
			return;
		}
		
		//move around the common point this npc was placed at.
		if(Time.time >= lastWanderTime)
		{
			wanderPoint = wanderPoints[Random.Range(0, wanderPoints.Length)].position;
			lastWanderTime = Time.time + Random.Range(minWanderTime, maxWanderTime);
		}
		
		agent.SetDestination(wanderPoint);
	}
	
	public void SetTarget(Transform T)
	{
		truckT = T;
		target = truckT;
		PickedFromTruck = true;
	}
}
