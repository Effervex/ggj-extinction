using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Extinction;
using Extinction.Objects;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Extinction
{
    class GameState
    {
        public static int NUM_LANES = 8;
        public static int NUM_ROWS = 10;
        public static float SPAWN_CHANCE = 0.01f;
        public static float TREE_BUFFER_PERCENT = 0.2f;
        public static float SAFE_Y_HEIGHT = 20;
        // Indexed by lane, then by row
        public BoundingSphere[,] pathPoints;
        Dictionary<Vector2, ToolEntity> placedTools;
        public static Tree tree;

        List<Enemy> enemies;
        ProbabilityDistribution<Enemy> enemySpawner;

        public static float minIslandRadius = 6;

        public GameState()
        {
            Initialise();
        }

        public void Initialise()
        {
            placedTools = new Dictionary<Vector2, ToolEntity>();

            // Random scattering of rocks here?
            enemies = new List<Enemy>();
            enemySpawner = new ProbabilityDistribution<Enemy>();
            Enemy enemy = new Possum();
            enemySpawner.addItem(enemy, enemy.getSpawnProb());
            pathPoints = new BoundingSphere[NUM_LANES, NUM_ROWS + 1];

            tree = new Tree();
        }

        public void LoadContent(Model islandModel, Matrix modelTransform)
        {
            // Locate the model bounding spheres, and set sphere size large enough to fit between
            float buffer = TREE_BUFFER_PERCENT * minIslandRadius;
            for (int lane = 0; lane < NUM_LANES; lane++)
            {
                // Find the outer edge
                float islandEdge = minIslandRadius;
                Vector2 xzPos = GetLocation(lane, NUM_ROWS, islandEdge);
                Vector3 intersect = new Vector3(xzPos.X, SAFE_Y_HEIGHT, xzPos.Y);
                float? y;
                do
                {
                    islandEdge++;
                    xzPos = GetLocation(lane, NUM_ROWS, islandEdge);
                    intersect = new Vector3(xzPos.X, SAFE_Y_HEIGHT, xzPos.Y);
                    bool insideBoundingSphere;
                    Vector3 a, b, c;
                    y = Picking.RayIntersectsModel(new Ray(intersect, Vector3.Down), islandModel, modelTransform, out insideBoundingSphere, out a, out b, out c);
                } while (y.HasValue);
                islandEdge--;
                float gridDistance = (islandEdge - buffer) / NUM_ROWS;

                // Create bounding spheres for each point
                for (int row = 0; row <= NUM_ROWS; row++)
                {
                    xzPos = GetLocation(lane, row, islandEdge);
                    intersect = new Vector3(xzPos.X, SAFE_Y_HEIGHT, xzPos.Y);
                    bool insideBoundingSphere;
                    Vector3 a, b, c;
                    y = Picking.RayIntersectsModel(new Ray(intersect, Vector3.Down), islandModel, modelTransform, out insideBoundingSphere, out a, out b, out c);
                    intersect.Y = SAFE_Y_HEIGHT - (float)y;

                    float avRadius = gridDistance / 2;
                    BoundingSphere sphere = new BoundingSphere(intersect, avRadius);
                    pathPoints[lane, row] = sphere;
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            // Spawn new enemy with chance


            // Update the active tools
            if (placedTools.Count > 0)
            {
                foreach (ToolEntity toolEntity in placedTools.Values)
                {
                    if (toolEntity != null)
                        toolEntity.Update(gameTime, enemies);
                }
            }

            // Update the active enemies
            foreach (Enemy enemy in enemies)
                enemy.Update(gameTime, placedTools);
        }

        internal void Draw(GameTime gameTime)
        {
            // Draw the tools

            // Draw the enemies
        }


        // Gets the location of a vector with regards to a lane number and row number (can be float).
        public Vector2 GetLocation(float laneNumber, float rowNumber)
        {
            float buffer = TREE_BUFFER_PERCENT * minIslandRadius;
            float distance = buffer + (minIslandRadius - buffer) * rowNumber / NUM_ROWS;
            double rotation = (laneNumber * 2.0 / GameState.NUM_LANES) * Math.PI;
            return new Vector2((float)(distance * Math.Cos(rotation)), (float)(distance * Math.Sin(rotation)));
        }

        // Gets the location of a vector with regards to a lane number and row number (can be float).
        public Vector2 GetLocation(float laneNumber, float rowNumber, float maxDistance)
        {
            float buffer = TREE_BUFFER_PERCENT * maxDistance;
            float distance = buffer + (maxDistance - buffer) * rowNumber / NUM_ROWS;
            double rotation = (laneNumber * 2.0 / GameState.NUM_LANES) * Math.PI;
            return new Vector2((float)(distance * Math.Cos(rotation)), (float)(distance * Math.Sin(rotation)));
        }
    }
}
