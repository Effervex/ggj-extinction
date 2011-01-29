using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Extinction;
using Extinction.Objects;
using Microsoft.Xna.Framework;

namespace Extinction
{
    class GameState
    {
        public static int NUM_LANES = 8;
        public static int NUM_ROWS = 10;
        public static float TREE_BUFFER_PERCENT = 0.1f;
        Dictionary<Vector2, ToolEntity> placedTools;

        List<Enemy> enemies;

        public static float islandRadius = 2;

        public GameState()
        {
            Initialise();
        }

        public void Initialise()
        {
            placedTools = new Dictionary<Vector2, ToolEntity>();

            // Random scattering of rocks here?

            enemies = new List<Enemy>();
        }

        public void Update(GameTime gameTime)
        {
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

        /**
         * Gets the row.
         */
        internal static int GetRow(double distance)
        {
            return (int) ((1 - distance) / (islandRadius / NUM_ROWS));
        }
    }
}
