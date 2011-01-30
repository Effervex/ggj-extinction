using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Extinction;
using Extinction.Screens;

namespace ZuneGameState
{
    class TitleScreen : GameScreen
    {
        Texture2D title;
        public override void LoadContent()
        {
            InGameScreen.CreateSharedAssets();
            InGameScreen.LoadSharedAssets();

            title = (Texture2D)ExtinctionGame.LoadTexture(@"title_screen");
            base.LoadContent();

            ExtinctionGame.PlaySound(@"sound/title screen 01.wav", true);
        }
        public override void Draw(GameTime gameTime)
        {
            Vector3 cameraPosition = new Vector3(
                (float)Math.Sin(ExtinctionGame.GetTimeTotal() * 0.81f),
                (float)Math.Sin(ExtinctionGame.GetTimeTotal() * 0.21f) * 0.1f + .51f,
                (float)Math.Cos(ExtinctionGame.GetTimeTotal() * 0.81f)) * 15f;
            cameraPosition = Vector3.Transform(cameraPosition, Matrix.CreateRotationY(0f));

            ExtinctionGame.view = Matrix.CreateLookAt(cameraPosition, Vector3.Zero + Vector3.Up * 5f, Vector3.Up);
            ExtinctionGame.projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90f),
                (float)ScreenManager.GraphicsDevice.Viewport.Width / (float)ScreenManager.GraphicsDevice.Viewport.Height, 1f, 500f);
           
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.BigFont;
            float alpha = ((float)Math.Sin(ExtinctionGame.GetTimeTotal() * 5) + 1f) * 0.5f;

            InGameScreen.DrawSharedAssets();


            spriteBatch.Begin();
            spriteBatch.Draw(title,
                new Vector2(ExtinctionGame.instance.GraphicsDevice.Viewport.Width - title.Width,
                    ExtinctionGame.instance.GraphicsDevice.Viewport.Height - title.Height - 100) * 0.5f,
                    Color.White);
            spriteBatch.DrawString(font, "Press space key to begin", new Vector2(
                ExtinctionGame.instance.GraphicsDevice.Viewport.Width - title.Width,
                (ExtinctionGame.instance.GraphicsDevice.Viewport.Height) + 400f) * 0.5f, Color.FromNonPremultiplied(11, 11, 11, (int)(alpha * 255)));
            string credits = "Credits: Sam, Dacre, Ryan, Elijah, Chih";
            
            spriteBatch.DrawString(font, credits, new Vector2(
                ExtinctionGame.instance.GraphicsDevice.Viewport.Width - font.MeasureString(credits).X * 0.5f,
                (ExtinctionGame.instance.GraphicsDevice.Viewport.Height) -640f) * 0.5f, Color.Yellow, 0f, Vector2.Zero, 0.55f, SpriteEffects.None, 0f);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        #region Handle Input

        /// <summary>
        /// Responds to user input, accepting or cancelling the message box.
        /// </summary>
        public override void HandleInput()
        {
            if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space))
            {
                ExitScreen();
            }
        }


        #endregion
    }
}