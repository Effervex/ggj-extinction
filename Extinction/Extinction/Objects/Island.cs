using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Extinction.Objects
{
    public class Island : Entity
    {

        // To keep things efficient, the picking works by first applying a bounding
        // sphere test, and then only bothering to test each individual triangle
        // if the ray intersects the bounding sphere. This allows us to trivially
        // reject many models without even needing to bother looking at their triangle
        // data. This field keeps track of which models passed the bounding sphere
        // test, so you can see the difference between this approximation and the more
        // accurate triangle picking.
        List<Model> insideBoundingSpheres = new List<Model>();

        // Store the name of the model underneath the cursor (or null if there is none).
        Model pickedModel;

        // Vertex array that stores exactly which triangle was picked.
        VertexPositionColor[] pickedTriangle =
        {
            new VertexPositionColor(Vector3.Zero, Color.Magenta),
            new VertexPositionColor(Vector3.Zero, Color.Magenta),
            new VertexPositionColor(Vector3.Zero, Color.Magenta),
        };

        // Effect and vertex declaration for drawing the picked triangle.
        BasicEffect lineEffect;

        // Custom rasterizer state for drawing in wireframe.
        static RasterizerState WireFrame = new RasterizerState
        {
            FillMode = FillMode.WireFrame,
            CullMode = CullMode.None
        };


        /// <summary>
        /// Runs a per-triangle picking algorithm over all the models in the scene,
        /// storing which triangle is currently under the cursor.
        /// </summary>
        void UpdatePicking()
        {
            // Look up a collision ray based on the current cursor position. See the
            // Picking Sample documentation for a detailed explanation of this.
            Ray cursorRay = ExtinctionGame.instance.cursor.CalculateCursorRay(ExtinctionGame.projection, ExtinctionGame.view);

            // Clear the previous picking results.
            insideBoundingSpheres.Clear();

            pickedModel = null;

            // Keep track of the closest object we have seen so far, so we can
            // choose the closest one if there are several models under the cursor.
            float closestIntersection = float.MaxValue;

            bool insideBoundingSphere;
            Vector3 vertex1, vertex2, vertex3;

            // Perform the ray to model intersection test.
            float? intersection = Picking.RayIntersectsModel(cursorRay, model,
                                                        world,
                                                        out insideBoundingSphere,
                                                        out vertex1, out vertex2,
                                                        out vertex3);

            // If this model passed the initial bounding sphere test, remember
            // that so we can display it at the top of the screen.
            if (insideBoundingSphere)
                insideBoundingSpheres.Add(model);

            // Do we have a per-triangle intersection with this model?
            if (intersection != null)
            {
                // If so, is it closer than any other model we might have
                // previously intersected?
                if (intersection < closestIntersection)
                {
                    // Store information about this model.
                    closestIntersection = intersection.Value;

                    pickedModel = model;

                    // Store vertex positions so we can display the picked triangle.
                    pickedTriangle[0].Position = vertex1;
                    pickedTriangle[1].Position = vertex2;
                    pickedTriangle[2].Position = vertex3;

                    //System.Console.WriteLine("x: " + vertex1.X + ", y: " + vertex1.Y + ", z: " + vertex1.Z + "\n");
                }
            }

        }


        public override void Draw()
        {

            // Draw the table.
            //DrawModel(table, Matrix.Identity, tableAbsoluteBoneTransforms);
            ExtinctionGame.SetState_DepthWrite();
            ExtinctionGame.SetState_NoCull();
            //ExtinctionGame.DrawModel(model, world);

            // Draw the outline of the triangle under the cursor.
            //DrawPickedTriangle();

            //base.Draw();

        }


        /// <summary>
        /// Helper for drawing the outline of the triangle currently under the cursor.
        /// </summary>
        void DrawPickedTriangle()
        {
            if (pickedModel != null)
            {
                GraphicsDevice device = ExtinctionGame.instance.GraphicsDevice;
                // create the effect and vertex declaration for drawing the
                // picked triangle.
                lineEffect = new BasicEffect(device);
                lineEffect.VertexColorEnabled = true;

                // Set line drawing renderstates. We disable backface culling
                // and turn off the depth buffer because we want to be able to
                // see the picked triangle outline regardless of which way it is
                // facing, and even if there is other geometry in front of it.
                device.RasterizerState = WireFrame;
                device.DepthStencilState = DepthStencilState.None;
                
                // Activate the line drawing BasicEffect.
                lineEffect.Projection = ExtinctionGame.projection;
                lineEffect.View = ExtinctionGame.view;

                lineEffect.CurrentTechnique.Passes[0].Apply();

                // Draw the triangle.
                device.DrawUserPrimitives(PrimitiveType.TriangleList,
                                          pickedTriangle, 0, 1);

                // Reset renderstates to their default values.
                device.RasterizerState = RasterizerState.CullCounterClockwise;
                device.DepthStencilState = DepthStencilState.Default;
            }
        }


    }

}
