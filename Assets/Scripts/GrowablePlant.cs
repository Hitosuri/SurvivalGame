using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Items;
using Assets.Scripts.Items.Implements;
using UnityEngine;
using Random = System.Random;
using RandomUnity = UnityEngine.Random;

public class GrowablePlant : MonoBehaviour {
    public PlantableItem PlantableItem { get; set; }
    private SpriteRenderer _renderer;

    private void Start() {
        _renderer = GetComponent<SpriteRenderer>();
        GrowOverPhase(0);
    }

    public void GrowOverPhase(int phaseIndex) {
        _renderer.sprite = Resources.Load<Sprite>(PlantableItem.GrowPhase[phaseIndex].Item2);
        if (phaseIndex + 1 < PlantableItem.GrowPhase.Count) {
            GameManager.Instance.CallDelay(
                () => {
                    GrowOverPhase(phaseIndex + 1);
                }, PlantableItem.GrowPhase[phaseIndex + 1].Item1
            );
        } else {
            GenerateProduct();
        }
    }

    public void GenerateProduct() {
        Vector3 position = transform.position + (Vector3)(RandomUnity.insideUnitCircle + Vector2.up) / 2;
        var x = Instantiate(
            GameManager.Instance.DroppedItemTemplate, position,
            Quaternion.identity
        );
        x.GetComponent<DroppedItem>().ItemData = new FPotato();
        GameManager.Instance.CallDelay(
            GenerateProduct,
            GameManager.Instance.DayLengthInSeconds + new Random().Next(GameManager.Instance.DayLengthInSeconds)
        );
    }
}