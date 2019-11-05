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
        public bool rt, rb, lt, lb; //키 입력        
        public bool rollInput; //구르기 입력
        public bool itemInput; //아이템 입력

        [Header("Stats")]
        public float moveSpeed = 2; //걸을때 속도
        public float runSpeed = 3.5f; //뛸때 속도
        public float rotateSpeed = 5; //회전 속도       
        public float toGround = 0.5f; //중력
        public float rollSpeed = 1; // 구르는 속도        

        [Header("Statas")]
        public bool onGround; //땅에 닿는거
        public bool run; //걷는지 달리는지
        public bool lockOn; //록온
        public bool inAction;   //움직임 
        public bool canMove;
        public bool isTwoHanded;    //쌍수   
        public bool usingItem;  //아이템 이벤트 속도

        [Header("Other")]
        public EnemyTarget lockOnTarget;
        public Transform lockOnTransform;
        public AnimationCurve roll_curve;   //애니메이션(구르기) 루트모션 제어

        [HideInInspector]
        public Animator anim; //애니메이터 스크립트 변수
        [HideInInspector]
        public Rigidbody rigid; // 리지드 바디 변수        
        [HideInInspector]
        public AnimatorHook a_hook;
        [HideInInspector]
        public ActionManager actionManager;
        [HideInInspector]
        public InventoryManager inventoryManager;

        [HideInInspector]
        public float delta; //시간 체크 변수
        [HideInInspector]
        public LayerMask ignoreLayers; //레이어변수 

        float _actionDelay; //모션 연결 변수

        public void Init()
        {
            SetupAnimator();
            rigid = GetComponent<Rigidbody>(); // 조종 하는 플레이어의 리지드 바디 
            rigid.angularDrag = 999;
            rigid.drag = 4;
            rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; //충돌시 회전 방지            

            inventoryManager = GetComponent<InventoryManager>();
            inventoryManager.Init();

            actionManager = GetComponent<ActionManager>();
            actionManager.Init(this);

            a_hook = activeModel.GetComponent<AnimatorHook>();
            if (a_hook == null)
                a_hook = activeModel.AddComponent<AnimatorHook>();           
            a_hook.Init(this,null);

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

            usingItem = anim.GetBool("interacting");         
            DetectItemAction();
            DetectAction();
            inventoryManager.curWeapon.weaponModel.SetActive(!usingItem);

            if (inAction)
            {
                anim.applyRootMotion = true;
    
                _actionDelay += delta;
                if(_actionDelay > 0.3f)
                {
                    inAction = false;
                    _actionDelay = 0;
                }
                else
                {
                    return;
                }                
            }               
                     
            canMove = anim.GetBool("canMove");

            if (!canMove)
                return;

            //a_hook.rm_multi = 1;
            a_hook.CloseRoll();
            HandleRolls();

            anim.applyRootMotion = false;
            rigid.drag = (moveAmount > 0 || onGround == false) ? 0 : 4; //움직이거나, 떨어지면 중력이 크게 작용

            float targetSpeed = moveSpeed;
            if(usingItem)
            {
                run = false;
                moveAmount = Mathf.Clamp(moveAmount, 0, 0.45f);
            }
            if (run)
                targetSpeed = runSpeed;

            if (onGround)
                rigid.velocity = moveDir * (targetSpeed * moveAmount); //움직이는 방향으로 속도제어

            if (run)            
                lockOn = false;            
          
            Vector3 targerDir = (lockOn == false) ? 
                moveDir 
                :
                (lockOnTransform != null)?
                    lockOnTransform.transform.position - transform.position
                    :
                    moveDir;

            targerDir.y = 0; //캐릭터 방향이 하늘로 안가게
            if (targerDir == Vector3.zero)  //0. 0 . 0
                targerDir = transform.forward;  //캐릭터의 방향이 유니티 기준 파란색 방향
            Quaternion tr = Quaternion.LookRotation(targerDir); //캐릭터 바라보게 함
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, delta * moveAmount * rotateSpeed);    //캐릭터의 회전 부드럽게
            transform.rotation = targetRotation;    //직접 접근 못하니 선언    

            anim.SetBool("lockon", lockOn);

            if (lockOn == false)
                HandleMovementAnimations();
            else
                HandleLockOnAnimations(moveDir);
        }

        public void DetectItemAction()
        {
            if (canMove == false || usingItem)
                return;
            if (itemInput == false)
                return;

            ItemAction slot = actionManager.consumableItem;
            string targetAnim = slot.targetAnim;
            if (string.IsNullOrEmpty(targetAnim))
                return;

            //inventoryManager.curWeapon.weaponModel.SetActive(false);
            usingItem = true;
            anim.Play(targetAnim);
        }

        public void DetectAction()
        {

            if (canMove == false || usingItem)
                return;

            if (rb == false && rt == false && lt == false && lb == false)
                return;

            string targetAnim = null;

            Action slot = actionManager.GetActionSlot(this);
            if (slot == null)
                return;
            targetAnim = slot.targetAnim;

            if (string.IsNullOrEmpty(targetAnim))
                return;

            canMove = false;
            inAction = true;
            anim.CrossFade(targetAnim, 0.2f);                  
        }

        public void Tick(float d)
        {
            delta = d;
            onGround = OnGround();

            anim.SetBool("onGround", onGround);
        }

        void HandleRolls() //Lockon 했을때 , 안했을때의 구르기 모션 명령
        {
            if (!rollInput || usingItem)
                return;
            float v = vertical;
            float h = horizontal;
            v = (moveAmount > 0.3f)? 1 : 0;
            h = 0;

            //if (lockOn == false)
            //{
            //    v = (moveAmount > 0.3f)? 1 : 0;                
            //    h = 0;
            //}
            //else
            //{
            //    if (Mathf.Abs(v) > 0.3f) //데이터의 절대값이 0.3을 넘을경우
            //        v = 0;
            //    if (Mathf.Abs(h) < 0.3f) //데이터의 절대값이 0.3보다 작을경우
            //        h = 0;
            //}

            if (v != 0)
            {
                if (moveDir == Vector3.zero)
                    moveDir = transform.forward;
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = targetRot;
                a_hook.InitForRoll();
                a_hook.rm_multi = rollSpeed;
            }

            else
            {
                a_hook.rm_multi = 1.3f;
            }

            anim.SetFloat("vertical", v);
            anim.SetFloat("horizontal", h);

            canMove = false;
            inAction = true;
            anim.CrossFade("Rolls", 0.2f);            
        }

        void HandleMovementAnimations()
        {
            anim.SetBool("run",run);
            anim.SetFloat("vertical", moveAmount, 0.4f ,delta); //블렌더 트리 
        }

        void HandleLockOnAnimations(Vector3 moveDir)
        {
            Vector3 relativeDir = transform.InverseTransformDirection(moveDir);
            float h = relativeDir.x;
            float v = relativeDir.z;

            anim.SetFloat("vertical", v, 0.2f, delta);
            anim.SetFloat("horizontal", h, 0.2f, delta);
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

        public void HandleTwoHanded()
        {
            anim.SetBool("two_handed", isTwoHanded);
            if (isTwoHanded)
                actionManager.UpdateAcionsTwoHanded();
            else
                actionManager.UpdateAcionsOneHanded();
        }
    }
}
