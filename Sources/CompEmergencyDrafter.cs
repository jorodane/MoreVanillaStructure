using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MoreVanillaStructure
{
    public static class Extension_Pawn
    {
        public static bool IsDraftable(this Pawn pawn) => pawn.Spawned && pawn.Faction == Faction.OfPlayer && pawn.drafter != null && !pawn.DeadOrDowned && !pawn.InMentalState;
    }

    public class ITab_DraftSetting : ITab
    {
		static readonly Vector2 tabSize = new Vector2(400f,600f);
		static readonly Vector2 portraitSize = new Vector2(rowHeight, rowHeight);
		bool? mouseSelected = null;

        const string tabNameKey = "MVS_Tab_DraftSelecter";
		const float headerHeight = 28f;
		const float headerPadding = 12.0f;
		const float listPadding = 15.0f;
		const float viewPadding = 16.0f;
		const float rowPadding = 0.0f;
        const float rowHeight = 30f;
		const float checkSize = 24f;

        Vector2 scrollPosition;
		bool allSelected = false;


		public string GetDraftAllString() => "MVS_Button_DraftAll".Translate();

		public ITab_DraftSetting()
        {
            size = tabSize;
            labelKey = tabNameKey;
        }

        protected override void FillTab()
        {
			Event currentEvent = Event.current;

			CompEmergencyDrafter drafter = SelThing?.TryGetComp<CompEmergencyDrafter>();
            Map currentMap = SelThing?.Map;
            if (drafter == null || currentMap == null) return;

            Rect windowRect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
            Rect headerRect = windowRect;
            headerRect.height = headerHeight;

            Text.Font = GameFont.Medium;
            Widgets.Label(headerRect, tabNameKey.Translate());
            Text.Font = GameFont.Small;

			float currentHeight = headerHeight + headerPadding;

			List<Pawn> colonists = Find.ColonistBar.GetColonistsInOrder();
			Rect listRect = new Rect(windowRect.x, currentHeight, windowRect.width, windowRect.height - currentHeight - listPadding);
            Rect viewRect = new Rect(0f, 0f, listRect.width - viewPadding, colonists.Count * rowHeight);

            Rect rowRect = new Rect(rowPadding, 0f, viewRect.width, rowHeight);
            Rect pawnInfoRect = new Rect(rowRect.x, rowRect.y, viewRect.width - checkSize - rowPadding, rowHeight);
			Rect checkRect = new Rect(viewRect.width - checkSize, rowRect.y, checkSize, checkSize);

			Rect draftAllRect = pawnInfoRect;
			draftAllRect.x = windowRect.width - 100.0f;
			draftAllRect.width = 76.0f;
			draftAllRect.y = currentHeight;
			listRect.y = currentHeight += draftAllRect.height;

			Rect pawnPortraitRect = pawnInfoRect;
			pawnPortraitRect.width = rowHeight;
			Rect pawnDetailRect = pawnPortraitRect;
			pawnDetailRect.x = pawnPortraitRect.width;
			Rect pawnNameRect = pawnInfoRect;
			pawnNameRect.x = pawnDetailRect.x + pawnDetailRect.width;
			pawnNameRect.width = pawnInfoRect.width - pawnNameRect.x - pawnPortraitRect.width;
			Rect pawnWeaponRect = pawnPortraitRect;
			pawnWeaponRect.x = pawnNameRect.x + pawnNameRect.width;

			bool wasAllSelected = allSelected;
			Widgets.Label(draftAllRect, GetDraftAllString());
			Widgets.Checkbox(draftAllRect.position + (Vector2.right * draftAllRect.width), ref allSelected);
			if (wasAllSelected != allSelected) drafter.SetSelected(colonists, allSelected);

			bool isLeftClick = currentEvent.button == 0;
			if (!isLeftClick && mouseSelected.HasValue) mouseSelected = null;
			
			Widgets.BeginScrollView(listRect, ref scrollPosition, viewRect);
			allSelected = true;
			foreach (Pawn currentPawn in colonists)
            {
				RenderTexture currentPortrait = PortraitsCache.Get(currentPawn, portraitSize, Rot4.South);
				ThingWithComps weapon = currentPawn.equipment?.Primary;
                Widgets.DrawHighlightIfMouseover(rowRect);
                if(currentPawn.Drafted) Widgets.DrawLightHighlight(rowRect);

				bool wasSelected = drafter.IsSelected(currentPawn);
				bool isSelected = wasSelected;
				bool tempSelected = isSelected;
				checkRect.y = rowRect.y;
				GUI.DrawTexture(pawnPortraitRect, currentPortrait);
				TooltipHandler.TipRegion(pawnPortraitRect, new TipSignal($"{currentPawn.LabelCap}\n{currentPawn.GetInspectString()}", currentPawn.thingIDNumber));
				Widgets.InfoCardButton(pawnDetailRect.x, pawnDetailRect.y, currentPawn);
				if(weapon != null)
				{
					Widgets.ThingIcon(pawnWeaponRect, weapon);
					TooltipHandler.TipRegion(pawnWeaponRect, new TipSignal(weapon.LabelCapNoCount, weapon.thingIDNumber));
				}
				Widgets.Label(pawnNameRect, currentPawn.LabelCap);
                Widgets.Checkbox(checkRect.position, ref tempSelected);
				if(currentEvent.isMouse && checkRect.Contains(currentEvent.mousePosition) && isLeftClick)
				{
					if(currentEvent.type == EventType.MouseDown) mouseSelected = isSelected = !wasSelected;
					else if(mouseSelected.HasValue)
					{
						if (currentEvent.type == EventType.MouseDrag)	isSelected = mouseSelected.Value;
						else											mouseSelected = null;
					}
				}

				if (isSelected != wasSelected)
				{
					drafter.SetSelected(currentPawn, isSelected);
				}

				if(Widgets.ButtonInvisible(pawnInfoRect))
				{
					switch (currentEvent.button)
					{
						case 0:
							CameraJumper.TryJump(currentPawn);
							break;
					}
					currentEvent.Use();
				}

				pawnPortraitRect.y = pawnDetailRect.y = pawnWeaponRect.y = pawnNameRect.y = pawnInfoRect.y = rowRect.y += rowHeight;
				allSelected &= isSelected;
            }
            Widgets.EndScrollView();

			currentHeight += colonists.Count * rowHeight;
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
		public void ToggleSelected(Pawn pawn) { if (IsSelected(pawn)) selectedPawn.Remove(pawn); else selectedPawn.Add(pawn); }
		public void SetSelected(Pawn pawn, bool value) { if (value) { if(!IsSelected(pawn))selectedPawn.Add(pawn); } else selectedPawn.Remove(pawn); }
		public void SetSelected(List<Pawn> newList, bool value) 
		{
			selectedPawn.Clear();
			if (value) selectedPawn.AddRange(newList); 
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Collections.Look(ref selectedPawn, "SelectedPawn", LookMode.Reference);
			if(Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				selectedPawn?.RemoveAll((currentPawn) => currentPawn.DestroyedOrNull());
			}
		}

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Action
            {
                Disabled = !HasDraftableSelectedPawn,
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
            foreach(Pawn currentPawn in selectedPawn)
			{
				if(currentPawn.IsDraftable())
				{
					currentPawn.drafter.Drafted = true;
				}
			}
        }

        public void OnCallToArms4All()
        {
			List<Pawn> colonists = Find.ColonistBar.GetColonistsInOrder();
			foreach (Pawn currentPawn in colonists)
			{
				if (currentPawn.IsDraftable() && currentPawn.Map == parent.Map)
				{
					currentPawn.drafter.Drafted = true;
				}
			}
		}
	}
}
