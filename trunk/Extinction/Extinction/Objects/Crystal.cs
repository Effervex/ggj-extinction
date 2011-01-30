using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Extinction.Objects
{

    class Crystal : ToolEntity
    {
        ParticleSystem magic;

        public Crystal () : base(150, 0.05f, 0, 0)
        {
            filename = @"crystal/crystals";
            modelTransformation = Matrix.CreateScale(0.25f);
            modelTransformation = Matrix.Multiply(modelTransformation, Matrix.CreateRotationX((float)(-Math.PI / 2)));
        }

        public override bool Create()
        {
            return base.Create();
        }


        public override bool Update(GameTime gameTime, List<Enemy> enemyPositions)
        {
            bool result = base.Update(gameTime, enemyPositions);

            // Return if cannot fire
            if (attackDelay > 0)
                return result;

            // Scan for enemies in this lane or next lane at lower rows
            bool[] laneUsed = new bool[3];
            foreach (Enemy enemy in enemyPositions)
            {
                int diff = (int) (location.X - (int)enemy.location.X + GameState.NUM_LANES) % GameState.NUM_LANES;
                if (diff >= -1 && diff <= 1)
                {
                    // Enemy is within range
                    // TODO Fire zappy beam
                    if (laneUsed[diff + 1])
                        enemy.takeDamage(damage);
                    laneUsed[diff + 1] = true;
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
