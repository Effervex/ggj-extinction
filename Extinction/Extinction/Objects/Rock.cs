using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Extinction.Objects
{
    class Rock : ToolEntity
    {
        public Rock () : base(500, 0, 0, 0)
        {
            filename = @"rock/rock";
            modelTransformation = Matrix.CreateScale(1);
            modelTransformation = Matrix.Multiply(modelTransformation, Matrix.CreateRotationX((float)(-Math.PI / 2)));
            modelTransformation = Matrix.Multiply(modelTransformation, Matrix.CreateRotationY((float)(Math.PI / 2)));
        }

        public override bool Update(GameTime gameTime, List<Enemy> enemyPositions)
        {
            return base.Update(gameTime, enemyPositions); ;
        }

        public override CombatEntity NewModel()
        {
            return new Rock();
        }
    }
}
