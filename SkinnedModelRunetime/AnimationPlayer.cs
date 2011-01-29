//#define SKINDEBUG

#if DEBUG
    #pragma warning disable 0162
#endif

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AlphaSubmarines
{
    /// <summary>
    /// The animation player is in charge of decoding bone position
    /// matrices from an animation clip.
    /// </summary>
    public partial class AnimationPlayer
    {
        /// <summary>
        /// The currently playing clip.
        /// </summary>
        public AnimationClip CurrentClip;

        /// <summary>
        /// List of all available animation clips for this player.
        /// </summary>
        public readonly AnimationClip[] Animations;

        /// <summary>
        /// List of all available animation clips names for this player.
        /// </summary>
        public readonly string[] AnimationNames;

        /// <summary>
        /// Get animation-clip by name.
        /// </summary>
        /// <param name="name">Neme of the animation.</param>
        public AnimationClip GetAnimation(string name)
        {
            if (this.skinningDataValue == null)
            {
                return null;
            }
            return this.skinningDataValue.GetAnimationClipByName(name);
        }

        /// <summary>
        /// Get animation-clip by id.
        /// </summary>
        /// <param name="animationId">Id. of the animation</param>
        public AnimationClip GetAnimation(int animationId)
        {
            if (this.skinningDataValue == null)
            {
                return null;
            }

            if (animationId > 0 && animationId < this.skinningDataValue.AnimationClips.Length)
            {
                return this.skinningDataValue.AnimationClips[animationId];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get the id of an animation based on it's name.
        /// </summary>
        public int GetAnimationIdByName(string name)
        {
            for (int i = 0; i < skinningDataValue.AnimationClipsNames.Length; i++)
            {
                if (name == skinningDataValue.AnimationClipsNames[i])
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Get the name of an animation based on it's id.
        /// </summary>
        public string GetAnimationNameById(int id)
        {
            if (id > 0 && id < skinningDataValue.AnimationClips.Length)
            {
                return skinningDataValue.AnimationClipsNames[id];
            }
            return string.Empty;
        }

        /// <summary>
        /// If true the player will not update.
        /// </summary>
        public bool IsPaused;

        // The animation data will update for one frame.
        private bool forceUpdate;

        /// <summary>
        /// If true the player will not update the current time but not the actual transforms.
        /// </summary>
        public bool IsNotVisible;

        /// <summary>
        /// If true the animation will loop wants it reaches it's end
        /// </summary>
        public bool IsLoopable;

        /// <summary>
        /// If true the animation will play in revers.
        /// </summary>
        public bool IsInReverse;

        //animation space bone transforms from the last used frame on the old animation currently used in the bland animation operation.
        //(available only when using the quaternion based engine)
        private Quaternion[] oldBoneRotation;
        private Vector3[] oldBonePositions;
        //(available only when using the matrix based engine)
        private Matrix[] oldBoneTransforms;

        /// <summary>
        /// Animation speed multyplyer.
        /// </summary>
        public int AnimationSpeed = 1;

        /// <summary>
        /// Skip a number of calls to the update function.
        /// </summary>
        public int SkipUpdates = 0;
        private int currentSkipedUpdates = 0;

        private float currentInterpolation;
        //total time to bland between the 2 animations.
        private float totalTimeToInterpolate;
        //time left in ticks until the end of the bland.
        private float timeLeftToInterpolate;
        //bland between 2 animations
        private bool useAnimationInterpolation;

        private int currentKeyframe;
        /// <summary>
        /// Current keyframe in the animation.
        /// </summary>
        public int CurrentKeyframe
        {
            get
            {
                return currentKeyframe;
            }
        }

        private long timeInRevers;

        private long currentTimeValue;
        /// <summary>
        /// Current time in the animation.
        /// </summary>
        public long CurrentTimeInTheAnimationInTicks
        {
            get
            {
                return currentTimeValue;
            }
            set
            {
                currentTimeValue = value;
                currentKeyframe = 0;
            }
        }

        //Transforms selected for this frame in animation space.
        private Matrix[] boneTransforms;
        //Transforms selected for this frame in world space.
        private readonly Matrix[] worldTransforms;

        /// <summary>
        /// Bones that reperesent the annimation on the current frame.
        /// </summary>
        public readonly Matrix[] SkinTransforms;

        //Positions selected for this frame in animation space.
        private Vector3[] bonePositions;
        //Rotations selected for this frame in animation space.
        private Quaternion[] boneRotations;

        //Positions selected for this frame in world space.
        private Vector3[] worldPositions;
        //Rotations selected for this frame in world space.
        private Quaternion[] worldRotations;

        //what keyframes are selected for each bone on this frame.
        private readonly int[] selectedBones;

        //system that holds all the skining informations.
        private readonly SkinningData skinningDataValue;

        //True if the constructor finished his job.
        private bool initializationPassed = false;

        private bool useMatrixTransforms = false;
        /// <summary>
        /// If true it will use the matrix based engine.
        /// If false it will use the quaternion based engine.
        /// </summary>
        public bool UseMatrixTransforms
        {
            get
            {
                return useMatrixTransforms;
            }
            set
            {
                if (value)
                {
                    if (boneTransforms == null)
                    {
                        boneTransforms = new Matrix[skinningDataValue.InverseBindPose.Length];
                        oldBoneTransforms = new Matrix[skinningDataValue.InverseBindPose.Length];

                        if (!usedAsTrack)
                        {
                            addedTransforms = new Matrix[skinningDataValue.InverseBindPose.Length];
                            for (int i = 0; i < addedTransforms.Length; i++)
                            {
                                addedTransforms[i] = Matrix.Identity;
                            }
                        }
                    }
                    if (initializationPassed && !usedAsTrack)
                    {
                        ConvertAddedQuaternionsToTransforms();
                    }
                }
                else
                {
                    if (bonePositions == null)
                    {
                        bonePositions = new Vector3[skinningDataValue.InverseBindPose.Length];
                        oldBonePositions = new Vector3[skinningDataValue.InverseBindPose.Length];

                        boneRotations = new Quaternion[skinningDataValue.InverseBindPose.Length];
                        oldBoneRotation = new Quaternion[skinningDataValue.InverseBindPose.Length];

                        if (!usedAsTrack)
                        {
                            worldPositions = new Vector3[skinningDataValue.InverseBindPose.Length];
                            worldRotations = new Quaternion[skinningDataValue.InverseBindPose.Length];

                            addedPositions = new Vector3[skinningDataValue.InverseBindPose.Length];
                            addedRotations = new Quaternion[skinningDataValue.InverseBindPose.Length];
                            for (int i = 0; i < addedRotations.Length; i++)
                            {
                                addedRotations[i] = Quaternion.Identity;
                            }
                        }
                    }
                    if (initializationPassed && !usedAsTrack)
                    {
                        ConvertAddedTransformsToQuaternion();
                    }
                }

                useMatrixTransforms = value;
            }
        }

        /// <summary>
        /// True if you want to use linear interpolation in place of spherical interpolation.
        /// </summary>
        public bool UseLinearInterpolation = false;

        //Added transforms.(available only when using the matrix based engine)
        private Matrix[] addedTransforms;
        //Added rotations.(available only when using the quaternion based engine)
        private Quaternion[] addedRotations;
        //Aded positions.(available only when using the quaternion based engine)
        private Vector3[] addedPositions;

        //Array that holds the right to use additional transform per bone.
        private readonly System.Collections.BitArray useAddedTransforms;

        //If true when updated this animation player will not place the bones in world space or in annimation space and it will ignore the added transforms.
        private bool usedAsTrack;

        //Used when blanding between 2 animations if we are using matrices to hold transforms.
        private Vector3 oldPosition;
        private Vector3 oldScale;
        private Quaternion oldRotation;
        private Vector3 newPosition;
        private Vector3 newScale;
        private Quaternion newRotation;

        #if SKINDEBUG
            //Used to track how much time my processor needs with an update cycle.
            private long timeTicks = 0;
            public long totalTimeTicks = 0;
        #endif

        /// <summary>
        /// Constructs a new animation player.
        /// </summary>
        public AnimationPlayer(SkinningData skinningData, bool useMatrixTransforms, bool useLinearInterpolation)
            : this(skinningData, useMatrixTransforms, useLinearInterpolation,false)
        {
        }

        /// <summary>
        /// Constructs a new animation player.
        /// </summary>
        private AnimationPlayer(SkinningData skinningData, bool useMatrixTransforms, bool useLinearInterpolation, bool usedAsTrack)
        {
            this.usedAsTrack = usedAsTrack;

            if (skinningData == null)
                throw new ArgumentNullException("skinningData");

            UseLinearInterpolation = useLinearInterpolation;

            skinningDataValue = skinningData;

            initializationPassed = false;
            this.UseMatrixTransforms = useMatrixTransforms;
            initializationPassed = true;

            if (!usedAsTrack)
            {
                SkinTransforms = new Matrix[skinningData.InverseBindPose.Length];
                for (int i = 0; i < SkinTransforms.Length; i++)
                {
                    SkinTransforms[i] = Matrix.Identity;
                }

                worldTransforms = new Matrix[skinningData.InverseBindPose.Length];
                for (int i = 0; i < worldTransforms.Length; i++)
                {
                    worldTransforms[i] = Matrix.Identity;
                }

                useAddedTransforms = new System.Collections.BitArray(skinningData.InverseBindPose.Length);
                useAddedTransforms.SetAll(false);

                for (int i = 0; i < SkinTransforms.Length; i++)
                {
                    SkinTransforms[i] = Matrix.Identity;
                }
            }
            else
            {
                SkinTransforms = new Matrix[1] { Matrix.Identity };
            }

            this.Animations = skinningData.AnimationClips;
            this.AnimationNames = skinningData.AnimationClipsNames;

            selectedBones = new int[skinningData.InverseBindPose.Length];
        }

        /// <summary>
        /// Starts decoding the specified animation "clip".
        /// </summary>
        /// <param name="clip">The clip that you want to play.</param>
        public void StartClip(AnimationClip clip)
        {
            this.BlandToClip(clip, 0, 0);
        }

        /// <summary>
        /// Starts decoding the specified animation "clip" at the "timeInClip" in ticks.
        /// </summary>
        /// <param name="clip">The clip that you want to play.</param>
        /// <param name="timeInClip">The time in the new clip where to start playing.</param>
        public void StartClip(AnimationClip clip, long timeInClip)
        {
            this.BlandToClip(clip, timeInClip, 0);
        }

        /// <summary>
        /// Starts decoding the specified animation "clip" and blands the first frame of the new clip with the current frame available int the player for "totalTimeToInterpolate" ticks.
        /// </summary>
        /// <param name="clip">The new clip that you want this one to bland in.</param>
        /// <param name="timeInClip">The time in the new clip where to bland.</param>
        public void BlandToClip(AnimationClip clip, long totalTimeToInterpolate)
        {
            this.BlandToClip(clip,0, totalTimeToInterpolate);
        }

        /// <summary>
        /// Starts decoding the specified animation "clip" and blands the frame at "timeInClip" from the given clip with the current frame available int the player for "totalTimeToInterpolate" ticks.
        /// </summary>
        /// <param name="clip">The new clip that you want this one to bland in.</param>
        /// <param name="timeInClip">The time in the new clip where to bland.</param>
        /// <param name="totalTimeToInterpolate">The total time to bland.</param>
        public void BlandToClip(AnimationClip clip, long timeInClip, long totalTimeToInterpolate)
        {
            if (clip == null)
                throw new ArgumentNullException("clip");


            if (useMatrixTransforms)
            {
                boneTransforms.CopyTo(oldBoneTransforms, 0);
            }
            else
            {
                bonePositions.CopyTo(oldBonePositions, 0);
                boneRotations.CopyTo(oldBoneRotation, 0);
            }

            this.totalTimeToInterpolate = totalTimeToInterpolate;
            this.timeLeftToInterpolate = totalTimeToInterpolate;
            this.currentInterpolation = 0;

            if (totalTimeToInterpolate > 0)
            {
                this.useAnimationInterpolation = true;
            }

            this.CurrentClip = clip;
            this.currentTimeValue = timeInClip;
            this.currentKeyframe = 0;
            this.timeInRevers = this.CurrentClip.Keyframes[this.CurrentClip.Keyframes.Length - 1].Time;
        }

        /// <summary>
        /// Is it blanding between 2 animations ?
        /// </summary>
        /// <returns></returns>
        public bool IsBlanding()
        {
            return this.useAnimationInterpolation;
        }

        /// <summary>
        /// Force the update of the animation state.
        /// </summary>
        public void ForceUpdate()
        {
            forceUpdate = true;
            Update(0);
        }

        //optional[Added Transform] * Animation||blending(currentAnimation[frame 0],oldAnimation[frame 0]) * Parent_Animation_In_World_Space * Inverse_Binding_Pose_In_World_Space
        /// <summary>
        /// Advances the current animation position.
        /// </summary>
        public void Update(long time)
        {
            #if SKINDEBUG
                timeTicks = System.Diagnostics.Stopwatch.GetTimestamp();
            #endif

            if (this.CurrentClip == null)
            {
                return;
            }

            if (!forceUpdate && SkipUpdates > 0)
            {
                if (currentSkipedUpdates == SkipUpdates)
                {
                    currentSkipedUpdates = 0;
                    return;
                }
                else
                {
                    currentSkipedUpdates++;
                }


            }

            if (IsPaused && !forceUpdate)
                return;

            if (!useAnimationInterpolation && !forceUpdate)
            {
                time = currentTimeValue + (time * AnimationSpeed);

                // If we reached the end, loop back to the start.
                if (time > CurrentClip.Duration)
                    time = 0;

                // If the position moved backwards, reset the keyframe index.
                if (IsLoopable && time <= currentTimeValue)
                {
                    currentKeyframe = 0;
                }

                currentTimeValue = time;

                if (IsInReverse)
                {
                    timeInRevers = CurrentClip.Duration - currentTimeValue;
                }
            }

            while (currentKeyframe < CurrentClip.Keyframes.Length)
            {
                if (!IsInReverse)
                {
                    if (CurrentClip.Keyframes[currentKeyframe].Time > currentTimeValue)
                        break;

                    selectedBones[CurrentClip.Keyframes[currentKeyframe].Bone] = currentKeyframe;
                }
                else
                {
                    int backKey = CurrentClip.Keyframes.Length - currentKeyframe - (currentKeyframe > 0 ? 0 : 1);

                    if (CurrentClip.Keyframes[backKey].Time >= timeInRevers)
                    {
                        selectedBones[CurrentClip.Keyframes[backKey].Bone] = backKey;

                    }
                    else
                    {
                        break;
                    }
                }

                currentKeyframe++;
            }

            if (useAnimationInterpolation)
            {
                if (this.timeLeftToInterpolate < 1)
                {
                    useAnimationInterpolation = false;
                }

                this.currentInterpolation = (this.timeLeftToInterpolate/this.totalTimeToInterpolate);

                this.timeLeftToInterpolate = this.timeLeftToInterpolate - time;
            }

            if (IsNotVisible && !forceUpdate)
                return;

            for (int i = 0; i < selectedBones.Length; i++)
            {
                Keyframe keyframe = CurrentClip.Keyframes[selectedBones[i]];

                if (useMatrixTransforms)
                {
                    if (!useAnimationInterpolation)
                    {
                        boneTransforms[keyframe.Bone] = keyframe.Transform;
                    }
                    else
                    {
                        if (UseLinearInterpolation)
                        {
                            Matrix.Lerp(ref keyframe.Transform, ref oldBoneTransforms[keyframe.Bone], currentInterpolation, out boneTransforms[keyframe.Bone]);
                        }
                        else
                        {
                            oldBoneTransforms[keyframe.Bone].Decompose(out oldScale, out oldRotation, out oldPosition);

                            keyframe.Transform.Decompose(out newScale, out newRotation, out newPosition);

                            Vector3.Lerp(ref newPosition, ref oldPosition, this.currentInterpolation, out newPosition);
                            Vector3.Lerp(ref newScale, ref oldScale, this.currentInterpolation, out newScale);

                            Quaternion.Slerp(ref newRotation, ref oldRotation, this.currentInterpolation, out newRotation);
                            
                            Matrix.CreateScale(ref newScale, out boneTransforms[keyframe.Bone]);
                            Matrix.Transform(ref boneTransforms[keyframe.Bone], ref newRotation, out boneTransforms[keyframe.Bone]);
                            boneTransforms[keyframe.Bone].M41 = newPosition.X;
                            boneTransforms[keyframe.Bone].M42 = newPosition.Y;
                            boneTransforms[keyframe.Bone].M43 = newPosition.Z;
                        }
                    }
                }
                else
                {
                    if (!useAnimationInterpolation)
                    {
                        bonePositions[keyframe.Bone] = keyframe.Position;
                        boneRotations[keyframe.Bone] = keyframe.Rotation;
                    }
                    else
                    {
                        Vector3.Lerp(ref keyframe.Position, ref oldBonePositions[keyframe.Bone], this.currentInterpolation, out bonePositions[keyframe.Bone]);
                        if (UseLinearInterpolation)
                        {
                            Quaternion.Lerp(ref keyframe.Rotation, ref oldBoneRotation[keyframe.Bone], this.currentInterpolation, out boneRotations[keyframe.Bone]);
                        }
                        else
                        {
                            Quaternion.Slerp(ref keyframe.Rotation, ref oldBoneRotation[keyframe.Bone], this.currentInterpolation, out boneRotations[keyframe.Bone]);
                        }
                    }
                }
            }

            if (forceUpdate)
                forceUpdate = false;

            //Skip finam transforms when mixing.
            if (usedAsTrack)
            {
                return;
            }

            // Root bone.
            if (useMatrixTransforms)
            {
                if (useAddedTransforms[0])
                {
                    Matrix.Multiply(ref addedTransforms[0], ref boneTransforms[0], out worldTransforms[0]);
                }
                else
                {
                    worldTransforms[0] = boneTransforms[0];
                }
            }
            else
            {
                if (useAddedTransforms[0])
                {
                    Vector3.Transform(ref addedPositions[0], ref boneRotations[0], out worldPositions[0]);
                    Vector3.Add(ref worldPositions[0], ref bonePositions[0], out worldPositions[0]);

                    Quaternion.Concatenate(ref addedRotations[0], ref boneRotations[0], out worldRotations[0]);
                }
                else
                {
                    worldPositions[0] = bonePositions[0];
                    worldRotations[0] = boneRotations[0];
                }
            }

            // Child bones.
            for (int bone = 1; bone < SkinTransforms.Length; bone++)
            {
                int parentBone = skinningDataValue.SkeletonHierarchy[bone];

                if (useMatrixTransforms)
                {
                    if (useAddedTransforms[bone])
                    {
                        Matrix.Multiply(ref addedTransforms[bone],ref boneTransforms[bone],out worldTransforms[bone]);
                        Matrix.Multiply(ref worldTransforms[bone], ref worldTransforms[parentBone], out worldTransforms[bone]);
                    }
                    else
                    {
                        Matrix.Multiply(ref boneTransforms[bone], ref worldTransforms[parentBone], out worldTransforms[bone]);
                    }
                }
                else
                {
                    if (useAddedTransforms[bone])
                    {
                        Vector3.Transform(ref addedPositions[bone], ref boneRotations[bone], out worldPositions[bone]);
                        Vector3.Add(ref worldPositions[bone], ref bonePositions[bone], out worldPositions[bone]);
                        Quaternion.Concatenate(ref addedRotations[bone], ref boneRotations[bone], out worldRotations[bone]);

                        Vector3.Transform(ref worldPositions[bone], ref worldRotations[parentBone], out worldPositions[bone]);
                        Vector3.Add(ref worldPositions[bone], ref worldPositions[parentBone], out worldPositions[bone]);
                        Quaternion.Concatenate(ref worldRotations[bone], ref worldRotations[parentBone], out worldRotations[bone]);
                    }
                    else
                    {
                        Vector3.Transform(ref bonePositions[bone], ref worldRotations[parentBone], out worldPositions[bone]);
                        Vector3.Add(ref worldPositions[bone], ref worldPositions[parentBone], out worldPositions[bone]);
                        Quaternion.Concatenate(ref boneRotations[bone], ref worldRotations[parentBone], out worldRotations[bone]);
                    }
                }

            }

            for (int bone = 0; bone < SkinTransforms.Length; bone++)
            {
                if (useMatrixTransforms)
                {
                    Matrix.Multiply(ref skinningDataValue.InverseBindPose[bone], ref worldTransforms[bone], out SkinTransforms[bone]);
                }
                else
                {
                    CreateFromQuaternionAndPosition(ref worldRotations[bone], ref worldPositions[bone], ref worldTransforms[bone]);

                    Matrix.Multiply(ref skinningDataValue.InverseBindPose[bone], ref worldTransforms[bone], out SkinTransforms[bone]);
                }
            }

            #if SKINDEBUG
                totalTimeTicks = System.Diagnostics.Stopwatch.GetTimestamp() - timeTicks;
            #endif
        }

        //Creates a matrix from a quaternion and a position.
        private static void CreateFromQuaternionAndPosition(ref Quaternion quaternion,ref Vector3 position, ref Matrix result)
        {
            float num9 = quaternion.X * quaternion.X;
            float num8 = quaternion.Y * quaternion.Y;
            float num7 = quaternion.Z * quaternion.Z;
            float num6 = quaternion.X * quaternion.Y;
            float num5 = quaternion.Z * quaternion.W;
            float num4 = quaternion.Z * quaternion.X;
            float num3 = quaternion.Y * quaternion.W;
            float num2 = quaternion.Y * quaternion.Z;
            float num = quaternion.X * quaternion.W;
            result.M11 = 1f - (2f * (num8 + num7));
            result.M12 = 2f * (num6 + num5);
            result.M13 = 2f * (num4 - num3);
            //result.M14 = 0f;
            result.M21 = 2f * (num6 - num5);
            result.M22 = 1f - (2f * (num7 + num9));
            result.M23 = 2f * (num2 + num);
            //result.M24 = 0f;
            result.M31 = 2f * (num4 + num3);
            result.M32 = 2f * (num2 - num);
            result.M33 = 1f - (2f * (num8 + num9));
            //result.M34 = 0f;
            result.M41 = position.X;
            result.M42 = position.Y;
            result.M43 = position.Z;
            //result.M44 = 1f;
        }

        //Decompress a matrix to quaternion and position.
        private static void DecompressMatrixNoScale(ref Matrix matrix, out Quaternion rotation, out Vector3 position)
        {
            Quaternion.CreateFromRotationMatrix(ref matrix, out rotation);
            position.X = matrix.M41;
            position.Y = matrix.M42;
            position.Z = matrix.M43;
        }

        //Converts all the added transforms to added quaternions and positions.
        private void ConvertAddedTransformsToQuaternion()
        {
            for (int i = 0; i < addedTransforms.Length; i++)
            {
                if (useAddedTransforms[i])
                {
                    //Vector3 scale;
                    //addedTransforms[i].Decompose(out scale, out addedRotations[i],out addedPositions[i]);
                    DecompressMatrixNoScale(ref addedTransforms[i], out addedRotations[i], out addedPositions[i]);
                    if (float.IsNaN(addedRotations[i].X))
                    {
                        addedRotations[i] = Quaternion.Identity;
                    }
                }
            }
        }

        //Converts all the added quaternions and positions to added transforms.
        private void ConvertAddedQuaternionsToTransforms()
        {
            for (int i = 0; i < addedRotations.Length; i++)
            {
                CreateFromQuaternionAndPosition(ref addedRotations[i], ref addedPositions[i], ref addedTransforms[i]);
            }
        }

        /// <summary>
        /// Set the state of the additional transform set with the SetAddedTransform() function.
        /// </summary>
        /// <param name="state">Should the additional transform be applyed ?</param>
        /// <param name="boneNumber">Bone id.</param>
        public void AddedTransformState(bool state, int boneNumber)
        {
            if (usedAsTrack)
            {
                #if DEBUG
                throw new Exception("Added transform set on track.(When an AnimationPlayer is instanced as a track it does not have support for added transforms)");
                #endif

                return;
            }

            if (boneNumber < 0)
            {
                return;
            }

            if (boneNumber < skinningDataValue.InverseBindPose.Length)
            {
                this.useAddedTransforms[boneNumber] = state;
            }
        }

        /// <summary>
        /// Get bone id by it's name.
        /// </summary>
        /// <param name="name">The name of the bone.</param>
        /// <returns>The id of the bone.</returns>
        public int GetBoneNumberByName(string name)
        {
            for (int i = 0; i < skinningDataValue.BoneNames.Length; i++)
            {
                if (name == skinningDataValue.BoneNames[i])
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Get a list of all the available bone names.
        /// The position in the list is the id of the bone.
        /// </summary>
        public string[] GetBoneNames()
        {
            return this.skinningDataValue.BoneNames;
        }

        /// <summary>
        /// Disable all local additional transforms.
        /// </summary>
        public void DisableAllAddedTransforms()
        {
            if (usedAsTrack)
            {
                #if DEBUG
                    throw new Exception("Added transform set on track.(When an AnimationPlayer is instanced as a track it does not have support for added transforms)");
                #endif

                return;
            }

            this.useAddedTransforms.SetAll(false);
        }

        /// <summary>
        /// Enable all local additional transforms.
        /// </summary>
        public void EnableAllAddedTransforms()
        {
            if (usedAsTrack)
            {
                #if DEBUG
                    throw new Exception("Added transform set on track.(When an AnimationPlayer is instanced as a track it does not have support for added transforms)");
                #endif

                return;
            }
            this.useAddedTransforms.SetAll(true);
        }

        /// <summary>
        /// Set a transform that will be applyed as an additional local transform on the bone with the boneNumber id.
        /// To get the bone number id use the GetBoneNumberByName(string boneName) function.
        /// To enable the transform use the AddedTransformState(bool state, int boneNumber)
        /// </summary>
        /// <param name="rotation">The rotation you want to apply.</param>
        /// <param name="position">The position you want to apply.</param>
        /// <param name="boneNumber">Bone id.</param>
        /// <returns>True if the transform was set.</returns>
        public bool SetAddedTransform(ref Quaternion rotation, ref Vector3 position, int boneNumber)
        {
            if (usedAsTrack)
            {
                #if DEBUG
                    throw new Exception("Added transform set on track.(When an AnimationPlayer is instanced as a track it does not have support for added transforms)");
                #endif

                return false;
            }

            if (boneNumber < 0)
            {
                return false;
            }

            if (boneNumber < skinningDataValue.InverseBindPose.Length)
            {
                if (useMatrixTransforms)
                {
                    CreateFromQuaternionAndPosition(ref rotation, ref position, ref addedTransforms[boneNumber]);
                    return true;
                }
                else
                {
                    addedRotations[boneNumber] = rotation;
                    addedPositions[boneNumber] = position;
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Set a transform that will be applyed as an additional local transform on the bone with the boneNumber id.
        /// To get the bone number id use the GetBoneNumberByName(string boneName) function.
        /// To enable the transform use the AddedTransformState(bool state, int boneNumber)
        /// </summary>
        /// <param name="position">The position you want to apply.</param>
        /// <param name="boneNumber">Bone id.</param>
        /// <returns>True if the position was set.</returns>
        public bool SetAddedTransform(ref Vector3 position, int boneNumber)
        {
            if (usedAsTrack)
            {
                #if DEBUG
                    throw new Exception("Added transform set on track.(When an AnimationPlayer is instanced as a track it does not have support for added transforms)");
                #endif

                return false;
            }

            if (boneNumber < 0)
            {
                return false;
            }

            if (boneNumber < skinningDataValue.InverseBindPose.Length)
            {
                if (useMatrixTransforms)
                {
                    addedTransforms[boneNumber].M41 = position.X;
                    addedTransforms[boneNumber].M42 = position.Y;
                    addedTransforms[boneNumber].M43 = position.Z;
                    return true;
                }
                else
                {
                    addedPositions[boneNumber] = position;
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Set a transform that will be applyed as an additional local transform on the bone with the boneNumber id.
        /// To get the bone number id use the GetBoneNumberByName(string boneName) function.
        /// To enable the transform use the AddedTransformState(bool state, int boneNumber)
        /// </summary>
        /// <param name="rotation">The rotation you want to apply.</param>
        /// <param name="boneNumber">Bone id.</param>
        /// <returns>True if the rotation was set.</returns>
        public bool SetAddedTransform(ref Quaternion rotation,int boneNumber)
        {
            if (usedAsTrack)
            {
                #if DEBUG
                    throw new Exception("Added transform set on track.(When an AnimationPlayer is instanced as a track it does not have support for added transforms)");
                #endif

                return false;
            }

            if (boneNumber < 0)
            {
                return false;
            }

            if (boneNumber < skinningDataValue.InverseBindPose.Length)
            {
                if (useMatrixTransforms)
                {
                    Vector3 localPosition = addedTransforms[boneNumber].Translation;
                    CreateFromQuaternionAndPosition(ref rotation, ref localPosition, ref addedTransforms[boneNumber]);
                    return true;
                }
                else
                {
                    addedRotations[boneNumber] = rotation;
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Set a transform that will be applyed as an additional local transform on the bone with the boneNumber id.
        /// To get the bone number id use the GetBoneNumberByName(string boneName) function.
        /// To enable the transform use the AddedTransformState(bool state, int boneNumber)
        /// </summary>
        /// <param name="transform">The matrix you want to apply.</param>
        /// <param name="boneNumber">Bone id.</param>
        /// <returns>True if the matrix was set.</returns>
        public bool SetAddedTransform(ref Matrix transform, int boneNumber)
        {
            if (usedAsTrack)
            {
                #if DEBUG
                    throw new Exception("Added transform set on track.(When an AnimationPlayer is instanced as a track it does not have support for added transforms)");
                #endif

                return false;
            }

            if (boneNumber < 0)
            {
                return false;
            }

            if (boneNumber < skinningDataValue.InverseBindPose.Length)
            {
                if (useMatrixTransforms)
                {
                    addedTransforms[boneNumber] = transform;
                    return true;
                }
                else
                {
                    //transform.Decompose(out decompressScale, out addedRotations[boneNumber], out addedPositions[boneNumber]);
                    DecompressMatrixNoScale(ref transform, out addedRotations[boneNumber], out addedPositions[boneNumber]);
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get the transform that was added to this bone.
        /// </summary>
        /// <param name="transform">The transform.</param>
        /// <param name="boneNumber">The bone id.</param>
        /// <returns>True if the operation was successfull.</returns>
        public bool GetAddedSpaceTransform(ref Matrix transform, int boneNumber)
        {
            if (usedAsTrack)
            {
                #if DEBUG
                    throw new Exception("Added transform get on track.(When an AnimationPlayer is instanced as a track it does not have support for added transforms)");
                #endif

                return false;
            }

            if (boneNumber < 0 || boneNumber >= skinningDataValue.InverseBindPose.Length)
            {
                return false;
            }

            if (this.useMatrixTransforms)
            {
                transform = this.addedTransforms[boneNumber];
                return true;
            }
            else
            {
                CreateFromQuaternionAndPosition(ref addedRotations[boneNumber], ref addedPositions[boneNumber], ref transform);
                return true;
            }
        }

        /// <summary>
        /// Get the transform that was added to this bone.
        /// </summary>
        /// <param name="rotation">The rotation.</param>
        /// <param name="boneNumber">The bone id.</param>
        /// <returns>True if the operation was successfull.</returns>
        public bool GetAddedSpaceTransform(ref Quaternion rotation, int boneNumber)
        {
            if (usedAsTrack)
            {
                #if DEBUG
                    throw new Exception("Added transform get on track.(When an AnimationPlayer is instanced as a track it does not have support for added transforms)");
                #endif

                return false;
            }

            if (boneNumber < 0 || boneNumber >= skinningDataValue.InverseBindPose.Length)
            {
                return false;
            }

            if (this.useMatrixTransforms)
            {
                Quaternion.CreateFromRotationMatrix(ref addedTransforms[boneNumber], out rotation);
                return true;
            }
            else
            {
                rotation = addedRotations[boneNumber];
                return true;
            }
        }

        /// <summary>
        ///  Get the transform that was added to this bone.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="boneNumber">The bone id.</param>
        /// <returns>True if the operation was successfull.</returns>
        public bool GetAddedSpaceTransform(ref Vector3 position, int boneNumber)
        {
            if (usedAsTrack)
            {
                #if DEBUG
                    throw new Exception("Added transform get on track.(When an AnimationPlayer is instanced as a track it does not have support for added transforms)");
                #endif

                return false;
            }

            if (boneNumber < 0 || boneNumber >= skinningDataValue.InverseBindPose.Length)
            {
                return false;
            }

            if (this.useMatrixTransforms)
            {
                position.X = addedTransforms[boneNumber].M41;
                position.Y = addedTransforms[boneNumber].M42;
                position.Z = addedTransforms[boneNumber].M43;
                return true;
            }
            else
            {
                position = addedPositions[boneNumber];
                return true;
            }
        }

        /// <summary>
        /// Get the world space transform of a bone.
        /// </summary>
        /// <param name="transform">The transform.</param>
        /// <param name="boneNumber">The bone id.</param>
        /// <returns>True if the operation was successfull.</returns>
        public bool GetWorldSpaceTransform(ref Matrix transform, int boneNumber)
        {
            if (usedAsTrack)
            {
                #if DEBUG
                    throw new Exception("World transform get on track.(When an AnimationPlayer is instanced as a track it does not have support for world transforms)");
                #endif

                return false;
            }

            if (boneNumber < 0 || boneNumber >= skinningDataValue.InverseBindPose.Length)
            {
                return false;
            }

            transform = this.worldTransforms[boneNumber];
            return true;
        }

        /// <summary>
        /// Get the world space rotation of a bone.
        /// </summary>
        /// <param name="rotation">The rotation.</param>
        /// <param name="boneNumber">The bone id.</param>
        /// <returns>True if the operation was successfull.</returns>
        public bool GetWorldSpaceTransform(ref Quaternion rotation, int boneNumber)
        {
            if (usedAsTrack)
            {
                #if DEBUG
                    throw new Exception("World transform get on track.(When an AnimationPlayer is instanced as a track it does not have support for world transforms)");
                #endif

                return false;
            }

            if (boneNumber < 0 || boneNumber >= skinningDataValue.InverseBindPose.Length)
            {
                return false;
            }

            if (this.useMatrixTransforms)
            {
                Quaternion.CreateFromRotationMatrix(ref worldTransforms[boneNumber], out rotation);
                return true;
            }
            else
            {
                rotation = worldRotations[boneNumber];
                return true;
            }
        }

        /// <summary>
        /// Get the world space position of a bone.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="boneNumber">The bone id.</param>
        /// <returns>True if the operation was successfull.</returns>
        public bool GetWorldSpaceTransform(ref Vector3 position, int boneNumber)
        {
            if (usedAsTrack)
            {
                #if DEBUG
                    throw new Exception("World transform get on track.(When an AnimationPlayer is instanced as a track it does not have support for world transforms)");
                #endif

                return false;
            }

            if (boneNumber < 0 || boneNumber >= skinningDataValue.InverseBindPose.Length)
            {
                return false;
            }

            if (this.useMatrixTransforms)
            {
                position.X = worldTransforms[boneNumber].M41;
                position.Y = worldTransforms[boneNumber].M42;
                position.Z = worldTransforms[boneNumber].M43;
                return true;
            }
            else
            {
                position = worldPositions[boneNumber];
                return true;
            }
        }

        //Get the list of ints that hold the flags for the used bones.
        private int[] GetAddedTransfomrUsedBoneInfo()
        {
            if (usedAsTrack)
            {
                #if DEBUG
                    throw new Exception("Added transform get instance data on track.(When an AnimationPlayer is instanced as a track it does not have support for added transforms)");
                #endif

                return new int[0];
            }

            if (useAddedTransforms != null)
            {
                int[] data = (int[])typeof(System.Collections.BitArray).GetField("m_array", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(useAddedTransforms);
                return data;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Return true if the given bone is using an added transform.
        /// </summary>
        /// <param name="boneID">The bone id.</param>
        /// <returns>True if the added transform for this bone is used.</returns>
        public bool GetAddedTransformState(int boneID)
        {
            if (this.useAddedTransforms == null)
            {
                return false;
            }

            if (boneID > 0 && boneID < useAddedTransforms.Count)
            {
                return this.useAddedTransforms[boneID];
            }
            else
            {
                return false;
            }
        }
    }
}
