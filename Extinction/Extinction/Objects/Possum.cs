﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Extinction.Objects
{
    class Possum : Enemy
    {

        public object getTag()
        {
            return model.Tag;
        }

    }
}
