using PlayerController;
using PlayerStatsController;
using System;
using UnityEngine;

public enum BuffType { LockStaminaBuff, RegenHP, SpeedMultiplierBuff }

[Serializable]
public class BuffEffect
{
    public BuffType type;
    public float value;
    public bool isPermanent;
    public float effectDuration;

    public void CreateAndApplyEffect()
    {
        Type fxType = Type.GetType(type.ToString());
        if (fxType != null)
        {
            BuffEffect buff = (BuffEffect)Activator.CreateInstance(fxType);
            PlayerBuffManager.instance.RegistBuff(buff.Init(this));
        }
    }

    public BuffEffect Init(BuffEffect buff)
    {
        type = buff.type;
        value = buff.value;
        isPermanent = buff.isPermanent;
        effectDuration = buff.effectDuration;
        
        return this;
    }

    public bool isBuffDurationRunOut(float deltaTime)
    {
        effectDuration -= deltaTime;
        Debug.Log(type.ToString() + " " + effectDuration);
        if(effectDuration > 0)
        {
            return false;
        } 
        else
        {
            return true;
        }
    }

    public virtual void ApplyEffect() { }
    public virtual void RemoveEffect() { }
    public virtual void OverrideEffect(BuffEffect fx) 
    {
        //remove current fx
        RemoveEffect();
        //apply new fx
        Init(fx);
        ApplyEffect();
    }
}


public class LockStaminaBuff : BuffEffect
{
    public override void ApplyEffect()
    {
        base.ApplyEffect();
        PlayerStats.instance.isLockStamina = true;
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        PlayerStats.instance.isLockStamina = false;
    }
}

public class SpeedMultiplierBuff : BuffEffect
{
    public override void ApplyEffect()
    {
        base.ApplyEffect();
        PlayerStats.instance.IncreaseSpeedMultiplier(value);
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();
        PlayerStats.instance.ReduceSpeedMultiplier(value);
    }
}

public class RegenHP : BuffEffect
{
    public override void ApplyEffect()
    {
        base.ApplyEffect();
        PlayerStats.instance.HealPlayer((int)value);
    }

    
}