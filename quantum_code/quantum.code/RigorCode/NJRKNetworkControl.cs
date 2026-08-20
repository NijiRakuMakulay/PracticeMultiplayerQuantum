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
                    input.MoveDir = new FPVector2((FP)input.DirectionX * filter.Config->speed / 10, (FP)input.DirectionY * filter.Config->speed / 10);
                    if (input.MoveDir.SqrMagnitude > 1) { input.MoveDir = input.MoveDir.Normalized; }
                    if (input.MoveDir.SqrMagnitude != default) { filter.Transform->Rotation = FPQuaternion.Lerp(filter.Transform->Rotation, FPQuaternion.LookRotation(input.MoveDir.XOY), f.DeltaTime * 10); }
                    if (filter.Transform->Position.Y < -50) { filter.Transform->Position = GetSpawnPosition(filter.Link->Player, f.PlayerCount); filter.Config->HP -= 10; }
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
            var bProto = f.FindAsset<EntityPrototype>("Resources/DB/Bullet|EntityPrototype");
            EntityRef bullet = f.Create(bProto);
            Transform3D* bulletTransform = f.Unsafe.GetPointer<Transform3D>(bullet);
            bulletTransform->Position = new FPVector3(filter.Transform->Position.X, filter.Transform->Position.Y + (short)2.0, filter.Transform->Position.Z) + filter.Transform->Forward * 2;
            bulletTransform->Rotation = filter.Transform->Rotation;
        }

        public void OnPlayerDataSet(Frame frame, PlayerRef player)
        {
            var data = frame.GetPlayerData(player);
            var prototype = frame.FindAsset<EntityPrototype>(data.CharacterPrototype.Id);
            var entity = frame.Create(prototype);
            if (frame.Unsafe.TryGetPointer<playerlink>(entity, out var playerLink))
            {
                playerLink->Player = player;
            }
            if (frame.Unsafe.TryGetPointer<Transform3D>(entity, out var transform))
            {
                transform->Position = GetSpawnPosition(player, frame.PlayerCount);
            }
        }
        FPVector3 GetSpawnPosition(int playerNumber, int players)
        {
            int playerwidth = players * 2;
            return new FPVector3( (playerNumber * 15) + 1 - (playerwidth / 2), 0, 10);
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
