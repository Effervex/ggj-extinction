using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extinction.Objects
{
    class Tree : CombatEntity
    {
        public Tree()
            : base(500, 0, 7, 0)
        {
            filename = @"tree/tree_mesh";
        }

        public override bool Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            bool result = base.Update(gameTime);

            // Return if cannot fire
            if (attackDelay > 0)
                return result;

            // Spawn a magical fruit above the bush
            //MagicalFruit fruit = new MagicalFruit(location3D + new Vector3(0, 1, 0));
            // Hack for increasing magicks
            GameState.magicks += 25;
            attackDelay = rateOfAttack;

            return result;
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
