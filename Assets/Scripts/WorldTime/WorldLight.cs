using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace WorldTime
{
    [RequireComponent(typeof(Light2D))]
    public class WorldLight : MonoBehaviour
    {
        private Light2D _light;

        [SerializeField]
        private WorldTime _worldTime;

        private Gradient _gradient;

        private void Awake()
        {
            _gradient = new Gradient();
            GradientColorKey[] gradientColorKeys = new GradientColorKey[8];
            gradientColorKeys[0].color = new Color32(0x12, 0x19, 0x95,0xff);
            gradientColorKeys[0].time = 0;
            gradientColorKeys[1].color = new Color32(0x12, 0x19, 0x95, 0xff);
            gradientColorKeys[1].time = 0.25f;
            gradientColorKeys[2].color = new Color32(0xEC, 0xC2, 0x80, 0xff);
            gradientColorKeys[2].time = 0.31f;
            gradientColorKeys[3].color = new Color32(0xF3, 0xF2, 0xF2, 0xff);
            gradientColorKeys[3].time = 0.34f;
            gradientColorKeys[4].color = new Color32(0xEC, 0xE9, 0x92, 0xff);
            gradientColorKeys[4].time = 0.65f;
            gradientColorKeys[5].color = new Color32(0xE6, 0x7E, 0x45, 0xff);
            gradientColorKeys[5].time = 0.68f;
            gradientColorKeys[6].color = new Color32(0xE6, 0x64, 0x45, 0xff);
            gradientColorKeys[6].time = 0.71f;
            gradientColorKeys[7].color = new Color32(0x12, 0x19, 0x95, 0xff);
            gradientColorKeys[7].time = 0.78f;
            GradientAlphaKey[] gradientAlphaKeys = new GradientAlphaKey[2];
            gradientAlphaKeys[0].alpha = 1f;
            gradientAlphaKeys[0].time = 0;
            gradientAlphaKeys[1].alpha = 1f;
            gradientAlphaKeys[1].time = 1f;
            _gradient.SetKeys(gradientColorKeys, gradientAlphaKeys);
            _light = GetComponent<Light2D>();
            _worldTime.WorldTimeChanged += OnWordTimeChanged;
        }

        private void OnDestroy()
        {
            _worldTime.WorldTimeChanged -= OnWordTimeChanged;
        }
        private void OnWordTimeChanged(object sender, TimeSpan newTime)
        {
            
            _light.color = _gradient.Evaluate(Percentofday(newTime));
        }

        private float Percentofday(TimeSpan timeSpan)
        {
            return (float)timeSpan.TotalMinutes % WorldTimeContants.MinutesDay /
                WorldTimeContants.MinutesDay;
        }
    }
}
