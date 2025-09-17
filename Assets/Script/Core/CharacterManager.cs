using PlayerStatsController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class CharacterManager : MonoBehaviour
    {
        [Header("Lock On Transform")]
        public Transform lockOnTransform;

        [Header("Combat Colliders")]
        public BoxCollider backStabBoxCollider;

        public int pendingCriticalDamage;
    }
}

