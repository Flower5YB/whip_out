using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NYB
{
    public class PlayerStateManager : CharacterStateManager
    {
        [Header("Inputs")]
        public float mouseX;
        public float mouseY;
        public float moveAmount;
        public Vector3 rotateDirection;

        [Header("States")]
        public bool isGrounded;

        [Header("References")]
        public new Transform camera;

        [Header("Movement Stats")]
        public float frontRayOffset = .5f;
        public float movementSpeed = 1;
        public float adaptSpeed = 10;
        public float rotationSpeed = 10;

        [HideInInspector]
        public LayerMask ignoreForGroundCheck;

        [HideInInspector]
        public string locomotionId = "locomotion";
        [HideInInspector]
        public string attackStateId = "attackState";

        public override void Init()
        {
            base.Init();

            State locomotion = new State(
                new List<StateAction>() //Fixed Update
                {                    
                    new MovePlayerCharacter(this)
                },
                new List<StateAction>() //Update
                {
                    new InputManager(this),
                },
                new List<StateAction>() //Late Update
                {

                }
                );

            State attackState = new State(
               new List<StateAction>() //Fixed Update
                {

               },
               new List<StateAction>() //Update
                {

               },
               new List<StateAction>() //Late Update
                {

               }
               );


            RegisterState(locomotionId, locomotion);
            RegisterState(attackStateId, attackState);

            ChangeState(locomotionId);

            ignoreForGroundCheck = ~(1 << 0 | 1 << 10);
        }

        private void FixedUpdate()
        {
            delta = Time.fixedDeltaTime;
            base.FixedTick();   //StateManagers의 FixedTick() 스크립트 수행
        }

        private void Update()
        {
            delta = Time.deltaTime;
            base.Tick();
        }

        private void LateUpdate()
        {
            base.LateTick();
        }
    }
}
