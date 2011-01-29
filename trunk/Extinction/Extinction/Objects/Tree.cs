using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extinction.Objects
{
    class Tree : CombatEntity
    {
        public bool Create()
        {
            return base.Create(@"tree/tree_mesh");
        }
        public override void Draw()
        {
            base.Draw();
        }
    }
}
