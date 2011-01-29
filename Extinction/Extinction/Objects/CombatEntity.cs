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
        public float rateOfAttack;
        public float attackDelay;
        public Vector2 location;

        public CombatEntity()
        {
            Initialise();
        }

        public virtual void Initialise()
        {
            currentHealth = health;
        }

        public void SetParameters(int health, int damage, float rateOfAttack)
        {
            this.health = health;
            this.damage = damage;
            this.rateOfAttack = rateOfAttack;
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
                attackDelay -= ExtinctionGame.dt;

            // Kill if no health left
            if (currentHealth <= 0)
                return true;
            return false;
        }

        public override void Draw()
        {
            base.Draw();
            // TODO Spin the model
        }
    }
}
