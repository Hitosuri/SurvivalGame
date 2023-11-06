using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCollection : MonoBehaviour {
    private PlayerController playerController;

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.tag == "DroppedItem") {
            var item = collider.gameObject.GetComponent<DroppedItem>();
            GameManager.Instance.Player.CollectableItems.Add(item);
        }
    }

    void OnTriggerExit2D(Collider2D collider) {
        if (collider.tag == "DroppedItem") {
            var item = collider.gameObject.GetComponent<DroppedItem>();
            GameManager.Instance.Player.CollectableItems.Remove(item);
        }
    }
}