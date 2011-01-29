using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Extinction.Objects
{
    public abstract class Enemy : CombatEntity
    {
        public double spawnProb = 0.1;
        public int laneNum;

        public Enemy(int health, int damage, float rateOfAttack, float speed, double spawnProb)
            : base(health, damage, rateOfAttack, speed)
        {
            this.spawnProb = spawnProb;
            location.Y = GameState.NUM_ROWS;
        }

        public bool Update(GameTime gameTime, List<ToolEntity> placedTools)
        {
            bool result = base.Update(gameTime);

            // The enemy has encountered a tool
            Vector2 enemyLoc = new Vector2((int)location.X, (int)location.Y);
            ToolEntity collidedTool;
            if (enemyCollision(enemyLoc, placedTools, out collidedTool))
            {
                currentSpeed = 0;
                if (attackDelay <= 0)
                {
                    collidedTool.takeDamage(damage);
                    attackDelay = rateOfAttack;
                }
            }
            else if (location.Y <= 0)
            {
                location.Y = 0;
                // This creature is at the tree. Attack the tree!
                GameState.tree.takeDamage(damage);
            }

            return result;
        }

        // Checks if an enemy has collided with any of the tools in the list. If so, returns True and sets the collided tool.
        private bool enemyCollision(Vector2 enemyLoc, List<ToolEntity> placedTools, out ToolEntity collidedTool)
        {
            foreach (ToolEntity tool in placedTools)
            {
                // Basic int checking
                Vector2 intLocation = new Vector2((int) tool.location.X, (int) tool.location.Y);
                if (enemyLoc.Equals(intLocation))
                {
                    collidedTool = tool;
                    return true;
                }
            }
            collidedTool = null;
            return false;
        }

        internal double getSpawnProb()
        {
            return spawnProb;
        }
    }
}
