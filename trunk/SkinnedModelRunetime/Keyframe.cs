using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace AlphaSubmarines
{
    /// <summary>
    /// Describes the position of a single bone at a single point in time.
    /// </summary>
    public class Keyframe
    {
        /// <summary>
        /// Constructs a new keyframe object.
        /// </summary>
        public Keyframe(int bone, long time, Matrix transform,Quaternion rotation,Vector3 position)
        {
            Bone = bone;
            Time = time;
            Transform = transform;
            Position = position;
            Rotation = rotation;
        }


        /// <summary>
        /// Private constructor for use by the XNB deserializer.
        /// </summary>
        private Keyframe()
        {
        }


        /// <summary>
        /// Gets the index of the target bone that is animated by this keyframe.
        /// </summary>
        [ContentSerializer]
        public int Bone;


        /// <summary>
        /// Gets the time offset from the start of the animation to this keyframe.
        /// </summary>
        [ContentSerializer]
        public long Time;


        /// <summary>
        /// Gets the bone transform for this keyframe.
        /// </summary>
        [ContentSerializer]
        public Matrix Transform;

        /// <summary>
        /// Gets the bone position for this keyframe.
        /// </summary>
        [ContentSerializer]
        public Vector3 Position;

        /// <summary>
        /// Gets the bone rotation for this keyframe.
        /// </summary>
        [ContentSerializer]
        public Quaternion Rotation;
    }
}
