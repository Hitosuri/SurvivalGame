namespace Assets.Scripts.Items {
    public abstract class BaseItem {
        public PlayerController Owner { get; set; }
        public abstract float UseAnimationLength { get; }

        public abstract void Use();
    }
}