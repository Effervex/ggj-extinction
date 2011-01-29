using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Extinction.Objects;

namespace Extinction.Icons
{
    abstract class ToolIcon
    {
        public int cost = 0;
        public double cooldown = 0;
        public double cooldownLeft = 0;

        public Texture2D iconTexture;
        public String iconName = "Icon";
        public Texture2D transparentFill;
        public static int ICON_WIDTH;

        public ToolEntity model;

        public ToolIcon(ToolEntity model, String iconName)
        {
            if (iconName != null)
                this.iconName = iconName;
            this.model = model;
        }


        internal void LoadContent(Microsoft.Xna.Framework.Content.ContentManager Content)
        {
            iconTexture = Content.Load<Texture2D>(iconName);
            transparentFill = Content.Load<Texture2D>("Transparent");

            ICON_WIDTH = iconTexture.Width;
        }

        internal void Update(GameTime gameTime)
        {
            // Pass cooldown time
            if (cooldownLeft > 0)
            {
                cooldownLeft -= gameTime.ElapsedGameTime.Milliseconds;
                cooldownLeft = Math.Max(0, cooldownLeft);
            }
        }

        public bool readyForUse()
        {
            if (cooldownLeft == 0)
            {
                return true;
            }
            return false;
        }

        public void toolHasBeenPlaced()
        {
            cooldownLeft = cooldown;
            GameState.magicks -= cost;
        }

        internal void Draw(GameTime gameTime, SpriteBatch spriteBatch, Vector2 location)
        {
            spriteBatch.Draw(iconTexture, location, Color.White);
            // Draw cooldown layer if necessary
            if (cooldownLeft > 0)
            {
                double percent = 1 - cooldownLeft / cooldown;
                int x = (int) (iconTexture.Width * percent);
                Rectangle amount = new Rectangle((int) location.X + x, (int) location.Y, iconTexture.Width - x, iconTexture.Height);
                spriteBatch.Draw(transparentFill, amount, Color.Black);
            }
            if (GameState.magicks < cost)
            {
                Rectangle amount = new Rectangle((int)location.X, (int)location.Y, iconTexture.Width, iconTexture.Height);
                spriteBatch.Draw(transparentFill, amount, Color.Black);
            }
        }

        internal int iconHeight()
        {
            return iconTexture.Height;
        }
    }


}
