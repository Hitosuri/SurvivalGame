using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Assets;
using Spine;
using Spine.Unity;
using Spine.Unity.AttachmentTools;
using Unity.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public float runSpeed = 8f;
    public float walkSpeed = 2f;
    public float acceleration = 6f;
    public float decceleration = 10f;
    public float velocityPower = 0.9f;

    private Rigidbody2D rigidbody;
    private Animator animator;
    private SkeletonMecanim skeletonMecanim;
    private Bone weaponMainBone;
    private Vector2 deltraPos;
    private int currentSide = -1;
    private bool isEquiped;

    private void Start() {
        rigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        skeletonMecanim = GetComponent<SkeletonMecanim>();
        SetupBasicInfo();

        var skeleton = skeletonMecanim.Skeleton;
        var centerBone = skeleton.FindBone("Center");
        deltraPos = centerBone.GetWorldPosition(transform) - transform.position;
        weaponMainBone = skeleton.FindBone("Weapon_Main");
        ChangeSkin(null);
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

        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            ChangeSkin(null);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            ChangeSkin(WeaponDetailType.IronLance);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            ChangeSkin(WeaponDetailType.SlingshotMetal);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4)) {
            ChangeSkin(WeaponDetailType.BatWood);
        }

        if (Input.GetKeyDown(KeyCode.Alpha5)) {
            ChangeSkin(WeaponDetailType.CrossBow);
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

        if (Input.GetButtonDown("Fire1")) {
            print("Fire");
            if (isEquiped) {
                animator.SetTrigger("DoShot");
            } else {
                animator.SetTrigger("AttackTrigger");
            }
        }
    }

    private void SetupBasicInfo() {
        animator.SetFloat("MoveSpeed", 1);
        animator.SetFloat("AttackSpeed", 1);
    }

    private void Unequip() {
        animator.SetLayerWeight(1, 0);
        isEquiped = false;
    }

    private void ChangeSkin(SpineSkinInfo? weapon) {
        Skin skin = AddDefaultSkin(new Skin("new-skin"));
        if (weapon != null) {
            skin = EquipWeapon(skin, (SpineSkinInfo)weapon);
        } else {
            Unequip();
        }

        var skeleton = skeletonMecanim.Skeleton;
        skeleton.SetSkin(skin);
        skeleton.SetSlotsToSetupPose();
    }

    private Skin EquipWeapon(Skin skin, SpineSkinInfo weapon) {
        var skeletonData = skeletonMecanim.Skeleton.Data;
        animator.SetLayerWeight(1, 1);
        skin.AddAttachments(skeletonData.FindSkin(weapon.Name));
        animator.SetFloat(WeaponDetailType.ParameterName, weapon.Id);
        animator.SetFloat(WeaponType.ParameterName, weapon.Type);
        isEquiped = true;
        return skin;
    }

    private Skin AddDefaultSkin(Skin skin) {
        var skeletonData = skeletonMecanim.Skeleton.Data;
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