using Assets.Scripts.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItem : MonoBehaviour {
    private SpriteRenderer _spriteRenderer;
    private BaseItem _itemData;

    public BaseItem ItemData {
        get => _itemData;
        set {
            _itemData = value;
            if (_spriteRenderer == null) {
                _spriteRenderer = GetComponent<SpriteRenderer>();
                _spriteRenderer.sortingOrder = Mathf.CeilToInt((transform.position.y - 0.2f) * 4 * -1);
            }
            _spriteRenderer.sprite = Resources.Load<Sprite>(_itemData.SpritePath);
        }
    }
}