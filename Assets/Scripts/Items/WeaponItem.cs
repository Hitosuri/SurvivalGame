using Spine;
using Spine.Unity;

namespace Assets.Scripts.Items {
    public abstract class WeaponItem : BaseItem {
        public override bool CanPlaceOnQuickSlot => true;
        private Skin _weaponSkin;
        public abstract string Name { get; }
        public abstract int Type { get; }
        public abstract int WeaponId { get; }

        public Skin WeaponSkin {
            get {
                if (_weaponSkin == null) {
                    var skeletonData = Owner.PlayerMecanim.Skeleton.Data;
                    _weaponSkin = skeletonData.FindSkin(Name);
                }

                return _weaponSkin;
            }
        }

        public override void Use() {
            Owner.animator.SetTrigger("DoShot");
            Owner.PreventMoving = true;
            GameManager.Instance.CallDelay(
                () => {
                    Owner.PreventMoving = false;
                    Owner.CheckFire(true);
                    Owner.hitbox.gameObject.SetActive(true);
                    GameManager.Instance.CallDelay(
                        () => {
                            if (Owner.hitbox.gameObject.activeSelf) {
                                Owner.hitbox.gameObject.SetActive(false);
                            }
                        }, 0.1f
                    );
                }, UseAnimationLength
            );
        }
    }
}