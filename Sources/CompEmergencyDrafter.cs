using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MoreVanillaStructure
{
    public class ITab_DraftSetting : ITab
    {
        protected override void FillTab()
        {

        }
    }

    public class CompProperties_EmergencyDrafter : CompProperties
    {

    }


    internal class CompEmergencyDrafter : ThingComp
    {
        public CompProperties_EmergencyDrafter Props => (CompProperties_EmergencyDrafter)props;

        public Texture2D GetCallToArms4Selected_MenuIcon() => ContentFinder<Texture2D>.Get("UI/Commands/CallToArms_Selected", true);
        public string GetCallToArms4SelectedLableString() => "MVS_CallToArms_Selected_Lable".Translate();
        public string GetCallToArms4SelectedDescriptionString() => "MVS_CallToArms_Selected_Description".Translate();
        public Texture2D GetCallToArms4All_MenuIcon() => ContentFinder<Texture2D>.Get("UI/Commands/CallToArms_All", true);
        public string GetCallToArms4AllLableString() => "MVS_CallToArms_All_Lable".Translate();
        public string GetCallToArms4AllDescriptionString() => "MVS_CallToArms_All_Description".Translate();

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Action
            {
                defaultLabel = GetCallToArms4SelectedLableString(),
                defaultDesc = GetCallToArms4SelectedDescriptionString(),
                icon = GetCallToArms4Selected_MenuIcon(), // 임시 아이콘 경로
                action = OnCallToArms4Selected
            };

            yield return new Command_Action
            {
                defaultLabel = GetCallToArms4AllLableString(),
                defaultDesc = GetCallToArms4AllDescriptionString(),
                icon = GetCallToArms4All_MenuIcon(), // 임시 아이콘 경로
                action = OnCallToArms4All
            };
        }

        public void OnCallToArms4Selected()
        {

        }

        public void OnCallToArms4All()
        {

        }
    }
}
