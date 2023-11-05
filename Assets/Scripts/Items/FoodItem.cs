namespace Assets.Scripts.Items {
    public abstract class FoodItem : BaseItem {
        public override float UseAnimationLength => 0.6f;
        public abstract float HungerAmount { get; }
        public abstract float ThirstAmount { get; }

        public override void Use() {
            Owner.animator.SetBool("IsEat", true);
            GameManager.Instance.CallDelay(
                () => {
                    Owner.Hunger += HungerAmount;
                    Owner.Thirst += ThirstAmount;
                    if (Owner.CheckFire(true)) {
                        Use();
                    } else {
                        Owner.animator.SetBool("IsEat", false);
                    }
                }, UseAnimationLength
            );
        }
    }
}