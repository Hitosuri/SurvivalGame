using System;
using System.Collections;
using System.Collections.Generic;
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
using static System.Runtime.CompilerServices.RuntimeHelpers;

public class PlayerController : MonoBehaviour {
    public float runSpeed = 8f;
    public float walkSpeed = 2f;
    public float acceleration = 6f;
    public float decceleration = 10f;
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

    private int SelectedQuickSlotItemIndex {
        get => _selectedQuickSlotItemIndex;
        set {
            _selectedQuickSlotItemIndex = value;
            OnSelectedQuickSlotChange(_selectedQuickSlotItemIndex);
        }
    }

    private const float maxHealth = 100;

    public float Health {
        get => _health;
        set {
            _health = Mathf.Clamp(value, 0, 100);
        }
    }

    private const float maxHunger = 100;

    public float Hunger {
        get => _hunger;
        set {
            _hunger = Mathf.Clamp(value, 0, 100);
            animator.SetBool("Exhausted", _hunger <= 20 || _thirst <= 20);
            animator.SetBool("IsHunger", _hunger <= 20);
            GameManager.Instance.HudController?.SetHunger(_hunger / maxHunger);
            print($"set hunger = {_hunger / maxHunger}");
        }
    }

    private const float maxThirst = 100;

    public float Thirst {
        get => _thirst;
        set {
            _thirst = Mathf.Clamp(value, 0, 100);
            animator.SetBool("Exhausted", _hunger <= 20 || _thirst <= 20);
            animator.SetBool("IsThirsty", _thirst <= 20);
            GameManager.Instance.HudController?.SetThirsty(_thirst / maxThirst);
            print($"set thirst = {_thirst / maxThirst}");
        }
    }

    private float thirst = 0;
    private int _selectedQuickSlotItemIndex;
    private float _health;
    private float _hunger;
    private float _thirst;

    public bool PreventMoving { get; set; }
    public bool ReFire { get; set; }
    public SkeletonMecanim PlayerMecanim { get; set; }

    private void Start() {
        rigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        PlayerMecanim = GetComponent<SkeletonMecanim>();
        bagItems = new BaseItem[12];
        quickSlotItems = new BaseItem[5];
        SetupBasicInfo();

        ReFire = false;
        var skeleton = PlayerMecanim.Skeleton;
        var centerBone = skeleton.FindBone("Center");
        deltraPos = centerBone.GetWorldPosition(transform) - transform.position;
        weaponMainBone = skeleton.FindBone("Weapon_Main");
        ChangeSkin(null);
        meshRenderer = GetComponent<MeshRenderer>();
        TestSetup();
    }

    private void TestSetup() {
        quickSlotItems[1] = new WCrossBow() {
            Owner = this
        };
        quickSlotItems[2] = new WLance() {
            Owner = this
        };
        quickSlotItems[3] = new FApple() {
            Owner = this
        };
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

    private void UpdateSortingOrder() {
        meshRenderer.sortingOrder = Mathf.CeilToInt((rigidbody.position.y - 0.2f) * 4 * -1);
    }

    private void Update() {
        if (Time.timeScale == 0) {
            return;
        }
        float mouseAngle;
        var newSide = GetMouseSide(out mouseAngle);
        if (currentSide != newSide) {
            currentSide = newSide;
            animator.SetFloat("Rotate", currentSide == 4 ? 2 : currentSide);
            UpdateAnimation(newSide);
        }

        if (newSide == 4) {
            mouseAngle = 180 - mouseAngle;
        }
        weaponMainBone.Rotation = mouseAngle;

        var controlVector = new Vector2();
        controlVector.x = Input.GetAxisRaw("Horizontal");
        controlVector.y = Input.GetAxisRaw("Vertical");
        if (PreventMoving) {
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
                SelectedQuickSlotItemIndex = i + 1;
                break;
            }
        }

        var moveAngle = Mathf.Atan2(controlVector.y, controlVector.x) * Mathf.Rad2Deg;

        float targetSpeed = isWalking ? walkSpeed : runSpeed;

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

        if (!ReFire) {
            CheckFire(false);
        }
        if (ReFire) {
            ReFire = false;
            Fire();
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
        animator.SetFloat(WeaponDetailType.ParameterName, weapon.Id);
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