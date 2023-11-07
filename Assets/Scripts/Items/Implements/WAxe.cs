namespace Assets.Scripts.Items.Implements {
    public class WAxe : WeaponItem {
        public override float UseAnimationLength => 0.36f;
        public override string SpritePath => "Sprite/Id05_Stone_Ax";
        public override int Id => 5;
        public override string Name => "04. Melee Weapon/MW_StoneAxe";
        public override int Type => WeaponType.Melee;
        public override int WeaponId => 3;
    }
}