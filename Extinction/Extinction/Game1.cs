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

            InitGraphicsMode(800, 600, false);
            Content.RootDirectory = "Content";
        }

        private bool InitGraphicsMode(int iWidth, int iHeight, bool bFullScreen)
        {
            // If we aren't using a full screen mode, the height and width of the window can
            // be set to anything equal to or smaller than the actual screen size.
            if (bFullScreen == false)
            {
                if ((iWidth <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width)
                    && (iHeight <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height))
                {
                    graphics.PreferredBackBufferWidth = iWidth;
                    graphics.PreferredBackBufferHeight = iHeight;
                    graphics.IsFullScreen = bFullScreen;
                    graphics.ApplyChanges();
                    return true;
                }
            }
            else
            {
                // If we are using full screen mode, we should check to make sure that the display
                // adapter can handle the video mode we are trying to set.  To do this, we will
                // iterate thorugh the display modes supported by the adapter and check them against
                // the mode we want to set.
                foreach (DisplayMode dm in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
                {
                    // Check the width and height of each mode against the passed values
                    if ((dm.Width == iWidth) && (dm.Height == iHeight))
                    {
                        // The mode is supported, so set the buffer formats, apply changes and return
                        graphics.PreferredBackBufferWidth = iWidth;
                        graphics.PreferredBackBufferHeight = iHeight;
                        graphics.IsFullScreen = bFullScreen;
                        graphics.ApplyChanges();
                        return true;
                    }
                }
            }
            return false;
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
