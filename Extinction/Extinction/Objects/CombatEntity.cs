using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Extinction.Objects
{
    abstract class CombatEntity : Entity
    {
        public int health;
        public int currentHealth;
        public int damage;
        public int rateOfAttack;
        public int attackDelay;
        public Vector2 location;

        public CombatEntity()
        {
            Initialise();
        }

        public virtual void Initialise()
        {
            currentHealth = health;
        }

        internal void takeDamage(int damage)
        {
            currentHealth -= damage;
        }

        /**
         * Updates all stuff needed for a combat entity.
         */
        public bool Update(GameTime gameTime)
        {
            // Reduce the attack delay
            if (attackDelay > 0)
                attackDelay -= gameTime.ElapsedGameTime.Milliseconds;

            // Kill if no health left
            if (currentHealth <= 0)
                return true;
            return false;
        }

        public Vector2 GetCurrentLocation()
        {
            float inverseDist = location.Y;
            double rotation = (location.X * 2.0 / GameState.NUM_LANES) * Math.PI;
            return new Vector2((float)(inverseDist * Math.Cos(rotation)), (float)(inverseDist * Math.Sin(rotation)));
        }
    }
}
