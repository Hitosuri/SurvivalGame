using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Assets;
using Assets.Scripts.Items;
using Assets.Scripts.Items.Implements;
using JetBrains.Annotations;
using Spine;
using Spine.Unity;
using Spine.Unity.AttachmentTools;
using Unity.Collections;
using UnityEngine;
using WorldTime;
using static System.Runtime.CompilerServices.RuntimeHelpers;

public class PlayerController : MonoBehaviour {
    public float runSpeed = 6f;
    public float walkSpeed = 1.8f;
    public float exhaustedSpeed = 1.6f;
    public float acceleration = 8f;
    public float decceleration = 12f;
    public float velocityPower = 0.9f;

    private Rigidbody2D rigidbody;
    [NonSerialized]
    public Animator animator;

    private MeshRenderer meshRenderer;
    private Bone weaponMainBone;
    private Vector2 deltraPos;
    private int currentSide = -1;

    private BaseItem[] bagItems;
    private BaseItem[] quickSlotItems;
    private bool isBagOpened = false;

    private int SelectedQuickSlotItemIndex {
        get => _selectedQuickSlotItemIndex;
        set {
            _selectedQuickSlotItemIndex = value;
            GameManager.Instance.HudController.SelectSlot(_selectedQuickSlotItemIndex);
            OnSelectedQuickSlotChange(_selectedQuickSlotItemIndex);
        }
    }

    public float MaxHealth => 100f;

    public float Health {
        get => _health;
        set {
            _health = Mathf.Clamp(value, 0, 100);
            GameManager.Instance.HudController?.SetHealth(_health / MaxHealth);
        }
    }

    public float MaxHunger => 100f;

    public float Hunger {
        get => _hunger;
        set {
            _hunger = Mathf.Clamp(value, 0, 100);
            animator.SetBool("Exhausted", _hunger <= 20 || _thirst <= 20);
            animator.SetBool("IsHunger", _hunger <= 20);
            GameManager.Instance.HudController?.SetHunger(_hunger / MaxHunger);
        }
    }

    public float MaxThirst => 100f;

    public float Thirst {
        get => _thirst;
        set {
            _thirst = Mathf.Clamp(value, 0, 100);
            animator.SetBool("Exhausted", _hunger <= 20 || _thirst <= 20);
            animator.SetBool("IsThirsty", _thirst <= 20);
            GameManager.Instance.HudController?.SetThirsty(_thirst / MaxThirst);
        }
    }

    private int _selectedQuickSlotItemIndex;
    private float _health;
    private float _hunger;
    private float _thirst;

    public bool PreventMoving { get; set; }
    public bool ReFire { get; set; }
    public SkeletonMecanim PlayerMecanim { get; set; }
    public List<DroppedItem> CollectableItems { get; set; }
    public int CurrentMouseSide { get; set; } = 1;

    private void Start() {
        GameManager.Instance.Player = this;
        rigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        PlayerMecanim = GetComponent<SkeletonMecanim>();
        bagItems = new BaseItem[12];
        quickSlotItems = new BaseItem[5];
        CollectableItems = new List<DroppedItem>();
        SetupBasicInfo();

        ReFire = false;
        var skeleton = PlayerMecanim.Skeleton;
        var centerBone = skeleton.FindBone("Center");
        deltraPos = centerBone.GetWorldPosition(transform) - transform.position;
        weaponMainBone = skeleton.FindBone("Weapon_Main");
        ChangeSkin(null);
        meshRenderer = GetComponent<MeshRenderer>();
        GameManager.Instance.OnPropertyChange(nameof(GameManager.HudController), () => {
            GameManager.Instance.HudController.SetBagState(false);
            Health = MaxHealth * 0.8f;
            Hunger = MaxHunger * 0.6f;
            Thirst = MaxThirst * 0.6f;
            TestSetup();
            SelectedQuickSlotItemIndex = 0;
        });
        GameManager.Instance.OneSecondTick += gameManager => {
            Hunger -= (MaxHunger * 0.3f) / gameManager.DayLengthInSeconds;
            Thirst -= (MaxThirst * 0.5f) / gameManager.DayLengthInSeconds;
            if (Hunger < 0.01f || Thirst < 0.01f) {
                Health -= MaxHealth / gameManager.DayLengthInSeconds;
            } else if (Hunger < 0.2f || Thirst < 0.2f) {
                Health -= (MaxHealth * 0.5f) / gameManager.DayLengthInSeconds;
            }
        };
    }

