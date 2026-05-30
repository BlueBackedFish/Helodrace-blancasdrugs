using Verse;
using RimWorld;

namespace HelodBlancasDrug
{
    public class HediffCompProperties_CompareSetNeedAndRemove : HediffCompProperties
    {
        public HediffDef hediffA;
        public float offsetA = 0f;
        
        public HediffDef hediffB;
        public float offsetB = 0f;
        
        public NeedDef need;
        public float targetValue = 1.0f;

        public HediffCompProperties_CompareSetNeedAndRemove()
        {
            this.compClass = typeof(HediffComp_CompareSetNeedAndRemove);
        }
    }

    public class HediffComp_CompareSetNeedAndRemove : HediffComp
    {
        public HediffCompProperties_CompareSetNeedAndRemove Props => (HediffCompProperties_CompareSetNeedAndRemove)this.props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            if (Pawn == null || Pawn.Dead) return;

            // Perform the comparison logic
            if (Props.need != null)
            {
                float strengthA = Props.offsetA;
                if (Props.hediffA != null)
                {
                    Hediff hA = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffA);
                    if (hA != null) strengthA += hA.Severity;
                }

                float strengthB = Props.offsetB;
                if (Props.hediffB != null)
                {
                    Hediff hB = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffB);
                    if (hB != null) strengthB += hB.Severity;
                }

                if (strengthA > strengthB)
                {
                    Need targetNeed = Pawn.needs.TryGetNeed(Props.need);
                    if (targetNeed != null)
                    {
                        targetNeed.CurLevel = Props.targetValue;
                    }
                }
            }

            // Immediately delete the script hediff that triggered this
            Pawn.health.RemoveHediff(parent);
        }
    }
}
