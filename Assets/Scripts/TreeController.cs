using Assets.Scripts.Items.Implements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeController : MonoBehaviour {
    public bool dropApple;
    private bool cantrigger = true;
    private Animator animator;
    private System.Random random = new System.Random();
    private int hitCount = 0;

    void Start() {
        animator = GetComponent<Animator>();
    }

    void Update() { }

    void OnTriggerEnter2D(Collider2D collider) {
        if (cantrigger && collider.tag == "PlayerHitbox") {
            if (hitCount == 20) {
                Destroy(gameObject);
            }

            cantrigger = false;
            collider.gameObject.SetActive(false);
            PlayerController player = GameManager.Instance.Player;
            bool alreadyDropped = false;
            if (dropApple && random.Next(100) < 10) {
                var x = Instantiate(
                    GameManager.Instance.DroppedItemTemplate,
                    transform.position + (Vector3)Random.insideUnitCircle * 1.5f,
                    Quaternion.identity
                );
                x.GetComponent<DroppedItem>().ItemData = new FApple();
                alreadyDropped = true;
            }

            if (!alreadyDropped && player.QuickSlotItems[player.SelectedQuickSlotItemIndex].Id == 5
                                && random.Next(100) < 20) {
                var x = Instantiate(
                    GameManager.Instance.DroppedItemTemplate,
                    transform.position + (Vector3)Random.insideUnitCircle * 1.5f,
                    Quaternion.identity
                );
                x.GetComponent<DroppedItem>().ItemData = new SWoodBranch();
            }

            hitCount++;

            animator.SetTrigger("DoHit");
            GameManager.Instance.CallDelay(
                () => {
                    cantrigger = true;
                }, 0.1f
            );
        }
    }
}