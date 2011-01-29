using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Extinction.Objects;

namespace Extinction.Icons
{
    class RockIcon : ToolIcon
    {
        public RockIcon(ToolEntity model, String iconName) : base(model, iconName)
        {
            cost = 100;
            cooldown = 7500;
        }
    }
}
