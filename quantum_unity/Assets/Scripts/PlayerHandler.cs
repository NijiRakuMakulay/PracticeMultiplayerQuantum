using Cinemachine;
using Photon.Deterministic;
using Quantum;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHandler : MonoBehaviour
{
    [SerializeField] Slider HealthBar;
    [SerializeField] TextMeshProUGUI HPText;
    [SerializeField] EntityView eV;
    public void OnEntityInstantiated()
    {
        QuantumGame g = QuantumRunner.Default.Game;
        Frame f = g.Frames.Verified;
        if(f.TryGet(eV.EntityRef, out playerlink pL))
        {
            if (g.PlayerIsLocal(pL.Player))
            {
                CinemachineVirtualCamera vCam = FindAnyObjectByType<CinemachineVirtualCamera>();
                vCam.m_Follow = transform;
            }
        }
    }

    private void Awake()
    {
        HealthBar.wholeNumbers = true;
        HPText = GameObject.FindGameObjectWithTag("PlayerHealth").GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        QuantumGame g = QuantumRunner.Default.Game;
        Frame f = g.Frames.Verified;
        if (f.TryGet(eV.EntityRef, out playerlink pL))
        {
            if (g.PlayerIsLocal(pL.Player))
            {
                if (f.TryGet(eV.EntityRef, out PlayerConfig pc))
                {
                    HealthBar.maxValue = 100;
                    HealthBar.value = (int)pc.HP;
                    HPText.text = $"{(int)pc.HP}";
                }
            }
            else
            {
                if (f.TryGet(eV.EntityRef, out PlayerConfig pc))
                {
                    HealthBar.maxValue = 100;
                    HealthBar.value = (int)pc.HP;
                }
            }
        }
    }
}