using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantum
{
    public unsafe class ProjectileControl : SystemMainThreadFilter<ProjectileControl.Bullet>
    {
        public struct Bullet
        {
            public EntityRef Entity;
            public Transform3D* Transform;
            public BulletConfig* Configuration;
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
    }
}
