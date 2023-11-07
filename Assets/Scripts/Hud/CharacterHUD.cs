using Assets.Scripts.Enums;
using Assets.Scripts.Interface;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Items;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;
using System;
using System.IO;
using System.Linq;

public class CharacterHUD : MonoBehaviour, IHudController {
    public UIDocument hud;

    VisualElement Inventory;
    ProgressBar Health;
    ProgressBar Thirsty;
    ProgressBar Hungry;
    VisualElement Hours;
    VisualElement Minutes;
    VisualElement SlotIcon1, SlotIcon2, 
        SlotIcon3, SlotIcon4, SlotIcon5, 
        SlotIcon6, SlotIcon7, SlotIcon8, 
        SlotIcon9, SlotIcon10, SlotIcon11, 
        SlotIcon12;
    VisualElement Slot1, Slot2,
    Slot3, Slot4, Slot5;
    VisualElement Slot1_Container, Slot2_Container,
Slot3_Container, Slot4_Container, Slot5_Container;

    private Vector2 mousePosition = Vector2.zero;
    private bool ismouseDown;
    private bool isBagClose = true;
    VisualElement dragFrom;


    private Texture2D[] ItemTexture;
    void Start() {
        var root = hud.rootVisualElement;
        Health = root.Q<ProgressBar>("Health");
        Thirsty = root.Q<ProgressBar>("Thirsty");
        Hungry = root.Q<ProgressBar>("Hungry");
        Hours = root.Q<VisualElement>("Hours");
        Minutes = root.Q<VisualElement>("Minutes");

        Inventory = root.Q<VisualElement>("Inventory");

        Slot1_Container = root.Q<VisualElement>("Slot1_Container");
        Slot2_Container = root.Q<VisualElement>("Slot2_Container");
        Slot3_Container = root.Q<VisualElement>("Slot3_Container");
        Slot4_Container = root.Q<VisualElement>("Slot4_Container");
        Slot5_Container = root.Q<VisualElement>("Slot5_Container");

        SlotIcon1 = root.Q<VisualElement>("SlotIcon1");
        SlotIcon2 = root.Q<VisualElement>("SlotIcon2");
        SlotIcon3 = root.Q<VisualElement>("SlotIcon3");
        SlotIcon4 = root.Q<VisualElement>("SlotIcon4");
        SlotIcon5 = root.Q<VisualElement>("SlotIcon5");
        SlotIcon6 = root.Q<VisualElement>("SlotIcon6");
        SlotIcon7 = root.Q<VisualElement>("SlotIcon7");
        SlotIcon8 = root.Q<VisualElement>("SlotIcon8");
        SlotIcon9 = root.Q<VisualElement>("SlotIcon9");
        SlotIcon10 = root.Q<VisualElement>("SlotIcon10");
        SlotIcon11 = root.Q<VisualElement>("SlotIcon11");
        SlotIcon12 = root.Q<VisualElement>("SlotIcon12");

        Slot1 = root.Q<VisualElement>("Slot1");
        Slot2 = root.Q<VisualElement>("Slot2");
        Slot3 = root.Q<VisualElement>("Slot3");
        Slot4 = root.Q<VisualElement>("Slot4");
        Slot5 = root.Q<VisualElement>("Slot5");

        
        List<VisualElement> visualElements = new List<VisualElement>() 
        { Slot1, Slot2, Slot3, Slot4, Slot5,
        SlotIcon1, SlotIcon2, SlotIcon3, SlotIcon4, SlotIcon5,
        SlotIcon6, SlotIcon7, SlotIcon8, SlotIcon9, SlotIcon10,
        SlotIcon11, SlotIcon12};
        ItemTexture = Resources.LoadAll<Texture2D>("Sprite");
        GameManager.Instance.HudController = this;

        hud.rootVisualElement.RegisterCallback<MouseMoveEvent>(OnMouseMoveEvent, TrickleDown.TrickleDown);
        hud.rootVisualElement.RegisterCallback<MouseUpEvent>(OnMouseUpEvent, TrickleDown.TrickleDown);
        foreach (VisualElement itm in visualElements)
        {
            itm.RegisterCallback<MouseDownEvent>(OnMouseDownEventX =>
            {
                if (isBagClose) {
                    return;
                }
                dragFrom = itm;
                mousePosition = OnMouseDownEventX.mousePosition;
                ismouseDown = true;
            }, TrickleDown.TrickleDown);

            itm.RegisterCallback<MouseEnterEvent>(OnMouseEnterEvent => {
                if (isBagClose) {
                    return;
                }
                if (dragFrom != null)
                {
                    SwapItems(dragFrom, itm);
                }

            }, TrickleDown.TrickleDown);
        }
    }

    public void SetHealth(float health) {
        if (GameManager.Instance == null) {
            return;
        }
        Health.value = health * GameManager.Instance.Player.MaxHealth;
    }

