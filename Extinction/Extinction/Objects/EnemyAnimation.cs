using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Extinction.Objects
{
    public class EnemyAnimation : CombatEntity
    {
        public EnemyAnimation()
            : base(0,0,0,0)
        {
        }
        public override CombatEntity NewModel()
        {
            return null;
        }
    }
}
