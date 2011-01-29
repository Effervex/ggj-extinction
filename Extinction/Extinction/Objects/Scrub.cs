using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Extinction.Objects
{
    class Scrub : ToolEntity
    {
        public Scrub () : base(100, 0, 5, 0)
        {
            filename = @"scrubs/scrubs";
            modelTransformation = Matrix.CreateScale(0.25f);
            
            modelTransformation = Matrix.Multiply(modelTransformation, Matrix.CreateRotationX((float)(-Math.PI / 2)));
            modelTransformation = Matrix.Multiply(modelTransformation, Matrix.CreateTranslation(new Vector3(0, 1, 0)));
        }

        public override bool Update(GameTime gameTime, List<Enemy> enemyPositions)
        {
            bool result = base.Update(gameTime, enemyPositions);

            // Return if cannot fire
            if (attackDelay > 0)
                return result;

            // Spawn a magical fruit above the bush
            //MagicalFruit fruit = new MagicalFruit(location3D + new Vector3(0, 1, 0));
            // Hack for increasing magicks
            GameState.magicks += 25;
            attackDelay = rateOfAttack;

            return result;
        }

        public override CombatEntity NewModel()
        {
            return new Scrub();
        }
    }
}
