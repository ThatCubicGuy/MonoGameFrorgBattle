using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.UI
{
    public class DynamicMenu : SelectableMenu
    {
        public DynamicMenu(Point location, Point menuSize, Texture2D box_texture, Point box_spacing, Texture2D cursor_texture, Point cursor_offset) : base(location, menuSize, box_texture, box_spacing, cursor_texture, cursor_offset)
        {
        }

        public override Sprite this[int number] => GetBox(number / Columns, number % Columns);

        private Sprite GetBox(int row, int column)
        {
            return new(boxTexture, Location + new Point(
                        // Width + distance between the rightmost bound of a box and the leftmost bound of the next = horizontal distance between each box location
                        (boxTexture.Width + boxSpacing.X) * column,
                        // Height + distance between the lower bound of a box and the upper bound of the next = vertical distance between each box location
                        (boxTexture.Height + boxSpacing.Y) * row));
        }
    }
}