    private void TestSetup() {
        var x = Instantiate(GameManager.Instance.DroppedItemTemplate, Vector3.zero, Quaternion.identity);
        x.GetComponent<DroppedItem>().ItemData = new WAxe();

        quickSlotItems[0] = new WPickaxe() {
            Owner = this,
        };
        GameManager.Instance.HudController.SetQuickSlotItem(0, quickSlotItems[0]);

        quickSlotItems[1] = new WHoe() {
            Owner = this,
        };
        GameManager.Instance.HudController.SetQuickSlotItem(1, quickSlotItems[1]);

        quickSlotItems[2] = new FPotato() {
            Owner = this,
        };
        GameManager.Instance.HudController.SetQuickSlotItem(2, quickSlotItems[2]);
    }

    private void FixedUpdate() {
        if (Mathf.Abs(rigidbody.velocity.y) > 0.01) {
            UpdateSortingOrder();
        }
    }

    private void OnSelectedQuickSlotChange(int currentIndex) {
        BaseItem x = quickSlotItems[currentIndex];
        if (x != null) {
            if (x is WeaponItem p) {
                ChangeSkin(p);
            } else {
                ChangeSkin(null);
            }
        } else {
            ChangeSkin(null);
        }
    }

    public void SwitchSlot(int fromIndex, bool fromBag, int toIndex, bool toBag) {
        var from = fromBag ? bagItems : quickSlotItems;
        var to = toBag ? bagItems : quickSlotItems;
        (from[fromIndex], to[toIndex]) = (to[toIndex], from[fromIndex]);
    }

    private void UpdateSortingOrder() {
        meshRenderer.sortingOrder = Mathf.CeilToInt((rigidbody.position.y - 0.2f) * 4 * -1);
    }

    private void AddToInventory(BaseItem item) {
        if (item is StackableItem stackableItem) {
            int index = Array.FindIndex(bagItems, x => x?.Id == stackableItem.Id);
            if (index >= 0) {
                ((StackableItem)bagItems[index]).Quantity += stackableItem.Quantity;
                print($"bag {index} - quantity: {((StackableItem)bagItems[index]).Quantity}");
                GameManager.Instance.HudController.SetInventoryItem(index, item);
                return;
            }
            index = Array.FindIndex(quickSlotItems, x => x?.Id == stackableItem.Id);
            if (index >= 0) {
                ((StackableItem)quickSlotItems[index]).Quantity += stackableItem.Quantity;
                print($"quickslot {index} - quantity: {((StackableItem)quickSlotItems[index]).Quantity}");
                GameManager.Instance.HudController.SetQuickSlotItem(index, item);
                return;
            }
        }

        int emptyIndex = Array.FindIndex(quickSlotItems, x => x == null);
        if (emptyIndex >= 0) {
            quickSlotItems[emptyIndex] = item;
            print($"quickslot {emptyIndex}");
            GameManager.Instance.HudController.SetQuickSlotItem(emptyIndex, item);
            return;
        }
        emptyIndex = Array.FindIndex(bagItems, x => x == null);
        if (emptyIndex >= 0) {
            bagItems[emptyIndex] = item;
            print($"bag {emptyIndex}");
            GameManager.Instance.HudController.SetInventoryItem(emptyIndex, item);
        }
    }

