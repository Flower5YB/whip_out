using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SA
{
    public class StateManager : MonoBehaviour
    {
        [Header("Init")]
        public GameObject activeModel;

        [Header("Inputs")]
        public float vertical; //수직
        public float horizontal; //수평
        public float moveAmount; //캐릭터 움직임
        public Vector3 moveDir; //캐릭터 움직임 방향
        public bool rt, rb, lt, lb;

        [Header("Stats")]
        public float moveSpeed = 3; //걸을때 속도
        public float runSpeed = 4.5f; //뛸때 속도
        public float rotateSpeed = 5; //회전 속도
        public float toGround = 0.5f; //중력

        [Header("Statas")]
        public bool onGround; //땅에 닿는거 체크
        public bool run; //걷는지 달리는지 체크
        public bool lockOn;
        public bool inAction;

        [HideInInspector]
        public Animator anim; //애니메이터 스크립트 변수
        [HideInInspector]
        public Rigidbody rigid; // 리지드 바디 변수

        [HideInInspector]
        public float delta; //시간 체크 변수
        [HideInInspector]
        public LayerMask ignoreLayers; //레이어변수 

        public void Init()
        {
            SetupAnimator();
            rigid = GetComponent<Rigidbody>(); // 조종 하는 플레이어의 리지드 바디 
            rigid.angularDrag = 999;
            rigid.drag = 4;
            rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; //충돌시 회전 방지

            gameObject.layer = 8;
            ignoreLayers = ~(1 << 9);

            anim.SetBool("onGround", true);
        }
        
        void SetupAnimator()
        {
            if (activeModel == null)
            {
                anim = GetComponentInChildren<Animator>();
                if (anim == null)
                {
                    Debug.Log("No model found");
                }
                else
                {
                    activeModel = anim.gameObject;
                }
            }
            if (anim == null)
                anim = activeModel.GetComponent<Animator>(); 
            anim.applyRootMotion = false;
        }

        public void FixedTick(float d) //프레임 보정
        {
            delta = d;
            inAction = !anim.GetBool("canMove");

            if (inAction)
                return;

            DetectAction();

            if (inAction)
                return;

            rigid.drag = (moveAmount > 0 || onGround == false) ? 0 : 4; //움직이거나, 떨어지면 중력이 크게 작용


            float targetSpeed = moveSpeed;
            if (run)
                targetSpeed = runSpeed;

            if (onGround)
                rigid.velocity = moveDir * (targetSpeed * moveAmount); //움직이는 방향으로 속도제어
            if (run)
            {
                lockOn = false;
            }
            if (!lockOn)
            {
                Vector3 targerDir = moveDir;
                targerDir.y = 0; //캐릭터 방향이 하늘로 안가게
                if (targerDir == Vector3.zero)  //0. 0 . 0
                    targerDir = transform.forward;  //캐릭터의 방향이 유니티 기준 파란색 방향
                Quaternion tr = Quaternion.LookRotation(targerDir); //캐릭터 바라보게 함
                Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, delta * moveAmount * rotateSpeed);    //캐릭터의 회전 부드럽게
                transform.rotation = targetRotation;    //직접 접근 못하니 선언
            }

            HandleMovementAnimations();
        }

        public void DetectAction()
        {
            if (rb == false && rt == false && lt == false && lb == false)
                return;
            string targetAnim = null;

            if (rb)
                targetAnim = "oh_attack_1";
            if (rt)
                targetAnim = "oh_attack_2";
            if (lt)
                targetAnim = "oh_attack_3";
            if (lb)
                targetAnim = "th_attack_1";

            if (string.IsNullOrEmpty(targetAnim))
                return;

            inAction = true;           
            anim.CrossFade(targetAnim, 0.4f);
        }

        public void Tick(float d)
        {
            delta = d;
            onGround = OnGround();

            anim.SetBool("onGround", onGround);
        }

        void HandleMovementAnimations()
        {
            anim.SetBool("run",run);
            anim.SetFloat("vertical", moveAmount, 0.4f ,delta); //블렌더 트리 
        }

        public bool OnGround()
        {
            bool r = false;

            Vector3 origin = transform.position + (Vector3.up * toGround);
            Vector3 dir = -Vector3.up;
            float dis = toGround + 0.3f;

            RaycastHit hit;

            Debug.DrawRay(origin, dir * dis);

            if(Physics.Raycast(origin,dir,out hit, dis,ignoreLayers))
            {
                r = true;
                Vector3 targetPosition = hit.point; //땅에 닿았을때 캐릭터가 가만히 서있게 함
                transform.position = targetPosition;
            }
            return r;
        }
    }
}
