using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Extinction
{
    public partial class ExtinctionGame : Microsoft.Xna.Framework.Game
    {
        private static Vector2 lastMouse = Vector2.Zero;
        private static Vector2 deltaMouse = Vector2.Zero;

        private static Keys[] keysPressed;

        public static Vector2 MouseDelta
        {
            get
            {
                return deltaMouse;
            }
        }

        public static bool IsKeyPressed(Keys k)
        {

            if (keysPressed != null)
                foreach (Keys i in keysPressed)
                    if (i == k)
                        return Keyboard.GetState().IsKeyUp(k);

            return false;
        }

        private static void UpdateInput()
        {

            MouseState state = Mouse.GetState();

            deltaMouse.X = state.X - lastMouse.X;
            deltaMouse.Y = state.Y - lastMouse.Y;

            lastMouse.X = state.X;
            lastMouse.Y = state.Y;

            KeyboardState keyboard = Keyboard.GetState();
            keysPressed = keyboard.GetPressedKeys();
        }
    }
}