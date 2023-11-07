namespace Assets.Scripts.Items.Implements {
    public class WPickaxe : WeaponItem {
        public override float UseAnimationLength => 0.36f;
        public override string SpritePath => "Sprite/Id178_StonePick";
        public override int Id => 178;
        public override string Name => "04. Melee Weapon/MW_StonePick";
        public override int Type => WeaponType.Melee;
        public override int WeaponId => 3;
    }
}