using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YB
{
    public class InventoryManager : MonoBehaviour
    {
        public ItemInstance rightHandWeapon;
        public bool hasLeftHandWeapon = true;
        public ItemInstance leftHandWeapon;

        public GameObject parryCollider;

        StateManager states;

        public void Init(StateManager st)
        {
            states = st;
            if (rightHandWeapon != null)
                EquipWeapon(rightHandWeapon.instance, false);
            if (leftHandWeapon != null)
                EquipWeapon(leftHandWeapon.instance, false);

            hasLeftHandWeapon = (leftHandWeapon != null);

            EquipWeapon(rightHandWeapon.instance, false);
            EquipWeapon(leftHandWeapon.instance, true);
            InitDamageColliders(st);
            CloseAllDamageColliders();

            ParryCollider pr = parryCollider.GetComponent<ParryCollider>();
            pr.InitPlayer(st);
            CloseParryCollider();
        }

        public void EquipWeapon(Weapon w, bool isLeft = false)
        {
            string targetIdle = w.oh_idle;
            targetIdle += (isLeft) ? "_l" : "_r";
            states.anim.SetBool(StaticStrings.mirror, isLeft);
            states.anim.Play(StaticStrings.changeWeapon);
            states.anim.Play(targetIdle);

            UI.QuickSlot uiSlot = UI.QuickSlot.singleton;
            uiSlot.UpdateSlot(
                (isLeft)?
                UI.QSlotType.left_H : UI.QSlotType.right_H, w.icon);

            w.weaponModel.SetActive(true);
        }        

        public Weapon GetCurrentWeapon(bool isLeft)
        {
            if (isLeft)
                return leftHandWeapon.instance;
            else
                return rightHandWeapon.instance;
        }

        public void OpenAllDamageColliders()
        {
            if (rightHandWeapon.instance.w_hook != null)
                rightHandWeapon.instance.w_hook.OpenDamageColliders();

            if (leftHandWeapon.instance.w_hook != null)
                leftHandWeapon.instance.w_hook.OpenDamageColliders();
        }

        public void CloseAllDamageColliders()
        {
            if (rightHandWeapon.instance.w_hook != null)
                rightHandWeapon.instance.w_hook.CloseDamageColliders();          

            if (leftHandWeapon.instance.w_hook != null)
                leftHandWeapon.instance.w_hook.CloseDamageColliders();
        }

        public void InitDamageColliders(StateManager states)
        {
            if (rightHandWeapon.instance.w_hook != null)
                rightHandWeapon.instance.w_hook.InitDamageColliders(states);

            if (leftHandWeapon.instance.w_hook != null)
                leftHandWeapon.instance.w_hook.InitDamageColliders(states);
        }

        public void CloseParryCollider()
        {
            parryCollider.SetActive(false);
        }

        public void OpenParryCollider()
        {
            parryCollider.SetActive(true);
        }

        public ItemInstance WeaponToItemInstance(Weapon w)
        {
            GameObject go = new GameObject();
            ItemInstance inst = go.AddComponent<ItemInstance>();

            return inst;
        }
    }

    [System.Serializable]
    public class Weapon // 양수 , 쌍수 이름 설정
    {
        public string weaponId;
        public Sprite icon;
        public string oh_idle;
        public string th_idle;


        public List<Action> actions;        
        public List<Action> two_handedActions;

        public float parryMultiplier;
        public float backstabMultiplier;
        public bool LeftHandMirror;

        public GameObject modelprefab;

        public GameObject weaponModel;
        public WeaponHook w_hook;

        public Action GetAction(List<Action> l,ActionInput inp)
        {
            for (int i = 0; i < l.Count; i++)
            {
                if(l[i].input == inp)
                {
                    return l[i];
                }
            }

            return null;
        }

        public Vector3 model_pos;
        public Vector3 model_eulers;
        public Vector3 model_scale;
    }
}