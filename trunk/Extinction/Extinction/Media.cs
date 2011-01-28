using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Extinction
{
    public partial class ExtinctionGame : Microsoft.Xna.Framework.Game
    {
        private static Microsoft.Xna.Framework.Game instance;
        private static Dictionary<string, Model> models = new Dictionary<string, Model>();
        private static Dictionary<string, Effect> effects = new Dictionary<string, Effect>();

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
                    effects[filename] = newEffect;
                else
                    effects.Add(filename, newEffect);

                return newEffect;
            }
            catch (Exception e)
            {
                return errorResult;
            }
        }

        public static Model LoadModel(string filename)
        {
            return LoadModel(filename, null);
        }

        public static Model LoadModel(string filename, string shader)
        {
            Model errorResult = null;
            try
            {
                if (models.ContainsKey(filename))
                    models.TryGetValue(filename, out errorResult);

                Model newModel = instance.Content.Load<Model>(filename);
                if (newModel == null)
                    throw new Exception("Failed to load new model:" + filename);

                if (shader != null)
                {

                    Effect effect = LoadShader(shader);
                    if(effect == null)
                        throw new Exception("Failed to load new effect:" + shader);

                    effect = new ExtShader(effect);

                    for(int i = 0; i < newModel.Meshes.Count; i++)
                    {
                        for(int j = 0; j < newModel.Meshes[i].MeshParts.Count; j++)
                        {
                            ModelMeshPart p = newModel.Meshes[i].MeshParts[j];
                            p.Effect = effect;
                        }
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
         
    }
}
