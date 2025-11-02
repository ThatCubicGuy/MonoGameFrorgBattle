using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace FrogBattle.UI
{
    public abstract class SelectableTable
    {
        private Texture2D boxTexture;
        private Texture2D cursorTexture;
        private Point boxSpacing;
        private Point cursorOffset;
        private Point cursorIndex;
        private readonly List<Sprite> boxes = [];
        private readonly List<Point> boxLocations = [];

        public SelectableTable(Point location, Point menuSize, Texture2D box_texture, Point box_spacing, Texture2D cursor_texture, Point cursor_offset)
        {
            if (menuSize.X <= 0 || menuSize.Y <= 0) throw new System.ArgumentException("Cannot create a menu with less than 1 box!", nameof(menuSize));
            Location = location;
            TableSize = menuSize;
            boxTexture = box_texture;
            boxSpacing = box_spacing;
            cursorTexture = cursor_texture;
            cursorOffset = cursor_offset;
            cursorIndex = Point.Zero;
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    // Create a static list of the locations of each box, relative to the table's location.
                    boxLocations[i * Columns + j] = new Point(
                        // Width + distance between the rightmost bound of a box and the leftmost bound of the next = horizontal distance between each box location
                        (boxTexture.Width + boxSpacing.X) * j,
                        // Height + distance between the lower bound of a box and the upper bound of the next = vertical distance between each box location
                        (boxTexture.Height + boxSpacing.Y) * i);
                }
            }
            GenerateBoxes();
        }

        public Sprite Cursor => new(cursorTexture, this[cursorIndex].position.Location + cursorOffset);
        public int CursorRow
        {
            get => cursorIndex.X;
            set
            {
                if (value > Rows) cursorIndex.X = 0;
                if (value < 0) cursorIndex.X = Rows;
                cursorIndex.X = value;
            }
        }
        public int CursorColumn
        {
            get => cursorIndex.Y;
            set
            {
                if (value > Columns) cursorIndex.Y = 0;
                if (value < 0) cursorIndex.Y = Columns;
                cursorIndex.Y = value;
            }
        }
        /// <summary>
        /// Represents the size of the table in rows and columns, not in pixels.
        /// </summary>
        public Point TableSize { get; }
        public int Rows => TableSize.X;
        public int Columns => TableSize.Y;
        public int BoxCount => TableSize.X * TableSize.Y;
        public Point Location { get; }
        public Rectangle Bounds => new(Location, new(boxTexture.Width * Columns + boxSpacing.X * (Columns - 1), boxTexture.Height * Rows + boxSpacing.Y * (Rows - 1)));
        public Sprite this[int number] => boxes[number];
        public Sprite this[int row, int column] => this[row * Columns + column];
        public Sprite this[Point location] => this[location.X, location.Y];

        private void GenerateBoxes()
        {
            for (int i = 0; i < BoxCount; i++)
            {
                boxes[i] = new(boxTexture, Location + boxLocations[i]);
            }
        }
    }
}
