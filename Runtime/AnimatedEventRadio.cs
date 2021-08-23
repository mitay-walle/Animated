using System;
using System.Collections.Generic;
using UnityEngine;

namespace Plugins.Own.Animated
{
    public class AnimatedEventRadio : AnimatedEvents
    {
        #region Vars

        [SerializeField] private List<cEventData> EventData = new List<cEventData>();
        public bool NeedLessVolume { get; private set; }
        public bool NeedMute { get; private set; }

        public enum cEventCategory
        {
            onReset,
            onPlay,
            onTime,
            onFinish,
        }
    
        [Serializable]
        public class cEventData
        {
            [SerializeField] private cEventCategory category;
            [SerializeField] private bool Use;
            [SerializeField] private bool LessVol;
            [SerializeField] private bool Mute;

            public void InvokeCheck(cEventCategory cat, AnimatedEventRadio events)
            {
                if (Use)
                {
                    events.NeedLessVolume = LessVol;
                    events.NeedMute = Mute;
                }
            }
        }

        #endregion

        protected override void Start()
        {
            if (EventData.Count <= 0) return;
        
        }

        public override void OnPlay()
        {
            foreach (var data in EventData) data.InvokeCheck(cEventCategory.onPlay,this);
        }

        public override void OnFinish()
        {
            foreach (var data in EventData) data.InvokeCheck(cEventCategory.onFinish,this);
        }

        public override void OnReset()
        {
            foreach (var data in EventData) data.InvokeCheck(cEventCategory.onReset,this);
        }

        public override void OnTime()
        {
            foreach (var data in EventData) data.InvokeCheck(cEventCategory.onTime,this);
        }
    }
}
