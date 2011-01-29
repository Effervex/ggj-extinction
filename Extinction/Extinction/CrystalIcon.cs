using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Extinction.Objects;

namespace Extinction
{
    class CrystalIcon : ToolIcon
    {
        public CrystalIcon(ToolEntity model, String iconName) : base(model, iconName)
        {
            cost = 50;
            cooldown = 5000;
        }
    }
}
