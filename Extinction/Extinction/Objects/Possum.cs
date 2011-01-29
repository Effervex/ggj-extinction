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
            : base(50, 5, 5, -0.005f, 0.5)
        {
            filename = @"possum/possum";
            modelTransformation = Matrix.CreateScale(0.25f);
            modelTransformation = Matrix.Multiply(modelTransformation, Matrix.CreateRotationZ((float)Math.PI));
            modelTransformation = Matrix.Multiply(modelTransformation, Matrix.CreateRotationY((float)Math.PI / 2));
        }

        public override CombatEntity NewModel()
        {
            return new Possum();
        }
    }
}
