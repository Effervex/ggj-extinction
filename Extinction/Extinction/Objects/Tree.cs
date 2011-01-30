using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Extinction.Screens;

namespace Extinction.Objects
{
    class Tree : CombatEntity
    {
        Model foliage;

        public Tree()
            : base(500, 0, 0, 0)
        {
        }

        public bool Create()
        {
            foliage = ExtinctionGame.LoadModel(@"foliage/foliage_mesh");
            filename = @"tree/tree_mesh";
            base.Create();
			return true;
        }

        public override void Draw()
        {
            ExtinctionGame.SetState_NoCull();
            ExtinctionGame.SetState_Opaque();
            base.Draw();

            ExtinctionGame.SetState_AlphaBlend();
            ExtinctionGame.SetState_NoCull();
            //ExtinctionGame.SetState_NoDepthWrite();


            
            Matrix worldView = 
                //Matrix.Invert(
                ExtinctionGame.view
                //)
                ;
            Effect shader = ExtinctionGame.GetShader(foliage);
            shader.Parameters["ViewRight"].SetValue(worldView.Forward);
            shader.Parameters["Time"].SetValue(ExtinctionGame.GetTimeTotal());
            
            ExtinctionGame.SetState_DepthWrite();
            ExtinctionGame.DrawModel(foliage, world);

            ExtinctionGame.SetState_Opaque();
            ExtinctionGame.SetState_Cull();
            ExtinctionGame.SetState_DepthWrite();
        }

        public override CombatEntity NewModel()
        {
            return new Tree();
        }
    }
}
