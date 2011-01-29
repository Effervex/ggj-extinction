using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace AlphaSubmarines
{
    /// <summary>
    /// An animation clip is the runtime equivalent of the
    /// Microsoft.Xna.Framework.Content.Pipeline.Graphics.AnimationContent type.
    /// It holds all the keyframes needed to describe a single animation.
    /// </summary>
    public class AnimationClip
    {
        /// <summary>
        /// Constructs a new animation clip object.
        /// </summary>
        public AnimationClip(long duration, Keyframe[] keyframes,string name)
        {
            this.name = name;
            Duration = duration;
            Keyframes = keyframes;
        }

        /// <summary>
        /// Private constructor for use by the XNB deserializer.
        /// </summary>
        private AnimationClip()
        {
        }

        /// <summary>
        /// Gets the total length of the animation.
        /// </summary>
        [ContentSerializer]
        public long Duration;

        /// <summary>
        /// Number of ticks/second
        /// </summary>
        [ContentSerializer]
        private long frameRate = 0;

        [ContentSerializer]
        private string name;

        /// <summary>
        /// Name for this clip.
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// The framerate for this clip.
        /// </summary>
        [ContentSerializerIgnore]
        public long FrameRate
        {
            get
            {
                return this.frameRate;
            }
        }

        /// <summary>
        /// Gets a combined list containing all the keyframes for all bones,
        /// sorted by time.
        /// </summary>
        [ContentSerializer]
        public Keyframe[] Keyframes;

        /// <summary>
        /// Get an animation with a lenght of 0 that places the charachter in it's no-animation pose.
        /// </summary>
        public static AnimationClip GetEmptyClip(SkinningData data)
        {
            AlphaSubmarines.AnimationClip a = new AlphaSubmarines.AnimationClip(0, new AlphaSubmarines.Keyframe[0], "Empty");
            a.Keyframes = new AlphaSubmarines.Keyframe[data.BoneNames.Length];
            for (int i = 0; i < data.BoneNames.Length; i++)
            {
                Matrix localTransform = (data.BindPose[i]);
                Vector3 position;
                Vector3 scale;
                Quaternion rotation;
                localTransform.Decompose(out scale, out rotation, out position);

                a.Keyframes[i] = new AlphaSubmarines.Keyframe(i, 0, localTransform, rotation, position);
            }
            return a;
        }
    }
}
