using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Extinction.Objects;

namespace Extinction.Icons
{
    class HoneyIcon : ToolIcon
    {
        public HoneyIcon(ToolEntity model, String iconName) : base(model, iconName)
        {
            cost = 150;
            cooldown = 25000;
        }
    }
}
