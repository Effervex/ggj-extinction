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
        public Model model;
        public Matrix world = Matrix.Identity;

        virtual public bool Create(string filename)
        {
            model = ExtinctionGame.LoadModel(filename);
            return (model != null);
        }

        virtual public void Draw()
        {
            if (model != null)
            {
                Dictionary<string, object> dict = (Dictionary<string, object>)model.Tag;
                (dict["shader"] as Effect).Parameters["Time"].SetValue(
                    ExtinctionGame.GetTimeTotal());

                ExtinctionGame.DrawModel(model, world);
            }
        }

        virtual public void Update()
        {
        }
    }
}
