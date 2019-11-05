using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SA
{
    public class EnemyStates : MonoBehaviour
    {
        public float health;
        public bool isInvicible;
        public bool canMove;
        public bool isDead;

        public Animator anim;
        EnemyTarget enTarget;
        AnimatorHook a_hook;
        public Rigidbody rigid;
        public float delta;

        List<Rigidbody> ragdollRigids = new List<Rigidbody>();  //Ragdoll 모델을 이용하여 3D 모델을 맵핑하여 부위별로 움직임
        List<Collider> ragdollColliders = new List<Collider>();       

        void Start()
        {
            health = 100;
            anim = GetComponentInChildren<Animator>();
            enTarget = GetComponent<EnemyTarget>();
            enTarget.Init(this);

            rigid = GetComponent<Rigidbody>();

            a_hook = anim.GetComponent<AnimatorHook>();
            if (a_hook == null)
                a_hook = anim.gameObject.AddComponent<AnimatorHook>();
            a_hook.Init(null,this);

            InitRagdoll();
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
            canMove = anim.GetBool("canMove");

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

            if(canMove)
            {
                anim.applyRootMotion = false;
            }
        }

        public void DoDamage(float v)
        {
            if (isInvicible)
                return;

            health -= v;
            isInvicible = true;
            anim.Play("damage_1");
            anim.applyRootMotion = true;
            anim.SetBool("canMove",false);
        }
    }
}
