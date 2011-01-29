using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Extinction.Objects
{
    class Honey : ToolEntity
    {
        public Honey () : base(10000, 0, 5, 0.01f)
        {
            filename = @"honey/honey";
            modelTransformation = Matrix.CreateScale(0.5f);
            modelTransformation = Matrix.Multiply(modelTransformation, Matrix.CreateRotationX((float)(-Math.PI / 2)));
            //modelTransformation = Matrix.Multiply(modelTransformation, Matrix.CreateTranslation(new Vector3(0, -0.5f, 0)));
        }

        public override bool Update(GameTime gameTime, List<Enemy> enemyPositions)
        {
            bool result = base.Update(gameTime, enemyPositions);

            // Slow enemies moving through honey
            foreach (Enemy enemy in enemyPositions)
            {
                int diff = (int)(location.X - (int)enemy.location.X + GameState.NUM_LANES) % GameState.NUM_LANES;
                Vector2 thisLoc = new Vector2((int) location.X, (int) location.Y);
                Vector2 enemyLoc = new Vector2((int) enemy.location.X, (int) enemy.location.Y);
                if (thisLoc.Equals(enemyLoc))
                {
                    // Slow the enemy
                    enemy.currentSpeed = speed / 3;
                }
            }

            // Return if cannot fire
            if (location.Y >= GameState.NUM_ROWS)
                return true;

            return result;
        }

        public override CombatEntity NewModel()
        {
            return new Honey();
        }
    }
}
