using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Extinction.Objects
{
    public class Beetle : Enemy
    {

        public Beetle()
            : base(0,0,0,0,0)
        {
            filename = @"beetle/beetle";
        }
        public override CombatEntity NewModel()
        {
            return new Beetle();
        }
    }
}
