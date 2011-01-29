using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Extinction.Objects
{
    class IceCubes : ToolEntity
    {
        public IceCubes () : base(10000, 10, 0, 0.05f)
        {
            filename = @"iceCubes/icecube";
            modelTransformation = Matrix.CreateScale(0.5f);
            modelTransformation = Matrix.Multiply(modelTransformation, Matrix.CreateRotationX((float)(-Math.PI / 2)));
        }

        public override bool Update(GameTime gameTime, List<Enemy> enemyPositions)
        {
            bool result = base.Update(gameTime, enemyPositions);

            // Return if cannot fire
            if (location.Y >= GameState.NUM_ROWS)
                return true;

            // Damage enemies moving through honey
            foreach (Enemy enemy in enemyPositions)
            {
                int diff = (int)(location.X - (int)enemy.location.X + GameState.NUM_LANES) % GameState.NUM_LANES;
                Vector2 thisLoc = new Vector2((int)location.X, (int)location.Y);
                Vector2 enemyLoc = new Vector2((int)enemy.location.X, (int)enemy.location.Y);
                if (thisLoc.Equals(enemyLoc))
                {
                    // Slow the enemy
                    enemy.takeDamage(damage);
                }
            }

            return result;
        }

        public override CombatEntity NewModel()
        {
            return new IceCubes();
        }
    }
}
