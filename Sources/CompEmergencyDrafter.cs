using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MoreVanillaStructure
{
    public static class Extension_Pawn
    {
        public static bool IsDraftable(this Pawn pawn) => pawn.Spawned && pawn.Faction == Faction.OfPlayer && !pawn.DeadOrDowned;
    }

    public class ITab_DraftSetting : ITab
    {
        static readonly Vector2 tabSize = new Vector2(300f,480f);
        const string tabNameKey = "MVS_Tab_DraftSelecter";

        Vector2 scrollPos;

        public ITab_DraftSetting()
        {
            size = tabSize;
            labelKey = tabNameKey;
        }

        protected override void FillTab()
        {
            CompEmergencyDrafter drafter = SelThing?.TryGetComp<CompEmergencyDrafter>();
            Map currentMap = SelThing?.Map;
            if (drafter == null || currentMap == null) return;

            Rect windowRect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
            Rect headerRect = windowRect;
            headerRect.height = 28f;

            Text.Font = GameFont.Medium;
            Widgets.Label(headerRect, tabNameKey.Translate());
            Text.Font = GameFont.Small;

            List<Pawn> colonists = currentMap.mapPawns.FreeColonistsSpawned;
            foreach(Pawn pawn in colonists)
            {

            }
        }
    }

    public class CompProperties_EmergencyDrafter : CompProperties
    {
        public CompProperties_EmergencyDrafter()
        {
            compClass = typeof(CompEmergencyDrafter);
        }
    }


    internal class CompEmergencyDrafter : ThingComp
    {
        public CompProperties_EmergencyDrafter Props => (CompProperties_EmergencyDrafter)props;

        public Texture2D GetCallToArms4Selected_MenuIcon() => ContentFinder<Texture2D>.Get("UI/Commands/CallToArms_Selected", true);
        public string GetCallToArms4SelectedLableString() => "MVS_CallToArms_Selected_Lable".Translate();
        public string GetCallToArms4SelectedDescriptionString() => "MVS_CallToArms_Selected_Description".Translate();
        public string GetCallToArms4NotSelectedDescriptionString() => "MVS_CallToArms_Not_Selected_Description".Translate();
        public string GetCallToArms4HasNotDraftableDescriptionString() => "MVS_CallToArms_HasNot_Draftable_Description".Translate();
        public Texture2D GetCallToArms4All_MenuIcon() => ContentFinder<Texture2D>.Get("UI/Commands/CallToArms_All", true);
        public string GetCallToArms4AllLableString() => "MVS_CallToArms_All_Lable".Translate();
        public string GetCallToArms4AllDescriptionString() => "MVS_CallToArms_All_Description".Translate();

        List<Pawn> selectedPawn = new List<Pawn>();

        public bool HasSelectedPawn => selectedPawn.Count > 0;
        public bool HasDraftableSelectedPawn => selectedPawn.Any((current) => current != null && current.IsDraftable());

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Action
            {
                Disabled = !HasSelectedPawn,
                disabledReason = !HasSelectedPawn ? GetCallToArms4NotSelectedDescriptionString() : !HasDraftableSelectedPawn ? GetCallToArms4HasNotDraftableDescriptionString() : "",
                defaultLabel = GetCallToArms4SelectedLableString(),
                defaultDesc = GetCallToArms4SelectedDescriptionString(),
                icon = GetCallToArms4Selected_MenuIcon(),
                action = OnCallToArms4Selected
            };

            yield return new Command_Action
            {
                defaultLabel = GetCallToArms4AllLableString(),
                defaultDesc = GetCallToArms4AllDescriptionString(),
                icon = GetCallToArms4All_MenuIcon(),
                action = OnCallToArms4All
            };
        }

        public void OnCallToArms4Selected()
        {
            if (!HasSelectedPawn)
            {
            }
        }

        public void OnCallToArms4All()
        {

        }
    }
}
