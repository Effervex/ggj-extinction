using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Extinction.Objects
{
    class Crystal : ToolEntity
    {
        public Crystal () : base(150, 15, 10)
        {
            transformation = Matrix.CreateScale(0.25f);
            transformation = Matrix.Multiply(transformation, Matrix.CreateRotationX((float)(-Math.PI / 2)));
        }

        public override void Update(GameTime gameTime, List<Enemy> enemyPositions)
        {
            // Scan for enemies in this lane or next lane at lowre rows

        }

        public override CombatEntity NewModel()
        {
            return new Crystal();
        }
    }
}
