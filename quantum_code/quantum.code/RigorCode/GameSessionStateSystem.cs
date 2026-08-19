using Quantum.Prototypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantum
{
    public unsafe class GameSessionStateSystem : SystemMainThreadFilter<GameSessionStateSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public GameSession* Session;
        }
        public override void OnInit(Frame f)
        {
            GameSession* gs = f.Unsafe.GetPointerSingleton<GameSession>();
            Log.Debug("Ready...");
            gs->State = GameState.StartingUp;
            gs->TimeUntilStart = 5;
        }
        public override void Update(Frame f, ref Filter filter)
        {
            GameSession* gs = f.Unsafe.GetPointerSingleton<GameSession>();
            if (gs == null) { return; }
            else
            {
                switch (gs->State)
                {
                    case GameState.StartingUp:
                        if(f.PlayerCount >= 2)
                        {
                            if (gs->TimeUntilStart < 0) { Log.Debug("Go!"); gs->State = GameState.InProgress; }
                            else { gs->TimeUntilStart = gs->TimeUntilStart - f.DeltaTime; }
                        }
                        else
                        {
                            return;
                        }
                        break;
                    case GameState.InProgress:
                        break;
                    case GameState.Finished:
                        break;
                }
            }
        }
    }
}
