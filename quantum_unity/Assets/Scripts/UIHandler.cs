using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Quantum;
public class UIHandler : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI ReadyText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Utils.TryGetQuantumFrame(out Frame f))
        {
            if (f.TryGetSingletonEntityRef<GameSession>(out var entity) == false)
            {
                ReadyText.text = "Singleton Error";
            }
            else
            {
                var gs = f.GetSingleton<GameSession>();
                int starttimer = (int)gs.TimeUntilStart;
                
                switch (gs.State)
                {
                    case GameState.StartingUp:
                        if(f.PlayerCount <= 1)
                        {
                            ReadyText.text = "Waiting for players...";
                        }
                        else
                        {
                            if (starttimer == 0)
                            {
                                ReadyText.text = "Start!";
                            }
                            else if (starttimer < 0)
                            {
                                ReadyText.text = "";
                            }
                            else
                            {
                                ReadyText.text = $"Get Ready! ({starttimer})";
                            }
                        }
                        break;
                    case GameState.InProgress:
                        ReadyText.text = "";
                        break;
                    case GameState.Finished:
                        ReadyText.text = "Finish!";
                        break;
                }
            }
        }
    }
}
