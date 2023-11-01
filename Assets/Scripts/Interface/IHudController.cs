using Assets.Scripts.Enums;

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

        public void SetQuickSlotItem(int slotIndex, GameItems item);

        public void CleatQuickSlotItem(int slotIndex);

        public void SelectSlot(int slotIndex);
    }
}