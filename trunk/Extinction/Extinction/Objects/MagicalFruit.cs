using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Extinction.Objects
{
    class MagicalFruit : Entity
    {
        public MagicalFruit(Vector3 location)
        {
            location3D = location;
        }

        public override bool Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            world = Matrix.CreateRotationY(gameTime.TotalGameTime.Milliseconds / 100f);
            return base.Update(gameTime);
        }
    }
}
