using Assets.Scripts.Enums;
using Assets.Scripts.Interface;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Items;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CharacterHUD : MonoBehaviour, IHudController {
    public UIDocument hud;

    VisualElement Inventory;
    ProgressBar Health;
    ProgressBar Thirsty;
    ProgressBar Hungry;
    VisualElement Hours;
    VisualElement Minutes;

    void Start() {
        var root = hud.rootVisualElement;
        Health = root.Q<ProgressBar>("Health");
        Thirsty = root.Q<ProgressBar>("Thirsty");
        Hungry = root.Q<ProgressBar>("Hungry");
        Hours = root.Q<VisualElement>("Hours");
        Minutes = root.Q<VisualElement>("Minutes");

        Inventory = root.Q<VisualElement>("Inventory");
        GameManager.Instance.HudController = this;
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
    }

    public void SetQuickSlotItem(int slotIndex, BaseItem item) {
        throw new System.NotImplementedException();
    }

    public void ClearQuickSlotItem(int slotIndex) {
        throw new System.NotImplementedException();
    }

    public void SelectSlot(int slotIndex) {
        throw new System.NotImplementedException();
    }

    public void SetInventoryItem(int slotIndex, BaseItem item) {
        throw new System.NotImplementedException();
    }

    public void ClearInventoryItem(int slotIndex) {
        throw new System.NotImplementedException();
    }
}