    private void Update() {
        if (Time.timeScale == 0) {
            return;
        }

        if (Input.GetKeyDown(KeyCode.B)) {
            isBagOpened = !isBagOpened;
            GameManager.Instance.HudController?.SetBagState(isBagOpened);
        }

        if (Input.GetKeyDown(KeyCode.F)) {
            var collectableItemArray = CollectableItems.ToArray();
            for (int i = 0; i < collectableItemArray.Length; i++) {
                var item = collectableItemArray[i].ItemData;
                item.Owner = this;
                Destroy(collectableItemArray[i].gameObject);
                AddToInventory(item);
            }
        }

        var newSide = GetMouseSide(out var mouseAngle);
        CurrentMouseSide = newSide;
        if (currentSide != newSide && !isBagOpened) {
            currentSide = newSide;
            animator.SetFloat("Rotate", currentSide == 4 ? 2 : currentSide);
            UpdateAnimation(newSide);
        }

        if (newSide == 4) {
            mouseAngle = 180 - mouseAngle;
        }
        weaponMainBone.Rotation = mouseAngle;

        var controlVector = new Vector2 {
            x = Input.GetAxisRaw("Horizontal"),
            y = Input.GetAxisRaw("Vertical")
        };
        if (PreventMoving || isBagOpened) {
            controlVector = Vector2.zero;
        }

        bool backMove = false;
        bool sideMove = false;
        if (newSide == 1) {
            if (controlVector.x > 0.01) {
                animator.SetFloat("SideWalk", -1);
                animator.SetFloat("BackWalk", 0);
                sideMove = true;
            } else if (controlVector.x < -0.01) {
                animator.SetFloat("SideWalk", 1);
                animator.SetFloat("BackWalk", 0);
                sideMove = true;
            } else if (controlVector.y < -0.01) {
                animator.SetFloat("SideWalk", 0);
                animator.SetFloat("BackWalk", 0);
            } else if (controlVector.y > 0.01) {
                animator.SetFloat("SideWalk", 0);
                animator.SetFloat("BackWalk", 1);
                backMove = true;
            }
        } else if (newSide == 2) {
            if (controlVector.x > 0.01) {
                animator.SetFloat("SideWalk", 0);
                animator.SetFloat("BackWalk", 1);
                backMove = true;
            } else if (controlVector.x < -0.01) {
                animator.SetFloat("SideWalk", 0);
                animator.SetFloat("BackWalk", 0);
            } else if (controlVector.y < -0.01) {
                animator.SetFloat("SideWalk", -1);
                animator.SetFloat("BackWalk", 0);
                sideMove = true;
            } else if (controlVector.y > 0.01) {
                animator.SetFloat("SideWalk", 1);
                animator.SetFloat("BackWalk", 0);
                sideMove = true;
            }
        } else if (newSide == 3) {
            if (controlVector.x > 0.01) {
                animator.SetFloat("SideWalk", 1);
                animator.SetFloat("BackWalk", 0);
                sideMove = true;
            } else if (controlVector.x < -0.01) {
                animator.SetFloat("SideWalk", -1);
                animator.SetFloat("BackWalk", 0);
                sideMove = true;
            } else if (controlVector.y < -0.01) {
                animator.SetFloat("SideWalk", 0);
                animator.SetFloat("BackWalk", 1);
                backMove = true;
            } else if (controlVector.y > 0.01) {
                animator.SetFloat("SideWalk", 0);
                animator.SetFloat("BackWalk", 0);
            }
        } else if (newSide == 4) {
            if (controlVector.x > 0.01) {
                animator.SetFloat("SideWalk", 0);
                animator.SetFloat("BackWalk", 0);
            } else if (controlVector.x < -0.01) {
                animator.SetFloat("SideWalk", 0);
                animator.SetFloat("BackWalk", 1);
                backMove = true;
            } else if (controlVector.y < -0.01) {
                animator.SetFloat("SideWalk", 1);
                animator.SetFloat("BackWalk", 0);
                sideMove = true;
            } else if (controlVector.y > 0.01) {
                animator.SetFloat("SideWalk", -1);
                animator.SetFloat("BackWalk", 0);
                sideMove = true;
            }
        }

        bool isControling = Math.Abs(controlVector.x) > 0.01 || Math.Abs(controlVector.y) > 0.01;
        bool isWalking = Input.GetKey(KeyCode.LeftControl);
        if (isControling) {
            animator.SetFloat("Speed", isWalking ? 1 : 2);
        } else {
            animator.SetFloat("Speed", 0);
            animator.SetFloat("BackWalk", 0);
            animator.SetFloat("SideWalk", 0);
        }

        KeyCode[] quickSlotKey = new[] {
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
            KeyCode.Alpha5,
        };

        for (int i = 0; i < quickSlotKey.Length; i++) {
            if (Input.GetKeyDown(quickSlotKey[i])) {
                SelectedQuickSlotItemIndex = i;
                break;
            }
        }

        var moveAngle = Mathf.Atan2(controlVector.y, controlVector.x) * Mathf.Rad2Deg;

        float targetSpeed = isWalking ? walkSpeed : runSpeed;
        targetSpeed = _hunger <= 20 || _thirst <= 20 ? exhaustedSpeed : targetSpeed;

        if (isWalking || (!backMove && !sideMove)) {
            animator.SetFloat("MoveSpeed", 1f);
        } else if (backMove) {
            animator.SetFloat("MoveSpeed", 0.5f);
            targetSpeed *= 0.4f;
        } else {
            animator.SetFloat("MoveSpeed", 0.75f);
            targetSpeed *= 0.8f;
        }

        Vector2 currentVelocity = rigidbody.velocity;

        float targetSpeedX = Mathf.Abs(targetSpeed * Mathf.Cos(moveAngle * Mathf.Deg2Rad)) * controlVector.x;
        float speedDifX = targetSpeedX - currentVelocity.x;
        float accelRateX = (Mathf.Abs(targetSpeedX) > 0.01f) ? acceleration : decceleration;
        float movementX = Mathf.Pow(Mathf.Abs(speedDifX) * accelRateX, velocityPower) * Mathf.Sign(speedDifX);

        float targetSpeedY = Mathf.Abs(targetSpeed * Mathf.Sin(moveAngle * Mathf.Deg2Rad)) * controlVector.y;
        float speedDifY = targetSpeedY - currentVelocity.y;
        float accelRateY = (Mathf.Abs(targetSpeedY) > 0.01f) ? acceleration : decceleration;
        float movementY = Mathf.Pow(Mathf.Abs(speedDifY) * accelRateY, velocityPower) * Mathf.Sign(speedDifY);

        rigidbody.AddForce(new Vector2(movementX, movementY));

        if (isBagOpened) {
            return;
        }
        if (!ReFire) {
            CheckFire(false);
        }
        if (ReFire) {
            ReFire = false;
            Fire();
        }

        if (Input.GetButtonDown("Fire2")) {
            if (quickSlotItems[SelectedQuickSlotItemIndex] is PlantableItem plantableItem) {
                plantableItem.Plant();
            }
        }
    }

