using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extinction.Objects
{
    class Tree : CombatEntity
    {
        public Tree()
            : base(500, 0, 0)
        {
            filename = @"tree/tree_mesh";
        }

        public override void Draw()
        {
            base.Draw();
        }

        public override CombatEntity NewModel()
        {
            return new Tree();
        }
    }
}
