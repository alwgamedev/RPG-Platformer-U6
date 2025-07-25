﻿namespace RPGPlatformer.Combat
{
    //for non-async abilities that have delayed execution
    //(i.e. abilities that use StoreAction and are executed via an animation event)
    //Have I not included projectile abilities in this?
    public struct DelayedAbilityExecutionOptions
    {
        public readonly bool delayExecute;
        public readonly bool channelDuringDelay;
        public readonly bool endChannelOnExecute;//otherwise must end channel manually (via another animation event)

        public DelayedAbilityExecutionOptions(bool delayExecute, bool channelDuringDelay, bool endChannelOnExecute)
        {
            this.delayExecute = delayExecute;
            this.channelDuringDelay = channelDuringDelay;
            this.endChannelOnExecute = endChannelOnExecute;
        }

        public static DelayedAbilityExecutionOptions NoDelay = default;
        public static DelayedAbilityExecutionOptions DelayWithNoChannel
            = new DelayedAbilityExecutionOptions(true, false, false);
        public static DelayedAbilityExecutionOptions DelayAndEndChannelOnExecute
            = new DelayedAbilityExecutionOptions(true, true, true);
        public static DelayedAbilityExecutionOptions DelayAndManuallyEndChannel
            = new DelayedAbilityExecutionOptions(true, true, false);
    }
}