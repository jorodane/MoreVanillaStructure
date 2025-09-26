using MoreVanillaStructure;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using static UnityEngine.GraphicsBuffer;

namespace CallToArms
{
    public static class Extension_Pawn
    {
        public static bool IsDraftable(this Pawn target, Map map) => target.Spawned && !target.DestroyedOrNull() && target.Faction == Faction.OfPlayer && target.drafter != null && !target.drafter.Drafted && !target.DeadOrDowned && !target.InMentalState && target.Map == map;
		public static bool IsValidArea(this Area area, Map map) => area.Map != null && area.Map == map && map.areaManager.AllAreas.Contains(area);
		public static Pawn IsCarryingBaby(this Pawn target)
		{
			if (!ModsConfig.BiotechActive) return null;

			Pawn carriedPawn = target.carryTracker?.CarriedThing as Pawn;
			if(carriedPawn != null && carriedPawn.RaceProps.Humanlike)
			{
				DevelopmentalStage stage = carriedPawn.ageTracker?.CurLifeStage?.developmentalStage ?? DevelopmentalStage.None;

				return stage <= DevelopmentalStage.Baby ? carriedPawn : null;
			}
			return null;
		}
    }

    public class ITab_DraftSetting : ITab
    {
		static List<Pawn> savedList = null;
		Area savedArea = null;

		static readonly Vector2 tabSize = new Vector2(400f,600f);
		static readonly Vector2 portraitSize = new Vector2(rowHeight, rowHeight);
		bool? mouseSelected = null;

        const string tabNameKey = "CallToArms_Tab_DraftSelecter";
		const float headerHeight = 28f;
		const float headerPadding = 16.0f;
		const float copyButtonSize = 50.0f;
		const float listPadding = 5.0f;
		const float viewPadding = 16.0f;
		const float rowPadding = 0.0f;
        const float rowHeight = 30f;
		const float checkSize = 24f;

        Vector2 scrollPosition;
		bool allSelected = false;


		public string GetDraftAllString() => "CallToArms_Button_DraftAll".Translate();
		public string GetDraftCopyString() => "CallToArms_Button_DraftCopy".Translate();
		public string GetDraftPasteString() => "CallToArms_Button_DraftPaste".Translate();
		public string GetDraftAreaEmptyString() => "CallToArms_Button_DraftAreaEmpty".Translate();

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

			Rect copyRect = headerRect;
			copyRect.width = copyButtonSize;
			copyRect.x = size.x - copyRect.width - 30.0f;
			if (Widgets.ButtonText(copyRect, GetDraftCopyString()))
			{
				savedList = drafter.GetSelected();
				savedArea = drafter.DraftArea;
			}

			if (savedList != null)
			{
				Rect pasteRect = copyRect;
				copyRect.width = copyButtonSize;
				copyRect.x -= copyButtonSize;
				if (Widgets.ButtonText(copyRect, GetDraftPasteString()))
				{
					drafter?.SetSelected(savedList, true);
					drafter.DraftArea = savedArea;
				}
			}

			Text.Font = GameFont.Medium;
            Widgets.Label(headerRect, tabNameKey.Translate());
            Text.Font = GameFont.Small;

			float currentHeight = headerHeight + headerPadding;

			List<Pawn> colonists = Find.ColonistBar.GetColonistsInOrder();
			Rect listRect = new Rect(windowRect.x, currentHeight, windowRect.width, 0.0f);
            Rect viewRect = new Rect(0f, 0f, listRect.width - viewPadding, colonists.Count * rowHeight);

            Rect rowRect = new Rect(rowPadding, 0f, viewRect.width, rowHeight);
            Rect pawnInfoRect = new Rect(rowRect.x, rowRect.y, viewRect.width - checkSize - rowPadding, rowHeight);
			Rect checkRect = new Rect(viewRect.width - checkSize, rowRect.y, checkSize, checkSize);

            Rect draftAreaRect = pawnInfoRect;
            draftAreaRect.x = viewPadding;
            draftAreaRect.width = windowRect.width - 150.0f;
            draftAreaRect.y = currentHeight;

            Rect draftAllRect = draftAreaRect;
			draftAllRect.x = draftAreaRect.x + draftAreaRect.width + viewPadding;
			draftAllRect.width = windowRect.width - draftAllRect.x - viewPadding;

