using Assets.Scripts.Enums;
using Assets.Scripts.Interface;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CharacterHUD : MonoBehaviour, IHudController
{
    public UIDocument hud;
    public float currentHealth = 100;
    [Range(0,1)]
    public float hearthPercent = 1;

    private float maxHeart = 100;
    private float thirstyPercent;
    private float hungryPercent;

    [Range(0, 100)]
    public float currentThirsty = 50;
    [Range(0, 100)]
    public float currentHungry = 50;
    public bool isDecrease = true;
    public bool isIncrease = true;

    public bool veryHungry = true;
    public bool veryThirsty = true;

    public int timeADay = 15;
    [Range(0, 1)]
    public float HungryPerDay;
    [Range(0, 1)]
    public float ThirstyPerDay;

    VisualElement Inventory;
    public bool InventoryOpenCheck = false;
    ProgressBar Health;
    ProgressBar Thirsty;
    ProgressBar Hungry;
    Label watchTimer;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.HudController = this;
        var root = hud.rootVisualElement;
        Health = root.Q<ProgressBar>("Health");
        Thirsty = root.Q<ProgressBar>("Thirsty");
        Hungry = root.Q<ProgressBar>("Hungry");
        watchTimer = root.Q<Label>("WatchTimer");
        Inventory = root.Q<VisualElement>("Inventory");
        currentHealth = Health.value;
        currentThirsty = Thirsty.value;
        currentHungry = Hungry.value;
        //StartCoroutine(ConditionOfPlayer(timeADay));
    }

    public void TakeDamage(int val)
    {
        currentHealth -= val;
        currentHealth = Mathf.Clamp(currentHealth, 0, 100);
        hearthPercent = (float) currentHealth / 100;
        Health.value = currentHealth;
    }

    public void Healing(int val)
    {
        currentHealth += val;
        currentHealth = Mathf.Clamp(currentHealth, 0, 100);
        hearthPercent = (float)currentHealth / 100;
        Health.value = currentHealth;
    }


    private IEnumerator ConditionOfPlayer(int timeADay)
    {
        while (true)
        {
            while(veryHungry || veryThirsty){
                HungryStatus(timeADay);
                ThirstyStatus(timeADay);
                yield return new WaitForSeconds(1f);
            }
            if (currentHungry <= 0 && currentThirsty <= 0)
            {
                isDecrease = true;
                Health.value -= 10;
            }
            else if (currentHungry >= 100 && currentThirsty >= 100)
            {
                isIncrease = true;
                Health.value += 10;
            }
            yield return new WaitForSeconds(5f);
        }
    }

    public void HungryStatus(int timeOneDay)
    {
        if(currentHungry > 0)
        {
            float hungryLost = maxHeart * HungryPerDay;
            float hungryLostPerSeconds = hungryLost / timeOneDay;
            Hungry.value -= hungryLostPerSeconds;
            currentHungry = Hungry.value;
            currentHungry = Mathf.Clamp(currentHungry, 0, 100);
        }
        else
        {
            veryHungry = false;
        }
    }

    public void ThirstyStatus(int timeOneDay)
    {
        if(currentThirsty > 0)
        {
            float thirstyLost = maxHeart * ThirstyPerDay;
            float thirstyLostPerSeconds = thirstyLost / timeOneDay;
            Thirsty.value -= thirstyLostPerSeconds;
            currentThirsty = Thirsty.value;
            currentThirsty = Mathf.Clamp(currentThirsty, 0, 100);
        }
        else
        {
            veryThirsty = false;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B) && InventoryOpenCheck == false)
        {
            Inventory.style.display = DisplayStyle.Flex;
            InventoryOpenCheck = true;
        }else if(Input.GetKeyDown(KeyCode.B) && InventoryOpenCheck == true)
        {
            Inventory.style.display = DisplayStyle.None;
            InventoryOpenCheck = false;
        }
    }

    public void SetHealth(float health)
    {
        currentHealth = health * maxHeart;
        Health.value = currentHealth;
    }

    public void SetHunger(float hunger)
    {
        currentHungry = hunger * maxHeart;
        Hungry.value = currentHungry;
    }

    public void SetThirsty(float thirsty)
    {
        currentThirsty = thirsty * maxHeart;
        Thirsty.value = currentThirsty;
    }

    public void SetTime(int minutes)
    {
        watchTimer.text = minutes.ToString();
    }

    public void SetQuickSlotItem(int slotIndex, GameItems item)
    {
        throw new System.NotImplementedException();
    }

    public void CleatQuickSlotItem(int slotIndex)
    {
        throw new System.NotImplementedException();
    }

    public void SelectSlot(int slotIndex)
    {
        throw new System.NotImplementedException();
    }
}
