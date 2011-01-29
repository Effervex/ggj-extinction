using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace AlphaSubmarines
{
    /// <summary>
    /// Custom processor extends the builtin framework ModelProcessor class,
    /// adding animation support.
    /// </summary>
    [ContentProcessor(DisplayName="Communist animation processor")]
    public class SkinnedModelPipeline : ModelProcessor
    {
        [Browsable(false)]
        public override bool GenerateTangentFrames
        {
            get
            {
                return true;
            }
            set
            {
            }
        }

        [Browsable(false)]
        public override bool ColorKeyEnabled
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        [Browsable(false)]
        public override bool ResizeTexturesToPowerOfTwo
        {
            get
            {
                return false;
            }
            set
            {
            }

        }

        [Browsable(false)]
        public override bool GenerateMipmaps
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        [Browsable(false)]
        public override TextureProcessorOutputFormat TextureFormat
        {
            get
            {
                return base.TextureFormat;
            }
            set
            {
            }
        }

        [Browsable(false)]
        [DefaultValue(MaterialProcessorDefaultEffect.SkinnedEffect)]
        public override MaterialProcessorDefaultEffect DefaultEffect
        {
            get { return MaterialProcessorDefaultEffect.SkinnedEffect; }
            set { }
        }

        [Browsable(false)]
        public override bool PremultiplyTextureAlpha
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        [Browsable(false)]
        public override bool PremultiplyVertexColors
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        [Browsable(false)]
        public override Color ColorKeyColor
        {
            get
            {
                return base.ColorKeyColor;
            }
            set
            {
            }
        }

        /// <summary>
        /// List of available verts.
        /// </summary>
        protected List<Vector3> verts;

        /// <summary>
        /// The main Process method converts an intermediate format content pipeline
        /// NodeContent tree to a ModelContent object with embedded animation data.
        /// </summary>
        public override ModelContent Process(NodeContent input,
                                             ContentProcessorContext context)
        {

            ValidateMesh(input, context, null);

            AddExtraUvChannel(input, context);
            RemoveDefaultEffect(input);

            Matrix rotation = Matrix.CreateScale(this.Scale) * Matrix.CreateRotationX(MathHelper.ToRadians(90.0f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(RotationZ)) * Matrix.CreateRotationX(MathHelper.ToRadians(RotationX)) * Matrix.CreateRotationY(MathHelper.ToRadians(RotationY));
            MeshHelper.TransformScene(input,rotation);

            this.Scale = 1;
            this.RotationX = 0;
            this.RotationY = 0;
            this.RotationZ = 0;

            
            Dictionary<string, object> tagData = new Dictionary<string, object>();

            #region Animation processor part

            SkinningData skinData = CollectSkiningData(input,72);
            //You have to add this somewere(to the .Tag is a good place) so you can load it at runetime.

            #endregion

            //Store the animation data.
            tagData.Add("SkinData", skinData);

            // Chain to the base ModelProcessor class so it can convert the model data.
            ModelContent model = base.Process(input, context);

            verts = new List<Vector3>();
            FindVertices(input);

            // Store vertex information in the tag data, as an array of Vector3.
            tagData.Add("Vertices", verts.ToArray());
            //context.Logger.LogImportantMessage(verts.Count.ToString());

            // Also store a custom bounding sphere.
            BoundingSphere bs = new BoundingSphere();
            BoundingBox bBox = new BoundingBox();
            if (verts.Count > 0)
            {
                bs = BoundingSphere.CreateFromPoints(verts);
                bBox = BoundingBox.CreateFromPoints(verts);
            }
            tagData.Add("BoundingSphere", bs);
            tagData.Add("Min", bBox.Min);
            tagData.Add("Max", bBox.Max);
            tagData.Add("Center", this.GetVertsCenter(verts));


            model.Tag = tagData;

            return model;
        }

        //You only need this set of functions and the marked part in the Process() to get animation data the rest is how I use it in my engine.
        #region Animation-export

        /// <summary>
        /// Collect all the information that the system needs from the model for animation.
        /// </summary>
        /// <param name="input">The input that contains the model</param>
        /// <param name="maxNumberOfSupportedBones">The maximum number of bones that you would like the exporter to pass without exceptions.</param>
        private static SkinningData CollectSkiningData(NodeContent input,int maxNumberOfSupportedBones)
        {
            // Find the skeleton.
            BoneContent skeleton = MeshHelper.FindSkeleton(input);

            if (skeleton == null)
                throw new InvalidContentException("Input skeleton not found.");

            // We don't want to have to worry about different parts of the model being
            // in different local coordinate systems, so let's just bake everything.
            FlattenTransforms(input, skeleton);

            // Read the bind pose and skeleton hierarchy data.
            IList<BoneContent> bones = MeshHelper.FlattenSkeleton(skeleton);

            if (bones.Count > SkinnedEffect.MaxBones)
            {
                throw new InvalidContentException(string.Format(
                    "Skeleton has {0} bones, but the maximum supported is {1}.",
                    bones.Count, maxNumberOfSupportedBones));
            }

            List<Matrix> bindPose = new List<Matrix>();
            List<Matrix> inverseBindPose = new List<Matrix>();
            List<int> skeletonHierarchy = new List<int>();
            List<string> boneNames = new List<string>();

            foreach (BoneContent bone in bones)
            {
                bindPose.Add(bone.Transform);
                inverseBindPose.Add(Matrix.Invert(bone.AbsoluteTransform));
                skeletonHierarchy.Add(bones.IndexOf(bone.Parent as BoneContent));
                boneNames.Add(bone.Name);
            }

            // Convert animation data to our runtime format.
            Dictionary<string, AnimationClip> animationClips;
            animationClips = ProcessAnimations(skeleton.Animations, bones,bindPose);

            return new SkinningData(animationClips, inverseBindPose.ToArray(),bindPose.ToArray(), skeletonHierarchy.ToArray(), boneNames.ToArray());
        }

        /// <summary>
        /// Converts an intermediate format content pipeline AnimationContentDictionary
        /// object to our runtime AnimationClip format.
        /// </summary>
        private static Dictionary<string, AnimationClip> ProcessAnimations(
            AnimationContentDictionary animations, IList<BoneContent> bones, List<Matrix> bindPose)
        {
            // Build up a table mapping bone names to indices.
            Dictionary<string, int> boneMap = new Dictionary<string, int>();

            for (int i = 0; i < bones.Count; i++)
            {
                string boneName = bones[i].Name;

                if (!string.IsNullOrEmpty(boneName))
                    boneMap.Add(boneName, i);
            }

            // Convert each animation in turn.
            Dictionary<string, AnimationClip> animationClips;
            animationClips = new Dictionary<string, AnimationClip>();

            foreach (KeyValuePair<string, AnimationContent> animation in animations)
            {
                AnimationClip processed = ProcessAnimation(animation.Value, boneMap,bones.Count,bindPose);
                
                animationClips.Add(animation.Key, processed);
            }

            if (animationClips.Count == 0)
            {
                throw new InvalidContentException(
                            "Input file does not contain any animations.");
            }

            return animationClips;
        }


        /// <summary>
        /// Converts an intermediate format content pipeline AnimationContent
        /// object to our runtime AnimationClip format.
        /// </summary>
        private static AnimationClip ProcessAnimation(AnimationContent animation,
                                              Dictionary<string, int> boneMap, int numBones, List<Matrix> bindPose)
        {
            List<Keyframe> keyframes = new List<Keyframe>();

            // For each input animation channel.
            foreach (KeyValuePair<string, AnimationChannel> channel in
                animation.Channels)
            {
                // Look up what bone this channel is controlling.
                int boneIndex;

                if (!boneMap.TryGetValue(channel.Key, out boneIndex))
                {
                    throw new InvalidContentException(string.Format(
                        "Found animation for bone '{0}', " +
                        "which is not part of the skeleton.", channel.Key));
                }

                Vector3 scale;
                Vector3 position;
                Quaternion rotation;

                // Convert the keyframe data.
                foreach (AnimationKeyframe keyframe in channel.Value)
                {
                    keyframe.Transform.Decompose(out scale,out rotation,out position);

                    keyframes.Add(new Keyframe(boneIndex, keyframe.Time.Ticks,
                                               keyframe.Transform,rotation,position));
                }
            }

            if (keyframes.Count == 0)
                throw new InvalidContentException("Animation has no keyframes.");

            if (animation.Duration <= TimeSpan.Zero)
                throw new InvalidContentException("Animation has a zero duration.");

            keyframes.Sort(new SortKeys());
            AllTransformsSetOnTheFirstFrame(keyframes, numBones, bindPose);
            keyframes.Sort(new SortKeys());
            AllTransformsSetOnTheLastFrame(keyframes, numBones, bindPose);
            keyframes.Sort(new SortKeys());

            return new AnimationClip(animation.Duration.Ticks, keyframes.ToArray(),animation.Name);
        }

        /// <summary>
        /// We want to know that there is a keyframe for every bone on the last frame of the animation.
        /// </summary>
        private static void AllTransformsSetOnTheLastFrame(List<Keyframe> keyframes, int numBones, List<Matrix> bindPose)
        {
            long compareTime = keyframes[keyframes.Count - 1].Time;
            List<int> bones = new List<int>();

            for (int i = 0; i < keyframes.Count; i++)
            {
                if (keyframes[i].Time == compareTime)
                {
                    if (!bones.Contains(keyframes[i].Bone))
                    {
                        bones.Add(keyframes[i].Bone);
                    }
                }
            }


            if (bones.Count < numBones)
            {
                for (int i = 0; i < numBones; i++)
                {
                    if (!bones.Contains(i))
                    {
                        Keyframe k = BackTrackAnim(keyframes,i);
                        if (k != null)
                        {
                            keyframes.Add(new Keyframe(i, compareTime, k.Transform, k.Rotation, k.Position));
                        }
                        else
                        {
                            Vector3 pos;
                            Vector3 scale;
                            Quaternion rotation;
                            bindPose[i].Decompose(out scale, out rotation, out pos);
                            keyframes.Add(new Keyframe(i, compareTime, bindPose[i], rotation, pos));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// We want to know that there is a keyframe for every bone on the first frame of the animation.
        /// </summary>
        private static void AllTransformsSetOnTheFirstFrame(List<Keyframe> keyframes, int numBones, List<Matrix> bindPose)
        {
            long compareTime = keyframes[0].Time;

            List<int> bones = new List<int>();
            bones.Add(keyframes[0].Bone);

            for (int i = 0; i < keyframes.Count; i++)
            {
                if (keyframes[i].Time == compareTime)
                {
                    if (bones.Contains(keyframes[i].Bone))
                    {
                        continue;
                    }
                    else
                    {
                        bones.Add(keyframes[i].Bone);
                    }
                }
            }

            if (bones.Count < numBones)
            {
                for (int i = 0; i < numBones; i++)
                {
                    if (bones.Contains(i))
                    {
                        continue;
                    }
                    Vector3 pos;
                    Vector3 scale;
                    Quaternion rotation;
                    bindPose[i].Decompose(out scale, out rotation, out pos);
                    keyframes.Add(new Keyframe(i, compareTime, bindPose[i], rotation, pos));
                }
            }
        }

        /// <summary>
        /// Get the closest key to the end of the animation that referes to the boneNum bone.
        /// </summary>
        private static Keyframe BackTrackAnim(List<Keyframe> keyframes,int boneNum)
        {
            for (int i = (keyframes.Count - 1); i > -1; i--)
            {
                if (keyframes[i].Bone == boneNum)
                {
                    return keyframes[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Comparison function for sorting keyframes into ascending time order.
        /// </summary>
        private class SortKeys : IComparer<Keyframe>
        {
            public int Compare(Keyframe x, Keyframe y)
            {
                if (x.Time > y.Time)
                {
                    return 1;
                }
                if (x.Time < y.Time)
                {
                    return -1;
                }
                return 0;
            }
        }

        /// <summary>
        /// Makes sure this mesh contains the kind of data we know how to animate.
        /// </summary>
        static void ValidateMesh(NodeContent node, ContentProcessorContext context,
                                 string parentBoneName)
        {
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                // Validate the mesh.
                if (parentBoneName != null)
                {
                    context.Logger.LogWarning(null, null,
                        "Mesh {0} is a child of bone {1}. SkinnedModelProcessor " +
                        "does not correctly handle meshes that are children of bones.",
                        mesh.Name, parentBoneName);
                }

                if (!MeshHasSkinning(mesh))
                {
                    context.Logger.LogWarning(null, null,
                        "Mesh {0} has no skinning information, so it has been deleted.",
                        mesh.Name);

                    mesh.Parent.Children.Remove(mesh);
                    return;
                }
            }
            else if (node is BoneContent)
            {
                // If this is a bone, remember that we are now looking inside it.
                parentBoneName = node.Name;
            }

            // Recurse (iterating over a copy of the child collection,
            // because validating children may delete some of them).
            foreach (NodeContent child in new List<NodeContent>(node.Children))
                ValidateMesh(child, context, parentBoneName);
        }

        /// <summary>
        /// Checks whether a mesh contains skininng information.
        /// </summary>
        static bool MeshHasSkinning(MeshContent mesh)
        {
            foreach (GeometryContent geometry in mesh.Geometry)
            {
                if (!geometry.Vertices.Channels.Contains(VertexChannelNames.Weights()))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Bakes unwanted transforms into the model geometry,
        /// so everything ends up in the same coordinate system.
        /// </summary>
        static void FlattenTransforms(NodeContent node, BoneContent skeleton)
        {
            foreach (NodeContent child in node.Children)
            {
                // Don't process the skeleton, because that is special.
                if (child == skeleton)
                    continue;

                // Bake the local transform into the actual geometry.
                MeshHelper.TransformScene(child, child.Transform);

                // Having baked it, we can now set the local
                // coordinate system back to identity.
                child.Transform = Matrix.Identity;

                // Recurse.
                FlattenTransforms(child, skeleton);
            }
        }

        #endregion

        protected override MaterialContent ConvertMaterial(MaterialContent material, ContentProcessorContext context)
        {
            return null;
        }

        /// <summary>
        /// Helper for extracting a list of all the vertex positions in a model.
        /// </summary>
        protected void FindVertices(NodeContent node)
        {
            // Is this node a mesh?
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                // Look up the absolute transform of the mesh.
                Matrix absoluteTransform = mesh.AbsoluteTransform;

                // Loop over all the pieces of geometry in the mesh.
                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    // Loop over all the indices in this piece of geometry.
                    // Every group of three indices represents one triangle.
                    foreach (int index in geometry.Indices)
                    {
                        // Look up the position of this vertex.
                        Vector3 vertex = geometry.Vertices.Positions[index];

                        // Transform from local into world space.
                        //vertex = Vector3.Transform(vertex, absoluteTransform);

                        //vertex = Vector3.Transform(vertex, Matrix.CreateScale(this.Scale));
                        //Matrix rotation = Matrix.CreateRotationZ(MathHelper.ToRadians(RotationZ)) * Matrix.CreateRotationX(MathHelper.ToRadians(RotationX)) * Matrix.CreateRotationY(MathHelper.ToRadians(RotationY));
                        //vertex = Vector3.Transform(vertex, rotation);

                        // Store this vertex.
                        verts.Add(vertex);
                    }
                }
            }

            // Recursively scan over the children of this node.
            foreach (NodeContent child in node.Children)
            {
                FindVertices(child);
            }
        }

        protected void RemoveDefaultEffect(NodeContent node)
        {
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                for (int i = 0; i < mesh.Geometry.Count; i++)
                {
                    mesh.Geometry[i].Material = null;
                }
            }

            for (int i = 0; i < node.Children.Count; i++)
            {
                RemoveDefaultEffect(node.Children[i]);
            }
        }

        protected void AddExtraUvChannel(NodeContent node, ContentProcessorContext context)
        {
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                for (int i = 0; i < mesh.Geometry.Count; i++)
                {
                    if (!(mesh.Geometry[i].Vertices.Channels.Contains("TextureCoordinate1")))
                    {
                        List<Vector2> data = new List<Vector2>(mesh.Geometry[i].Vertices.VertexCount);
                        for (int k = 0; k < mesh.Geometry[i].Vertices.VertexCount; k++)
                        {
                            data.Add(Vector2.Zero);
                        }
                        mesh.Geometry[i].Vertices.Channels.Add("TextureCoordinate1", data);
                    }
                }
            }

            for (int i = 0; i < node.Children.Count; i++)
            {
                AddExtraUvChannel(node.Children[i], context);
            }
        }

        /// <summary>
        /// Return the center in a list of vertrs.
        /// </summary>
        protected Vector3 GetVertsCenter(List<Vector3> verts)
        {
            Vector3 result = new Vector3();
            for (int i = 0; i < verts.Count; i++)
            {
                result += verts[i];
            }
            result /= verts.Count;
            return result;
        }

        private void GetModelVertices(NodeContent node, List<Vector3> vertexList)
        {
            MeshContent meshContent = node as MeshContent;
            if (meshContent != null)
            {
                for (int i = 0; i < meshContent.Geometry.Count; i++)
                {
                    GeometryContent geometryContent = meshContent.Geometry[i];
                    for (int j = 0; j < geometryContent.Vertices.Positions.Count; j++)
                        vertexList.Add(geometryContent.Vertices.Positions[j]);
                }
            }

            foreach (NodeContent child in node.Children)
                GetModelVertices(child, vertexList);
        }


    }
}