			listRect.y = currentHeight += draftAllRect.height + viewPadding;
			listRect.height = windowRect.height - currentHeight - listPadding;

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
			string draftAreaEmpty = GetDraftAreaEmptyString();
			drafter.CheckValidDraftArea();
            string draftButtonText = drafter.DraftArea?.Label ?? draftAreaEmpty;
			if (Widgets.ButtonText(draftAreaRect, draftButtonText))
			{
				List<FloatMenuOption> areaOptions = new List<FloatMenuOption>();
				areaOptions.Add(new FloatMenuOption(draftAreaEmpty, () => drafter.DraftArea = null));

				foreach (Area currentArea in currentMap.areaManager.AllAreas.OfType<Area_Allowed>())
				{
					Area tempArea = currentArea;
					areaOptions.Add(new FloatMenuOption(tempArea.Label, () => drafter.DraftArea = tempArea));
				}
				Find.WindowStack.Add(new FloatMenu(areaOptions));
			}
			Widgets.Label(draftAllRect, GetDraftAllString());
			Widgets.Checkbox(draftAllRect.position + (Vector2.right * draftAllRect.width), ref allSelected);
			if (wasAllSelected != allSelected) drafter.SetSelected(colonists, allSelected);

			bool isLeftClick = currentEvent.button == 0;
			if (!isLeftClick && mouseSelected.HasValue) mouseSelected = null;
			
			Widgets.BeginScrollView(listRect, ref scrollPosition, viewRect);
			allSelected = true;
			foreach (Pawn currentColonist in colonists)
            {
				RenderTexture currentPortrait = PortraitsCache.Get(currentColonist, portraitSize, Rot4.South);
				ThingWithComps weapon = currentColonist.equipment?.Primary;
                Widgets.DrawHighlightIfMouseover(rowRect);
                if(currentColonist.Drafted) Widgets.DrawLightHighlight(rowRect);

				bool wasSelected = drafter.IsSelected(currentColonist);
				bool isSelected = wasSelected;
				bool tempSelected = isSelected;
				checkRect.y = rowRect.y;
				GUI.DrawTexture(pawnPortraitRect, currentPortrait);
				TipSignal currentColonistTip = new TipSignal($"{currentColonist.LabelCap}\n{currentColonist.GetInspectString()}", currentColonist.thingIDNumber);
                TooltipHandler.TipRegion(pawnPortraitRect, currentColonistTip);
				Widgets.InfoCardButton(pawnDetailRect.x, pawnDetailRect.y, currentColonist);
				if(weapon != null)
				{
					Widgets.ThingIcon(pawnWeaponRect, weapon);
					TooltipHandler.TipRegion(pawnWeaponRect, new TipSignal(weapon.LabelCapNoCount, weapon.thingIDNumber));
				}
				Widgets.Label(pawnNameRect, currentColonist.LabelCap);
                TooltipHandler.TipRegion(pawnNameRect, currentColonistTip);
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
					drafter.SetSelected(currentColonist, isSelected);
				}

				if(Widgets.ButtonInvisible(pawnInfoRect))
				{
					switch (currentEvent.button)
					{
						case 0:
							CameraJumper.TryJump(currentColonist);
                            Find.Selector.ClearSelection();
                            Find.Selector.Select(currentColonist, playSound: true);
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

	public class PlacWorker_TownBellDraftArea : PlaceWorker
	{
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            base.DrawGhost(def, center, rot, ghostCol, thing);
			Map currentMap = Find.CurrentMap;
			if (currentMap == null) return;
			CompProperties_EmergencyDrafter props = def.GetCompProperties<CompProperties_EmergencyDrafter>();

            GenDraw.DrawRadiusRing(center, Mathf.Max(1, props?.draftRadius ?? 8));
        }
    }

    public class CompProperties_EmergencyDrafter : CompProperties
    {
		public int draftRadius = 25;
        public CompProperties_EmergencyDrafter()
        {
            compClass = typeof(CompEmergencyDrafter);
        }
    }

	public class Building_TownBell : Building
	{
		//public override void DrawExtraSelectionOverlays()
		//{
		//	base.DrawExtraSelectionOverlays();
		//	InspectPaneUtility.OpenTab(typeof(ITab_DraftSetting));
		//}
	}

    public class JobDriver_DraftAsJob : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_General.DoAtomic(() =>
			{
				if (pawn.IsDraftable(pawn.Map))
				{
					Queue<Job> origins = new Queue<Job>();
                    foreach (QueuedJob currentQueue in pawn.jobs.jobQueue.ToArray()) origins.Enqueue(currentQueue.job.Clone());
                    pawn.drafter.Drafted = true;

					foreach (Job currentQueue in origins) pawn.jobs.jobQueue.EnqueueLast(currentQueue);
                }
            });
        }
    }


