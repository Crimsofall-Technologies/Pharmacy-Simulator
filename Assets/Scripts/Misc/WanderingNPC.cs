using UnityEngine;
using UnityEngine.AI;

public class WanderingNPC : MonoBehaviour
{
    public Transform[] wanderPoints;
    public float minWanderTime, maxWanderTime;

    private NavMeshAgent agent;
    private Animator animator;
    private Vector3 startPoint;
    private float lastWanderTime;
    private Vector3 wanderPoint;

    private void Start()
    {
        startPoint = transform.position;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        //update animations
        animator.SetBool("walk", agent.velocity.magnitude > 0);

        //just for moving from one place to other and standing?
        if (wanderPoints.Length > 1) 
        {
            agent.SetDestination(wanderPoints[0].position);
            return;
        }

        //move around the common point this npc was placed at.
        if (Time.time >= lastWanderTime)
        {
            wanderPoint = wanderPoints[Random.Range(0, wanderPoints.Length)].position;
            lastWanderTime = Time.time + Random.Range(minWanderTime, maxWanderTime);
        }

        agent.SetDestination(wanderPoint);
    }
}
