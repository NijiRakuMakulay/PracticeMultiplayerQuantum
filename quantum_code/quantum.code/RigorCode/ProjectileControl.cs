using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantum
{
    public unsafe class ProjectileControl : SystemMainThreadFilter<ProjectileControl.Bullet>, ISignalOnCollisionEnter3D
    {
        public struct Bullet
        {
            public EntityRef Entity;
            public Transform3D* Transform;
            public BulletConfig* Configuration;
            public ObjectType* ObjType;
        }

        public void OnInit(ref Bullet filter)
        {
            filter.Configuration->AliveTime = 10;
            filter.Configuration->BulletSpeed = 5;
            
        }

        public override void Update(Frame f, ref Bullet filter)
        {
            if(filter.Configuration->AliveTime > 0)
            {
                filter.Configuration->AliveTime -= f.DeltaTime;
                filter.Transform->Position += filter.Transform->Forward;
            }
            else
            {
                f.Destroy(filter.Entity);
            }
        }

        public void OnCollisionEnter3D(Frame f, CollisionInfo3D collider)
        {
            if (!f.Has<ObjectType>(collider.Other)) { return; }
            else
            {
                Obj Target = f.Get<ObjectType>(collider.Other).Object;
                if (Target == Obj.Obstacle || Target == Obj.Player) { f.Destroy(collider.Entity); }
            }
        }
    }
}
