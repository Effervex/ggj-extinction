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
            this.world = Matrix.CreateTranslation(4f,5f,4f);

            ExtinctionGame.SetState_AlphaBlend();
            ExtinctionGame.SetState_NoCull();
            ExtinctionGame.SetState_NoDepthWrite();

            ModelDataSet textures = new ModelDataSet();
            textures.color = (Texture)((Dictionary<string, object>)model.Tag)["color"];
            textures.mask = (Texture)((Dictionary<string, object>)model.Tag)["mask"];
            textures.normal = (Texture)((Dictionary<string, object>)model.Tag)["normal"];
            textures.shader = (Effect)((Dictionary<string, object>)model.Tag)["shader"];
            
            textures.shader.Parameters["Time"].SetValue((float)ExtinctionGame.instance.getGameTime().TotalGameTime.TotalMilliseconds / 1000f);
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
