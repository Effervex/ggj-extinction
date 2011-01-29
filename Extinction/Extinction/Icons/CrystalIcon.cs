using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Extinction.Objects;

namespace Extinction.Icons
{
    class CrystalIcon : ToolIcon
    {
        public CrystalIcon(ToolEntity model, String iconName) : base(model, iconName)
        {
            cost = 100;
            cooldown = 5000;
        }
    }
}
