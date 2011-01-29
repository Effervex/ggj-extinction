using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Extinction.Objects
{
    class Crystal : ToolEntity
    {
        public Crystal () : base(150, 0.1f, 0)
        {
            filename = @"crystal/crystals";
            transformation = Matrix.CreateScale(0.25f);
            transformation = Matrix.Multiply(transformation, Matrix.CreateRotationX((float)(-Math.PI / 2)));
        }

        public override bool Update(GameTime gameTime, List<Enemy> enemyPositions)
        {
            bool result = base.Update(gameTime, enemyPositions);

            // Scan for enemies in this lane or next lane at lower rows
            foreach (Enemy enemy in enemyPositions)
            {
                int enemyLane = (int) enemy.location.X;
                int minLane = (int) (location.X - 1 + GameState.NUM_LANES) % GameState.NUM_LANES;
                int maxLane = (int) (location.X + 1) % GameState.NUM_LANES;
                if (enemyLane == minLane || enemyLane == location.X || enemyLane == maxLane)
                {
                    // Enemy is within range
                    // TODO Fire zappy beam
                    enemy.takeDamage(damage);
                }
            }

            return result;
        }

        public override CombatEntity NewModel()
        {
            return new Crystal();
        }
    }
}
