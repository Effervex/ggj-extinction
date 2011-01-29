using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Extinction.Objects;

namespace Extinction.Icons
{
    class ScrubIcon : ToolIcon
    {
        public ScrubIcon(ToolEntity model, String iconName) : base(model, iconName)
        {
            cost = 50;
            cooldown = 3000;
        }
    }
}
