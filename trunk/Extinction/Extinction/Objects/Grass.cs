using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Extinction.Objects
{
    public class Grass : Entity
    {
        public Grass()
        {
            filename = @"foliage/grass_mesh";
        }

        public void Draw(List<Matrix> matrix)
        {
           // this.world = Matrix.CreateTranslation(4f,5f,4f);

            ExtinctionGame.SetState_AlphaBlend();
            ExtinctionGame.SetState_NoCull();
            ExtinctionGame.SetState_NoDepthWrite();


            ExtinctionGame.GetShader(model).Parameters["Time"].SetValue(ExtinctionGame.GetTimeTotal());
            ExtinctionGame.GetShader(model).Parameters["AT"].SetValue(false);

            foreach(Matrix m in matrix) { 
                ExtinctionGame.DrawModel(model, m);
            }

            ExtinctionGame.SetState_Opaque();
            ExtinctionGame.SetState_DepthWrite();



            ExtinctionGame.GetShader(model).Parameters["Time"].SetValue(ExtinctionGame.GetTimeTotal());
            ExtinctionGame.GetShader(model).Parameters["AT"].SetValue(true);

            foreach (Matrix m in matrix)
            {
                ExtinctionGame.DrawModel(model, m);
            }

            ExtinctionGame.SetState_Cull();
        }
    }
}
