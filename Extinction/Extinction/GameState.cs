using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Extinction;
using Extinction.Objects;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.ObjectModel;

namespace Extinction
{
    class GameState
    {
        public static int NUM_LANES = 12;
        public static int NUM_ROWS = 8;
        public static float SPAWN_CHANCE = 0.005f;
        public static float TREE_BUFFER_PERCENT = 0.2f;
        public static float SAFE_Y_HEIGHT = 20;
        // Indexed by lane, then by row
        public static BoundingSphere[,] pathPoints;
        public Dictionary<BoundingSphere, Vector2> sphereGridPoints;
        public List<ToolEntity> placedTools;
        public static Tree tree;

        public List<Enemy> currentEnemies;
        ProbabilityDistribution<Enemy> enemySpawner;

        public static int magicks = 5000;

        public static float minIslandRadius = 6;

        public GameState(List<Enemy> enemies)
        {
            Initialise(enemies);
        }

        public void Initialise(List<Enemy> enemies)
        {
            placedTools = new List<ToolEntity>();

            // Random scattering of rocks here?
            currentEnemies = new List<Enemy>();
            enemySpawner = new ProbabilityDistribution<Enemy>();
            foreach (Enemy enemy in enemies)
                enemySpawner.addItem(enemy, enemy.getSpawnProb());
            enemySpawner.normalise();
            pathPoints = new BoundingSphere[NUM_LANES, NUM_ROWS + 1];
            sphereGridPoints = new Dictionary<BoundingSphere, Vector2>();

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

                    float avRadius = gridDistance;
                    BoundingSphere sphere = new BoundingSphere(intersect, avRadius);
                    pathPoints[lane, row] = sphere;
                    sphereGridPoints.Add(sphere, new Vector2(lane, row));
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            // Spawn new enemy with chance
            if (ExtinctionGame.random.NextDouble() < SPAWN_CHANCE)
            {
                Enemy newEnemy = enemySpawner.sample();
                int lane = ExtinctionGame.random.Next(GameState.NUM_LANES);
                Enemy spawned = (Enemy)newEnemy.NewEntity(pathPoints[lane, NUM_ROWS].Center);
                spawned.location.X = lane;
                currentEnemies.Add(spawned);
            }
            SPAWN_CHANCE *= 1.0001f; //<==============================*************************************************************************************/

            // Update the active tools
            if (placedTools.Count > 0)
            {
                List<ToolEntity> destroyed = new List<ToolEntity>();
                foreach (ToolEntity toolEntity in placedTools)
                {
                    if (toolEntity != null)
                        if (toolEntity.Update(gameTime, currentEnemies))
                            destroyed.Add(toolEntity);
                }
                foreach (ToolEntity destroy in destroyed)
                    placedTools.Remove(destroy);
            }
            tree.Update(gameTime);

            // Update the active enemies
            List<Enemy> killed = new List<Enemy>();
            foreach (Enemy enemy in currentEnemies)
            {
                if (enemy.Update(gameTime, placedTools))
                    killed.Add(enemy);
            }
            foreach (Enemy kill in killed)
                currentEnemies.Remove(kill);
        }

        internal void Draw(GameTime gameTime)
        {
            // Draw the active tools
            foreach (ToolEntity tool in placedTools)
            {
                tool.Draw();
            }

            // Draw the active enemies
            foreach (Enemy enemy in currentEnemies)
            {
                enemy.Draw();
            }
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

        internal bool CanPlaceTool(Vector2 gridPoint)
        {
            foreach (ToolEntity tool in placedTools)
            {
                Vector2 intVector = new Vector2((int)tool.location.X, (int)tool.location.Y);
                if (gridPoint.Equals(intVector))
                    return false;
            }
            return true;
        }
    }
}