    public class CompEmergencyDrafter : ThingComp
    {
        public Texture2D GetCallToArms4Selected_MenuIcon() => ContentFinder<Texture2D>.Get("UI/Commands/CallToArms_Selected", true);
        public string GetCallToArms4SelectedLableString() => "CallToArms_Selected_Lable".Translate();
        public string GetCallToArms4SelectedDescriptionString() => "CallToArms_Selected_Description".Translate();
        public string GetCallToArms4NotSelectedDescriptionString() => "CallToArms_Not_Selected_Description".Translate();
        public string GetCallToArms4HasNotDraftableDescriptionString() => "CallToArms_HasNot_Draftable_Description".Translate();

        public Texture2D GetCallToArms4All_MenuIcon() => ContentFinder<Texture2D>.Get("UI/Commands/CallToArms_All", true);
        public string GetCallToArms4AllLableString() => "CallToArms_All_Lable".Translate();
        public string GetCallToArms4AllDescriptionString() => "CallToArms_All_Description".Translate();

		public Texture2D GetDraftAllowCarryingBaby_MenuIcon() => ContentFinder<Texture2D>.Get("UI/Commands/DraftWithBaby", true);
		public string GetDraftAllowCarryingBabyLableString() => "CallToArms_AllowCarryingBaby_Lable".Translate();
		public string GetDraftAllowCarryingBabyDescriptionString() => "CallToArms_AllowCarryingBaby_Description".Translate();

		public string GetDraftAreaNotEnoughString(int count) => "CallToArms_Message_DraftAreaNotEnough".Translate(count.Named("count"));
		public string GetDraftCancelByCarryingBabyString(int count) => "CallToArms_Message_DraftCancelByCarryingBaby".Translate(count.Named("count"));

        List<Pawn> selectedColonist = new List<Pawn>();

        public CompProperties_EmergencyDrafter Props => (CompProperties_EmergencyDrafter)props;

		public bool draftGlobal = false;

		public bool draftCarryingBaby = false;

		Area _draftArea;
        public Area DraftArea
        {
            get => HasValidDraftArea() ? _draftArea : null;
            set => _draftArea = value;
        }

        public bool HasSelectedColonist => selectedColonist.Count > 0;
        public bool HasDraftableSelectedColonist => selectedColonist.Any((current) => current != null && current.IsDraftable(parent.Map));

		public bool HasValidDraftArea() => _draftArea != null && _draftArea.IsValidArea(parent.Map);
		public void CheckValidDraftArea() { if (_draftArea != null && !_draftArea.IsValidArea(parent.Map)) _draftArea = null; }
        public bool IsSelected(Pawn target) => selectedColonist.Contains(target);
		public void ToggleSelected(Pawn target) { if (IsSelected(target)) selectedColonist.Remove(target); else selectedColonist.Add(target); }
		public void SetSelected(Pawn target, bool value) { if (value) { if(!IsSelected(target))selectedColonist.Add(target); } else selectedColonist.Remove(target); }
		public void SetSelected(List<Pawn> newList, bool value) 
		{
			selectedColonist.Clear();
			if (value) selectedColonist.AddRange(newList.Where(current => current.Map == parent.Map)); 
		}
		public List<Pawn> GetSelected() => selectedColonist;
		public bool GetAllowCarryingBaby() => draftCarryingBaby;
		public void SetAllowCarryingBaby(bool value) => draftCarryingBaby = value;
		public void ToggleAllowCarryingBaby() => SetAllowCarryingBaby(!draftCarryingBaby);

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Collections.Look(ref selectedColonist, "SelectedPawn", LookMode.Reference);
			Scribe_References.Look(ref _draftArea, "DraftArea");
			Scribe_Values.Look(ref draftCarryingBaby, "AllowCarryingBaby");
			if(Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				selectedColonist?.RemoveAll((currentPawn) => currentPawn.DestroyedOrNull());
			}
		}

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Action
            {
                Disabled = !HasDraftableSelectedColonist,
                disabledReason = !HasSelectedColonist ? GetCallToArms4NotSelectedDescriptionString()
								: !HasDraftableSelectedColonist ? GetCallToArms4HasNotDraftableDescriptionString() : "",
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

			if(ModsConfig.BiotechActive)
			{
				yield return new Command_Toggle
				{
					isActive = GetAllowCarryingBaby,
					defaultLabel = GetDraftAllowCarryingBabyLableString(),
					defaultDesc = GetDraftAllowCarryingBabyDescriptionString(),
					icon = GetDraftAllowCarryingBaby_MenuIcon(),
					toggleAction = ToggleAllowCarryingBaby
				};
			}
        }

