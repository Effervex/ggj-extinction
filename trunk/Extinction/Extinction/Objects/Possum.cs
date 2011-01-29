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
            : base(50, 5, 5, 0.1f, 0.5)
        {
        }

        public override CombatEntity NewModel()
        {
            return new Possum();
        }
    }
}
