using Quantum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.Deterministic;
using System.Diagnostics;

namespace Quantum
{
    public unsafe class NJRKNetworkControl : SystemMainThreadFilter<NJRKNetworkControl.Filter>, ISignalOnPlayerDataSet, ISignalOnPlayerDisconnected, ISignalOnCollisionEnter3D
    {
        FP maxHP;
        public struct Filter
        {
            public EntityRef Entity;
            public CharacterController3D* CharacterController;
            public Transform3D* Transform;
            public playerlink* Link;
            public PlayerConfig* Config;
            public ObjectType* ObjType;
        }

        void OnInit(ref Filter filter)
        {
            maxHP = 100;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            Input input = default;
            if (f.Unsafe.TryGetPointer(filter.Entity, out playerlink* Link))
            {
                input = *f.GetPlayerInput(Link->Player);
            }
            GameSession* gs = f.Unsafe.GetPointerSingleton<GameSession>();
            if(gs->State == GameState.InProgress)
            {
                if(filter.Config->HP > 0) { filter.Config->isAlive = true; } else { filter.Config->isAlive = false; gs->State = GameState.Finished; }
                if (filter.Config->isAlive)
                {
                    //Log.Debug($"Is alive: {filter.Config->isAlive} | HP: {filter.Config->HP} | Movement Speed: {filter.Config->speed}");
                    input.MoveDir = new FPVector2((FP)input.DirectionX / 10, (FP)input.DirectionY / 10);
                    if (input.MoveDir.SqrMagnitude > 1) { input.MoveDir = input.MoveDir.Normalized; }
                    if (input.MoveDir.SqrMagnitude != default) { filter.Transform->Rotation = FPQuaternion.Lerp(filter.Transform->Rotation, FPQuaternion.LookRotation(input.MoveDir.XOY), f.DeltaTime * 10); }
                    if (filter.Transform->Position.Y < -50) { filter.Transform->Position = GetSpawnPosition(f, filter.Link->Player._index, f.PlayerCount); filter.Config->HP -= 10; }
                    if (input.Jump > 0)
                    {
                        filter.CharacterController->Jump(f);
                    }
                    if (input.FireTrigger > 0)
                    {
                        if (filter.Config->ShotInterval <= 0)
                        {
                            filter.Config->ShotInterval = filter.Config->TimeToShoot;
                            FireBullet(f, filter);
                        }
                        else
                        {
                            filter.Config->ShotInterval -= f.DeltaTime;
                        }
                    }
                    else
                    {
                        filter.Config->ShotInterval = 0;
                    }
                    filter.CharacterController->Move(f, filter.Entity, input.MoveDir.XOY);
                }
            }
            else
            {
                return;
            }
        }

        void FireBullet(Frame f, Filter filter)
        {
            Log.Debug("Fire!");
            EntityPrototype bProto = f.FindAsset<EntityPrototype>("Resources/DB/Bullet|EntityPrototype");
            EntityRef bullet = f.Create(bProto);
            Transform3D* bulletTransform = f.Unsafe.GetPointer<Transform3D>(bullet);
            bulletTransform->Position = new FPVector3(filter.Transform->Position.X, filter.Transform->Position.Y + (short)2.0, filter.Transform->Position.Z) + filter.Transform->Forward * 2;
            bulletTransform->Rotation = filter.Transform->Rotation;
        }

        public void OnPlayerDataSet(Frame f, PlayerRef player)
        {
            RuntimePlayer data = f.GetPlayerData(player);
            EntityPrototype playerProto = f.FindAsset<EntityPrototype>(data.CharacterPrototype.Id);
            EntityRef Player = f.Create(playerProto);
            if (f.Unsafe.TryGetPointer<playerlink>(Player, out var playerLink))
            {
                playerLink->Player = player;
            }
            if (f.Unsafe.TryGetPointer<Transform3D>(Player, out var transform))
            {
                transform->Position = GetSpawnPosition(f, player._index, f.PlayerCount);
            }
        }

        FPVector3 GetSpawnPosition(Frame f, int playerNumber, int players)
        {
            EntityRef SpawnPoint = default;
            FPVector3 pos = default;
            EntityPrototype SpawnA = f.FindAsset<EntityPrototype>("Resources/DB/SpawnPointA|EntityPrototype");
            EntityPrototype SpawnB = f.FindAsset<EntityPrototype>("Resources/DB/SpawnPointB|EntityPrototype");
            EntityPrototype SpawnC = f.FindAsset<EntityPrototype>("Resources/DB/SpawnPointC|EntityPrototype");
            EntityPrototype SpawnD = f.FindAsset<EntityPrototype>("Resources/DB/SpawnPointD|EntityPrototype");
            EntityPrototype SpawnE = f.FindAsset<EntityPrototype>("Resources/DB/SpawnPointE|EntityPrototype");
            EntityPrototype SpawnF = f.FindAsset<EntityPrototype>("Resources/DB/SpawnPointF|EntityPrototype");
            switch (playerNumber)
            {
                case 0: SpawnPoint = f.Create(SpawnA); if (f.Unsafe.TryGetPointer<Transform3D>(SpawnPoint, out Transform3D* transform)){ pos=transform->Position; } break;
                case 1: SpawnPoint = f.Create(SpawnB); if (f.Unsafe.TryGetPointer<Transform3D>(SpawnPoint, out Transform3D* transformB)){ pos=transformB->Position; } break;
                case 2: SpawnPoint = f.Create(SpawnC); if (f.Unsafe.TryGetPointer<Transform3D>(SpawnPoint, out Transform3D* transformC)){ pos=transformC->Position; } break;
                case 3: SpawnPoint = f.Create(SpawnD); if (f.Unsafe.TryGetPointer<Transform3D>(SpawnPoint, out Transform3D* transformD)){ pos=transformD->Position; } break;
                case 4: SpawnPoint = f.Create(SpawnE); if (f.Unsafe.TryGetPointer<Transform3D>(SpawnPoint, out Transform3D* transformE)){ pos=transformE->Position; } break;
                case 5: SpawnPoint = f.Create(SpawnF); if (f.Unsafe.TryGetPointer<Transform3D>(SpawnPoint, out Transform3D* transformF)){ pos=transformF->Position; } break;
                default: SpawnPoint = f.Create(SpawnA); if (f.Unsafe.TryGetPointer<Transform3D>(SpawnPoint, out Transform3D* transformA)) { pos = transformA->Position; } break;
            }
            Log.Debug($"Spawing player {playerNumber} of {players}");
            return pos;
            //int playerwidth = players * 2;
            //return new FPVector3( (playerNumber * 15) + 1 - (playerwidth / 2), 0, 10);
        }

        public void OnPlayerDisconnected(Frame f, PlayerRef Player)
        {
            foreach(var playerLink in f.GetComponentIterator<playerlink>())
            {
                if(playerLink.Component.Player != Player) { continue; }
                f.Destroy(playerLink.Entity);
            }
        }

        public void OnCollisionEnter3D(Frame f, CollisionInfo3D collider)
        {
            if (!f.Has<ObjectType>(collider.Other)) { return; }
            else
            {
                Obj Target = f.Get<ObjectType>(collider.Other).Object;
                if (Target == Obj.Bullet)
                {
                    if (f.Unsafe.TryGetPointer<PlayerConfig>(collider.Entity, out var cfg))
                    {
                        cfg->HP -= 1;
                    }
                }
            }
        }
    }
}
