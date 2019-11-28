using UnityEngine;
using System.Collections;

namespace NYB
{
    public class MovePlayerCharacter : StateAction
    {
        PlayerStateManager states;

        public MovePlayerCharacter(PlayerStateManager playerStateManager)
        {
            states = playerStateManager;
        }

        public override bool Execute()
        {
            float frontY = 0;
            RaycastHit hit;
            Vector3 origin = states.mTransform.position + (states.mTransform.forward * states.frontRayOffset);
            origin.y += .5f;
            Debug.DrawRay(origin, -Vector3.up, Color.red, .01f, false);
            if (Physics.Raycast(origin, -Vector3.up, out hit, 1 ,states.ignoreForGroundCheck))
            {
                float y = hit.point.y;
                frontY = y - states.mTransform.position.y;
            }

            Vector3 currentVelocity = states.rigidbody.velocity;
            Vector3 targetVelocity = states.mTransform.forward * states.moveAmount * states.movementSpeed;

            //if (states.isLockingOn)
            //{
            //    targetVelocity = states.rotateDirection * states.moveAmount * movementSpeed;
            //}

            if(states.isGrounded)
            {
                float moveAmount = states.moveAmount;

                if (moveAmount > 0.1f)
                {
                    states.rigidbody.isKinematic = false;
                    states.rigidbody.drag = 0;
                    if (Mathf.Abs(frontY) > 0.02f)
                    {
                        targetVelocity.y = ((frontY > 0) ? frontY + 0.2f : frontY - 0.2f) * states.movementSpeed;
                    }
                }
                else
                {
                    float abs = Mathf.Abs(frontY);

                    if (abs > 0.02f)
                    {
                        states.rigidbody.isKinematic = true;
                        targetVelocity.y = 0;
                        states.rigidbody.drag = 4;
                    }
                }

                HandleRotation();
            }

            else
            {
                states.rigidbody.isKinematic = false;
                states.rigidbody.drag = 0;
                targetVelocity.y = currentVelocity.y;
            }

            HandleAnimation();

            Debug.DrawRay((states.mTransform.position + Vector3.up * .2f), targetVelocity, Color.green, 0.01f, false);
            states.rigidbody.velocity = targetVelocity;

            //states.rigidbody.velocity = Vector3.Lerp(currentVelocity, targetVelocity, states.delta * states.adaptSpeed);

            return false;
        }

        void HandleRotation()
        {
            float h = states.horizontal;
            float v = states.vertical;

            Vector3 targetDir = states.camera.transform.forward * v;
            targetDir += states.camera.transform.right * h;
            targetDir.Normalize();

            targetDir.y = 0;
            if (targetDir == Vector3.zero)
                targetDir = states.transform.forward;

            Quaternion tr = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(
                states.mTransform.rotation, tr,
                states.delta * states.moveAmount * states.rotationSpeed);

            states.mTransform.rotation = targetRotation;
        }

        void HandleAnimation()
        {
            if (states.isGrounded)
            {
                float m = states.moveAmount;    //movement value
                float f = 0;    //forward value
                if (m > 0 && m <= .4f)
                    f = .5f;
                else if (m > 0.5f)
                    f = 1;

                states.anim.SetFloat("forward",f,.2f,states.delta);
            }
            else
            {

            }
        }
    }
}
