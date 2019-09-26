using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

        int oneHandedAttackNum = 0;

        StateManager states;
        CameraManager camManger;    //게임매니저의 매니저
        float delta;               

        void Start()
        {
            states = GetComponent<StateManager>();
            states.Init();

            camManger = CameraManager.singleton;
            camManger.Init(this.transform);
        }
        void FixedUpdate()
        {
            delta = Time.fixedDeltaTime;
            GetInput();
            UpdateStates();
            states.FixedTick(Time.deltaTime);
            states.FixedTick(delta);
            camManger.Tick(delta);
        }
        void Update()
        {
            delta = Time.deltaTime;
            states.Tick(delta);
        }

        void GetInput()
        {
            vertical = Input.GetAxis("Vertical");
            horizontal = Input.GetAxis("Horizontal");
            //b_input = Input.GetButton("b_input");
            //rt_input = Input.GetButton("RT");
            rt_input = Input.GetMouseButtonDown(0);
            rt_axis = Input.GetAxis("RT");
            if (rt_axis != 0)
                rt_input = true;

            //lt_input = Input.GetButton("LT");
            lt_input = Input.GetKey(KeyCode.Q);
            lt_axis = Input.GetAxis("LT");
            if (lt_axis != 0)
                lt_input = true;


            Debug.Log(rt_input);
        }
        void UpdateStates()
        {
            states.horizontal = horizontal;
            states.vertical = vertical;

            Vector3 v = states.vertical * camManger.transform.forward; //카메라 수직 이동(유니티 파란색 화살표)
            Vector3 h = horizontal * camManger.transform.right; //카메라 수평이동(유니티 빨간색 화살표)
            states.moveDir = (v + h).normalized; //카메라 이동 정규화(방향만 가져온다)
            float m = Mathf.Abs(horizontal) + Mathf.Abs(vertical); //수평 , 수직이동 절대값(음수방지)
            states.moveAmount = Mathf.Clamp01(m);   //m값을 0~1 까지 제한

            if(b_input)
            {
                states.run = (states.moveAmount > 0);                            
            }
            else
            {
                states.run = false;
            }
            
            states.rt = rt_input;
            states.lt = lt_input;
            states.rb = rb_input;
            states.lb = lb_input;
        }
    }
}