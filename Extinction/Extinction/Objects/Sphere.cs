using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Extinction.Objects
{
    public class Sphere : Entity
    {
        public override void Draw()
        {
            this.world = Matrix.CreateTranslation(2f, 8f, 2f);
            this.world = Matrix.Multiply(this.world, Matrix.CreateScale(1f));

            base.Draw();

        }
    }
}
