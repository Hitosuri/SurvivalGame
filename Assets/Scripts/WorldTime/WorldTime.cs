using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace WorldTime
{
    public class WorldTime : MonoBehaviour
    {
        public EventHandler<TimeSpan> WorldTimeChanged;

        [SerializeField]
        private float _dayLength = 30; //in senconds

        private TimeSpan _currentTime;

        private float _minuteLength => _dayLength / WorldTimeContants.MinutesDay;

        private void Start()
        {
            StartCoroutine(AddMinute());
        }
        private IEnumerator AddMinute()
        {
            _currentTime += TimeSpan.FromMinutes(1);
            WorldTimeChanged?.Invoke(this, _currentTime);
            yield return new WaitForSeconds(_minuteLength);
            StartCoroutine(AddMinute());
        }
    }
}