    public void SetHunger(float hunger) {
        if (GameManager.Instance == null) {
            return;
        }
        Hungry.value = hunger * GameManager.Instance.Player.MaxHunger;
    }

    public void SetThirsty(float thirsty) {
        if (GameManager.Instance == null) {
            return;
        }
        Thirsty.value = thirsty * GameManager.Instance.Player.MaxThirst;
    }

    public void SetTime(int minutes) {
        float hour = Mathf.FloorToInt(minutes / 60);
        float minute = minutes % 60;
        float degOfHour = (30 * hour) + (minute / 2);
        float degOfMinute = 6 * minute;
        Hours.transform.rotation = Quaternion.Euler(0, 0, degOfHour);
        Minutes.transform.rotation = Quaternion.Euler(0, 0, degOfMinute);
    }

    public void SetBagState(bool isOpened) {
        Inventory.style.display = isOpened ? DisplayStyle.Flex : DisplayStyle.None;
        isBagClose = !isOpened;
    }

    public void SetQuickSlotItem(int slotIndex, BaseItem item) {
        VisualElement[] slots = new VisualElement[] { Slot1, Slot2, Slot3, Slot4, Slot5 };
        SetSlotItem(slots, slotIndex, item);
    }


    public void ClearQuickSlotItem(int slotIndex) {
        VisualElement[] slots = new VisualElement[] { Slot1, Slot2, Slot3, Slot4, Slot5 };
        if (slotIndex >= 0 && slotIndex < slots.Length)
        {
            slots[slotIndex].style.backgroundImage = new StyleBackground(StyleKeyword.None);
        }
    }

    public void SelectSlot(int slotIndex) {
        VisualElement[] slots = new VisualElement[] { Slot1_Container, Slot2_Container, Slot3_Container, Slot4_Container, Slot5_Container };
        for(int i = 0; i < slots.Count(); i++)
        {
            if(i == slotIndex)
            {
                slots[i].AddToClassList("quicanim--up");
            }
            else
            {
                slots[i].RemoveFromClassList("quicanim--up");
            }
        }
    }

    public void SetInventoryItem(int slotIndex, BaseItem item) {
        VisualElement[] slots = new VisualElement[] {
            SlotIcon1, SlotIcon2, SlotIcon3,
            SlotIcon4, SlotIcon5, SlotIcon6, SlotIcon7,
            SlotIcon8, SlotIcon9, SlotIcon10, SlotIcon11, SlotIcon12 };
        SetSlotItem(slots, slotIndex, item);
    }

    public void ClearInventoryItem(int slotIndex) {
        VisualElement[] slots = new VisualElement[] {
            SlotIcon1, SlotIcon2, SlotIcon3,
            SlotIcon4, SlotIcon5, SlotIcon6, SlotIcon7,
            SlotIcon8, SlotIcon9, SlotIcon10, SlotIcon11, SlotIcon12 };
        if (slotIndex >= 0 && slotIndex < slots.Length)
        {
            slots[slotIndex].style.backgroundImage = new StyleBackground(StyleKeyword.None);
        }
    }

    void Update()
    {
    }

    private void OnMouseUpEvent(MouseUpEvent mouseEvent) {
        if (isBagClose || dragFrom == null) {
            return;
        }
        SetItemPosition(dragFrom, Vector2.zero);
        ismouseDown = false;
    }
    private void OnMouseMoveEvent(MouseMoveEvent mouseEvent) {
        if (isBagClose) {
            return;
        }
        if (ismouseDown)
        {
            Vector2 moveItem = mouseEvent.mousePosition - mousePosition;
            SetItemPosition(dragFrom, moveItem);
        }
        else
        {
            dragFrom = null;
        }
    }
    private static void SetItemPosition(VisualElement element, Vector2 vector)
    {
        element.style.left = vector.x;
        element.style.top = vector.y;
    }

    private void SwapItems(VisualElement from, VisualElement to)
    {
        StyleBackground style = null;
        style = from.style.backgroundImage;
        from.style.backgroundImage = to.style.backgroundImage;
        to.style.backgroundImage = style;
    }

    public string BaseItemName(BaseItem item) => Path.GetFileNameWithoutExtension(item.SpritePath);
    public Texture2D SearchItemTexture(Texture2D[] texture2Ds, string itemName)
    {
        Texture2D textureItem = texture2Ds.FirstOrDefault(t => t.name == itemName);
        return textureItem;
    }
    public void SetSlotItem(VisualElement[] slots, int slotIndex, BaseItem item)
    {
        Texture2D textureItem = SearchItemTexture(ItemTexture, BaseItemName(item));
        Sprite spriteItem = Sprite.Create(textureItem, new Rect(0.0f, 0.0f, textureItem.width, textureItem.height), new Vector2(0f, 0f));
        if (slotIndex >= 0 && slotIndex < slots.Length)
        {
            slots[slotIndex].style.backgroundImage = new StyleBackground(spriteItem);
        }
    }
}