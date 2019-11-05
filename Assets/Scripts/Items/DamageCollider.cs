using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SA
{
    public class DamageCollider : MonoBehaviour //데미지 수치
    {
        void OnTriggerEnter(Collider other)
        {
            EnemyStates eStates = other.transform.GetComponentInParent<EnemyStates>();

            if (eStates == null)
                return;

            eStates.DoDamage(35);
        }
    }
}