		IEnumerable<IntVec3> GetDraftableSpots(IntVec3 from, int radius, Area targetArea = null)
		{
			Map currentMap = parent.Map;
			return GenRadial.RadialCellsAround(from, radius, true)
				.Where(current => current.InBounds(currentMap) && current.Standable(currentMap) && (targetArea == null || targetArea[current]))
				.OrderBy(current => current.DistanceToSquared(from));
		}

		void DraftAndMove(Pawn target, IntVec3 location)
		{
			if(target == null) return;
			Map map = target.Map;
			if (map != parent.Map) return;


			target.jobs.ClearQueuedJobs();

			Queue<Job> jobs = new Queue<Job>();

			Pawn carryingBaby = target.IsCarryingBaby();
            if (draftCarryingBaby && carryingBaby != null) JobEnqueue(jobs, JobMaker.MakeJob(JobDefOf.BringBabyToSafety, carryingBaby));
            JobEnqueue(jobs, JobMaker.MakeJob(CallToArmsDefs.DraftAsJob));

            MakeJobQueue(target, location, jobs);

			Building interactionBuilding = map.listerBuildings.allBuildingsColonist.Find(current => (current.def?.hasInteractionCell ?? false) && current.InteractionCell == location && (current.GetComp<CompMannable>() != null));
			if (interactionBuilding != null)
			{
				JobEnqueue(jobs, JobMaker.MakeJob(JobDefOf.ManTurret, interactionBuilding));
			}
			else
			{
				JobEnqueue(jobs, JobMaker.MakeJob(JobDefOf.Goto, location));
			}
			Job firstJob = jobs.Dequeue();
			foreach (Job currentJob in jobs) target.jobs.jobQueue.EnqueueLast(currentJob);

			target.jobs.StartJob(firstJob, JobCondition.InterruptForced, target.thinker.MainThinkNodeRoot, false, true, null, JobTag.DraftedOrder, true);
        }

		public virtual void JobEnqueue(Queue<Job> result, Job wantJob)
		{
            wantJob.playerForced = true;
            result.Enqueue(wantJob);
        }

		public virtual void MakeJobQueue(Pawn target, IntVec3 location, Queue<Job> result)
		{

		}

		public void CheckCarryingBabyAlert(List<Pawn> from)
		{
			if (draftCarryingBaby) return;
            IEnumerable<Pawn> carryingBabyTargets = from.Where(current => current.IsCarryingBaby() != null);
            int carryingBabyCount = carryingBabyTargets.Count();
            if (carryingBabyCount > 0) Messages.Message(GetDraftCancelByCarryingBabyString(carryingBabyCount), carryingBabyTargets.ToList(), MessageTypeDefOf.NegativeEvent, false);
        }

        public void OnCallToArms4Selected()
		{
			CheckValidDraftArea();
            if (selectedColonist.Any(current => current.DestroyedOrNull())) selectedColonist = selectedColonist.Where(current => !current.DestroyedOrNull()).ToList();

			CheckCarryingBabyAlert(selectedColonist);

            List<Pawn> draftTargets = selectedColonist
			.Where(current => (draftCarryingBaby || current.IsCarryingBaby() == null) && current.IsDraftable(parent.Map))
			.OrderBy(current => current.Position.DistanceToSquared(parent.Position))
			.ToList();
			CalltoArms(draftTargets);
		}


		public void OnCallToArms4All()
        {
			List<Pawn> colonist = Find.ColonistBar.GetColonistsInOrder();
            CheckCarryingBabyAlert(colonist);

            List<Pawn> draftTargets = colonist
                .Where(current => (draftCarryingBaby || current.IsCarryingBaby() == null) && current.IsDraftable(parent.Map))
                .ToList();

			CalltoArms(draftTargets);
		}

		void CalltoArms(List<Pawn> targetList)
		{
			if(targetList == null) return;

			List<IntVec3> draftLocations = GetDraftableSpots(parent.Position, Props.draftRadius, DraftArea).ToList();

			int originCount = targetList.Count();
			int maxCount = Mathf.Min(originCount, draftLocations.Count());
			Find.Selector.ClearSelection();
			for (int i = 0; i < maxCount; i++)
			{
				Pawn currentTarget = targetList[i];
				DraftAndMove(currentTarget, draftLocations[i]);
				Find.Selector.Select(currentTarget, playSound: true);
			}

			if(originCount > maxCount)
			{
				List<Pawn> missingPawns = new List<Pawn>();
				for (int i = maxCount; i < originCount; i++){missingPawns.Add(targetList[i]);}

				Messages.Message(GetDraftAreaNotEnoughString(originCount - maxCount), missingPawns, MessageTypeDefOf.NegativeEvent, false);
			}
		}
	}
}
