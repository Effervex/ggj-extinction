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
using System.Collections.ObjectModel;
using Extinction.Icons;

namespace Extinction.Screens
{
    class InGameScreen : GameScreen
    {
        private int EDGE_ICON_BUFFER = 5;
        private int ICON_TO_ICON_BUFFER = 10;
        private int SELECTED_ICON_BUFFER = 2;
        private float RATE_OF_ROTATION = 0.1f;

        ContentManager content;

        public static void CreateSharedAssets()
        {
            if(dome==null)dome = new Dome();
            if (island == null) island = new Island();
            if (grass == null) { 
                grass = new Grass(); 
            }

            if (tree == null) tree = new Tree();
        }

        public static void LoadSharedAssets()
        {
            if (dome != null) dome.Create();
            if (island != null) island.Create();
            if (grass != null)
            {
                grass.Create();

                BuildFoliageList();

            }
            if (tree != null) tree.Create();
        }

        public static void DrawSharedAssets()
        {
            dome.Draw();
            island.Draw();
            tree.Draw();
            grass.Draw();
        }

        public static Vector3 cameraPosition;

        public static Tree tree;
        public static Dome dome;
        public static Island island;
        public static Grass grass;
        public static Possum possum;

        Crystal crystal;
        Scrub scrub;
        Honey honey;
        IceCubes iceCubes;
        Rock rock;

        Texture2D selectedIcon;

        List<ToolIcon> toolIcons;
        ToolIcon selectedTool = null;
        BoundingSphere hoveringPlacementPoint;
        bool hoveringPointFound = false;
        Matrix selectedWorld;

        MouseState prevMouseState;

        public static GameState gameState;


        public static List<Matrix> foliageList;




        public void Initialise()
        {
            CreateSharedAssets();
            List<Enemy> enemies = new List<Enemy>();
           
            possum = new Possum();
            enemies.Add(possum);

            crystal = new Crystal();
            scrub = new Scrub();
            rock = new Rock();
            honey = new Honey();
            iceCubes = new IceCubes();

            toolIcons = new List<ToolIcon>();
            toolIcons.Add(new ScrubIcon(scrub, "iconz/iconz-scrubs"));
            toolIcons.Add(new CrystalIcon(crystal, "iconz/iconz-crystals"));
            toolIcons.Add(new RockIcon(rock, "iconz/iconz-rock"));
            toolIcons.Add(new HoneyIcon(honey, "iconz/iconz-honeydrop"));
            toolIcons.Add(new IceCubesIcon(iceCubes, "iconz/iconz-icecubes"));

            gameState = new GameState(enemies);

            foliageList = new List<Matrix>();
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

        static void BuildFoliageList()
        {
            if (foliageList == null) foliageList = new List<Matrix>();
            else foliageList.Clear();

            Vector3 center = new Vector3(-1f, 0f, -.5f);
            for (int i = 0; i < 4000; i++)
            {

                Vector3 v = ExtinctionGame.RandomVector();
                v.Normalize();
                v.Y = 15f;



                v *= (3 + 13.5f * ExtinctionGame.Random());
                v += center;


                Ray ray = new Ray(v, Vector3.Down);
                bool o;
                Vector3 vz;
                float? d = Picking.RayIntersectsModel(ray, island.model, Matrix.Identity, out o, out vz, out vz, out vz);
                if (d.HasValue)
                {
                    Vector3 pp = ray.Position + ray.Direction * d.Value;
                    //pp.X = -pp.X;
                    //pp.Z = -pp.Z;
                    pp.Y -= .5f;
                    float r = 7f;
                    float scale = r / (r + Vector3.Distance(new Vector3(pp.X, 0f, pp.Z), Vector3.Zero));

                    if (scale < 0.5f) continue;
                    Matrix m =
                        //Matrix.CreateScale(.5f + ExtinctionGame.Random()) * 
                        Matrix.CreateScale(scale * (.5f + ExtinctionGame.Random())) * (Matrix.CreateRotationY(ExtinctionGame.Random() * (float)Math.PI * 2f) *
                        Matrix.CreateTranslation(pp))
                        ;// *Matrix.CreateRotationY(ExtinctionGame.Random() * (float)Math.PI * 2f);
                    foliageList.Add(m);
                }

            }
        }
        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            ExtinctionGame.PlaySound(@"sound\ambient_01", false);
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");
            sceneMagic = ParticleSystem.GetParticles_SceneMagic();

            CreateSharedAssets();
            LoadSharedAssets();
            //BuildFoliageList();

            crystal.Create();
            scrub.Create();
            rock.Create();
            iceCubes.Create();
            honey.Create();

            possum.Create(@"possum/possum");
            possum.isAnimated = true;
            possum.world = Matrix.CreateTranslation(new Vector3(2, 7, 2));
            possum.SetState(Enemy.EnemyState.RUNNING);

            /*
            beetle.Create();
            beetle.isAnimated = true;
            beetle.world = Matrix.CreateTranslation(new Vector3(5, 7, 5));
            beetle.queueAnimation(false);
            */
            selectedIcon = content.Load<Texture2D>("iconz/iconz-select");

            //cursor = content.Load<Texture2D>("Cursor");
            ExtinctionGame.cursor = new Cursor(ExtinctionGame.instance, content);

            gameState.LoadContent(island.model, island.world);

            foreach (ToolIcon tool in toolIcons)
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
         

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            ExtinctionGame.cursor.Update(gameTime);
            sceneMagic.Update(gameTime);
            if (ExtinctionGame.IsKeyPressed(Keys.H))
                BuildFoliageList();
       



            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
  
            foreach (ToolIcon tool in toolIcons)
            {
                tool.Update(gameTime);
            }

            // update moving objects
            island.Update(gameTime);

            gameState.Update(gameTime);


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
            try{
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
                        float? distance = cursorRay.Intersects(GameState.pathPoints[lane, row]);
                        if (distance.HasValue)
                        {
                            if (distance < closestDistance)
                            {
                                closestSphere = GameState.pathPoints[lane, row];
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
                    foreach (ToolIcon tool in toolIcons)
                    {
                        if (Mouse.GetState().Y >= y && Mouse.GetState().Y < y + tool.iconHeight())
                            if (tool.readyForUse())
                                selectedTool = tool;
                        y += tool.iconHeight() + ICON_TO_ICON_BUFFER;
                    }
                }
                else if (selectedTool != null)
                {
                    // Check for valid Tool placement
                    Vector2 gridPoint = gameState.sphereGridPoints[hoveringPlacementPoint];
                    // If nothing is already in the position
                    if (gameState.CanPlaceTool(gridPoint))
                    {
                        ToolEntity newTool = (ToolEntity) selectedTool.model.NewEntity(hoveringPlacementPoint.Center);
                        newTool.location = gridPoint;
                        gameState.placedTools.Add(newTool);
                        selectedTool.toolHasBeenPlaced();
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

        }catch(Exception e) {}
        }

        ParticleSystem sceneMagic;
        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            
            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.CornflowerBlue, 0, 0);

            cameraPosition = new Vector3(6, 4.5f, 6) * (1.3f + spin_info.Y);
            cameraPosition = Vector3.Transform(cameraPosition, Matrix.CreateRotationY(spin_info.X));

            ExtinctionGame.view = Matrix.CreateLookAt(cameraPosition, Vector3.Zero + Vector3.Up * 5f, Vector3.Up);
            ExtinctionGame.projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90f),
                (float)ScreenManager.GraphicsDevice.Viewport.Width / (float)ScreenManager.GraphicsDevice.Viewport.Height, 1f, 500f);
            //  ScreenManager.GraphicsDevice.Textures[0]
            // TODO: Add your drawing code here
            dome.Draw();
            //test.Draw();
            island.Draw();
            possum.Draw();
            tree.Draw();
            //grass.world = Matrix.CreateTranslation(4.5f, 4.5f, 4.5f);
            
