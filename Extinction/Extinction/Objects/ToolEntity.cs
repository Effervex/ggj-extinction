using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Extinction.Objects
{
    abstract class ToolEntity : Entity
    {
        public abstract void Update(GameTime gameTime, List<Enemy> enemyPositions);
    }
}
