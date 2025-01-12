using System;
using UnityEngine;
using RPGPlatformer.Combat;
using RPGPlatformer.Effects;
using Unity.VisualScripting;

namespace RPGPlatformer.Core
{
    using static MageAbilities;
    using static MeleeAbilities;
    using static RangedAbilities;
    using static UnarmedAbilities;

    [Serializable]
    public class AbilityResourceData
    {
        [SerializeField] Sprite abilityIcon;

        public Sprite AbilityIcon => abilityIcon;
        //for now this is all we need (as projectiles and effects are held by their respective poolers)
        //later we may also add audio clips here? if it makes sense to do so
        //(idea is to basically use the AbilityResourcesSO as a dictionary for looking up ability data)

        //we could eventually use AbilityResourcesSO to store the effects as well
        //(and populate the Poolers from those effects)
    }

    public abstract class AbilityResource
    {
        public abstract string AbilityName { get; }
        public abstract AbilityResourceData AbilityData { get; }
    }

    [Serializable]
    public class FlexibleAbilityResource : AbilityResource
    {
        [SerializeField] CombatStyle combatStyle;
        [SerializeField] string abilityName;
        [SerializeField] AbilityResourceData abilityData;

        public CombatStyle CombatStyle => combatStyle;
        public override string AbilityName => abilityName;
        public override AbilityResourceData AbilityData => abilityData;
    }

    [Serializable]
    public class MageAbilityResource : AbilityResource
    {
        [SerializeField] MageAbilitiesEnum ability;
        [SerializeField] AbilityResourceData abilityData;

        public MageAbilitiesEnum Ability => ability; 
        public override string AbilityName => ability.ToString();
        public override AbilityResourceData AbilityData => abilityData;



    }

    [Serializable]
    public class MeleeAbilityResource : AbilityResource
    {
        [SerializeField] MeleeAbilitiesEnum ability;
        [SerializeField] AbilityResourceData abilityData;

        public MeleeAbilitiesEnum Ability => ability;
        public override string AbilityName => ability.ToString();
        public override AbilityResourceData AbilityData => abilityData;

    }

    [Serializable]
    public class RangedAbilityResource : AbilityResource
    {
        [SerializeField] RangedAbilitiesEnum ability;
        [SerializeField] AbilityResourceData abilityData;

        public RangedAbilitiesEnum Ability => ability;
        public override string AbilityName => ability.ToString();
        public override AbilityResourceData AbilityData => abilityData;

    }

    [Serializable]
    public class UnarmedAbilityResource : AbilityResource
    {
        [SerializeField] UnarmedAbilitiesEnum ability;
        [SerializeField] AbilityResourceData abilityData;

        public UnarmedAbilitiesEnum Ability => ability;
        public override string AbilityName => ability.ToString();
        public override AbilityResourceData AbilityData => abilityData;
    }
}