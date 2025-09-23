using RimWorld;
using System.Collections.Generic;
using System.Linq;
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

        const float headerHeight = 28f;
        const float rowHeight = 26f;

        Vector2 scrollPosition;

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
            Rect listRect = new Rect(windowRect.x, headerHeight + 6.0f, windowRect.width, windowRect.height - headerHeight - 6.0f);
            Rect viewRect = new Rect(0f, 0f, listRect.width - 16.0f, colonists.Count * rowHeight);

            IEnumerable<Pawn> selectedPawn = Find.Selector.SelectedObjectsListForReading.OfType<Pawn>();

            Widgets.BeginScrollView(listRect, ref scrollPosition, viewRect);
            Rect rowRect = new Rect(0f, 0f, viewRect.width, rowHeight);
            Rect checkRect = new Rect(0f,0f,24f,24f);
            foreach (Pawn currentPawn in colonists)
            {
                Widgets.DrawHighlightIfMouseover(rowRect);
                if(selectedPawn.Contains(currentPawn)) Widgets.DrawLightHighlight(rowRect);

                bool isSelected = drafter.IsSelected(currentPawn);
                checkRect.x = rowRect.x;
                checkRect.y = rowRect.y;

                Widgets.Checkbox(checkRect.position,ref isSelected);

                rowRect.y += rowHeight;
            }
            Widgets.EndScrollView();
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

        public bool IsSelected(Pawn pawn) => selectedPawn.Contains(pawn);

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
