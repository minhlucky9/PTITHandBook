using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interaction
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Item/Usable Item")]
    public class UsableItemSO : ItemSO
    {
        public List<BuffEffect> buffs;

        public override void UseItem()
        {
            base.UseItem();
            //
            for(int i = 0; i < buffs.Count; i++)
            {
                buffs[i].CreateAndApplyEffect();
            }
        }
    }
}