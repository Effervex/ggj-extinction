using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ZuneGameState;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Extinction.Objects;

namespace Extinction.Screens
{
    class InGameScreen : GameScreen
    {
        ContentManager content;

        Vector3 cameraPosition;

        Model modelIsland;
        Model modelGrass;

        Island island;
        Grass grass;

        /// <summary>
        /// Constructor.
        /// </summary>
        public InGameScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            island = new Island();
            grass = new Grass();
        }

        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            island.Create(@"island/island_mesh");
            grass.Create(@"foliage/grass_mesh");
            // A real game would probably have more content than this sample, so
            // it would take longer to load. We simulate that by delaying for a
            // while, giving you a chance to admire the beautiful loading screen.
            Thread.Sleep(1000);

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();

            //modelIsland = ExtinctionGame.LoadModel(@"island/island_mesh");
            //modelGrass = ExtinctionGame.LoadModel(@"foliage/grass_mesh");
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (ExtinctionGame.IsKeyPressed(Keys.Q))
                modelIsland = ExtinctionGame.LoadModel(@"island_mesh");

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }

                /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.CornflowerBlue, 0, 0);
             
           cameraPosition = new Vector3(15,15,15) * 1.3f;


           ExtinctionGame.view = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);
           ExtinctionGame.projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(70f),
               (float)ScreenManager.GraphicsDevice.Viewport.Width / (float)ScreenManager.GraphicsDevice.Viewport.Height, 1f, 100f);
          //  ScreenManager.GraphicsDevice.Textures[0]
           // TODO: Add your drawing code here
           island.Draw();
           return;
           if (ScreenManager.GraphicsDevice.Textures[0] == null)
           {

            //   ScreenManager.GraphicsDevice.Textures[0] = content.Load<Texture2D>("blank");
           }
           //ExtinctionGame.DrawModel(modelIsland, Matrix.Identity, view, projection);

           try
           {
               RasterizerState stater = new RasterizerState();
               BlendState stateb = new BlendState();
               DepthStencilState stated = new DepthStencilState();
               stated.DepthBufferWriteEnable = false;
               stateb.AlphaBlendFunction = BlendFunction.Add;
               stater.CullMode = CullMode.None; 


               ScreenManager.GraphicsDevice.BlendState = BlendState.AlphaBlend;
               ScreenManager.GraphicsDevice.DepthStencilState = stated;
               ScreenManager.GraphicsDevice.RasterizerState = stater;
               ModelDataSet data = (ModelDataSet)modelGrass.Tag;
               data.shader.Parameters["Time"].SetValue((float)gameTime.TotalGameTime.TotalMilliseconds / 1000f);
               ExtinctionGame.DrawModel(modelGrass, Matrix.Identity);
           }
           catch (Exception e)
           {

           }
           base.Draw(gameTime);
            

        }


    }
}
