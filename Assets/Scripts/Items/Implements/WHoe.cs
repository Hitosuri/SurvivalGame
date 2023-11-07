using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Items.Implements {
    public class WHoe : WeaponItem {
        public override float UseAnimationLength => 0.36f;
        public override string SpritePath => "Sprite/Id51_Iron_Hoe";
        public override int Id => 51;
        public override string Name => "04. Melee Weapon/MW_IronHoe";
        public override int Type => WeaponType.Melee;
        public override int WeaponId => 3;
        public static List<Vector3Int> AlreadyPlant { get; } = new List<Vector3Int>();

        public override void Use() {
            base.Use();
            GameManager.Instance.CallDelay(
                () => {
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

                    GameManager.Instance.SoilLayer.SetTile(soilPosition, TerrainGenerator.soilTileBaseStatic);

                    string chunkName =
                        $"{Mathf.FloorToInt(soilPosition.x / 10f)}:{Mathf.FloorToInt(soilPosition.y / 10f)}";
                    MonoBehaviour.print(chunkName);
                    Transform chunkTransform = GameObject.Find(chunkName)?.transform;
                    List<GameObject> g = new List<GameObject>();
                    foreach (Transform childTranform in chunkTransform) {
                        Vector3Int plantPosition = new Vector3Int(
                            Mathf.FloorToInt(childTranform.position.x), Mathf.FloorToInt(childTranform.position.y)
                        );
                        if (plantPosition == soilPosition) {
                            g.Add(childTranform.gameObject);
                        }
                    }

                    foreach (var o in g) {
                        Object.Destroy(o);
                    }
                }, UseAnimationLength
            );
        }
    }
}