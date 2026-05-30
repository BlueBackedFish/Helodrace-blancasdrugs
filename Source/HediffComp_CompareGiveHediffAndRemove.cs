using Verse;
using RimWorld;

namespace HelodBlancasDrug
{
    public class HediffCompProperties_CompareGiveHediffAndRemove : HediffCompProperties
    {
        public HediffDef hediffA;
        public float offsetA = 0f;
        
        public HediffDef hediffB;
        public float offsetB = 0f;
        
        public HediffDef hediffToGive;
        public float severityToGive = 1.0f;

        public HediffCompProperties_CompareGiveHediffAndRemove()
        {
            this.compClass = typeof(HediffComp_CompareGiveHediffAndRemove);
        }
    }

    public class HediffComp_CompareGiveHediffAndRemove : HediffComp
    {
        public HediffCompProperties_CompareGiveHediffAndRemove Props => (HediffCompProperties_CompareGiveHediffAndRemove)this.props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            if (Pawn == null || Pawn.Dead) return;

            // Perform the comparison logic
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

            // Logic: If (A + offsetA) > (B + offsetB) -> Give hediffToGive
            if (strengthA > strengthB)
            {
                if (Props.hediffToGive != null)
                {
                    Hediff newHediff = HediffMaker.MakeHediff(Props.hediffToGive, Pawn, null);
                    newHediff.Severity = Props.severityToGive;
                    Pawn.health.AddHediff(newHediff);
                }
            }

            // Immediately delete the script hediff
            Pawn.health.RemoveHediff(parent);
        }
    }
}
