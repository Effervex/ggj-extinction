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
        private int EDGE_ICON_BUFFER = 5;
        private int ICON_TO_ICON_BUFFER = 10;
        private int SELECTED_ICON_BUFFER = 2;
        private float RATE_OF_ROTATION = 0.1f;

        ContentManager content;

        Vector3 cameraPosition;

        Model modelIsland;
        Model modelGrass;

        Dome dome;
        Island island;
        Grass grass;
        Sphere sphere;

        Texture2D selectedIcon;

        List<ToolIcon> tools;
        ToolIcon selectedTool = null;
        Texture2D cursor;

        MouseState prevMouseState;

        GameState gameState;

        public void Initialise()
        {
            dome = new Dome();
            island = new Island();
            grass = new Grass();
            sphere = new Sphere();

            tools = new List<ToolIcon>();
            tools.Add(new MagnifyingGlass());
            tools.Add(new MagnifyingGlass());

            gameState = new GameState();

        }
        /// <summary>
        /// Constructor.
        /// </summary>
        public InGameScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
            Initialise();
        }

        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            dome.Create(@"dome/dome_mesh");
            island.Create(@"island/island_mesh");
            grass.Create(@"foliage/grass_mesh");
            sphere.Create(@"sphere0-5");

            selectedIcon = content.Load<Texture2D>("SelectedIcon");

            cursor = content.Load<Texture2D>("Cursor");


            foreach (ToolIcon tool in tools)
            {
                tool.LoadContent(content);
            }

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();

            //modelIsland = ExtinctionGame.LoadModel(@"island/island_mesh");
            //modelGrass = ExtinctionGame.LoadModel(@"foliage/grass_mesh");
        }


        Vector2 spin_velocity = Vector2.Zero;
        Vector2 spin_info = Vector2.Zero;
        ParticleSystem test;

        void CreateCloud(ref Particle p, Matrix world)
        {
            float r = ExtinctionGame.Random() * MathHelper.Pi * 2f;
            float rad = ExtinctionGame.Random() * 13f + 12f;
            p.p = world.Translation + Vector3.Transform(Vector3.Right * rad, Matrix.CreateRotationY(r));
            p.v = ExtinctionGame.RandomVector() * 0.1f;
        }

        void UpdateCloud(ref Particle p, Matrix world)
        {
            //p.p += p.v;
            p.t += 0.1f;

            if (p.t > 2f) p.alive = false;
         //   p.Position += 
        }

        void UpdateClouds(ParticleSystem s)
        {
            if (s.particles.Count < 10)
            {
                s.spawnTime = 1f;
                s.AddPartcile(s.world.Translation, 1);
                s.spawnTime = ExtinctionGame.Random() + 1f;
                s.AddPartcile(s.world.Translation, 1);
                s.spawnTime = ExtinctionGame.Random() + 1f;
                s.AddPartcile(s.world.Translation, 1);
                s.spawnTime = ExtinctionGame.Random() + 1f;
                s.AddPartcile(s.world.Translation, 1);
                s.spawnTime = ExtinctionGame.Random() + 1f;
            }
            else
            {
                s.spawnTime -= ExtinctionGame.GetTimeDelta();
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (test == null)
            {
                test = new ParticleSystem();
                test.Create("cloud", UpdateCloud, CreateCloud, UpdateClouds);
                test.AddPartcile(Vector3.Zero, 1);
            }
            test.Update(gameTime);

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
            if (ExtinctionGame.IsKeyPressed(Keys.Q))
                modelIsland = ExtinctionGame.LoadModel(@"island_mesh");
            if (ExtinctionGame.IsKeyPressed(Keys.T))
                ExtinctionGame.ReloadTextures();

            foreach (ToolIcon tool in tools)
            {
                tool.Update(gameTime);
            }

            // Check the mouse in regards to the tool icons
            checkMouseClick();

            
            float spin_decel = 0.1f;
            Vector2 delta = ExtinctionGame.MouseDelta ;



            if (Math.Abs( delta.X) < 0.25)
            {
                //spin_velocity.X *= 0.8f;
            }
            else
            {

            } 
            
            spin_velocity.X = MathHelper.Clamp((spin_velocity.X + delta.X) * 0.998f, -1f, 1f) * 0.25f;
            //spin_velocity.X *= 0.8f;

            float maxY = 1.5f;
            float minY = 0.5f;
            
            spin_info.Y = MathHelper.Clamp(spin_info.Y - delta.Y, 0.5f, 1.5f);
            spin_info.X += spin_velocity.X;

            Console.WriteLine(spin_info);
            gameState.Update(gameTime);
            Console.WriteLine(spin_velocity.X);
            prevMouseState = Mouse.GetState();
        }

        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }



        private void checkMouseClick()
        {
            if (Mouse.GetState().LeftButton.Equals(ButtonState.Pressed) && prevMouseState.LeftButton.Equals(ButtonState.Released))
            {
                // Check for Icon clicks
                if (Mouse.GetState().X < EDGE_ICON_BUFFER + ToolIcon.ICON_WIDTH)
                {
                    int y = EDGE_ICON_BUFFER;
                    foreach (ToolIcon tool in tools)
                    {
                        if (Mouse.GetState().Y >= y && Mouse.GetState().Y < y + tool.iconHeight())
                            if (tool.clickedIcon())
                                selectedTool = tool;
                        y += tool.iconHeight() + ICON_TO_ICON_BUFFER;
                    }
                }
                else if (selectedTool != null)
                {
                    // Check for valid Tool placement
                    selectedTool.placedTool();
                    selectedTool = null;
                }
            }
        }

        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.CornflowerBlue, 0, 0);

            cameraPosition = new Vector3(5, 5, 5) * (1.3f + spin_info.Y);
            cameraPosition = Vector3.Transform(cameraPosition, Matrix.CreateRotationY(spin_info.X));

            ExtinctionGame.view = Matrix.CreateLookAt(cameraPosition, Vector3.Zero + Vector3.Up * 5f, Vector3.Up);
            ExtinctionGame.projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(70f),
                (float)ScreenManager.GraphicsDevice.Viewport.Width / (float)ScreenManager.GraphicsDevice.Viewport.Height, 1f, 500f);
            //  ScreenManager.GraphicsDevice.Textures[0]
            // TODO: Add your drawing code here
            dome.Draw();
            test.Draw(); 
            island.Draw();
            grass.Draw();

            

            if (ScreenManager.GraphicsDevice.Textures[0] == null)
            {

                ExtinctionGame.view = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);
                ExtinctionGame.projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(70f),
                    (float)ScreenManager.GraphicsDevice.Viewport.Width / (float)ScreenManager.GraphicsDevice.Viewport.Height, 1f, 100f);
                //  ScreenManager.GraphicsDevice.Textures[0]
                // TODO: Add your drawing code here
                dome.Draw();
                island.Draw();
                sphere.Draw();
                grass.Draw();
                

                int y = EDGE_ICON_BUFFER;
                ScreenManager.SpriteBatch.Begin();
                foreach (ToolIcon tool in tools)
                {
                    if (tool == selectedTool)
                    {
                        Rectangle selectionRect = new Rectangle(EDGE_ICON_BUFFER - SELECTED_ICON_BUFFER, y - SELECTED_ICON_BUFFER, tool.iconTexture.Width + SELECTED_ICON_BUFFER * 2, tool.iconTexture.Height + SELECTED_ICON_BUFFER * 2);
                        ScreenManager.SpriteBatch.Draw(selectedIcon, selectionRect, Color.White);
                    }
                    tool.Draw(gameTime, ScreenManager.SpriteBatch, new Vector2(EDGE_ICON_BUFFER, y));
                    y += tool.iconHeight() + ICON_TO_ICON_BUFFER;
                }
                ScreenManager.SpriteBatch.Draw(cursor, new Vector2(Mouse.GetState().X, Mouse.GetState().Y), Color.White);
                ScreenManager.SpriteBatch.End();
                base.Draw(gameTime);


            }
        }
    }
}
