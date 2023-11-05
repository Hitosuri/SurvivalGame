namespace Assets.Scripts.Items {
    public abstract class StackableItem : BaseItem {
        public int Quantity { get; set; } = 0;
    }
}