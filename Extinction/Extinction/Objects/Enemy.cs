using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Extinction.Objects
{
    abstract class Enemy : CombatEntity
    {
        public double spawnRate;
        public float speed;
        public int laneNum;

        public override void Initialise()
        {
            location.Y = GameState.NUM_ROWS;
        }
            
        
        public void Update(GameTime gameTime, Dictionary<Vector2, ToolEntity> placedTools)
        {
            base.Update(gameTime);

            // The enemy has encountered a tool
            Vector2 enemyLoc = new Vector2((int) location.X, (int) location.Y);
            if (placedTools.ContainsKey(enemyLoc))
            {
                if (attackDelay <= 0)
                {
                    placedTools[enemyLoc].takeDamage(damage);
                    attackDelay = rateOfAttack;
                }
            }
            else if (location.Y > 0)
            {
                // Move the enemy forward
                location.Y -= speed;
            }
        }
    }
}
