namespace Assets.Scripts.Items.Implements {
    public class SWoodBranch : StackableItem {
        public override float UseAnimationLength => 0;
        public override string SpritePath => "Sprite/Id01_Wood_Branch";
        public override int Id => 1;
        public override bool CanPlaceOnQuickSlot => true;
        public override void Use() {
            throw new System.NotImplementedException();
        }
    }
}