    public bool CheckFire(bool checkPress) {
        if (checkPress) {
            ReFire = Input.GetButton("Fire1");
            return ReFire;
        }
        ReFire = Input.GetButtonDown("Fire1");
        return ReFire;
    }

    private void Fire() {
        if (quickSlotItems[SelectedQuickSlotItemIndex] != null) {
            quickSlotItems[SelectedQuickSlotItemIndex].Use();
        } else {
            PreventMoving = true;
            animator.SetTrigger("AttackTrigger");
            GameManager.Instance.CallDelay(
                () => {
                    PreventMoving = false;
                    CheckFire(true);
                }, 0.5f
            );
        }
    }

    private void SetupBasicInfo() {
        animator.SetFloat("MoveSpeed", 1);
        animator.SetFloat("AttackSpeed", 1);
    }

    private void Unequip() {
        animator.SetLayerWeight(1, 0);
    }

    private void ChangeSkin(WeaponItem weapon) {
        Skin skin = AddDefaultSkin(new Skin("new-skin"));
        if (weapon != null) {
            skin = EquipWeapon(skin, weapon);
        } else {
            Unequip();
        }

        var skeleton = PlayerMecanim.Skeleton;
        skeleton.SetSkin(skin);
        skeleton.SetSlotsToSetupPose();
    }

    private Skin EquipWeapon(Skin skin, WeaponItem weapon) {
        animator.SetLayerWeight(1, 1);
        skin.AddAttachments(weapon.WeaponSkin);
        animator.SetFloat(WeaponDetailType.ParameterName, weapon.WeaponId);
        animator.SetFloat(WeaponType.ParameterName, weapon.Type);
        return skin;
    }

    private Skin AddDefaultSkin(Skin skin) {
        var skeletonData = PlayerMecanim.Skeleton.Data;
        skin.AddSkin(skeletonData.FindSkin("01. BodyColor/BodyColorType00"));
        skin.AddSkin(skeletonData.FindSkin("02. Hair/Hair01"));
        skin.AddSkin(skeletonData.FindSkin("08. Down Wear/DW00"));
        skin.AddSkin(skeletonData.FindSkin("07. Top Wear/TW00"));
        skin.AddSkin(skeletonData.FindSkin("03. Bag/Bag01"));
        return skin;
    }

    private void UpdateAnimation(int mouseSide, bool isQueue = false) {
        var newScale = transform.localScale;
        if ((mouseSide == 4 && newScale.x > 0) || (mouseSide > 0 && newScale.x < 0)) {
            newScale.x *= -1;
            transform.localScale = newScale;
        }
    }

    private int GetMouseSide(out float mouseAngle) {
        int cornerAngle = 45;
        Vector2 fromPoint = rigidbody.position + deltraPos;
        Vector2 toPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float contactAngle = Mathf.Atan2(toPoint.y - fromPoint.y, toPoint.x - fromPoint.x) * Mathf.Rad2Deg;
        mouseAngle = (float)Math.Round(contactAngle);
        int contactSide = 4;
        ;
        if (Mathf.Abs(contactAngle) >= cornerAngle) {
            contactSide = contactAngle > 0 ? 3 : 1;
        }

        if (Mathf.Abs(contactAngle) > 180 - cornerAngle) {
            contactSide = 2;
        }

        return contactSide;
    }
}