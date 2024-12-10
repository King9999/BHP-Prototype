using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

/* Manages buffs and debuffs, and terminal effects. */
public class EffectManager : MonoBehaviour
{
    public List<StatusEffect> statusEffects;    //contains both buffs and debuffs
    public List<TerminalEffect> terminalEffects;

    public static EffectManager instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }


    //add a given effect to a character.
    public void AddEffect(StatusEffect.Effect effect, Character character)
    {
        //find the status effect
        bool effectFound = false;
        int i = 0;
        StatusEffect statusEffect = null;
        while (!effectFound && i < statusEffects.Count)
        {
            if (statusEffects[i].effect == effect)
            {
                effectFound = true;
                statusEffect = Instantiate(statusEffects[i]);
            }
            else
            {
                i++;
            }
        }

        //show message
        GameManager gm = Singleton.instance.GameManager;
        gm.DisplayStatusEffect(character, statusEffect.effectName.ToUpper());

        if (statusEffect.effectType == StatusEffect.EffectType.Buff)
        {
            //if we already have this buff and it has a duration, refresh duration.
            if (character.buffs.Contains(statusEffect))
            {
                if (statusEffect.hasDuration)
                {
                    i = 0;
                    bool buffFound = false;
                    while (!buffFound && i < character.buffs.Count)
                    {
                        if (character.buffs[i] == statusEffect)
                        {
                            buffFound = true;
                            character.buffs[i].currentDuration = 0;
                        }
                        else
                        {
                            i++;
                        }
                    }
                }

                //if we get here, effect doesn't have duration, and we don't need to do anything more.
            }
            //if character already has 3 buffs, remove the oldest buff which will be the one in index 0.
            else if (character.MaxBuffs)
            {
                character.buffs[0].CleanupEffect(character);
                character.buffs.Add(statusEffect);
                statusEffect.ApplyEffect(character);
            }
            else
            {
                character.buffs.Add(statusEffect);
                statusEffect.ApplyEffect(character);
            }
        }
        else
        {
            //if character already has injured status, we update the one that's there.
            if (statusEffect is StatusEffect_Injured injured)
            {
                //update existing debuff
                StatusEffect_Injured inj = character.GetStatusEffect(StatusEffect.Effect.Injured, character.debuffs) as StatusEffect_Injured;
                if (inj != null)
                {
                    inj.ApplyEffect(character);
                }
                else if (character.MaxDebuffs)
                {
                    character.debuffs[0].CleanupEffect(character);
                    character.debuffs.Add(injured);
                    injured.ApplyEffect(character);
                }
                else
                {
                    character.debuffs.Add(injured);
                    injured.ApplyEffect(character);
                }
                
            }
            else
            {
                //if we already have this debuff and it has a duration, refresh duration.
                if (character.debuffs.Contains(statusEffect))
                {
                    if (statusEffect.hasDuration)
                    {
                        i = 0;
                        bool debuffFound = false;
                        while (!debuffFound && i < character.debuffs.Count)
                        {
                            if (character.debuffs[i] == statusEffect)
                            {
                                debuffFound = true;
                                character.debuffs[i].currentDuration = 0;
                            }
                            else
                            {
                                i++;
                            }
                        }
                    }
                    
                    //if we get here, effect doesn't have duration, and we don't need to do anything more.
                }
                else if (character.MaxDebuffs)
                {
                    character.debuffs[0].CleanupEffect(character);
                    character.debuffs.Add(statusEffect);
                    statusEffect.ApplyEffect(character);
                }
                else
                {
                    character.debuffs.Add(statusEffect);
                    statusEffect.ApplyEffect(character);
                }
            }

        }

    }

    public void AddEffect(TerminalEffect.EffectID effectID, Hunter hunter)
    {
        //search for the effect
        TerminalEffect effect = null;
        int i = 0;
        bool buffFound = false;
        while (!buffFound && i < terminalEffects.Count)
        {
            if (terminalEffects[i].effectID == effectID)
            {
                buffFound = true;
                effect = Instantiate(terminalEffects[i]);

                //does hunter already have this effect?
                if (!hunter.terminalEffects.Contains(effect))
                {
                    hunter.terminalEffects.Add(effect);
                    effect.ActivateEffect(hunter);
                }
            }
            else
            {
                i++;
            }
        }
           
    }

    public bool DebuffResisted(StatusEffect.Effect debuff, Character character, float baseChance = 1)
    {
        bool debuffResisted = false;

        switch(debuff)
        {
            case StatusEffect.Effect.Dizzy:
                if (Random.value > baseChance - character.resistDizzy)
                    debuffResisted = true;
                break;

            case StatusEffect.Effect.Berserk:
                if (Random.value > baseChance - character.resistBerserk)
                    debuffResisted = true;
                break;

            case StatusEffect.Effect.DisableSuper:
                if (Random.value > baseChance - character.resistDisableSuper)
                    debuffResisted = true;
                break;

            case StatusEffect.Effect.DisableSkill:
                if (Random.value > baseChance - character.resistDisableSkill)
                    debuffResisted = true;
                break;

            case StatusEffect.Effect.DisableLeg:
                if (Random.value > baseChance - character.resistDisableLeg)
                    debuffResisted = true;
                break;

            case StatusEffect.Effect.Blind:
                if (Random.value > baseChance - character.resistBlind)
                    debuffResisted = true;
                break;

            case StatusEffect.Effect.CardDrain:
                if (Random.value > baseChance - character.resistCardDrain)
                    debuffResisted = true;
                break;

            case StatusEffect.Effect.Poison:
                if (Random.value > baseChance - character.resistPoison)
                    debuffResisted = true;
                break;

            case StatusEffect.Effect.Weakened:
                if (Random.value > baseChance - character.resistWeak)
                    debuffResisted = true;
                break;

        }

        return debuffResisted;
    }

    /*private StatusEffect GetEffect(Character character, List<StatusEffect> effectList)
    {
        StatusEffect effect = null;
        //if we already have this buff and it has a duration, refresh duration.
        if (effect.hasDuration && effectList.Contains(effect))
        {
            int i = 0;
            bool buffFound = false;
            while (!buffFound && i < effectList.Count)
            {
                if (effectList[i] == effect)
                {
                    buffFound = true;
                    effectList[i].currentDuration = 0;
                    return effect;
                }
                else
                {
                    i++;
                }
            }

            if (!buffFound) return effect;
        }
        //if character already has 3 buffs/debuffs, remove the oldest de/buff which will be the one in index 0.
        else if (effectList.Count >= 3)
        {
            effectList[0].CleanupEffect(character);
            return effect;
            //effectList.Add(effect);
            //effect.ApplyEffect(character);
        }
        else
        {
            return effect;
            //effect.ApplyEffect(character);
        }
    }*/
   
}
