using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SA
{
    public class Helper : MonoBehaviour
    {
        

        [Range(-1, 1)]
        public float vertical; //수직 선언
        [Range(-1, 1)]
        public float horizontal; //수평 선언

        public bool playAni; //애니메이션 움직임 선언
        public string[] oh_attacks; //한손공격 문자열 선언
        public string[] th_attacks; //양손공격 문자열 선언


        public bool twoHanded; //양손 선언
        public bool enableRM;//반복 움직임 선언
        public bool useItem; //아이템 사용 선언
        public bool interacting; //상호작용 선언
        public bool lockon;

        Animator ani; //애니메이터 컴포넌트 선언

        void Start()
        {
            ani = GetComponent<Animator>(); //컴포넌트 접속
            ani.applyRootMotion = false;
        }

        void Update()
        {
            enableRM = !ani.GetBool("canMove"); //canMove Bool 값
            ani.applyRootMotion = enableRM;

            interacting = ani.GetBool("interacting");

            if(lockon == false)
            {
                horizontal = 0;
                vertical = Mathf.Clamp01(vertical);
            }
            ani.SetBool("lockon",lockon);

            if (enableRM)
                return;

            if (useItem) //아이템을 사용할때
            {
                ani.Play("use_item"); //아이템 사용 모션을 취함
                useItem = false; //아이템사용후 모션 끝
            }

            if (interacting)
            {
                playAni = false;
                vertical = Mathf.Clamp(vertical, 0, 0.5f); //수직 움직임 정수값
            }

            ani.SetBool("two_handed", twoHanded); // 양손 파라메터

            if (playAni)
            {
                string targetAni; //애니메이션 문자열 선언

                if (!twoHanded)
                {
                    int r = Random.Range(0, oh_attacks.Length);
                    targetAni = oh_attacks[r];

                    if (vertical > 0.5f) // 이동속도가 0.5보다 빠르게 달리면
                        targetAni = "oh_attack_3"; //양손 아니더라도 3번째 공격모션
                }
                else
                {
                    int r = Random.Range(0, th_attacks.Length);
                    targetAni = th_attacks[r];
                }

                if (vertical > 0.5f) // 이동속도가 0.5보다 빠르게 달리면
                    targetAni = "oh_attack_3"; //3번째 공격모션

                vertical = 0; //움직임 0


                ani.CrossFade(targetAni, 0.2f); //애니메이션 블렌딩 , 변경할 애니메이션 클립(1), 다른 애니메이션으로 페이드 아웃 0.2초(2)
                //ani.SetBool("canMove",false);
                //enableRM = true;
                playAni = false;
            }
            ani.SetFloat("vertical", vertical); //수직 속도
            ani.SetFloat("horizontal",horizontal); //수평 속도
        }

        public void OpenDamageColliders() { }
        public void CloseDamageColliders() { }
    }
}