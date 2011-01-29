using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Extinction.Objects
{
    public abstract class ToolEntity : CombatEntity
    {
        public ToolEntity(int health, int damage, float rateOfAttack)
            : base(health, damage, rateOfAttack)
        {
        }

        public abstract void Update(GameTime gameTime, List<Enemy> enemyPositions);
    }
}
