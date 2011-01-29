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
using System.Collections;

namespace Extinction
{

    public partial class ExtinctionGame : Game
    {
        GraphicsDeviceManager graphics;
        ScreenManager screenManager;
        GameTime _gametime;

        public ExtinctionGame()
        {
            instance = this;
            this.IsMouseVisible = true;

            /* Create 3D device */
            graphics = new GraphicsDeviceManager(this);

            /* Create game screens */
            screenManager = new ScreenManager(this);
            this.Components.Add(screenManager);
            screenManager.AddScreen(new InGameScreen());
      //      screenManager.AddScreen(new TitleScreen());
            
            Content.RootDirectory = "Content";
        }

        protected override void Update(GameTime gameTime)
        {
            if (ExtinctionGame.IsKeyPressed(Keys.T))
                ExtinctionGame.ReloadTextures();
            
            _gametime = gameTime;
            dt = _gametime.ElapsedGameTime.Milliseconds / 1000f;
            t += dt;
            screenManager.Update(gameTime);
            UpdateInput();
        }

        protected override void Draw(GameTime gameTime)
        {
            screenManager.Draw(gameTime);
        }

        public GameTime getGameTime()
        {
            return _gametime;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }
    }
}
