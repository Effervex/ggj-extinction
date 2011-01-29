using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Extinction.Objects
{
    abstract class CombatEntity : Entity
    {
        public int health;
        public int currentHealth;
        public int damage;
        public float rateOfAttack;
        public float attackDelay;
        public Vector2 location;
        public bool isAnimated = false;
        protected Effect animationRenderEffect;

        public CombatEntity()
        {
            Initialise();
        }

        public virtual void Initialise()
        {
            currentHealth = health;
        }

        public void SetParameters(int health, int damage, float rateOfAttack)
        {
            this.health = health;
            this.damage = damage;
            this.rateOfAttack = rateOfAttack;
        }

        internal void takeDamage(int damage)
        {
            currentHealth -= damage;
        }

        /**
         * Updates all stuff needed for a combat entity.
         */
        public bool Update(GameTime gameTime)
        {
            // Reduce the attack delay
            if (attackDelay > 0)
                attackDelay -= ExtinctionGame.dt;

            // Kill if no health left
            if (currentHealth <= 0)
                return true;
            return false;
        }

        public bool Create(string filename, Microsoft.Xna.Framework.Content.ContentManager contentManager)
        {
            // A simple effect that implements standard gpu animation.
            animationRenderEffect = contentManager.Load<Effect>("animated-mesh");

            return base.Create(filename);
        }

        public override void Draw()
        {

            if (isAnimated)
            {
                // Additional transformations
                Vector3 pos = new Vector3(world.M41, world.M42, world.M43);
                Quaternion rotation = Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(0), 0, MathHelper.ToRadians(-180));
                Matrix scale = Matrix.CreateScale(0.1f);
                //Set the position and rotation of the model(the bone with the id 0 is allways the root bone).
                ExtinctionGame.instance.mixer.SetAddedTransform(ref rotation, 0);
                ExtinctionGame.instance.mixer.SetAddedTransform(ref pos, 0);

                //Enable the extra transform to the root bone.
                ExtinctionGame.instance.mixer.AddedTransformState(true, 0);

                Matrix viewProj = Matrix.Multiply(ExtinctionGame.view, ExtinctionGame.projection);

                //Set the effect parameters.
                animationRenderEffect.Parameters["xViewProjection"].SetValue(viewProj);
                //exampleEffect.Parameters["xColor"].SetValue(Color.Khaki.ToVector3());
                //Copy the matrix array containing the animation in binding-pose space.
                animationRenderEffect.Parameters["xBones"].SetValue(ExtinctionGame.instance.mixer.SkinTransforms);

                ExtinctionGame.instance.GraphicsDevice.SetVertexBuffer(model.Meshes[0].MeshParts[0].VertexBuffer);
                ExtinctionGame.instance.GraphicsDevice.Indices = model.Meshes[0].MeshParts[0].IndexBuffer;

                //Draw the mesh.
                Dictionary<string, object> textures = (Dictionary<string, object>)model.Tag;
                if (null != textures["color"]) ExtinctionGame.instance.GraphicsDevice.Textures[0] = (Texture)textures["color"];
                animationRenderEffect.Techniques[0].Passes[0].Apply();
                animationRenderEffect.Parameters["xDiffuseTexture"].SetValue((Texture)textures["color"]);
                ExtinctionGame.instance.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, model.Meshes[0].MeshParts[0].NumVertices, model.Meshes[0].MeshParts[0].StartIndex, model.Meshes[0].MeshParts[0].PrimitiveCount);
            }
            else
            {
                base.Draw();
            }
            // TODO Spin the model
        }
    }
}
