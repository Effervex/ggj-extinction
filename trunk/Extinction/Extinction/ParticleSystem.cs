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
        public bool alive = true;
    }
    public class ParticleSystem
    {
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
            texture = ExtinctionGame.LoadTexture(@"particles\particle_" + filename + "_color");
            shader = ExtinctionGame.LoadShader(@"particles\particle_" + filename + "_shader");

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


            ExtinctionGame.SetState_AlphaBlend();
            ExtinctionGame.SetState_NoDepthWrite();
            ExtinctionGame.SetState_NoCull();
            ExtinctionGame.instance.GraphicsDevice.Textures[0] = texture;

            shader.Parameters["View"].SetValue(ExtinctionGame.view);
            shader.Parameters["Projection"].SetValue(ExtinctionGame.projection);

            foreach (EffectPass p in shader.CurrentTechnique.Passes)
            {
                p.Apply();
                ExtinctionGame.instance.GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(
                   PrimitiveType.TriangleList, vertices.ToArray(), 0, vertices.Count / 3);
            }

            vertices.Clear();
        }
    }
}
