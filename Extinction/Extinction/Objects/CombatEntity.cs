using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Extinction.Objects
{
    public abstract class CombatEntity : Entity
    {
        public int health;
        public int currentHealth;
        public int damage;
        public float rateOfAttack;
        public float attackDelay;
        public Vector2 location;
        public bool isAnimated = false;
        protected Effect animationRenderEffect;
        protected AlphaSubmarines.AnimationPlayer.AnimationMixer animationMixer;

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
        public override bool Update(GameTime gameTime)
        {
            //Update the internal state of the animation mixer.
            animationMixer.Update(gameTime.ElapsedGameTime.Ticks);

            // Reduce the attack delay
            if (attackDelay > 0)
                attackDelay -= ExtinctionGame.dt;

            // Kill if no health left
            if (currentHealth <= 0)
                return true;
            return false;
        }

        public object getTag()
        {
            if (model == null) return null;
            return model.Tag;
        }

        public bool Create(string filename, Microsoft.Xna.Framework.Content.ContentManager contentManager)
        {
            base.Create(filename);

            // A simple effect that implements standard gpu animation.
            animationRenderEffect = contentManager.Load<Effect>("animated-mesh");

            /*******
             * 
             *  ANIMATION MIXER
             *
             *******/

            //Loading the skining data object from the models.
            object tag = getTag();
            AlphaSubmarines.SkinningData skiningData = (AlphaSubmarines.SkinningData)((Dictionary<string, object>)tag)["SkinData"];

            //Createing a new mixer object, it has tracks that can play more then one animation at the same time on the same model.
            animationMixer = new AlphaSubmarines.AnimationPlayer.AnimationMixer(skiningData);

            //Start the "walk" animation on track 0.
            int i = 0;
            String[] animNames = animationMixer.GetAllAnimationNames();
            foreach (String name in animNames)
            {
                if (name != null && name != "")
                {
                    //Add a track to the mixer that will use matrices for internal node representation.
                    animationMixer.AddTrack(true, false);
                    animationMixer[i].StartClip(animationMixer.GetAnimation(name));
                    i++;
                }
            }
            // Set defaults for all bone weights
            animationMixer.BoneSetWeightForTrack("Default", 0, 0.5f);

            //Make the tracks loopable.
            animationMixer[0].IsLoopable = true;
            //Set the mixer as visible, if it's not visible it will calculate the time steps but it will not calculate the transforms.
            animationMixer.IsNotVisible = false;
            //If paused the mixer will skip all calculations.
            animationMixer.IsPaused = true;

            return true;
        }

        /**
         * Transitions to a different animation
         */
        public void queueAnimation(String animationName, bool loop)
        {
            // todo: blend animation
            animationMixer[0].StartClip(animationMixer.GetAnimation(animationName));
            animationMixer[0].IsLoopable = loop;
            animationMixer.IsPaused = false;
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
                animationMixer.SetAddedTransform(ref rotation, 0);
                animationMixer.SetAddedTransform(ref pos, 0);
                //Enable the extra transform to the root bone.
                animationMixer.AddedTransformState(true, 0);

                Matrix viewProj = Matrix.Multiply(ExtinctionGame.view, ExtinctionGame.projection);

                //Set the effect parameters.
                animationRenderEffect.Parameters["xViewProjection"].SetValue(viewProj);
                //exampleEffect.Parameters["xColor"].SetValue(Color.Khaki.ToVector3());
                //Copy the matrix array containing the animation in binding-pose space.
                animationRenderEffect.Parameters["xBones"].SetValue(animationMixer.SkinTransforms);

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
