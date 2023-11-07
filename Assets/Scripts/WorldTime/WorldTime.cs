using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldTime {
    public class WorldTime : MonoBehaviour {
        public EventHandler<TimeSpan> WorldTimeChanged;
        public float DayLengthInSeconds = 30f;

        private TimeSpan _currentTime;
        private float _fixedTimeDelta;

        private void Start() {
            _fixedTimeDelta = WorldTimeContants.MinutesDay * 60 / DayLengthInSeconds * Time.fixedDeltaTime;
        }

        private void FixedUpdate() {
            _currentTime += TimeSpan.FromSeconds(_fixedTimeDelta);
            WorldTimeChanged?.Invoke(this, _currentTime);
        }
    }
}