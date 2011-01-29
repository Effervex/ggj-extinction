﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Extinction.Objects
{
    public abstract class Enemy : CombatEntity
    {
        public double spawnProb = 0.1;
        public float speed;
        public int laneNum;

        public override void Initialise()
        {
            base.Initialise();
            location.Y = GameState.NUM_ROWS;
        }

        public virtual void SetParameters(int health, int damage, float rateOfAttack, float speed, double spawnProb)
        {
            base.SetParameters(health, damage, rateOfAttack);
            this.speed = speed;
            this.spawnProb = spawnProb;
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
            else if (location.Y <= 0)
            {
                location.Y = 0;
                // This creature is at the tree. Attack the tree!
                GameState.tree.takeDamage(damage);
            }
        }

        internal double getSpawnProb()
        {
            return spawnProb;
        }
    }
}
