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
        // A matrix to handle the righting of the model and initial scaling
        public Matrix modelTransformation = Matrix.Identity;
        public Vector3 location3D = Vector3.Zero;
        public String filename;

        virtual public bool Create()
        {
            model = ExtinctionGame.LoadModel(filename);
            return (model != null);
        }

        virtual public bool Create(String filename2)
        {
            filename = filename2;
            return Create();
        }

        virtual public void Draw()
        {
            if (model != null)
            {
                Dictionary<string, object> dict = (Dictionary<string, object>)model.Tag;
                (dict["shader"] as Effect).Parameters["Time"].SetValue(
                    ExtinctionGame.GetTimeTotal());

                world = Matrix.Multiply(world, Matrix.CreateTranslation(location3D));
                ExtinctionGame.DrawModel(model, Matrix.Multiply(modelTransformation, world));
            }
        }

        public virtual bool Update(GameTime gameTime)
        {
            return true;
        }

    }
}
