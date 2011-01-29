using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Extinction.Objects
{
    public class Possum : Enemy
    {
        public Possum()
            : base(50, 5, 5, 0.01f, 0.5)
        {
            filename = @"possum/possum";
            transformation = Matrix.CreateScale(0.25f);
            transformation = Matrix.Multiply(transformation, Matrix.CreateRotationZ((float)Math.PI));
        }

        public override CombatEntity NewModel()
        {
            return new Possum();
        }
    }
}
