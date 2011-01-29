using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Extinction.Objects
{
    public abstract class ToolEntity : CombatEntity
    {
        public ToolEntity(float health, float damage, float rateOfAttack, float speed)
            : base(health, damage, rateOfAttack, speed)
        {
        }

        public virtual bool Update(GameTime gameTime, List<Enemy> enemyPositions)
        {
            return base.Update(gameTime);
        }
    }
}
