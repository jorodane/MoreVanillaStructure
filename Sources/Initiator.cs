using RimWorld;
using Verse;
using UnityEngine;

namespace CallToArms
{
    [DefOf]
    public static class CallToArmsDefs
    {
        public static JobDef DraftAsJob;
    }
}

    namespace MoreVanillaStructure
{
    public class MoreVanillaStructureSettings : ModSettings
    {
        //public int draftRadius = 25;

        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_Values.Look(ref draftRadius, "DraftRadius", 25);
        }
    }

    public class MoreVanillaStructureMod : Mod
    {
        public string GetLevelDescriptionString(int level) => "MVS_Fan_Level_Desc".Translate();

        public static MoreVanillaStructureSettings Settings;

        public MoreVanillaStructureMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<MoreVanillaStructureSettings>();
        }

        //public override string SettingsCategory() => "More Vanilla Structure";

        //public override void DoSettingsWindowContents(Rect inRect)
        //{
        //    Listing_Standard listing = new Listing_Standard();

        //    listing.Begin(inRect);

        //    listing.End();
        //}
    }

    [DefOf]
    public static class MoreVanillaStructureDefs
    {
        public static JobDef Play_Darts;

        public static ThoughtDef HitBetweenTheEyes;

        public static HediffDef FanBreeze;

        public static FleckDef Fleck_Dart_Red;
        public static FleckDef Fleck_Dart_Yellow;
        public static FleckDef Fleck_Dart_Blue;
        public static FleckDef Fleck_Dart_Green;
    }
}
