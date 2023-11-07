namespace Assets.Scripts.Items {
    public abstract class FoodItem : StackableItem {
        public override float UseAnimationLength => 0.6f;
        public override bool CanPlaceOnQuickSlot => true;
        public abstract float HealthAmount { get; }
        public abstract float HungerAmount { get; }
        public abstract float ThirstAmount { get; }

        public override void Use() {
            Owner.PreventMoving = true;
            Owner.animator.SetBool("IsEat", true);
            GameManager.Instance.CallDelay(
                () => {
                    Owner.Health += HealthAmount;
                    Owner.Hunger += HungerAmount;
                    Owner.Thirst += ThirstAmount;
                    if (Owner.CheckFire(true)) {
                        Use();
                    } else {
                        Owner.PreventMoving = false;
                        Owner.animator.SetBool("IsEat", false);
                    }
                }, UseAnimationLength
            );
        }
    }
}