using Verse;
using RimWorld;
using System.Collections.Generic;

namespace HelodBlancasDrug
{
    public class HediffCompProperties_OnRemoval : HediffCompProperties
    {
        public HediffDef hediffToGive;
        public List<HediffDef> hediffsToRemove;
        public List<int> offsetStages; // Stages where this comp should NOT trigger

        public HediffCompProperties_OnRemoval()
        {
            this.compClass = typeof(HediffComp_OnRemoval);
        }
    }

    public class HediffComp_OnRemoval : HediffComp
    {
        public HediffCompProperties_OnRemoval Props => (HediffCompProperties_OnRemoval)this.props;

        // Flag to skip logic if removed by another component (like ExclusiveGroup)
        public bool skipNextRemoval = false;

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            
            if (Pawn == null || Pawn.Dead) return;

            // Prevent working if flagged to skip
            if (skipNextRemoval) return;

            // Prevent working when offset is active (current stage is in offsetStages)
            if (Props.offsetStages != null && Props.offsetStages.Contains(parent.CurStageIndex))
            {
                return;
            }

            // Give new hediff if defined
            if (Props.hediffToGive != null)
            {
                Pawn.health.AddHediff(Props.hediffToGive);
            }

            // Remove existing hediffs if defined
            if (Props.hediffsToRemove != null)
            {
                foreach (var def in Props.hediffsToRemove)
                {
                    Hediff existing = Pawn.health.hediffSet.GetFirstHediffOfDef(def);
                    if (existing != null)
                    {
                        Pawn.health.RemoveHediff(existing);
                    }
                }
            }
        }
    }
}
