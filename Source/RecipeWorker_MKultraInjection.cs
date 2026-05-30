using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace HelodBlancasDrug
{
    [StaticConstructorOnStartup]
    public static class MKultraBootloader
    {
        static MKultraBootloader()
        {
            Log.Warning("[HelodBlancasDrug] MKultra C# Module Initialized");
        }
    }

    public class RecipeWorker_MKultraInjection : Recipe_Surgery
    {
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            Log.Warning($"[MKultra] ApplyOnPawn triggered for {pawn?.LabelShort ?? "NULL"} by {billDoer?.LabelShort ?? "NULL"}. Target part: {part?.Label ?? "General"}");

            if (pawn == null)
            {
                Log.Error("[MKultra] Target pawn is null!");
                return;
            }

            Faction originalFaction = pawn.Faction;

            // --- 1. RECRUITMENT LOGIC ---
            if (pawn.guest != null && pawn.guest.IsPrisoner)
            {
                Log.Warning("[MKultra] Pawn is a prisoner. Proceeding with recruitment.");
                
                // Force faction join
                pawn.guest.SetGuestStatus(null, GuestStatus.Guest);
                pawn.SetFaction(Faction.OfPlayer, billDoer);
                
                // Feedback
                string label = "LetterLabelMKultraRecruit".Translate(pawn.LabelShortCap);
                string text = "LetterMKultraRecruit".Translate(pawn.LabelShortCap);
                Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.PositiveEvent, pawn);
                Messages.Message("MessageMKultraRecruited".Translate(pawn.LabelShortCap), pawn, MessageTypeDefOf.PositiveEvent);
                
                TaleRecorder.RecordTale(TaleDefOf.Recruited, new object[] { billDoer, pawn });

                if (originalFaction != null && originalFaction != Faction.OfPlayer)
                {
                    ReportViolation(pawn, billDoer, originalFaction, -50);
                }
            }
            else
            {
                Log.Warning($"[MKultra] Recruitment skipped. IsPrisoner: {pawn.guest?.IsPrisoner}, Faction: {pawn.Faction?.def?.defName ?? "None"}");
            }

            // --- 2. DAMAGE LOGIC ---
            Log.Warning("[MKultra] Applying neurological and ocular damage.");

            // Apply MKultra Conditioning Hediff
            pawn.health.AddHediff(HediffDef.Named("MKultraConditioning"), null, null);

            // Brain Scar
            BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
            if (brain != null)
            {
                ApplyPermanentInjury(pawn, brain, 5f, "MKultra brain conditioning");
                Log.Warning("[MKultra] Brain damage applied.");
            }
            else
            {
                Log.Warning("[MKultra] Could not find brain part!");
            }

            // Eye Scar
            var eyes = pawn.RaceProps.body.AllParts.Where(p => p.def == BodyPartDefOf.Eye && !pawn.health.hediffSet.PartIsMissing(p)).ToList();
            if (eyes.Count > 0)
            {
                BodyPartRecord eye = eyes.RandomElement();
                ApplyPermanentInjury(pawn, eye, 3f, "MKultra visual trauma");
                Log.Warning($"[MKultra] Eye damage applied to {eye.Label}.");
            }
            else
            {
                Log.Warning("[MKultra] No intact eyes found to damage.");
            }

            Log.Warning("[MKultra] Injection logic completed.");
        }

        private void ApplyPermanentInjury(Pawn pawn, BodyPartRecord part, float damage, string label)
        {
            // Create a standard cut injury
            Hediff_Injury injury = (Hediff_Injury)HediffMaker.MakeHediff(HediffDefOf.Cut, pawn, part);
            injury.Severity = damage;
            
            // Force it to be permanent (a scar)
            HediffComp_GetsPermanent comp = injury.TryGetComp<HediffComp_GetsPermanent>();
            if (comp != null)
            {
                comp.IsPermanent = true;
                // Optional: set a unique label if supported by version/comp
            }
            
            pawn.health.AddHediff(injury, part, null);
        }
    }
}
