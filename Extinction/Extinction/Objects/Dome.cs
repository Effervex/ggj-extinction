using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Extinction.Objects
{
    public class Dome : Entity
    {
        public override void Draw()
        {


            RasterizerState stater = new RasterizerState();
            stater.CullMode = CullMode.CullClockwiseFace;
            ExtinctionGame.instance.GraphicsDevice.RasterizerState = stater;
            base.Draw();
            stater = new RasterizerState();
            stater.CullMode = CullMode.None;
            ExtinctionGame.instance.GraphicsDevice.RasterizerState = stater;
        }
    }
}
