using Spine;

namespace Assets.Scripts.Items.Implements {
    public class WCrossBow : WeaponItem {
        public override float UseAnimationLength => 0.32f;
        public override string SpritePath => "Sprite/Id41_CrossBow";
        public override int Id => 41;
        public override string Name => "05. Ranged Weapon/RW_Bow_Crossbow";
        public override int Type => WeaponType.Ranged;
        public override int WeaponId => 8;
    }
}