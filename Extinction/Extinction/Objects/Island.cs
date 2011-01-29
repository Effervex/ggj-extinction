using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Extinction.Objects
{
    public class Island : Entity
    {
        public override void Draw()
        {
            ExtinctionGame.SetState_DepthWrite();
            ExtinctionGame.SetState_Opaque();
            base.Draw();
        }
    }
}
