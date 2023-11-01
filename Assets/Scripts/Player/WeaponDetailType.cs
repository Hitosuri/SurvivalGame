namespace Assets {
    public class WeaponDetailType {
        public const string ParameterName = "WeaponDetailType";
        public static readonly SpineSkinInfo Melee = new(3, "", WeaponType.Melee);
        public static readonly SpineSkinInfo Pistol = new(4, "", WeaponType.Ranged);
        public static readonly SpineSkinInfo Smg = new(5, "", WeaponType.Ranged);
        public static readonly SpineSkinInfo Rifle = new(6, "", WeaponType.Ranged);
        public static readonly SpineSkinInfo Shotgun = new(7, "", WeaponType.Ranged);
        public static readonly SpineSkinInfo CrossBow = new(8, "05. Ranged Weapon/RW_Bow_Crossbow", WeaponType.Ranged);
        public static readonly SpineSkinInfo IncomExplosive = new(10, "04. Melee Weapon/THW_IncomExplosive", WeaponType.Melee);
        public static readonly SpineSkinInfo SlingshotMetal = new(11, "05. Ranged Weapon/RW_SlingShot_Metal", WeaponType.Ranged);
        public static readonly SpineSkinInfo SlingshotWood = new(11, "05. Ranged Weapon/RW_SlingShot_Wood", WeaponType.Ranged);
        public static readonly SpineSkinInfo IronLance = new(12, "04. Melee Weapon/Anim_IronLance", WeaponType.Melee);
        public static readonly SpineSkinInfo RockLance = new(12, "04. Melee Weapon/Anim_RockLance", WeaponType.Melee);
        public static readonly SpineSkinInfo BatMetal = new(13, "04. Melee Weapon/MW_MetalBat", WeaponType.Melee);
        public static readonly SpineSkinInfo BatWood = new(13, "04. Melee Weapon/MW_WoodBat", WeaponType.Melee);
        public static readonly SpineSkinInfo Sickle = new(14, "04. Melee Weapon/MW_Sickle", WeaponType.Melee);
    }
}