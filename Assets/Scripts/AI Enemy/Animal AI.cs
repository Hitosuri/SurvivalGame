using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimalAI : MonoBehaviour
{
    private Rigidbody2D rb;

    public float lineOfSite;
    private Transform target;
    public float speed;
    private bool isFollowing = false;
    public float stoppingdistance;

    public Vector2 dir;

    public Transform[] patrolPoints;
    [SerializeField]
    public float waitTime;
    int currentPointIndex;
    bool once;

    private Animator Animator;
    private bool isInChaseRange;
    private int isAttack;

    //public bool rabit;

    public Image HeathBar;
    float MaxHeath = 100;
    float CurrentHeath;



    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        patrolPoints = GenerateRandomPatrolPoints();
        currentPointIndex = 0;
        Animator = gameObject.GetComponent<Animator>();
        target = GameObject.FindWithTag("Player").transform;

        CurrentHeath = MaxHeath;

    }

    // Update is called once per frame



    void Update()
    {

        Animator.SetBool("Speed", isInChaseRange);
        isInChaseRange = Physics2D.OverlapCircle(transform.position, lineOfSite);
        float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
        Animator.SetFloat("Horizontal", dir.x);
        Animator.SetFloat("Vertical", dir.y);
        Animator.SetInteger("Attack", isAttack);


    }
    void FixedUpdate()
    {
        float distanceFormPlayer = Vector2.Distance(target.position, transform.position);
        if (distanceFormPlayer <= lineOfSite && !isFollowing)
        {
            startFollowing();
        }
        else if (distanceFormPlayer > lineOfSite && isFollowing)
        {
            startPatrolling();
        }
        if (isFollowing)
        {
            FollowPlayer();
        }
        else
        {
            Patrol();
        }

        if (Input.GetButtonDown("Jump"))
        {
            Hit(10);
        }

    }


    private void startFollowing()
    {
        isFollowing = true;
        Debug.Log("Enemy is following the player.");
    }

    private void startPatrolling()
    {
        isFollowing = false;
        Debug.Log("Enemy is patrolling.");
        Patrol();
    }



    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, lineOfSite);
    }

    private void FollowPlayer()
    {
        if (Vector2.Distance(target.position, transform.position) > stoppingdistance)
        {
            isAttack = 0;
            dir = target.position - transform.position;
            rb.MovePosition((Vector2)transform.position + (dir * speed * Time.deltaTime));
        }
        else
        {
            //if (rabit)
            // {
            //    StartCoroutine(AttackAnimation());
            //}
            //else
            //{
            isAttack = 1;
            //}
        }
        dir.Normalize();


    }
    private void Patrol()
    {

        if (Vector2.Distance(transform.position, patrolPoints[currentPointIndex].transform.position) > 0.1)
        {
            dir = patrolPoints[currentPointIndex].transform.position - transform.position;
            dir.Normalize();
            //transform.position = Vector2.MoveTowards(transform.position, patrolPoints[currentPointIndex].transform.position, speed * Time.deltaTime);
            rb.MovePosition((Vector2)transform.position + (dir * speed * Time.deltaTime));
        }
        else
        {
            if (once == false)
            {
                Debug.Log("CHO");
                once = true;
                StartCoroutine(Wait());
            }
        }
    }


    IEnumerator Wait()
    {
        yield return new WaitForSeconds(waitTime);
        if (currentPointIndex + 1 < patrolPoints.Length)
        {
            currentPointIndex++;
            Debug.Log("DOI POIN");
        }
        else
        {
            currentPointIndex = 0;
        }
        once = false;

    }
    public Transform[] GenerateRandomPatrolPoints()
    {
        Transform[] patrolPoints = new Transform[4];

        for (int i = 0; i < 4; i++)
        {
            float randomX = UnityEngine.Random.Range(-10f, 10f);
            float randomY = UnityEngine.Random.Range(-5f, 5f);
            Vector3 randomPosition = new Vector3(randomX, randomY, 0f);

            GameObject emptyGameObject = new GameObject("PatrolPoint" + i);
            emptyGameObject.transform.position = randomPosition;
            patrolPoints[i] = emptyGameObject.transform;
        }

        return patrolPoints;
    }

    private IEnumerator AttackAnimation()
    {
        isAttack = 1;
        Vector3 initialPosition = transform.position;

        Vector3 jumpPosition = initialPosition + new Vector3(1f, 0, 0);
        float jumpDuration = 0.4f;
        float jumpTimer = 0f;

        while (jumpTimer < jumpDuration)
        {
            jumpTimer += Time.deltaTime;
            float t = jumpTimer / jumpDuration;
            transform.position = Vector3.Lerp(initialPosition, jumpPosition, t);
            yield return null;
        }


        float returnDuration = 0.4f;
        float returnTimer = 0f;

        while (returnTimer < returnDuration)
        {
            returnTimer += Time.deltaTime;
            float t = returnTimer / returnDuration;
            transform.position = Vector3.Lerp(jumpPosition, initialPosition, t);
            yield return null;
        }


        transform.position = initialPosition;
    }

    private void Hit(float dam)
    {
        CurrentHeath -= dam;
        HeathBar.fillAmount = CurrentHeath / MaxHeath;
        if (CurrentHeath <= 0)
        {
            Time.timeScale = 0;
        }
    }
}
