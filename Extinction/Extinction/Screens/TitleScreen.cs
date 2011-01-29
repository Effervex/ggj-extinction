using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ZuneGameState
{
    class TitleScreen : GameScreen
    {
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.BigFont;

            spriteBatch.Begin();
            spriteBatch.DrawString(font, "This is the title screen", new Vector2(190,210), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        #region Handle Input

        /// <summary>
        /// Responds to user input, accepting or cancelling the message box.
        /// </summary>
        public override void HandleInput()
        {
            if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter))
            {
                System.Console.WriteLine("Enter");
                ExitScreen();
            }
        }


        #endregion
    }
}