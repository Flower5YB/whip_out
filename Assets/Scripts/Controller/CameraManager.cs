﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SA
{
    public class CameraManager : MonoBehaviour
    {

        public bool lockon; //락온 모드 사용할 변수 선언
        public float followSpeed = 9; //카메라 따라오는 실수 선언
        public float mouseSpeed = 2; //마우스 움직이는 속도 실수 선언     
        public float controllerSpeed = 7; 

        public Transform target; //캐릭터 클래스 선언

        [HideInInspector] //인스펙터 감추기
        public Transform pivot; //메인 카메라 관리 선언
        [HideInInspector]
        public Transform camTrans; //카메라 위치 관리 선언

        float turnSmoothing = 0.1f; //카메라 회전 시간 지연
        public float minAngle = -35; //카메라 최소각도
        public float maxAngle = 35; // 카메라 최고각도
        
        float smoothX; //X축 회전시간
        float smoothY; //Y축 회전시간
        float smoothXvelocity; //X축 회전속도
        float smoothYvelocity; //Y축 회전속도
        public float lookAngle; //x축 캐릭터 회전각도 
        public float tiltAngle; //Y축 캐릭터 회전각도

        //카메라 설정값 초기화
        public void Init(Transform t)
        {
            target = t;

            camTrans = Camera.main.transform;
            pivot = camTrans.parent;
        }

        public void Tick(float d) //프레임당 움직임 제어
        {
            float h = Input.GetAxis("Mouse X"); //마우스의 X축 움직임 반환
            float v = Input.GetAxis("Mouse Y"); //마우스의 Y축 움직임 반환

            float c_h = Input.GetAxis("RightAxis X"); 
            float c_v = Input.GetAxis("RightAxis Y");

            float targetSpeed = mouseSpeed; //캐릭터 움직임 속도, 마우스 움직임 속도

            if(c_h != 0 || c_v != 0)
            {
                h = c_h;
                v = c_v;
                targetSpeed = controllerSpeed;
            }
            FollowTarget(d);
            HandleRotations(d,v,h,targetSpeed);
        }

        void FollowTarget(float d)
        {
            float speed = d * followSpeed; //카메라가 따라가는 속도 선언
            Vector3 targetPosition = Vector3.Lerp(transform.position,target.position,d); //카메라가 플레이어 따라오는거 부드럽게
            transform.position = targetPosition;
        }

        void HandleRotations(float d,float v,float h,float targetSpeed)
        {
            if(turnSmoothing > 0)
            {
                smoothX = Mathf.SmoothDamp(smoothX,h,ref smoothXvelocity, turnSmoothing);
                smoothY = Mathf.SmoothDamp(smoothY,v,ref smoothYvelocity, turnSmoothing);
            }
            else
            {
                smoothX = h;
                smoothY = v;
            }

            if(lockon)
            {

            }

            lookAngle += smoothX * targetSpeed; //카메라 X축 앵글 회전을 위한 값 + 캐릭터 움직임 보정
            transform.rotation = Quaternion.Euler(0,lookAngle, 0);
            tiltAngle -= smoothY * targetSpeed; //카메라 Y축 앵글 회전을 위한 값 + 캐릭터 움직임 보정
            tiltAngle = Mathf.Clamp(tiltAngle,minAngle,maxAngle);  // Y축 앵글 회전을 최소각도랑 최대각도까지만 움직이게 한다
            pivot.localRotation = Quaternion.Euler(tiltAngle,0,0);
        }

        public static CameraManager singleton;
        void Awake()
        {
            singleton = this;           
        }
    }
}