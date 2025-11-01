using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.InteropServices;

namespace FrogBattle.UI
{
    public class StaticMenu : SelectableMenu
    {
        private List<Sprite> boxes = [];
        private List<Point> boxLocations = [];

        /// <summary>
        /// Create and initialise a static selectable menu.
        /// </summary>
        /// <param name="menuSize">The size (in rows and columns) of the menu.</param>
        /// <param name="box_texture">The texture of each box.</param>
        /// <param name="box_spacing">Horizontal and vertical spacing in pixels between each box texture.</param>
        /// <param name="cursor_texture">Texture that is overlayed / displayed alongside the selected box.</param>
        /// <param name="cursor_offset">Offset of the cursor location from the box location.</param>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name="menuSize"/> has non-positive coordinates.</exception>
        public StaticMenu(Point location, Point menuSize, Texture2D box_texture, Point box_spacing, Texture2D cursor_texture, Point cursor_offset) : base(location, menuSize, box_texture, box_spacing, cursor_texture, cursor_offset)
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    // Create a static list of the locations of each box, relative to the menu's location.
                    boxLocations[i * Columns + j] = new Point(
                        // Width + distance between the rightmost bound of a box and the leftmost bound of the next = horizontal distance between each box location
                        (boxTexture.Width + boxSpacing.X) * j,
                        // Height + distance between the lower bound of a box and the upper bound of the next = vertical distance between each box location
                        (boxTexture.Height + boxSpacing.Y) * i);
                }
            }
        }
        public override Sprite this[int number] => boxes[number];

        private void GenerateBoxes()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    // Create a static list of sprites (to be treated as a matrix) by combining the menu's global location, texture size and spacing.
                    boxes[i * Columns + j] = new(boxTexture, Location + new Point(
                        // Width + distance between the rightmost bound of a box and the leftmost bound of the next = horizontal distance between each box location
                        (boxTexture.Width + boxSpacing.X) * j,
                        // Height + distance between the lower bound of a box and the upper bound of the next = vertical distance between each box location
                        (boxTexture.Height + boxSpacing.Y) * i));
                }
            }
        }
    }
}
