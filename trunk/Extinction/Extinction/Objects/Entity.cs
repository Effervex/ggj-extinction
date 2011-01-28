using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Extinction.Objects
{
    public class Entity
    {
        protected Model model;
        public Matrix world = Matrix.Identity;

        public bool Create(string filename)
        {
            model = ExtinctionGame.LoadModel(filename);
            return (model != null);
        }

        virtual public void Draw()
        {
            if(model!=null)
            ExtinctionGame.DrawModel(model, world);
        }
    }
}
