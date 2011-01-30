using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Extinction.Screens;

namespace Extinction.Objects
{
    public abstract class CombatEntity : Entity
    {
        public float health;
        public float currentHealth;
        public float damage;
        public float rateOfAttack;
        public float attackDelay;
        public float speed;
        public float currentSpeed;
        public Vector2 location;
        public bool isAnimated = false;
        protected Effect animationRenderEffect;
        protected AlphaSubmarines.AnimationPlayer.AnimationMixer animationMixer;
        protected String animationName;

        public CombatEntity(float health, float damage, float rateOfAttack, float speed)
        {
            this.health = health;
            this.damage = damage;
            this.rateOfAttack = rateOfAttack;
            attackDelay = rateOfAttack;
            this.speed = speed;
            currentSpeed = speed;
            currentHealth = health;
        }

        internal void takeDamage(float damage)
        {
            currentHealth -= damage;
        }

        /**
         * Updates all stuff needed for a combat entity.
         */
        public override bool Update(GameTime gameTime)
        {
            //Update the internal state of the animation mixer.
            if (animationMixer != null)
            {
                animationMixer.Update(gameTime.ElapsedGameTime.Ticks);
            }

            // Reduce the attack delay
            if (attackDelay > 0)
                attackDelay -= ExtinctionGame.dt;

            // If there is speed, move the object using speed
            Vector2 oldLocation = new Vector2(location.X, location.Y);
            if (currentSpeed != 0)
            {
                if (currentSpeed < 0 && location.Y > 0)
                    location.Y += currentSpeed;
                else if (currentSpeed > 0 && location.Y < GameState.NUM_ROWS)
                    location.Y += currentSpeed;
            }

            // Clamp location.Y between 0 and NUM_ROWS
            location.Y = (float) MathHelper.Clamp(location.Y, 0, GameState.NUM_ROWS);

            // Calculate current 3d location
            if (location.Y % 1 == 0)
                location3D = GameState.pathPoints[(int)location.X, (int)location.Y].Center;
            else
            {
                // Have to place object at point mid-way between two points
                Vector3 minPoint = GameState.pathPoints[(int)location.X, (int)location.Y].Center;
                Vector3 maxPoint = GameState.pathPoints[(int)location.X, (int)location.Y + 1].Center;
                if (speed < 0)
                    Vector3.Lerp(ref minPoint, ref maxPoint, location.Y % 1, out location3D);
                else if (speed > 0)
                    Vector3.Lerp(ref maxPoint, ref minPoint, 1 - (location.Y % 1), out location3D);
            }

            // Set up the world transformation
            // Row-based rotation
            world = Matrix.CreateRotationZ(-(float) ((GameState.NUM_ROWS - location.Y) / GameState.NUM_ROWS * Math.PI / 5));
                // Lane based rotation
            world = Matrix.Multiply(world, Matrix.CreateRotationY((float)-((location.X / GameState.NUM_LANES) * 2 * Math.PI)));

            // Return speed
            currentSpeed = speed;

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

        public bool Create(String filename)
        {
            base.Create(filename);

            // A simple effect that implements standard gpu animation.
            animationRenderEffect = ExtinctionGame.instance.Content.Load<Effect>("animated-mesh");

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
                    animationMixer[0].StartClip(animationMixer.GetAnimation(name));
                    animationName = name;
                    break;
                }
            }

            // Set defaults for all bone weights
            animationMixer.BoneSetWeightForAllTrack(0, 0.5f);

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
        public virtual void queueAnimation(bool loop)
        {
            if (animationName == null || animationName == "") return;

            // todo: blend animation
            animationMixer[0].StartClip(animationMixer.GetAnimation(animationName));
            animationMixer[0].IsLoopable = loop;
            animationMixer.IsPaused = false;
        }

        public virtual Vector2 getLocation()
        {
            return new Vector2(0,0);
        }
        public virtual Matrix getLocalTransformation()
        {
            Vector2 pos = InGameScreen.gameState.GetLocation(location.X, location.Y);
            return Matrix.CreateTranslation(location.X, 10, location.Y);
        }

        public override void Draw()
        {
            ExtinctionGame.SetState_NoCull();
            if (isAnimated)
            {

                // Additional transformations
                //Matrix worldOrient = Matrix.Multiply(world, transformation);
                //Matrix transformation = Matrix.Multiply(getLocalTransformation(), getLocation());
                Vector3 pos = new Vector3(getLocalTransformation().M41 + world.M41, getLocalTransformation().M42 + world.M42, getLocalTransformation().M43 + world.M43);
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

        public CombatEntity NewEntity(Vector3 location)
        {
            CombatEntity entity = NewModel();
            entity.model = model;
            entity.location3D = location;
            return entity;
        }

        public abstract CombatEntity NewModel();
    }
}
