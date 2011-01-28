using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using ZuneGameState;
using Extinction.Screens;

namespace Extinction
{

    public partial class ExtinctionGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        ScreenManager screenManager;

        public ExtinctionGame()
        {
            instance = this;

            /* Create 3D device */
            graphics = new GraphicsDeviceManager(this);

            /* Create game screens */
            screenManager = new ScreenManager(this);
            this.Components.Add(screenManager);
            screenManager.AddScreen(new InGameScreen());
            screenManager.AddScreen(new TitleScreen());
            
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            screenManager.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            screenManager.Draw(gameTime);
        }
    }
}
