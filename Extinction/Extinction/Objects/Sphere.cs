using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Extinction.Objects
{
    public class Sphere : Entity
    {
        public override void Draw()
        {
            this.world = Matrix.CreateTranslation(4.3f,4.3f,4.3f);

            /* Change settings */
            RasterizerState stater = new RasterizerState();
            BlendState stateb = new BlendState();
            DepthStencilState stated = new DepthStencilState();
            //stated.DepthBufferWriteEnable = false;
            //stated.DepthBufferEnable = false;
            stateb.AlphaBlendFunction = BlendFunction.Add;
            stater.CullMode = CullMode.None;


            ExtinctionGame.instance.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            ExtinctionGame.instance.GraphicsDevice.DepthStencilState = stated;
            ExtinctionGame.instance.GraphicsDevice.RasterizerState = stater;
            ModelDataSet data = (ModelDataSet)model.Tag;
            data.shader.Parameters["Time"].SetValue((float)ExtinctionGame.instance.getGameTime().TotalGameTime.TotalMilliseconds / 1000f);
            ExtinctionGame.DrawModel(model, Matrix.Identity);
            base.Draw();

            /* Restore settings */
            DepthStencilState statend = new DepthStencilState();
            //statend.DepthBufferEnable = true;
            ExtinctionGame.instance.GraphicsDevice.DepthStencilState = statend;
            ExtinctionGame.instance.GraphicsDevice.BlendState = BlendState.Opaque;
            RasterizerState statenr = new RasterizerState();
            statenr.CullMode = CullMode.None;
            ExtinctionGame.instance.GraphicsDevice.RasterizerState = statenr;

        }
    }
}
