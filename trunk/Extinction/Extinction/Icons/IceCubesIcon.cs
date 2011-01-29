using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Extinction.Objects;

namespace Extinction.Icons
{
    class IceCubesIcon : ToolIcon
    {
        public IceCubesIcon(ToolEntity model, String iconName) : base(model, iconName)
        {
            cost = 250;
            cooldown = 20000;
        }
    }
}
