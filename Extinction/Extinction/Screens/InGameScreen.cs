﻿using System;
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

        Tree tree;
        Dome dome;
        Island island;
        Grass grass;
        Possum possum;
        Crystal crystal;

        Texture2D selectedIcon;

        List<ToolIcon> tools;
        ToolIcon selectedTool = null;
        BoundingSphere hoveringPlacementPoint;
        bool hoveringPointFound = false;
        Matrix selectedWorld;

        MouseState prevMouseState;

        GameState gameState;






        public void Initialise()
        {
            dome = new Dome();
            island = new Island();
            grass = new Grass();
            possum = new Possum();
            tree = new Tree();
            crystal = new Crystal();

            tools = new List<ToolIcon>();
            tools.Add(new CrystalIcon(crystal, "iconz/iconz-crystals"));
            tools.Add(new CrystalIcon(crystal, "iconz/iconz-honeydrop"));

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

            tree.Create();
            dome.Create(@"dome/dome_mesh");
            island.Create(@"island/island_mesh");
            grass.Create(@"foliage/grass_mesh");
            crystal.Create(@"crystal/crystals");
            possum.Create(@"possum/possum", content);
            possum.isAnimated = true;
            possum.world = Matrix.CreateTranslation(new Vector3(2, 7, 2));
            possum.queueAnimation("Attacking", false);


            selectedIcon = content.Load<Texture2D>("SelectedIcon");

            //cursor = content.Load<Texture2D>("Cursor");
            ExtinctionGame.cursor = new Cursor(ExtinctionGame.instance, content);

            gameState.LoadContent(island.model, island.world);

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
            p.t += ExtinctionGame.GetTimeDelta();
            p.aux.Z = (float)(Math.Sin(p.t) * 0.5f + 0.5f);
            if (p.t > 2f) p.alive = false;
            //   p.Position += 
        }

        void UpdateClouds(ParticleSystem s)
        {
            if (s.particles.Count < 10)
            {
                s.spawnTime = 1f;
                s.AddParticle(s.world.Translation, 1);
                s.spawnTime = ExtinctionGame.Random() + 1f;
                s.AddParticle(s.world.Translation, 1);
                s.spawnTime = ExtinctionGame.Random() + 1f;
                s.AddParticle(s.world.Translation, 1);
                s.spawnTime = ExtinctionGame.Random() + 1f;
                s.AddParticle(s.world.Translation, 1);
                s.spawnTime = ExtinctionGame.Random() + 1f;
            }
            else
            {
                s.spawnTime -= ExtinctionGame.GetTimeDelta();
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            ExtinctionGame.cursor.Update(gameTime);

            if (test == null)
            {
                test = new ParticleSystem();
                test.Create("cloud", UpdateCloud, CreateCloud, UpdateClouds);
                test.AddParticle(Vector3.Zero, 1);
            }
            test.Update(gameTime);

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
            if (ExtinctionGame.IsKeyPressed(Keys.Q))
                modelIsland = ExtinctionGame.LoadModel(@"island/island_mesh");
            if (ExtinctionGame.IsKeyPressed(Keys.T))
                ExtinctionGame.ReloadTextures();

            foreach (ToolIcon tool in tools)
            {
                tool.Update(gameTime);
            }

            // update moving objects
            island.Update(gameTime);
            possum.Update(gameTime);


            // Check the mouse in regards to the tool icons
            checkMouseClick();

            // Deal with camera movement
            spin_velocity *= 0.9f;
            if (ExtinctionGame.IsKeyPressed(Keys.A))
                spin_velocity.X -= 0.01f;
            else if (ExtinctionGame.IsKeyPressed(Keys.D))
                spin_velocity.X += 0.01f;
            if (ExtinctionGame.IsKeyPressed(Keys.W))
                spin_velocity.Y -= 0.01f;
            else if (ExtinctionGame.IsKeyPressed(Keys.S))
                spin_velocity.Y += 0.01f;

            spin_info += spin_velocity;
            spin_info.Y = MathHelper.Clamp(spin_info.Y, 0, 0.5f);


            // If hovering point is true, update the selectedWorld Matrix
            if (hoveringPointFound)
            {
                selectedWorld = Matrix.Identity;
                float time = ExtinctionGame.GetTimeTotal() * 3;
                selectedWorld = Matrix.Multiply(selectedWorld, Matrix.CreateScale(1 + (float)Math.Sin(time) * 0.04f));
                selectedWorld = Matrix.Multiply(selectedWorld, Matrix.CreateRotationY(time));
                selectedWorld = Matrix.Multiply(selectedWorld, Matrix.CreateTranslation(hoveringPlacementPoint.Center));
            }
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
            // Check for a hovering placement check
            if (selectedTool != null)
            {
                // Find out where the tool is being placed
                Ray cursorRay = ExtinctionGame.cursor.CalculateCursorRay(ExtinctionGame.projection, ExtinctionGame.view);
                Vector3 cursorPos = ExtinctionGame.cursor.ThreeDPosition;
                BoundingSphere closestSphere = hoveringPlacementPoint;
                bool sphereFound = false;
                float closestDistance = float.MaxValue;
                for (int lane = 0; lane < GameState.NUM_LANES; lane++)
                {
                    for (int row = 0; row < GameState.NUM_ROWS; row++)
                    {
                        // If the cursor ray intersects, take the closest bounding sphere
                        float? distance = cursorRay.Intersects(gameState.pathPoints[lane, row]);
                        if (distance.HasValue)
                        {
                            if (distance < closestDistance)
                            {
                                closestSphere = gameState.pathPoints[lane, row];
                                closestDistance = (float)distance;
                                sphereFound = true;
                            }
                        }
                    }
                }

                if (sphereFound)
                {
                    hoveringPlacementPoint = closestSphere;
                    hoveringPointFound = true;
                }
            }

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
                    Vector2 gridPoint = gameState.sphereGridPoints[hoveringPlacementPoint];
                    // If nothing is already in the position
                    if (!gameState.placedTools.ContainsKey(gridPoint))
                    {
                        gameState.placedTools[gridPoint] = (ToolEntity) selectedTool.model.NewModel(hoveringPlacementPoint.Center);
                        selectedTool.placedTool();
                        selectedTool = null;
                        hoveringPointFound = false;
                    }
                }
            }

            // Right click removes selection
            if (Mouse.GetState().RightButton.Equals(ButtonState.Pressed))
            {
                selectedTool = null;
                hoveringPointFound = false;
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

            cameraPosition = new Vector3(5, 8, 5) * (1.3f + spin_info.Y);
            cameraPosition = Vector3.Transform(cameraPosition, Matrix.CreateRotationY(spin_info.X));

            ExtinctionGame.view = Matrix.CreateLookAt(cameraPosition, Vector3.Zero + Vector3.Up * 5f, Vector3.Up);
            ExtinctionGame.projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90f),
                (float)ScreenManager.GraphicsDevice.Viewport.Width / (float)ScreenManager.GraphicsDevice.Viewport.Height, 1f, 500f);
            //  ScreenManager.GraphicsDevice.Textures[0]
            // TODO: Add your drawing code here
            dome.Draw();
            test.Draw();
            island.Draw();
            possum.Draw();
            tree.Draw();
            grass.world = Matrix.CreateTranslation(4.5f, 4.5f, 4.5f);

            // Draw the active tools
            foreach (ToolEntity tool in gameState.placedTools.Values)
            {
                tool.Draw();
            }

            // If we have a hovering model, draw that (with spinning and throbbing)

            if (hoveringPointFound && selectedTool != null)
            {
                selectedTool.model.world = selectedWorld;
                selectedTool.model.Draw();
            }
            //sphere.Draw(gameTime, hoveringPlacementPoint);
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
            //ScreenManager.SpriteBatch.Draw(cursor, new Vector2(Mouse.GetState().X, Mouse.GetState().Y), Color.White);
            ScreenManager.SpriteBatch.End();

            if (ScreenManager.GraphicsDevice.Textures[0] == null)
            {

                ExtinctionGame.view = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);
                ExtinctionGame.projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(70f),
                    (float)ScreenManager.GraphicsDevice.Viewport.Width / (float)ScreenManager.GraphicsDevice.Viewport.Height, 1f, 100f);
                //  ScreenManager.GraphicsDevice.Textures[0]
                // TODO: Add your drawing code here
                /*
                dome.Draw();
                island.Draw();
                sphere.Draw();
                grass.Draw();
                */


                base.Draw(gameTime);


            }
        }
    }
}
