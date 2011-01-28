using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Extinction.Objects
{
    public class Dome : Entity
    {
        public override void Draw()
        {

            //ExtinctionGame.SetState_AlphaBlend();
            // ExtinctionGame.SetState_Cull();
            ExtinctionGame.SetState_Opaque();
            ExtinctionGame.SetState_NoCull();
            ExtinctionGame.SetState_NoDepthWrite();

            float time = (float)ExtinctionGame.instance.getGameTime().TotalGameTime.TotalMilliseconds / 1000f;
            time *= 5;

            ModelDataSet textures = (ModelDataSet)model.Tag;
            textures.shader.Parameters["blendSky"].SetValue(false);

            world = Matrix.CreateRotationY(MathHelper.ToRadians(time));
            //Console.WriteLine(world);
            base.Draw();


            textures.shader.Parameters["blendSky"].SetValue(true);
            ExtinctionGame.SetState_AlphaBlend(); 
            world = Matrix.CreateRotationY(MathHelper.ToRadians(time * 0.5f)) * Matrix.CreateScale(0.5f);
            base.Draw();
            //base.Draw();

            ExtinctionGame.SetState_Opaque();
            ExtinctionGame.SetState_NoCull();
            ExtinctionGame.SetState_DepthWrite();
            
        }
    }
}
