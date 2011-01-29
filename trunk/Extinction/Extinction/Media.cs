using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace Extinction
{

    public class ModelDataSet
    {
        public Effect shader;
        public Texture color;
        public Texture normal;
        public Texture mask;
    }

    public partial class ExtinctionGame : Microsoft.Xna.Framework.Game
    {
        public static ExtinctionGame instance;
        public static Matrix view = Matrix.Identity;
        public static Matrix projection = Matrix.Identity;
        public static float dt = 0f;
        public static float t = 0f;
        public static Random random = new Random();

        public Cursor cursor;

        public static float GetTimeDelta()
        {
            return dt;
        }

        public static float GetTimeTotal()
        {
            return t;
        }

        public static float Random()
        {
            return (float)random.NextDouble();
        }

        public static Vector3 RandomVector()
        {
            return new Vector3((float)random.NextDouble() - .5f, (float)random.NextDouble() - .5f, (float)random.NextDouble() - .5f) * 2f;
        }

        private static Dictionary<string, Model> models = new Dictionary<string, Model>();
        private static Dictionary<string, Effect> effects = new Dictionary<string, Effect>();
        private static Dictionary<string, Texture> textures = new Dictionary<string, Texture>();

        public static void SetState_NoDepthWrite()
        {
            DepthStencilState s = new DepthStencilState();
            s.DepthBufferWriteEnable = false;
            ExtinctionGame.instance.GraphicsDevice.DepthStencilState = s;
        }

        public static void SetState_DepthWrite()
        {
            DepthStencilState s = new DepthStencilState();
            s.DepthBufferWriteEnable = true;
            ExtinctionGame.instance.GraphicsDevice.DepthStencilState = s;
        }

        public static void SetState_AlphaBlend()
        {
            ExtinctionGame.instance.GraphicsDevice.BlendState = BlendState.AlphaBlend;
        }

        public static void SetState_NoCull()
        {
            RasterizerState stater = new RasterizerState();
            stater.CullMode = CullMode.None;
            ExtinctionGame.instance.GraphicsDevice.RasterizerState = stater;
        }

        public static void SetState_Cull()
        {
            RasterizerState stater = new RasterizerState();
            stater.CullMode = CullMode.CullClockwiseFace;
            ExtinctionGame.instance.GraphicsDevice.RasterizerState = stater;
        }

        public static void SetState_Opaque()
        {
            ExtinctionGame.instance.GraphicsDevice.BlendState = BlendState.Opaque;
        }

        

        private class ExtShader : Effect, IEffectMatrices
        {
            public ExtShader(Effect clone)
                : base(clone)
            {
            }

            public ExtShader(GraphicsDevice deivce, byte[] src)
                : base(deivce, src)
            {
            }

            public Microsoft.Xna.Framework.Matrix Projection
            {
                get
                {
                    return Parameters["Projection"].GetValueMatrix();
                }
                set
                {
                    Parameters["Projection"].SetValue(value);
                }
            }

            public Microsoft.Xna.Framework.Matrix View
            {
                get
                {
                    return Parameters["View"].GetValueMatrix();
                }
                set
                {
                    Parameters["View"].SetValue(value);
                }
            }

            public Microsoft.Xna.Framework.Matrix World
            {
                get
                {
                    return Parameters["World"].GetValueMatrix();
                }
                set
                {
                    Parameters["World"].SetValue(value);
                }
            }
        }

        private static void UpdateMedia()
        {
            /*
            if (ExtinctionGame.IsKeyPressed(Keys.Q))
            {
                int n = models.Keys.Count;
                while (n > 0)
                {
                    n--;
                    ExtinctionGame.LoadModel(models.Keys.ElementAt<string>(n));
                }
            }
            if (ExtinctionGame.IsKeyPressed(Keys.W))
            {
                int n = effects.Keys.Count;
                while (n > 0)
                {
                    n--;
                    ExtinctionGame.LoadShader(effects.Keys.ElementAt<string>(n));
                }
            }
             * */
        }

        public static Effect LoadShader(string filename)
        {
            Effect errorResult = null;
            try
            {
                if (effects.ContainsKey(filename))
                    effects.TryGetValue(filename, out errorResult);
                
                Effect newEffect = instance.Content.Load<Effect>(filename);
                if (newEffect == null)
                    throw new Exception("Failed to load new effect:" + filename);

                if (effects.ContainsKey(filename))
                {
                    effects.Remove(filename);
                    effects.Add(filename, newEffect);
                    //effects[filename] = newEffect;
                }
                else
                    effects.Add(filename, newEffect);

                return newEffect;
            }
            catch (Exception e)
            {
                return errorResult;
            }
        }

        public static void ReloadTextures(){

            int n = textures.Count;
            while (n > 0)
            {
                n--;
                string filename = textures.Keys.ElementAt(n);
                LoadTexture(filename);
            }
        }

        public static Texture LoadTexture(string filename)
        {
            Texture errorResult = null;
            try
            {
                if (textures.ContainsKey(filename))
                    textures.TryGetValue(filename, out errorResult);

                Texture newTexture = null;
                try
                {
                    newTexture = instance.Content.Load<Texture2D>(filename);
                }
                catch
                {
                    newTexture = instance.Content.Load<TextureCube>(filename);
                }
                
                if (newTexture == null)
                    throw new Exception("Failed to load new effect:" + filename);

                if (effects.ContainsKey(filename))
                    textures[filename] = newTexture;
                else
                    textures.Add(filename, newTexture);

                return newTexture;
            }
            catch (Exception e)
            {
                return errorResult;
            }
        }

        public static Model LoadModel(string filename)
        {
            Model errorResult = null;
            try
            {
                if (models.ContainsKey(filename))
                    models.TryGetValue(filename, out errorResult);

                Model newModel = instance.Content.Load<Model>(filename);
                if (newModel == null)
                    throw new Exception("Failed to load new model:" + filename);
                
                // add custom data for textures
                ModelDataSet set = new ModelDataSet();
                if (newModel.Tag == null)
                {
                    newModel.Tag = new Dictionary<string, object>();
                }
                ((Dictionary<string, object>)newModel.Tag)["color"] = LoadTexture(filename + "_color");
                ((Dictionary<string, object>)newModel.Tag)["mask"] = LoadTexture(filename + "_mask");
                ((Dictionary<string, object>)newModel.Tag)["normal"] = LoadTexture(filename + "_normal");


                Effect effect = LoadShader(filename + "_shader");
                if(effect == null)
                    throw new Exception("Failed to load new effect:" + filename + "_shader");

                effect = new ExtShader(effect);
                ((Dictionary<string, object>)newModel.Tag)["shader"] = effect;

                for(int i = 0; i < newModel.Meshes.Count; i++)
                {
                    for(int j = 0; j < newModel.Meshes[i].MeshParts.Count; j++)
                    {
                        ModelMeshPart p = newModel.Meshes[i].MeshParts[j];
                        p.Effect = effect;
                    }
                } 

                if (models.ContainsKey(filename))
                    models[filename] = newModel;
                else
                    models.Add(filename, newModel);

                return newModel;
            }
            catch (Exception e)
            {
                return errorResult;
            }
        }
         
        public static void DrawModel(Model model, Matrix world) {

            if (model != null)
            {
                if (model.Tag != null)
                {

                    Dictionary<string, object> textures = (Dictionary<string, object>)model.Tag;

                    if (null != textures["color"]) instance.GraphicsDevice.Textures[0] = (Texture)textures["color"];
                    if (null != textures["mask"]) instance.GraphicsDevice.Textures[1] = (Texture)textures["mask"];
                    if (null != textures["normal"]) instance.GraphicsDevice.Textures[2] = (Texture)textures["normal"];

                }
                model.Draw(world, view, projection);
            }
        }
    }
}
