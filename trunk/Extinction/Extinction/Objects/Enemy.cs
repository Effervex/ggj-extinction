using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Extinction.Objects
{
    abstract class Enemy : Entity
    {
        public abstract void Update(GameTime gameTime, Dictionary<Vector2, ToolEntity> placedTools);
    }
}
