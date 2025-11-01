using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FrogBattle.UI
{
    public abstract class SelectableMenu
    {
        protected Texture2D boxTexture;
        protected Texture2D cursorTexture;
        protected Point boxSpacing;
        protected Point cursorOffset;
        protected Point cursorIndex;

        public SelectableMenu(Point location, Point menuSize, Texture2D box_texture, Point box_spacing, Texture2D cursor_texture, Point cursor_offset)
        {
            if (menuSize.X <= 0 || menuSize.Y <= 0) throw new System.ArgumentException("Cannot create a menu with less than 1 box!", nameof(menuSize));
            Location = location;
            MenuSize = menuSize;
            boxTexture = box_texture;
            boxSpacing = box_spacing;
            cursorTexture = cursor_texture;
            cursorOffset = cursor_offset;
        }

        public Sprite Cursor => new(cursorTexture, this[cursorIndex.X, cursorIndex.Y].position.Location + cursorOffset);
        /// <summary>
        /// Represents the size of the menu in rows and columns, not in pixels.
        /// </summary>
        public Point MenuSize { get; }
        public int Rows => MenuSize.X;
        public int Columns => MenuSize.Y;
        public int BoxCount => MenuSize.X * MenuSize.Y;
        public Point Location { get; }
        public Rectangle Bounds => new(Location, new(boxTexture.Width * Columns + boxSpacing.X * (Columns - 1), boxTexture.Height * Rows + boxSpacing.Y * (Rows - 1)));
        public abstract Sprite this[int number] { get; }
        public virtual Sprite this[int row, int column] => this[row * Columns + column];
        public virtual Sprite this[Point location] => this[location.X, location.Y];
    }
}
