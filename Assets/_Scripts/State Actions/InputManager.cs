using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NYB
{
    public class InputManager : StateAction
    {
        PlayerStateManager s;

        //Triggers
        bool Rb,Rt,Lb,Lt, isAttacking, b_Input, y_Input, x_Input, inventoryInput, leftArrow,
            rightArrow, upArrow, downArrow;           
           
        public InputManager(PlayerStateManager states)
        {
            s = states;
        }

        public override bool Execute()
        {
            bool retVal = false;

            s.horizontal = Input.GetAxis("Horizontal");
            s.vertical = Input.GetAxis("Vertical");
            Rb = Input.GetButton("RB");
            Rt = Input.GetButton("RT");
            Lb = Input.GetButton("LB");
            Lt = Input.GetButton("LT");

            //inventoryInput = Input.GetButton("Inventory");

            b_Input = Input.GetButton("B");
            y_Input = Input.GetButtonDown("Y");
            x_Input = Input.GetButton("X");

            //leftArrow = Input.GetButton("Left");
            //rightArrow = Input.GetButton("Right");
            //upArrow = Input.GetButton("Up");
            //downArrow = Input.GetButton("Down");

            s.mouseX = Input.GetAxis("Mouse X");
            s.mouseY = Input.GetAxis("Mouse Y");

            s.moveAmount = Mathf.Clamp01(Mathf.Abs(s.horizontal) + Mathf.Abs(s.vertical));

            retVal = HandleAttacking();
            

            return false;
        }

        bool HandleAttacking()
        {
            if (Rb || Rt || Lb || Lt)
            {
                isAttacking = true;
            }

            if (y_Input)
            {
                isAttacking = false;
            }

            if (isAttacking)
            {
                //아이템 등에서 실제 공격 애니메이션 찾음
                //애니메이션 실행
                //s.PlayTargetAnimation()
                //s.ChangeState(s.attackStateId);
            }

            return isAttacking;
        }
        
    }    
}
