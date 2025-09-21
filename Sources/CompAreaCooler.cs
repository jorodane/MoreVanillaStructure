using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Noise;

namespace MoreVanillaStructure
{
    public class CompProperties_AreaCooler : CompProperties
    {
        public float heatPerSecond;
        public float heatPushMinTemperature;
        public const int tickRare = 250;

        public CompProperties_AreaCooler()
        {
            compClass = typeof(CompAreaCooler);
        }
    }


    public class CompAreaCooler : ThingComp
    {
        public CompProperties_AreaCooler Props => (CompProperties_AreaCooler)props;
        CompPowerTrader power;
        CompFlickable flick;
        IntVec3 cellMin, cellMax;

        float heatPushPerRareOrigin;
        int level = 0;
        bool isInstalled = false;

        public bool isOn => (power != null && power.PowerOn) && (flick == null || flick.SwitchIsOn);

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            power = parent.TryGetComp<CompPowerTrader>();
            flick = parent.TryGetComp<CompFlickable>();
            heatPushPerRareOrigin = Props.heatPerSecond * (CompProperties_AreaCooler.tickRare / 60f);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            RefreshCoolingCell();
            isInstalled = true;
        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            base.PostDeSpawn(map, mode);
            cellMin = cellMax = IntVec3.Zero;
            isInstalled = false;
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (!(isInstalled && isOn)) return;

            Map currentMap = parent.Map;
            if (currentMap == null) return;

            for (int i = cellMin.x; i <= cellMax.x; i++)
            {
                for (int j = cellMin.z; j <= cellMax.z; j++)
                {
                    InCellAction(new IntVec3(i, 0, j));
                }
            }
        }

        public void RefreshCoolingCell()
        {
            BuildingProperties currentBuilding = parent.def.building;
            IntRange depth = currentBuilding.watchBuildingStandDistanceRange;
            int width = currentBuilding.watchBuildingStandRectWidth;
            int halfWidth = (width - 1) / 2;

            IntVec3 forward = parent.Rotation.FacingCell;
            IntVec3 right = parent.Rotation.Rotated(RotationDirection.Clockwise).FacingCell;
            IntVec3 position = parent.Position;

            IntVec3 forwardMax = position + (forward * depth.max);
            IntVec3 forwardMin = position + (forward * depth.min);
            IntVec3 rightMax = position + (right * halfWidth);
            IntVec3 rightMin = position - (right * (width - 1 - halfWidth));

            cellMin.x = Mathf.Min(forwardMax.x, forwardMin.x, rightMax.x, rightMin.x);
            cellMin.z = Mathf.Min(forwardMax.z, forwardMin.z, rightMax.z, rightMin.z);
            cellMax.x = Mathf.Max(forwardMax.x, forwardMin.x, rightMax.x, rightMin.x);
            cellMax.z = Mathf.Max(forwardMax.z, forwardMin.z, rightMax.z, rightMin.z);
        }

        public void InCellAction(IntVec3 location)
        {
            Map currentMap = parent.Map;
            if (!location.InBounds(currentMap)) return;
            Building currentBuilding = location.GetEdifice(currentMap);
            if (currentBuilding != null && currentBuilding.def.passability == Traversability.Impassable) return;

            float cellTemperature = location.GetTemperature(currentMap);
            if (cellTemperature <= Props.heatPushMinTemperature) return;
            GenTemperature.PushHeat(location, currentMap, heatPushPerRareOrigin);

            foreach(Thing currentThing in location.GetThingList(currentMap))
            {
                if(currentThing is Pawn asPawn)
                {
                    Hediff fanBreeze = asPawn.health.GetOrAddHediff(MoreVanillaStructureDefs.FanBreeze);
                    fanBreeze.Severity = level * 0.5f;
                }
            }
        }
    }
}
