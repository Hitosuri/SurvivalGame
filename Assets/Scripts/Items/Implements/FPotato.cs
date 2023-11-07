using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts.Items.Implements {
    public class FPotato : FoodItem, PlantableItem {
        public override string SpritePath => "Sprite/Id84_Potato";
        public override int Id => 84;
        public override float HealthAmount => 15f;
        public override float HungerAmount => 20f;
        public override float ThirstAmount => 5f;
        public string PrefabPath => "prefabs/plants/PotatoPlant";

        public List<Tuple<float, string>> GrowPhase { get; } = new() {
            new Tuple<float, string>(3f, "Sprite/Plant/Crops/Potato/Potato_Sprout_01"),
            new Tuple<float, string>(4f, "Sprite/Plant/Crops/Potato/Potato_Plant")
        };

        public void Plant() {
            Vector3 x = Owner.transform.position;
            Vector3Int soilPosition = new Vector3Int(Mathf.FloorToInt(x.x), Mathf.FloorToInt(x.y));
            if (Owner.CurrentMouseSide == 1) {
                soilPosition += Vector3Int.down;
            } else if (Owner.CurrentMouseSide == 2) {
                soilPosition += Vector3Int.left;
            } else if (Owner.CurrentMouseSide == 3) {
                soilPosition += Vector3Int.up;
            } else if (Owner.CurrentMouseSide == 4) {
                soilPosition += Vector3Int.right;
            }

            if (WHoe.AlreadyPlant.Contains(soilPosition)) {
                return;
            }

            WHoe.AlreadyPlant.Add(soilPosition);

            if (GameManager.Instance.SoilLayer.GetTile(soilPosition) != null) {
                var loadedPrefabResource = Resources.Load<GameObject>(PrefabPath);
                var gameObject = Object.Instantiate(
                    loadedPrefabResource, soilPosition + new Vector3(0.5f, 0.4f), Quaternion.identity
                );
                gameObject.transform.parent = GameObject.Find("PlantOnSoil")?.transform;
                var renderer = gameObject.GetComponent<SpriteRenderer>();
                renderer.sortingOrder = Mathf.CeilToInt((soilPosition.y + 0.4f - 0.2f) * 4 * -1);
                GrowOverPhase(0, renderer);
            }
        }

        public void GrowOverPhase(int phaseIndex, SpriteRenderer renderer) {
            GameManager.Instance.CallDelay(
                () => {
                    renderer.sprite = Resources.Load<Sprite>(GrowPhase[phaseIndex].Item2);
                    if (phaseIndex + 1 < GrowPhase.Count) {
                        GrowOverPhase(phaseIndex + 1, renderer);
                    }
                }, GrowPhase[phaseIndex].Item1);
        }
    }
}