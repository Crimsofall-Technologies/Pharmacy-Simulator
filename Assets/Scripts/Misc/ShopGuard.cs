using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ShopGuard : MonoBehaviour
{
    public float lookRadius;
    public float catchRadius;
    public LayerMask checkMask;

    private NavMeshAgent agent;
    private Animator animator;
    
    private float lastCheckTime;
    private Shopper Target;
    private Vector3 post;
    private bool catchingThief = false;

    private void Start()
    {
        post = transform.position;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        //update animations:
        animator.SetBool("walk", agent.velocity.magnitude > 0f);

        if (catchingThief) {
            agent.SetDestination(transform.position);
            return;
        }

        if (Target != null)
        {
            agent.SetDestination(Target.transform.position);

            float dis = Vector3.Distance(transform.position, Target.transform.position);
            if (dis <= catchRadius)
            {
                catchingThief = true;
                StartCoroutine(Catch());
            }

            return;
        }
        else 
        {
            //back to post!
            agent.SetDestination(post);
        }

        if (Time.time >= lastCheckTime)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, lookRadius, checkMask);
            for (int i = 0; i < colliders.Length; i++)
            {
                Shopper shopper = colliders[i].GetComponent<Shopper>();
                if (shopper != null && shopper.IsThief && shopper.waitingForOrder && !shopper.leavingShop) //only catch standing thieves!
                {
                    Target = shopper;
                }
            }
            lastCheckTime = Time.time + 2f; //check every 2 seconds.
        }
    }

    private IEnumerator Catch() 
    {
        yield return new WaitForSeconds(2f);

        Target.OnBubbleClicked();
        Target = null;
        lastCheckTime = Time.time + 5f; //catch next after almost 5 seconds
        catchingThief = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }
}
