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

        public void Draw(GameTime gameTime, BoundingSphere bs)
        {
            this.world = Matrix.Identity;
            float time = ExtinctionGame.GetTimeTotal() * 3;
            this.world = Matrix.Multiply(this.world, Matrix.CreateScale(bs.Radius / 4 + (float) Math.Sin(time) * 0.04f));
            this.world = Matrix.Multiply(this.world, Matrix.CreateRotationY(time));
            this.world = Matrix.Multiply(this.world, Matrix.CreateTranslation(bs.Center));
            

            base.Draw();

        }
    }
}
