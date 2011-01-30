using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Extinction
{
    public class Particle
    {
        public Vector3 p, v, a;
        public Vector3 aux;
        public float t;
        public float life;
        public bool alive = true;
    }

    

    public class ParticleSystem
    {
        bool additve = false;

        #region SCENE
        static void CreateSceneMagic(ref Particle p, Matrix world)
        {
            float r = ExtinctionGame.Random() * MathHelper.Pi * 2f;
            float rad = ExtinctionGame.Random() * 8f + 21f;
            p.p = world.Translation
                + new Vector3(10f * 
                    (ExtinctionGame.Random() - 0.5f)
                    , 10f + ExtinctionGame.Random() * 5f,
                    10f * (ExtinctionGame.Random() - 0.5f));
                ;
            
            // +new Vector3(0f, (ExtinctionGame.Random() - 0.5f) * 30f, 0f) + Vector3.Transform(Vector3.Right * rad, Matrix.CreateRotationY(r));
                p.v = Vector3.Zero;// ExtinctionGame.RandomVector() * .231f;
                p.v.Y = ExtinctionGame.Random() * -.71f;

            //   p.aux.X = 0.50f;// ExtinctionGame.Random() > 0.5 ? 0 : 0.5f;
            p.aux.Y = (.1f + ExtinctionGame.Random() * .71f);
            p.life = 2.4f + ExtinctionGame.Random() * 4.5f;
        }

        static void UpdateSceneMagic(ref Particle p, Matrix world)
        {
            //p.p += p.v;
            //float life = 1f;
            p.v.X = (float)Math.Sin(1.2f * (p.t / p.life)) * 3f;
            p.v.Z = (float)Math.Cos(1.2f * (p.t / p.life)) * 2f;
            p.p += p.v * ExtinctionGame.GetTimeDelta();
            p.t += ExtinctionGame.GetTimeDelta() / p.life;
            p.aux.X = p.t / p.life;
            p.aux.Z = (float)((Math.Sin(p.t)));
            if (p.t > Math.PI) p.alive = false;
        }

        static void UpdateSceneMagicSystem(ParticleSystem s)
        {
            s.additve = true;
            if (s.particles.Count < 20)
            {
                s.spawnTime = 1f;
                s.AddParticle(s.world.Translation, 1);
            }
            else
            {
                s.spawnTime -= ExtinctionGame.GetTimeDelta();
            }
        }


        public static ParticleSystem GetParticles_SceneMagic()
        {
            ParticleSystem system = new ParticleSystem();
            system.Create("magic", UpdateSceneMagic, CreateSceneMagic, UpdateSceneMagicSystem);
            return system;
        }

        #endregion



        #region MAGIC
        static  void CreateMagic(ref Particle p, Matrix world)
        {
            float r = ExtinctionGame.Random() * MathHelper.Pi * 2f;
            float rad = ExtinctionGame.Random() * 8f + 21f;
            p.p = world.Translation;// +new Vector3(0f, (ExtinctionGame.Random() - 0.5f) * 30f, 0f) + Vector3.Transform(Vector3.Right * rad, Matrix.CreateRotationY(r));
            p.v = ExtinctionGame.RandomVector() * .231f;
            p.v.Y = Math.Abs(p.v.Y) *  1.2f;

         //   p.aux.X = 0.50f;// ExtinctionGame.Random() > 0.5 ? 0 : 0.5f;
            p.aux.Y = (.1f + ExtinctionGame.Random() * .71f);
            p.life = .1f + ExtinctionGame.Random() * .5f;
        }

        static  void UpdateMagic(ref Particle p, Matrix world)
        {
            //p.p += p.v;
            //float life = 1f;
            p.p += p.v * ExtinctionGame.GetTimeDelta();
            p.t += ExtinctionGame.GetTimeDelta() / p.life;
            p.aux.X = p.t / p.life;
            p.aux.Z = (float)((Math.Sin(p.t)));
            if (p.t > Math.PI) p.alive = false; 
        }

        static void UpdateMagicSystem(ParticleSystem s)
        {
            s.additve = true;
            if (s.particles.Count < 20)
            {
                s.spawnTime = 1f;
                s.AddParticle(s.world.Translation, 1); 
            }
            else
            {
                s.spawnTime -= ExtinctionGame.GetTimeDelta();
            }
        }


        public static ParticleSystem GetParticles_Magic()
        {
            ParticleSystem system = new ParticleSystem();
            system.Create("magic", UpdateMagic, CreateMagic, UpdateMagicSystem);
            return system;
        }

        #endregion





        #region CLOUDS
        public static ParticleSystem GetParticles_IslandClouds()
        {

            ParticleSystem system = new ParticleSystem();
            system.Create("cloud", UpdateIslandCloud, CreateIslandCloud, UpdateIslandCloudSystem);
            return system;
        }

        static void CreateIslandCloud(ref Particle p, Matrix world)
        {
            float r = ExtinctionGame.Random() * MathHelper.Pi * 2f;
            float rad = ExtinctionGame.Random() * 8f + 21f;
            p.p = world.Translation + new Vector3(0f, (ExtinctionGame.Random() - 0.5f) * 30f, 0f) + Vector3.Transform(Vector3.Right * rad, Matrix.CreateRotationY(r));
          
            p.v = ExtinctionGame.RandomVector() * 0.1f;
            p.t = 0f;
            p.aux.Y = (1f + ExtinctionGame.Random() * 26.5f);
            p.life = (1f + ExtinctionGame.Random() * 21.5f);
        }

        static void UpdateIslandCloud(ref Particle p, Matrix world)
        {
            float pc = (p.t / p.life);
            p.aux.Z =  (float)((Math.Sin((p.t / p.life) * 3.14f)));
           // p.aux.Z = 0.0f;
           // Console.WriteLine(p.aux.Z);
            if (pc > 1.0f) p.alive = false;
            p.t += ExtinctionGame.GetTimeDelta() / p.life;
            //   p.Position += 
        }

        static void UpdateIslandCloudSystem(ParticleSystem s)
        {
            s.additve = false;
            if (s.particles.Count <150)
            { 
                s.spawnTime = 1f;
                s.AddParticle(s.world.Translation, 1); 
            }
            else
            {
                s.spawnTime -= ExtinctionGame.GetTimeDelta();
            }
        }




        #endregion



        static Texture2D bubble;
        static Effect effect;

        public Matrix world;
        public float spawnTime;

        public List<Particle> particles = new List<Particle>();

        public delegate void SimParticle(ref Particle p, Matrix world);
        public delegate void SimSystem(ParticleSystem s);

        public void DefaultBehave(ref Particle p)
        {

        }

        SimSystem update;
        SimParticle behave;
        SimParticle create;
        Texture texture;
        Effect shader;

        public bool Create(string filename, SimParticle onframe, SimParticle oncreate, SimSystem onupdate)
        {
            behave = onframe;
            create = oncreate;
            update = onupdate;
            texture = ExtinctionGame.LoadTexture(@"particle\particle_" + filename + "_color");
            shader = ExtinctionGame.LoadShader(@"particle\particle_" + filename + "_shader");

            return true;
        }

        public void AddParticle(Vector3 position, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Particle p = new Particle();
                create(ref p, world);
                particles.Add(p);
            }
        }

        bool DeadUpdateFilter(Particle p)
        {
            
            if (!p.alive)
                return true;

            return false;

        }

        public void Update(GameTime time)
        {
            if (update != null) update(this);

            particles.RemoveAll(DeadUpdateFilter);
            for (int i = 0; i < particles.Count; i++)
            {
                Particle p = particles[i];
                behave(ref p,world);
            }

        }


        public void Draw()
        {
            if (particles.Count == 0)
                return;
            
            List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();
            foreach (Particle p in particles)
            {
                vertices.Add(new VertexPositionNormalTexture(p.p, p.aux, new Vector2(-1f, -1f)));
                vertices.Add(new VertexPositionNormalTexture(p.p, p.aux, new Vector2(1f, -1f)));
                vertices.Add(new VertexPositionNormalTexture(p.p, p.aux, new Vector2(1f, 1f)));

                vertices.Add(new VertexPositionNormalTexture(p.p, p.aux, new Vector2(-1f, -1f)));
                vertices.Add(new VertexPositionNormalTexture(p.p, p.aux, new Vector2(1f, 1f)));
                vertices.Add(new VertexPositionNormalTexture(p.p, p.aux, new Vector2(-1f, 1f)));
            }

            if (additve)
                ExtinctionGame.SetState_AdditiveBlend();
            else
              ExtinctionGame.SetState_AlphaBlend();

            ExtinctionGame.SetState_NoDepthWrite();
            ExtinctionGame.SetState_NoCull();
            ExtinctionGame.instance.GraphicsDevice.Textures[0] = texture;

            if (shader != null)
            {
                shader.Parameters["View"].SetValue(ExtinctionGame.view);
                shader.Parameters["Projection"].SetValue(ExtinctionGame.projection);

                foreach (EffectPass p in shader.CurrentTechnique.Passes)
                {
                    p.Apply();
                    ExtinctionGame.instance.GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(
                       PrimitiveType.TriangleList, vertices.ToArray(), 0, vertices.Count / 3);
                }
            }
            vertices.Clear();
        }
    }
}
