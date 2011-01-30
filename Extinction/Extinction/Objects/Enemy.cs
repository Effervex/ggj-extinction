using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Extinction.Objects
{
    /**
     * Represents a creature with the following states:
     * 
     * RUNNING
     * ATTACKING
     * DEAD
     * 
     */
    public abstract class Enemy : CombatEntity
    {
        public double spawnProb = 0.1;
        public int laneNum;

        // animated models
        protected EnemyAnimation entity;
        protected EnemyAnimation entityDead;
        protected EnemyAnimation entityRunning;
        protected EnemyAnimation entityAttacking;

        /* Controls animation triggers too */
        public enum EnemyState
        {
            RUNNING, ATTACKING, DYING, DEAD
        }
        private EnemyState state = EnemyState.RUNNING;

        // constructor
        public Enemy(int health, int damage, float rateOfAttack, float speed, double spawnProb)
            : base(health, damage, rateOfAttack, speed)
        {
            this.spawnProb = spawnProb;
            location.Y = GameState.NUM_ROWS;
        }


        public void SetState(EnemyState stateToSet)
        {
            
            state = stateToSet;
            if (state == EnemyState.DEAD)
            {
                // we are waiting to be cleaned up
            }
            else if (state == EnemyState.ATTACKING)
            {
                // Play looping attack animation
                ChangeAnim(state, true);
            }
            else if (state == EnemyState.RUNNING)
            {
                // Play looping run animation
                ChangeAnim(state, true);
            }
            else if (state == EnemyState.DYING)
            {
                // Play looping run animation
                ChangeAnim(state, false);
            }

        }
        public EnemyState GetState()
        {
            return state;
        }

        // switch model to one with a different animation
        protected void ChangeAnim(EnemyState anim, bool looping) {
            switch (anim)
            {
                case EnemyState.RUNNING:
                    entity = entityRunning;
                    break;
                case EnemyState.ATTACKING:
                    entity = entityAttacking;
                    break;
                case EnemyState.DEAD:
                    entity = entityDead;
                    break;
                case EnemyState.DYING:
                    entity = entityDead;
                    break;
            }
            entity.isAnimated = true;
            entity.queueAnimation(looping);
        }

        public bool Create(string filename)
        {
            /* Create all models */
            entityDead = new EnemyAnimation();
            entityRunning = new EnemyAnimation();
            entityAttacking = new EnemyAnimation();
            
            entityDead.Create(filename + "_dying");
            entityRunning.Create(filename + "_running");
            entityAttacking.Create(filename + "_attacking");
            
            entityDead.isAnimated = true;
            entityRunning.isAnimated = true;
            entityAttacking.isAnimated = true;

            entity = entityRunning;
            return true;
        }

        public virtual void queueAnimation(bool loop)
        {
            entity.queueAnimation(loop);
        }

        public override bool Create()
        {
            throw new Exception("LOGIC FAIL.");
        }

        public bool Update(GameTime gameTime, List<ToolEntity> placedTools)
        {
            bool result = base.Update(gameTime);
            entity.Update(gameTime);

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

        public override void Draw()
        {
            if (entity != null) entity.Draw();
        }
    }
}
