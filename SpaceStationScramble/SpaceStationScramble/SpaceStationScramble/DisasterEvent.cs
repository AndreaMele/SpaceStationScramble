﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceStationScramble {
    abstract class DisasterEvent {
        protected Texture2D texture;

        public PlayerNumber VisibleToPlayer {
            get;
            protected set;
        }

        public long StartTime {
            get;
            protected set;
        }

        public long EndTime {
            get;
            protected set;
        }

        public Vector2 Position {
            get;
            protected set;
        }

        //Fields that will really belong to subclasses
        private SteamColor steamColor;

        public DisasterEvent(long startTime, long endTime) {
            this.StartTime = startTime;
            this.EndTime = endTime;
        }

        public void Update() {

        }

        public abstract void Draw(SpriteBatch spriteBatch);
    }
}