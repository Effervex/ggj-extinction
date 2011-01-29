//#define SKINDEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace AlphaSubmarines
{
    public partial class AnimationPlayer
    {
        public class AnimationMixer
        {
            //Determins what bone is in what set.
            private readonly int[] boneInSet;
            //The list of the names used ofr the bone sets.
            private readonly List<string> setNames;
            //The weights used for the bone sets.
            private readonly List<List<float>> setWeights;
            //The normalized  weights used for the bone sets.
            private readonly List<List<float>> normalizedWeights;
            //The interpolation values used when blanding the tracks for each bone sets.
            private readonly List<List<float>> interpolationValues;

            //system that holds all the skining informations.
            private readonly SkinningData skinningDataValue;

            //Rotations bor bones in local space.
            private readonly Quaternion[] boneRotations;
            //Positions for bones in local space.
            private readonly Vector3[] bonePositions;

            //Added rotations.
            private readonly Quaternion[] addedRotations;
            //Aded positions.
            private readonly Vector3[] addedPositions;

            //Array that holds the right to use additional transform per bone.
            private readonly System.Collections.BitArray useAddedTransforms;

            //Transforms selected for this frame in world space.
            private readonly Matrix[] worldTransforms;
            //Rotations selected for this frame in world space.
            private readonly Quaternion[] worldRotations;
            //Positions selected for this frame in world space.
            private readonly Vector3[] worldPositions;

            /// <summary>
            /// Bones that reperesent the annimation on the current frame.
            /// </summary>
            public readonly Matrix[] SkinTransforms;

            //List used to holt the available tracks.
            private readonly List<AnimationPlayer> Tracks;

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
            /// Return the list containing all the available animations.
            /// </summary>
            public AnimationClip[] GetAllAnimations()
            {
                return this.skinningDataValue.AnimationClips;
            }

            /// <summary>
            /// Return a list of all the available animation's names the place in the list is the id of the animation.
            /// </summary>
            public string[] GetAllAnimationNames()
            {
                return this.skinningDataValue.AnimationClipsNames;
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


            //Used when blanding if we are using matrices to hold transforms.
            private Vector3 openPosition;
            private Quaternion openRotation;

            /// <summary>
            /// If true the mixer will use linear interpolation when mixing between animations.
            /// </summary>
            public bool UseLinearInterpolation;

            private bool isNotVisible;
            /// <summary>
            /// If true the mixer and it's animations will advance in time but the havy calculations will be skiped.
            /// </summary>
            public bool IsNotVisible
            {
                get
                {
                    return isNotVisible;
                }
                set
                {
                    if (isNotVisible != value)
                    {
                        isNotVisible = value;
                        for (int i = 0; i < Tracks.Count; i++)
                        {
                            Tracks[i].IsNotVisible = value;
                        }
                    }
                }
            }

            /// <summary>
            /// If true the mixer or any of it's tracks will not update.
            /// </summary>
            public bool IsPaused;

            #if SKINDEBUG
                //Used to track how much time my processor needs with an update cycle.
                private long timeTicks = 0;
                public long totalTimeTicks = 0;
            #endif

            /// <summary>
            /// Create a new annimation mixer.
            /// </summary>
            /// <param name="skinData">The data that contains all the information necessary to animate a mesh.</param>
            public AnimationMixer(SkinningData skinData)
            {
                skinningDataValue = skinData;
                boneInSet = new int[skinningDataValue.InverseBindPose.Length];
                for (int b = 0; b < boneInSet.Length;b++ )
                {
                    boneInSet[b] = 0;
                }
                setNames = new List<string>();
                setWeights = new List<List<float>>();
                normalizedWeights = new List<List<float>>();
                interpolationValues = new List<List<float>>();

                Tracks = new List<AnimationPlayer>();

                worldRotations = new Quaternion[skinningDataValue.InverseBindPose.Length];
                worldPositions = new Vector3[skinningDataValue.InverseBindPose.Length];

                SkinTransforms = new Matrix[skinningDataValue.InverseBindPose.Length];
                for (int i = 0; i < SkinTransforms.Length; i++)
                {
                    SkinTransforms[i] = Matrix.Identity;
                }

                worldTransforms = new Matrix[skinningDataValue.InverseBindPose.Length];
                for (int i = 0; i < worldTransforms.Length; i++)
                {
                    worldTransforms[i] = Matrix.Identity;
                }


                boneRotations = new Quaternion[skinningDataValue.InverseBindPose.Length];
                bonePositions = new Vector3[skinningDataValue.InverseBindPose.Length];

                addedRotations = new Quaternion[skinningDataValue.InverseBindPose.Length];
                addedPositions = new Vector3[skinningDataValue.InverseBindPose.Length];

                useAddedTransforms = new System.Collections.BitArray(skinningDataValue.InverseBindPose.Length);


                AddBoneSet("Default");
            }

            /// <summary>
            /// Get a track based on it's index.
            /// </summary>
            /// <param name="index">The index of the track.</param>
            /// <returns>The respective track.</returns>
            public AnimationPlayer this[int index]
            {
                get
                {
                    if (Tracks == null)
                    {
                        return null;
                    }

                    if (index < 0 || index >= Tracks.Count)
                    {
                        return null;
                    }

                    return Tracks[index];
                }
            }

            /// <summary>
            /// Number of available tracks.
            /// </summary>
            /// <returns>Number of available tracks.</returns>
            public int GetNumberOfTracks()
            {
                return Tracks.Count;
            }

            //Generate interpolation values between bones that we can use for annimations.
            private void GenerateLerpValues(List<float> weights, List<float> lerpValues)
            {
                float acumulator = weights[0];
                for (int i = 1; i < weights.Count; i++)
                {
                    acumulator = weights[i] + acumulator;
                    lerpValues[i - 1] = (weights[i] / acumulator);
                }
            }

            //Normalize the weights.
            private void NormalizeWeights(List<float> weights,List<float> normalizedWeights)
            {
                float acumulator = weights[0];
                for (int i = 1; i < weights.Count; i++)
                {
                    acumulator = weights[i] + acumulator;
                }
                for (int i = 0; i < weights.Count; i++)
                {
                    normalizedWeights[i] = weights[i] / acumulator;
                }
            }

            private void CalculateInterpolationFactors(int index)
            {
                NormalizeWeights(setWeights[index], normalizedWeights[index]);
                GenerateLerpValues(normalizedWeights[index],interpolationValues[index]);
            }

            /// <summary>
            /// Add a new track.
            /// </summary>
            /// <param name="useMatrixTransforms">Use matrix transforms for the data inside this track ?</param>
            /// <param name="useLinearInterpolation">Use linear interpolation when blanding between animations inside this track ?</param>
            public void AddTrack(bool useMatrixTransforms,bool useLinearInterpolation)
            {
                Tracks.Add(new AnimationPlayer(this.skinningDataValue,useMatrixTransforms,useLinearInterpolation,true));

                for (int i = 0; i < setWeights.Count; i++)
                {
                    setWeights[i].Add(0.0f);
                    normalizedWeights[i].Add(0.0f);
                    interpolationValues[i].Add(0.0f);
                }
            }

            /// <summary>
            /// Remove a track.
            /// </summary>
            /// <param name="index">The index of the track you want to remove.</param>
            /// <returns>True if the operation was successfull.</returns>
            public bool RemoveTrack(int index)
            {
                if (this.Tracks.Count > index)
                {
                    this.Tracks.RemoveAt(index);
                    for (int i = 0; i < setWeights.Count; i++)
                    {
                        this.setWeights[i].RemoveAt(index);
                        CalculateInterpolationFactors(i);
                    }
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Add a bone-set.
            /// </summary>
            /// <param name="name">The name of the new bone-set</param>
            /// <returns>True if the operation was successfull.</returns>
            public bool AddBoneSet(string name)
            {
                if (setNames.Contains(name))
                {
                    return false;
                }

                setNames.Add(name);

                float[] zeros = new float[Tracks.Count];

                setWeights.Add(new List<float>(zeros));
                normalizedWeights.Add(new List<float>(zeros));
                interpolationValues.Add(new List<float>(zeros));

                return true;
            }

            /// <summary>
            /// Add a bone to the given bone set.
            /// </summary>
            /// <param name="boneSetName">The name of the bone-set where you want this bone.</param>
            /// <param name="boneName">The name of the bone.</param>
            /// <returns>True if the operation was successfull.</returns>
            public bool AddBoneToBoneSet(string boneSetName, string boneName)
            {
                int boneId = Array.IndexOf<string>(skinningDataValue.BoneNames,boneName);
                if (boneId == -1)
                {
                    return false;
                }

                return AddBoneToBoneSet(boneSetName,boneId);
            }

            /// <summary>
            /// Add a bone to the given bone set.
            /// </summary>
            /// <param name="boneSetName">The name of the bone-set where you want this bone.</param>
            /// <param name="boneID">The id of the bone.</param>
            /// <returns>True if the operation was successfull.</returns>
            public bool AddBoneToBoneSet(string boneSetName,int boneID)
            {
                if (boneID < 0 || boneID >= boneInSet.Length)
                {
                    return false;
                }

                int boneSet = setNames.IndexOf(boneSetName);

                if (boneSet == -1)
                {
                    return false;
                }

                boneInSet[boneID] = boneSet;
                return true;
            }

            /// <summary>
            /// Return a bone to the "Default"(0) bone-set.
            /// </summary>
            /// <param name="boneID">The id of the bone.</param>
            /// <returns>True if the operation was successfull.</returns>
            public bool RemoveBoneFromBoneSet(int boneID)
            {
                if (boneID < 0 || boneID >= boneInSet.Length)
                {
                    return false;
                }
                boneInSet[boneID] = 0;

                return true;
            }

            /// <summary>
            /// Return a bone to the "Default"(0) bone-set.
            /// </summary>
            /// <param name="boneName">The name of the bone.</param>
            /// <returns>True if the operation was successfull.</returns>
            public bool RemoveBoneFromBoneSet(string boneName)
            {
                int boneId = Array.IndexOf<string>(skinningDataValue.BoneNames, boneName);
                if (boneId == -1)
                {
                    return false;
                }

                return RemoveBoneFromBoneSet(boneId);
            }

            /// <summary>
            /// Deletes a Bone-Set.
            /// (All the bones in that given bone set are returned to the "Default"(0) bone-set)
            /// </summary>
            /// <param name="boneSetName">The name of the bone-set you want to delete.</param>
            /// <returns>True if the operation was successfull.</returns>
            public bool RemoveBoneSet(string boneSetName)
            {
                int boneSet = setNames.IndexOf(boneSetName);

                if (boneSet == -1)
                {
                    return false;
                }

                if (boneSet == 0)
                {
                    return false;
                }

                for (int i = 0; i < boneInSet.Length; i++)
                {
                    if (boneInSet[i] == 0)
                    {
                        continue;
                    }
                    if (boneInSet[i] == boneSet)
                    {
                        boneInSet[i] = 0;
                        continue;
                    }
                    if (boneInSet[i] > boneSet)
                    {
                        boneInSet[i]--;
                    }
                }
                setNames.RemoveAt(boneSet);
                setWeights.RemoveAt(boneSet);
                normalizedWeights.RemoveAt(boneSet);
                interpolationValues.RemoveAt(boneSet);

                return true;
            }

            /// <summary>
            /// Set how much a certen bone-set will be affected by a certen annimation.
            /// </summary>
            /// <param name="boneSetName">The name of the bone-set.</param>
            /// <param name="track">The track where the animation is played.</param>
            /// <param name="weight">The weight you want for this animation</param>
            /// <returns>True if the operation was successfull</returns>
            public bool BoneSetWeightForTrack(string boneSetName, int track, float weight)
            {
                if (Math.Sign(weight) < 0)
                {
                    return false;
                }

                if (track >= Tracks.Count)
                {
                    return false;
                }

                int boneSet = setNames.IndexOf(boneSetName);

                if (boneSet == -1)
                {
                    return false;
                }

                setWeights[boneSet][track] = weight;
                CalculateInterpolationFactors(boneSet);

                return true;
            }

            /// <summary>
            /// Set how much a certen bone-set will be affected by a certen annimation.
            /// </summary>
            /// <param name="boneSetID">The id of the bone-set.</param>
            /// <param name="track">The track where the animation is played.</param>
            /// <param name="weight">The weight you want for this animation</param>
            /// <returns>True if the operation was successfull</returns>
            public bool BoneSetWeightForTrack(int boneSetID, int track, float weight)
            {
                if (Math.Sign(weight) < 0)
                {
                    return false;
                }

                if (track > Tracks.Count)
                {
                    return false;
                }

                if (boneSetID == -1)
                {
                    return false;
                }

                setWeights[boneSetID][track] = weight;
                CalculateInterpolationFactors(boneSetID);

                return true;
            }

            /// <summary>
            /// Set a weight of 0.0f on this bone-set for all the animations.
            /// </summary>
            /// <param name="boneSetID">The id of the bone-set.</param>
            /// <returns>True if the operation was sucessfull</returns>
            public bool BoneSetWeight0ForAllTrack(int boneSetID)
            {
                if (boneSetID == -1)
                {
                    return false;
                }
                for (int i = 0; i < setWeights[boneSetID].Count; i++)
                {
                    setWeights[boneSetID][i] = 0.0f;
                }
                CalculateInterpolationFactors(boneSetID);
                return true;
            }

            /// <summary>
            /// Set the given weight on this bone-set for all the animations.
            /// </summary>
            /// <param name="boneSetID">The id of the bone-set.</param>
            /// /// <param name="weight">The weight you want to set.</param>
            /// <returns>True if the operation was sucessfull</returns>
            public bool BoneSetWeightForAllTrack(int boneSetID, float weight)
            {
                if (boneSetID == -1)
                {
                    return false;
                }
                for (int i = 0; i < setWeights[boneSetID].Count; i++)
                {
                    setWeights[boneSetID][i] = weight;
                }
                CalculateInterpolationFactors(boneSetID);
                return true;
            }

            /// <summary>
            /// Set a weight of 0.0f on this bone-set for all the animations.
            /// </summary>
            /// <param name="boneSetName">The name of the bone-set.</param>
            /// <returns>True if the operation was sucessfull</returns>
            public bool BoneSetWeight0ForAllTrack(string boneSetName)
            {
                int boneSetID = setNames.IndexOf(boneSetName);

                if (boneSetID == -1)
                {
                    return false;
                }
                for (int i = 0; i < setWeights[boneSetID].Count; i++)
                {
                    setWeights[boneSetID][i] = 0.0f;
                }
                CalculateInterpolationFactors(boneSetID);
                return true;
            }

            /// <summary>
            /// Set the given weight on this bone-set for all the animations.
            /// </summary>
            /// <param name="boneSetName">The name of the bone-set.</param>
            /// /// <param name="weight">The weight you want to set.</param>
            /// <returns>True if the operation was sucessfull</returns>
            public bool BoneSetWeightForAllTrack(string boneSetName, float weight)
            {
                int boneSetID = setNames.IndexOf(boneSetName);

                if (boneSetID == -1)
                {
                    return false;
                }
                for (int i = 0; i < setWeights[boneSetID].Count; i++)
                {
                    setWeights[boneSetID][i] = weight;
                }
                CalculateInterpolationFactors(boneSetID);
                return true;
            }


            /// <summary>
            /// Get a list of all the available bone names.
            /// The position in the list is the id of the bone-set.
            /// </summary>
            public string[] GetBoneSetsNames()
            {
                return setNames.ToArray();
            }

            /// <summary>
            /// Get the bone-set id by it's name.
            /// (The Default bone set called "Default" has allways an ID of 0)
            /// </summary>
            /// <param name="boneSetName">The name of the bone set.</param>
            /// <returns>The id of the bone set or -1 if none found.</returns>
            public int GetBoneSetIdByName(string boneSetName)
            {
                for (int i = 0; i < setNames.Count; i++)
                {
                    if (boneSetName == setNames[i])
                    {
                        return i;
                    }
                }
                return -1;
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
                if (boneNumber < 0)
                {
                    return false;
                }

                if (boneNumber < skinningDataValue.InverseBindPose.Length)
                {
                    addedRotations[boneNumber] = rotation;
                    addedPositions[boneNumber] = position;
                    return true;
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
                if (boneNumber < 0)
                {
                    return false;
                }

                if (boneNumber < skinningDataValue.InverseBindPose.Length)
                {
                    addedPositions[boneNumber] = position;
                    return true;

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
            public bool SetAddedTransform(ref Quaternion rotation, int boneNumber)
            {
                if (boneNumber < 0)
                {
                    return false;
                }

                if (boneNumber < skinningDataValue.InverseBindPose.Length)
                {
                        addedRotations[boneNumber] = rotation;
                        return true;
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
                if (boneNumber < 0)
                {
                    return false;
                }

                if (boneNumber < skinningDataValue.InverseBindPose.Length)
                {
                    //transform.Decompose(out decompressScale, out addedRotations[boneNumber], out addedPositions[boneNumber]);
                    AnimationPlayer.DecompressMatrixNoScale(ref transform, out addedRotations[boneNumber], out addedPositions[boneNumber]);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// Set the state of the additional transform set with the SetAddedTransform() function.
            /// </summary>
            /// <param name="state">Should the additional transform be applyed ?</param>
            /// <param name="boneNumber">Bone id.</param>
            public void AddedTransformState(bool state, int boneNumber)
            {
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
            /// Disable all local additional transforms.
            /// </summary>
            public void DisableAllAddedTransforms()
            {
                this.useAddedTransforms.SetAll(false);
            }

            /// <summary>
            /// Enable all local additional transforms.
            /// </summary>
            public void EnableAllAddedTransforms()
            {
                this.useAddedTransforms.SetAll(true);
            }

            private bool forceUpdate;
            /// <summary>
            /// Force the update of the mixer's state.
            /// </summary>
            public void ForceUpdate()
            {
                forceUpdate = true;
                for (int i = 0; i < Tracks.Count; i++)
                {
                    Tracks[i].forceUpdate = true;
                }
                Update(0);
            }

            //optional[Added Transform] * Animation_Blending(Track[0]...Track[n]) * Parent_Animation_In_World_Space * Inverse_Binding_Pose_In_World_Space
            /// <summary>
            /// Advances the current animation position.
            /// </summary>
            public void Update(long time)
            {
                #if SKINDEBUG
                    timeTicks = System.Diagnostics.Stopwatch.GetTimestamp();
                #endif

                if (IsPaused && !forceUpdate)
                    return;

                if (Tracks.Count > 0)
                {
                    for (int i = 0; i < Tracks.Count; i++)
                    {
                        Tracks[i].Update(time);
                    }

                    if (isNotVisible)
                    {
                        return;
                    }

                    int numTracks = Tracks.Count;
                    int startTrack = 0;

                    for (int boneId = 0; boneId < boneInSet.Length; boneId++)
                    {
                        startTrack = 0;
                        for (int i = 0; i < numTracks; i++)
                        {
                            float boneWeight = normalizedWeights[boneInSet[boneId]][i];
                            if (boneWeight < float.Epsilon || float.IsNaN(boneWeight))
                            {
                                startTrack++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (numTracks <= startTrack)
                        {
                            continue;
                        }

                        if (Tracks[startTrack].useMatrixTransforms)
                        {
                            //Tracks[startTrack].boneTransforms[boneId].Decompose(out decompressScale, out boneRotations[boneId], out bonePositions[boneId]);
                            AnimationPlayer.DecompressMatrixNoScale(ref Tracks[startTrack].boneTransforms[boneId], out boneRotations[boneId], out bonePositions[boneId]);
                        }
                        else
                        {
                            boneRotations[boneId] = Tracks[startTrack].boneRotations[boneId];
                            bonePositions[boneId] = Tracks[startTrack].bonePositions[boneId];
                        }

                        if (Tracks.Count > startTrack)
                        {
                            for (int i = startTrack+1; i < Tracks.Count; i++)
                            {
                                if (Tracks[i].useMatrixTransforms)
                                {
                                    float boneWeight = normalizedWeights[boneInSet[boneId]][i];
                                    if (boneWeight < float.Epsilon || float.IsNaN(boneWeight))
                                    {
                                        continue;
                                    }

                                    //Tracks[i].boneTransforms[boneId].Decompose(out openScale, out openRotation, out openPosition);
                                    AnimationPlayer.DecompressMatrixNoScale(ref Tracks[i].boneTransforms[boneId], out openRotation, out openPosition);

                                    if (this.UseLinearInterpolation)
                                    {
                                        Quaternion.Lerp(ref boneRotations[boneId], ref openRotation, boneWeight, out boneRotations[boneId]);
                                    }
                                    else
                                    {
                                        Quaternion.Slerp(ref boneRotations[boneId], ref openRotation, boneWeight, out boneRotations[boneId]);
                                    }
                                    Vector3.Lerp(ref bonePositions[boneId], ref openPosition, boneWeight, out bonePositions[boneId]);
                                }
                                else
                                {
                                    float boneWeight = normalizedWeights[boneInSet[boneId]][i];
                                    if (boneWeight < float.Epsilon || float.IsNaN(boneWeight))
                                    {
                                        continue;
                                    }

                                    if (this.UseLinearInterpolation)
                                    {
                                        Quaternion.Lerp(ref boneRotations[boneId], ref Tracks[i].boneRotations[boneId], boneWeight, out boneRotations[boneId]);
                                    }
                                    else
                                    {
                                        Quaternion.Slerp(ref boneRotations[boneId], ref Tracks[i].boneRotations[boneId], boneWeight, out boneRotations[boneId]);
                                    }
                                    Vector3.Lerp(ref bonePositions[boneId], ref Tracks[i].bonePositions[boneId], boneWeight, out bonePositions[boneId]);
                                }
                            }
                        }


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

                        for (int bone = 1; bone < SkinTransforms.Length; bone++)
                        {
                            int parentBone = skinningDataValue.SkeletonHierarchy[bone];

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

                        for (int bone = 0; bone < worldTransforms.Length; bone++)
                        {
                            AnimationPlayer.CreateFromQuaternionAndPosition(ref worldRotations[bone], ref worldPositions[bone], ref worldTransforms[bone]);
                            Matrix.Multiply(ref skinningDataValue.InverseBindPose[bone], ref worldTransforms[bone], out SkinTransforms[bone]);
                        }
                    }
                }

                #if SKINDEBUG
                    totalTimeTicks = System.Diagnostics.Stopwatch.GetTimestamp() - timeTicks;
                #endif

                if (forceUpdate)
                    forceUpdate = false;
            }

            /// <summary>
            /// Get the transform that was added to this bone.
            /// </summary>
            /// <param name="transform">The transform.</param>
            /// <param name="boneNumber">The bone id.</param>
            /// <returns>True if the operation was successfull.</returns>
            public bool GetAddedSpaceTransform(ref Matrix transform, int boneNumber)
            {
                if (boneNumber < 0 || boneNumber >= this.worldPositions.Length)
                {
                    return false;
                }

                AnimationPlayer.CreateFromQuaternionAndPosition(ref addedRotations[boneNumber], ref addedPositions[boneNumber], ref transform);
                return true;

            }

            /// <summary>
            /// Get the transform that was added to this bone.
            /// </summary>
            /// <param name="rotation">The rotation.</param>
            /// <param name="boneNumber">The bone id.</param>
            /// <returns>True if the operation was successfull.</returns>
            public bool GetAddedSpaceTransform(ref Quaternion rotation, int boneNumber)
            {
                if (boneNumber < 0 || boneNumber >= this.worldPositions.Length)
                {
                    return false;
                }

                rotation = addedRotations[boneNumber];
                return true;
            }

            /// <summary>
            ///  Get the transform that was added to this bone.
            /// </summary>
            /// <param name="position">The position.</param>
            /// <param name="boneNumber">The bone id.</param>
            /// <returns>True if the operation was successfull.</returns>
            public bool GetAddedSpaceTransform(ref Vector3 position, int boneNumber)
            {
                if (boneNumber < 0 || boneNumber >= this.worldPositions.Length)
                {
                    return false;
                }
                position = addedPositions[boneNumber];
                return true;

            }

            /// <summary>
            /// Get the world space transform of a bone.
            /// </summary>
            /// <param name="transform">The transform.</param>
            /// <param name="boneNumber">The bone id.</param>
            /// <returns>True if the operation was successfull.</returns>
            public bool GetWorldSpaceTransform(ref Matrix transform, int boneNumber)
            {
                if (boneNumber < 0 || boneNumber >= this.worldPositions.Length)
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
                if (boneNumber < 0 || boneNumber >= this.worldPositions.Length)
                {
                    return false;
                }

                rotation = worldRotations[boneNumber];
                return true;
            }

            /// <summary>
            /// Get the world space position of a bone.
            /// </summary>
            /// <param name="position">The position.</param>
            /// <param name="boneNumber">The bone id.</param>
            /// <returns>True if the operation was successfull.</returns>
            public bool GetWorldSpaceTransform(ref Vector3 position, int boneNumber)
            {
                if (boneNumber < 0 || boneNumber >= this.worldPositions.Length)
                {
                    return false;
                }

                position = worldPositions[boneNumber];
                return true;
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

            //Get the list of ints that hold the flags for the used bones.
            private int[] GetAddedTransfomrUsedBoneInfo()
            {
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
        }
    }
}
