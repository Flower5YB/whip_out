using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


//키입력을 담당
namespace SA
{
    public class InputHandler : MonoBehaviour
    {
        float horizontal;
        float vertical;
        bool b_input;
        bool a_input;
        bool x_input;
        bool y_input;

        bool rb_input;
        float rt_axis;
        bool rt_input;

        bool lb_input;
        float lt_axis;
        bool lt_input;

        bool leftAxis_down;
        bool rightAxis_down;

        float b_timer;
        float rt_timer;
        float lt_timer;

        StateManager states;
        CameraManager camManager;    //게임매니저의 매니저
        float delta;               

        void Start()
        {
            states = GetComponent<StateManager>();
            states.Init();

            camManager = CameraManager.singleton;
            camManager.Init(states);
        }
        void FixedUpdate()
        {
            delta = Time.fixedDeltaTime;
            GetInput();
            UpdateStates();
            states.FixedTick(delta);
            camManager.Tick(delta);            
        }

        void Update()
        {
            delta = Time.deltaTime;
            states.Tick(delta);
            ResetInputNStates();
        }

        void GetInput()
        {
            vertical = Input.GetAxis("Vertical");
            horizontal = Input.GetAxis("Horizontal");

            b_input = Input.GetButton("B");
            a_input = Input.GetButton("A");
            y_input = Input.GetButtonUp("Y");
            x_input = Input.GetButton("X");

            rt_input = Input.GetButton("RT");            
            rt_axis = Input.GetAxis("RT");
            if (rt_axis != 0)
                rt_input = true;

            lt_input = Input.GetButton("LT");            
            lt_axis = Input.GetAxis("LT");
            if (lt_axis != 0)
                lt_input = true;

            rb_input = Input.GetButton("RB");
            lb_input = Input.GetButton("LB");

            rightAxis_down = Input.GetButtonUp("L");

            if (b_input)
                b_timer += delta;            
        }

        void UpdateStates()
        {
            states.horizontal = horizontal;
            states.vertical = vertical;

            Vector3 v = states.vertical * camManager.transform.forward; //카메라 수직 이동(유니티 파란색 화살표)
            Vector3 h = horizontal * camManager.transform.right; //카메라 수평이동(유니티 빨간색 화살표)
            states.moveDir = (v + h).normalized; //카메라 이동 정규화(방향만 가져온다)
            float m = Mathf.Abs(horizontal) + Mathf.Abs(vertical); //수평 , 수직이동 절대값(음수방지)
            states.moveAmount = Mathf.Clamp01(m);   //m값을 0~1 까지 제한                       

            if (x_input)
                b_input = false;

            if(b_input && b_timer > 0.5f)
            {
                states.run = (states.moveAmount > 0);                            
            }

            if (b_input == false && b_timer > 0 && b_timer < 0.5f)
                states.rollInput = true;

            states.itemInput = x_input;
            states.rt = rt_input;
            states.lt = lt_input;
            states.rb = rb_input;
            states.lb = lb_input;

            if(y_input) //Y 버튼 눌렀을시 쌍수로 모션 변화 이벤트
            {
                states.isTwoHanded = !states.isTwoHanded;
                states.HandleTwoHanded();
            }

            if (states.lockOnTarget != null)    //록온 타겟 모델 사망시
            {
                if (states.lockOnTarget.eStates.isDead) //록온된 모델 사망시 카메라 초기화
                {
                    states.lockOn = false;
                    states.lockOnTarget = null;
                    states.lockOnTransform = null;
                    camManager.lockon = false;
                    camManager.lockonTarget = null;
                }
            }

            if (rightAxis_down) //대상 Lock on
            {                
                states.lockOn = !states.lockOn;

                if(states.lockOnTarget == null)                
                    states.lockOn = false;                

                camManager.lockonTarget = states.lockOnTarget;
                states.lockOnTransform = camManager.lockonTransform;
                camManager.lockon = states.lockOn;
            }
        }

        void ResetInputNStates()
        {
            if (b_input == false)
                b_timer = 0;
            if (states.rollInput)
                states.rollInput = false;
            if (states.run)
                states.run = false;
        }
    }
}