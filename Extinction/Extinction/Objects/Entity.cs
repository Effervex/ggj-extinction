using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Extinction.Objects
{
    public class Entity
    {
        public Model model;
        public Matrix world = Matrix.Identity;
        public Matrix transformation = Matrix.Identity;
        public Vector3 initialLoc = Vector3.Zero;
        public String filename;

        virtual public bool Create()
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

                ExtinctionGame.DrawModel(model, Matrix.Multiply(transformation, world));
            }
        }

        public virtual bool Update(GameTime gameTime)
        {
            return true;
        }

    }
}
