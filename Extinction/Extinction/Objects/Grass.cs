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
        public void Draw(List<Matrix> matrix)
        {
            this.world = Matrix.CreateTranslation(4.3f,4.3f,4.3f);

            ExtinctionGame.SetState_AlphaBlend();
            ExtinctionGame.SetState_NoCull();
            ExtinctionGame.SetState_NoDepthWrite();

            ModelDataSet data = (ModelDataSet)model.Tag;
            data.shader.Parameters["Time"].SetValue((float)ExtinctionGame.instance.getGameTime().TotalGameTime.TotalMilliseconds / 1000f);
            foreach(Matrix m in matrix) {
                //ExtinctionGame.DrawModel(model, m);
                base.Draw();
            }

            ExtinctionGame.SetState_Opaque();
            ExtinctionGame.SetState_Cull();
            ExtinctionGame.SetState_DepthWrite();
        }
    }
}
