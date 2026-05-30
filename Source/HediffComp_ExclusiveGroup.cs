using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace HelodBlancasDrug
{
    public class HediffCompProperties_ExclusiveGroup : HediffCompProperties
    {
        public string groupName;
        public List<float> stageStrengths; // Strength value for each stage index
        public List<int> offsetStages; // Stages where this comp should NOT trigger

        public HediffCompProperties_ExclusiveGroup()
        {
            this.compClass = typeof(HediffComp_ExclusiveGroup);
        }
    }

    public class HediffComp_ExclusiveGroup : HediffComp
    {
        public HediffCompProperties_ExclusiveGroup Props => (HediffCompProperties_ExclusiveGroup)this.props;

        public float CurrentStrength
        {
            get
            {
                if (Props.stageStrengths == null || Props.stageStrengths.Count == 0 || parent.CurStageIndex < 0)
                    return 0f;
                
                int index = parent.CurStageIndex;
                if (index >= Props.stageStrengths.Count)
                    index = Props.stageStrengths.Count - 1;
                
                return Props.stageStrengths[index];
            }
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            CheckCompetitors();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Pawn.IsHashIntervalTick(60))
            {
                CheckCompetitors();
            }
        }

        private void CheckCompetitors()
        {
            if (Pawn == null || Pawn.Dead || string.IsNullOrEmpty(Props.groupName)) return;

            // 1. If I am protected by offset, I don't check or remove others
            if (Props.offsetStages != null && Props.offsetStages.Contains(parent.CurStageIndex))
            {
                return;
            }

            float myStrength = this.CurrentStrength;
            
            var competitors = Pawn.health.hediffSet.hediffs
                .Where(h => h != this.parent)
                .Select(h => new { Hediff = h, Comp = h.TryGetComp<HediffComp_ExclusiveGroup>() })
                .Where(x => x.Comp != null && x.Comp.Props.groupName == this.Props.groupName)
                .ToList();

            foreach (var other in competitors)
            {
                // 2. If the OTHER hediff is protected by offset, I cannot remove it
                if (other.Comp.Props.offsetStages != null && other.Comp.Props.offsetStages.Contains(other.Hediff.CurStageIndex))
                {
                    continue;
                }

                float otherStrength = other.Comp.CurrentStrength;

                if (otherStrength < myStrength)
                {
                    RemoveTarget(other.Hediff);
                }
                else if (otherStrength > myStrength)
                {
                    RemoveTarget(this.parent);
                    break;
                }
                else
                {
                    // Equal strength: Keep higher severity
                    if (other.Hediff.Severity > this.parent.Severity)
                    {
                        RemoveTarget(this.parent);
                        break;
                    }
                    else
                    {
                        RemoveTarget(other.Hediff);
                    }
                }
            }
        }

        private void RemoveTarget(Hediff target)
        {
            // Set flag to skip OnRemoval logic before removing
            var onRemovalComp = target.TryGetComp<HediffComp_OnRemoval>();
            if (onRemovalComp != null)
            {
                onRemovalComp.skipNextRemoval = true;
            }
            Pawn.health.RemoveHediff(target);
        }
    }
}
