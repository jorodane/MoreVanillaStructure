using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace MoreVanillaStructure
{
    internal class JobDriver_PlayDarts : JobDriver
    {
        private const int ThrowSpeed = 24;
        private const int ThrowInterval = 180;
        TargetIndex BoardIndex => TargetIndex.A;
        FleckDef dart;

        public override bool TryMakePreToilReservations(bool errorOnFailed) => pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(BoardIndex);
            Building board = job.GetTarget(BoardIndex).Thing as Building;
            if (board == null) { ReadyForNextToil(); }

            yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);

            TargetInfo pawnInfo = new TargetInfo(pawn.Position, pawn.Map);
            TargetInfo boardInfo = new TargetInfo(board.Position, board.Map);
            dart = GetDartFromIndex(Rand.Range(0,4)); //DefDatabase<FleckDef>.GetNamed("Fleck_Dart");
            Toil play = new Toil();
            play.tickAction = () =>
            {
                if (board == null) { ReadyForNextToil(); return; }
                pawn.rotationTracker.FaceTarget(board);
                if (pawn.IsHashIntervalTick(ThrowInterval)) ThrowDartFleck(pawn, board);
                if (JoyUtility.JoyTickCheckEnd(pawn, 1, JoyTickFullJoyAction.EndJob, 1f)) { ReadyForNextToil(); return; }
            };
            play.defaultCompleteMode = ToilCompleteMode.Never;
            play.handlingFacing = true;
            play.socialMode = RandomSocialMode.SuperActive;
            yield return play;
        }

        private void ThrowDartFleck(Pawn thrower, Thing board)
        {
            if (thrower == null || board == null) return;
            Vector3 from = thrower.DrawPos;
            Vector3 to = board.DrawPos;
            Vector3 dir = to - from;
            dir.y = 0f;
            dir.Normalize();
            Vector3 left = new Vector3(-dir.z, 0, dir.x);

            Vector3 offset = (Rand.Range(-0.1f,0f) * dir) + (Rand.Range(-0.2f, 0.2f) * left);
            to += offset;
            dir = to - from;
            float distance = dir.magnitude;
            dir.Normalize();

            FleckCreationData data = FleckMaker.GetDataStatic(from, thrower.Map, dart);

            data.velocity = dir * ThrowSpeed;
            data.rotation = (-Mathf.Rad2Deg * Mathf.Atan2(dir.z, dir.x)) + Rand.Range(-2f, 2f);
            data.airTimeLeft = distance / ThrowSpeed;
            thrower.Map.flecks.CreateFleck(data);
        }

        public static FleckDef GetDartFromIndex(int index)
        {
            switch(index) 
            {
                case 0: return MoreVanillaStructureDefs.Fleck_Dart_Red;
                case 1: return MoreVanillaStructureDefs.Fleck_Dart_Yellow;
                case 2: return MoreVanillaStructureDefs.Fleck_Dart_Blue;
                default: return MoreVanillaStructureDefs.Fleck_Dart_Green;
            }
        }
    }
}