            gameState.Draw(gameTime);

            // If we have a hovering model, draw that (with spinning and throbbing)

            if (hoveringPointFound && selectedTool != null)
            {
                selectedTool.model.world = selectedWorld;
                selectedTool.model.Draw();
            }
            //sphere.Draw(gameTime, hoveringPlacementPoint);
            grass.Draw(foliageList);
            sceneMagic.Draw();
            // SpriteBatch drawing
            int y = EDGE_ICON_BUFFER;
            ScreenManager.SpriteBatch.Begin();
            SpriteFont big = ScreenManager.BigFont;
            SpriteFont small = ScreenManager.SmallFont;

            // Draw score metrics
            WriteText("Magicks: " + GameState.magicks, big, new Vector2(102, 432));

            foreach (ToolIcon tool in toolIcons)
            {
                tool.Draw(gameTime, ScreenManager.SpriteBatch, new Vector2(EDGE_ICON_BUFFER, y));
                if (tool == selectedTool)
                {
                    Rectangle selectionRect = new Rectangle(EDGE_ICON_BUFFER - SELECTED_ICON_BUFFER, y - SELECTED_ICON_BUFFER, tool.iconTexture.Width + SELECTED_ICON_BUFFER * 2, tool.iconTexture.Height + SELECTED_ICON_BUFFER * 2);
                    ScreenManager.SpriteBatch.Draw(selectedIcon, selectionRect, Color.White);
                }
                WriteText(tool.cost + "", small, new Vector2(EDGE_ICON_BUFFER + 50, y + 50));
                
                y += tool.iconHeight() + ICON_TO_ICON_BUFFER;
            }
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

        public void WriteText(String text, SpriteFont font, Vector2 location)
        {
            ScreenManager.SpriteBatch.DrawString(font, text, location + new Vector2(2), Color.Black);
            ScreenManager.SpriteBatch.DrawString(font, text, location, Color.White);
        }
    }
}
