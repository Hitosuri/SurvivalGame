﻿namespace Assets.Scripts.Items.Implements {
    public class WLance : WeaponItem {
        public override float UseAnimationLength => 0.32f;
        public override string SpritePath => "Sprite/Id175_RockLance";
        public override int Id => 175;
        public override string Name => "04. Melee Weapon/Anim_IronLance";
        public override int Type => WeaponType.Melee;
        public override int WeaponId => 12;
    }
}