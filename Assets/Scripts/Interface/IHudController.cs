using Assets.Scripts.Enums;
using Assets.Scripts.Items;

namespace Assets.Scripts.Interface {
    public interface IHudController {
        /// <summary>
        /// set gia tri cho heath bar
        /// </summary>
        /// <param name="health">co gia tri tu 0 toi 1 tuong ung 0: can mau, 1: day mau</param>
        public void SetHealth(float health);

        /// <summary>
        /// tuong tu health bar
        /// </summary>
        /// <param name="hunger">co gia tri tu 0 toi 1 tuong ung 0: doi, 1: no</param>
        public void SetHunger(float hunger);

        /// <summary>
        /// tuong tu health bar
        /// </summary>
        /// <param name="thirsty">co gia tri tu 0 toi 1 tuong ung 0</param>
        public void SetThirsty(float thirsty);

        /// <summary>
        /// set thoi gian cho dong ho
        /// </summary>
        /// <param name="minutes">so phut tinh tu luc 0h sang</param>
        public void SetTime(int minutes);

        public void SetBagState(bool isOpened);

        public void SetQuickSlotItem(int slotIndex, BaseItem item);

        public void ClearQuickSlotItem(int slotIndex);

        public void SelectSlot(int slotIndex);

        public void SetInventoryItem(int slotIndex, BaseItem item);

        public void ClearInventoryItem(int slotIndex);
    }
}