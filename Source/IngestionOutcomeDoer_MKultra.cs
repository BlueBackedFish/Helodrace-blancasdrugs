using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace HelodBlancasDrug
{
    public class IngestionOutcomeDoer_MKultra : IngestionOutcomeDoer
    {
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            Log.Warning($"[MKultra] IngestionOutcome triggered for {pawn?.LabelShort ?? "NULL"}.");

            if (pawn == null) return;

            // Recruitment
            if (pawn.guest != null && pawn.guest.IsPrisoner)
            {
                pawn.guest.SetGuestStatus(null, GuestStatus.Guest);
                pawn.SetFaction(Faction.OfPlayer, null);
                
                string label = "LetterLabelMKultraRecruit".Translate(pawn.LabelShortCap);
                string text = "LetterMKultraRecruit".Translate(pawn.LabelShortCap);
                Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.PositiveEvent, pawn);
            }

            // Damage
            pawn.health.AddHediff(HediffDef.Named("BD_MKultraConditioning"), null, null);
            
            BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
            if (brain != null)
            {
                ApplyScar(pawn, brain, 5f);
            }

            var eyes = pawn.RaceProps.body.AllParts.Where(p => p.def == BodyPartDefOf.Eye && !pawn.health.hediffSet.PartIsMissing(p)).ToList();
            if (eyes.Any())
            {
                ApplyScar(pawn, eyes.RandomElement(), 3f);
            }
        }

        private void ApplyScar(Pawn pawn, BodyPartRecord part, float damage)
        {
            Hediff_Injury injury = (Hediff_Injury)HediffMaker.MakeHediff(HediffDefOf.Cut, pawn, part);
            injury.Severity = damage;
            HediffComp_GetsPermanent comp = injury.TryGetComp<HediffComp_GetsPermanent>();
            if (comp != null) comp.IsPermanent = true;
            pawn.health.AddHediff(injury, part, null);
        }
    }
}