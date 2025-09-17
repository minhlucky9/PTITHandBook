using PlayerStatsController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBuffManager : MonoBehaviour
{
    public static PlayerBuffManager instance;
    Dictionary<BuffType, BuffEffect> playerBuffs = new Dictionary<BuffType, BuffEffect>();

    PlayerStats playerStats;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    private void FixedUpdate()
    {
        if(playerBuffs.Count > 0)
        {
            foreach(BuffEffect buff in playerBuffs.Values)
            {
                if (buff.isBuffDurationRunOut(Time.deltaTime))
                {
                    buff.RemoveEffect();
                    playerBuffs.Remove(buff.type);
                    break;
                }
            }
        }
    }

    public void RegistBuff(BuffEffect buffFx)
    {
        if(buffFx.isPermanent)
        {
            buffFx.ApplyEffect();
            return;
        }

        if(playerBuffs.ContainsKey(buffFx.type))
        {
            playerBuffs[buffFx.type].OverrideEffect(buffFx);
        } else
        {
            playerBuffs.Add(buffFx.type, buffFx);
            playerBuffs[buffFx.type].ApplyEffect();
        }
    }
}
