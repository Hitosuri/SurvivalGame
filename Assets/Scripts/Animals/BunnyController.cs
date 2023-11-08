using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BunnyController : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject Player;
    public float EnemyDistanceRun = 5;
    private Rigidbody2D NPCRigidbody;
    private float maxSped = 2;
    private Animator animator;
    private bool isMoving;
    private Vector3 currentPosition;
    void Start()
    {
        NPCRigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentPosition = transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        float distance = Vector3.Distance(transform.position, Player.transform.position);
        Vector3 nextPostion = transform.position;
        if(distance < EnemyDistanceRun)
        {
            Vector3 direction = transform.position - Player.transform.position;
            direction = Vector3.Normalize(direction);
            transform.rotation = Quaternion.Euler(direction);
            NPCRigidbody.AddForce(direction);
            NPCRigidbody.velocity = Vector3.ClampMagnitude(NPCRigidbody.velocity, maxSped);
            float angle = Vector3.SignedAngle(Vector3.forward, direction, Vector3.up);
            if (NPCRigidbody.velocity != Vector2.zero)
            {
                if ((-0.70f < direction.x && direction.x < 0.70f) || (0.70f < direction.y && direction.y < 1f))
                {

                    animator.SetFloat("Rotate", 3);
                }
                else if ((-0.70f < direction.x && direction.x < 0.70f) || (-0.70f > direction.y && direction.y > -1f))
                {
                    animator.SetFloat("Rotate", 1);
                }
                else if ((0.70f < direction.x && direction.x < 1f) || (-0.70f < direction.y && direction.y < 0.70f))
                {
                    animator.SetFloat("Rotate", 2);
                }
                else if ((-0.70f > direction.x && direction.x > -1f) || (-0.70f < direction.y && direction.y < 0.70f))
                {
                    transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                    animator.SetFloat("Rotate", 2);
                }
                    CheckActionPlayer();
                transform.hasChanged = false;
            }
            else
            {
                animator.SetFloat("Speed", 0);
                animator.SetFloat("MoveSpeed", 0);
            }

        }
        else
        {
            NPCRigidbody.velocity = Vector3.zero;
            animator.SetFloat("Speed", 0);
            animator.SetFloat("MoveSpeed", 0);
        }
    }
    void CheckActionPlayer()
    {
        Animator animatorPlayer = Player.GetComponent<Animator>();
        if (animatorPlayer.GetFloat("Speed") == 1)
        {
            animator.SetFloat("Speed", 1);
            animator.SetFloat("MoveSpeed", 1);
        }
        else
        {
            animator.SetFloat("Speed", 2);
            animator.SetFloat("MoveSpeed", 1);
        }
    }
}
