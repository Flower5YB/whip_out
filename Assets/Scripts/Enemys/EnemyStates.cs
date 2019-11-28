using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YB
{
    public class EnemyStates : MonoBehaviour
    {
        public int health;

        public CharacterStats characterStats;

        public bool canBeParried = true;
        public bool parryIsOn = true;
        //public bool doParry = false;
        public bool isInvicible;
        public bool dontDoAnything;
        public bool canMove;
        public bool isDead;

        public StateManager parriedBy;
        

        public Animator anim;
        EnemyTarget enTarget;
        AnimatorHook a_hook;
        public Rigidbody rigid;
        public float delta;
        public float poiseDegrade = 2;

        List<Rigidbody> ragdollRigids = new List<Rigidbody>();  //Ragdoll 모델을 이용하여 3D 모델을 맵핑하여 부위별로 움직임
        List<Collider> ragdollColliders = new List<Collider>();

        float timer;

        void Start()
        {
            health = 10000;
            anim = GetComponentInChildren<Animator>();
            enTarget = GetComponent<EnemyTarget>();
            enTarget.Init(this);

            rigid = GetComponent<Rigidbody>();

            a_hook = anim.GetComponent<AnimatorHook>();
            if (a_hook == null)
                a_hook = anim.gameObject.AddComponent<AnimatorHook>();
            a_hook.Init(null,this);

            InitRagdoll();
            parryIsOn = false;
        }

        void InitRagdoll()  //Ragdoll 모델 초기화
        {
            Rigidbody[] rigs = GetComponentsInChildren<Rigidbody>();
            for (int i = 0; i < rigs.Length; i++)
            {
                if (rigs[i] == rigid)
                    continue;

                ragdollRigids.Add(rigs[i]);
                rigs[i].isKinematic = true; //물리 효과가 RigidBody에 영향을 주도록 함

                Collider col = rigs[i].gameObject.GetComponent<Collider>();
                col.isTrigger = true;
                ragdollColliders.Add(col);
            }
        }

        public void EnableRagdoll()
        {  
            for (int i = 0; i < ragdollRigids.Count; i++)
            {
                ragdollRigids[i].isKinematic = false;   ////물리 효과가 RigidBody에 영향을 주지 않도록 함
                ragdollColliders[i].isTrigger = false;
            }

            Collider controllerCollider = rigid.gameObject.GetComponent<Collider>();
            controllerCollider.enabled = false;
            rigid.isKinematic = true;


            StartCoroutine("CloseAnimator");
        }

        IEnumerator CloseAnimator()
        {
            yield return new WaitForEndOfFrame();
            anim.enabled = false;
            this.enabled = false;
        }

        void Update()
        {
            delta = Time.deltaTime;
            canMove = anim.GetBool(StaticStrings.canMove);

            if (dontDoAnything)
            {
                dontDoAnything = !canMove;
                return;
            }

            if(health <= 0)
            {
                if(!isDead)
                {
                    isDead = true;
                    EnableRagdoll();
                }
            }

            if (isInvicible)
            {
                isInvicible = !canMove;
            }
            if (parriedBy != null && parryIsOn == false)
            {
                //parriedBy.parryTarget = null;
                parriedBy = null;
            }

            if(canMove)
            {
                parryIsOn = false;
                anim.applyRootMotion = false;
                //Debug

                timer += Time.deltaTime;
                if (timer > 3)
                {
                    DoAction();
                    timer = 0;
                }
            }

            characterStats.poise -= delta * poiseDegrade;
            if (characterStats.poise < 0)
                characterStats.poise = 0;
        }

        void DoAction()
        {            
            anim.Play("oh_attack_1");
            anim.applyRootMotion = false;
            anim.SetBool(StaticStrings.canMove, false);
        }

        public void DoDamage(Action a)
        {
            if (isInvicible)
                return;

            int damage = StatsCalculations.CalculateBaseDamage(a.weaponStats, characterStats);

            characterStats.poise += damage;
            health -= damage;

            if(canMove || characterStats.poise > 100)
            {
                if (a.overrideDamageAnim)
                    anim.Play(a.damageAnim);
                else
                {
                    int ran = Random.Range(0, 100);
                    string tA = (ran > 50) ? StaticStrings.damage1 : StaticStrings.damage2;
                    anim.Play(tA);
                }
            }          



            isInvicible = true;
            anim.applyRootMotion = true;
            anim.SetBool(StaticStrings.canMove, false);
        }

        public void CheckForParry(Transform target, StateManager states)    //카운터 판정 확인
        {
            if (canBeParried == false || parryIsOn == false || isInvicible)
                return;

            Vector3 dir = transform.position - target.position;
            dir.Normalize();
            float dot = Vector3.Dot(target.forward, dir);
            if (dot < 0)
                return;

            isInvicible = true;
            anim.Play(StaticStrings.attack_interrupt);
            anim.applyRootMotion = true;
            anim.SetBool(StaticStrings.canMove, false);
            //states.parryTarget = this;
            parriedBy = states;            
            return;
        }

        public void IsGettingParried(Action a)
        {
            int damage = StatsCalculations.CalculateBaseDamage(a.weaponStats, characterStats, a.parryMultiplier);            
            health -= damage;
            dontDoAnything = true;
            anim.SetBool(StaticStrings.canMove, false);
            anim.Play(StaticStrings.parry_recieved);
        }
        public void IsGettingBackstabbed(Action a)
        {
            int damage = StatsCalculations.CalculateBaseDamage(a.weaponStats, characterStats, a.backstabMultiplier);
            health -= damage;
            dontDoAnything = true;
            anim.SetBool(StaticStrings.canMove, false);
            anim.Play(StaticStrings.backstabbed);
        }
    }
}
