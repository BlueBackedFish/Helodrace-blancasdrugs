using Verse;
using RimWorld;
using Verse.AI;

namespace HelodBlancasDrug
{
    public class HediffCompProperties_SleepLogic : HediffCompProperties
    {
        public HediffDef hediffToGive; // Should be 'DeepSleep'
        
        public HediffCompProperties_SleepLogic()
        {
            this.compClass = typeof(HediffComp_SleepLogic);
        }
    }

    public class HediffComp_SleepLogic : HediffComp
    {
        private bool hasTransitioned = false; 

        public HediffCompProperties_SleepLogic Props => (HediffCompProperties_SleepLogic)this.props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (Pawn == null || Pawn.Dead || hasTransitioned) return;

            // Bed-Exclusive Sleep Detection:
            // 1. !Pawn.Awake() handles the actual sleep/unconscious state.
            // 2. Pawn.InBed() ensures they are physically using a bed or sleeping spot (like a mattress).
            // This setup ensures that if they collapse on the floor, the transition WON'T trigger
            // until someone puts them in a bed or they find a sleeping spot.
            bool isActuallySleepingInBed = !Pawn.Awake() && Pawn.InBed();

            if (isActuallySleepingInBed)
            {
                TriggerDeepSleepTransition();
            }
        }

        private void TriggerDeepSleepTransition()
        {
            if (hasTransitioned) return;

            if (Props.hediffToGive != null)
            {
                // 1. Assign 'DeepSleep' with full severity
                Hediff deepSleep = HediffMaker.MakeHediff(Props.hediffToGive, Pawn, null);
                deepSleep.Severity = 1.0f;
                Pawn.health.AddHediff(deepSleep);
                
                hasTransitioned = true;

                // 2. Forcefully remove 'DeepSleeper' (this hediff)
                Pawn.health.RemoveHediff(parent);
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref hasTransitioned, "hasTransitioned", false);
        }
    }
}
