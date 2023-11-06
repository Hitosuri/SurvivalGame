namespace Assets.Scripts.Items {
    public abstract class BaseItem {
        public PlayerController Owner { get; set; }
        public abstract float UseAnimationLength { get; }
        public abstract string SpritePath { get; }
        public abstract int Id { get; }
        public abstract bool CanPlaceOnQuickSlot { get; }

        public abstract void Use();
    }
}