using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

namespace FrogBattle.UI
{
    public class SelectableTable : IMenuItem, IEnumerable
    {
        private readonly Point[] boxLocations;
        private readonly Sprite[] boxes;
        private readonly Texture2D boxTexture;
        private readonly Texture2D cursorTexture;
        private readonly Point boxSpacing;
        private readonly Point cursorOffset;
        private Point selectedItem;
        private Point location;

        public SelectableTable(Point location, Point menuSize, Texture2D box_texture, Point box_spacing, Texture2D cursor_texture, Point cursor_offset)
        {
            if (menuSize.X <= 0 || menuSize.Y <= 0) throw new System.ArgumentException("Cannot create a menu with less than 1 box!", nameof(menuSize));
            this.location = location;
            TableSize = menuSize;
            boxTexture = box_texture;
            boxSpacing = box_spacing;
            cursorTexture = cursor_texture;
            cursorOffset = cursor_offset;
            selectedItem = Point.Zero;
            boxLocations = new Point[BoxCount];
            boxes = new Sprite[BoxCount];
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

        public Sprite Cursor => new(cursorTexture, this[selectedItem].position.Location + cursorOffset);
        public int CursorRow
        {
            get => selectedItem.X;
            set
            {
                selectedItem.X = value;
                if (value > Rows - 1) selectedItem.X = 0;
                if (value < 0) selectedItem.X = Rows - 1;
            }
        }
        public int CursorColumn
        {
            get => selectedItem.Y;
            set
            {
                selectedItem.Y = value;
                if (value > Columns - 1) selectedItem.Y = 0;
                if (value < 0) selectedItem.Y = Columns - 1;
            }
        }
        public int SelectedItemID
        {
            get => CursorRow * Columns + CursorColumn;
            set
            {
                CursorRow = value / Columns;
                CursorColumn = value % Columns;
            }
        }
        /// <summary>
        /// Represents the size of the table in rows and columns, not in pixels.
        /// </summary>
        public Point TableSize { get; }
        public int Rows => TableSize.X;
        public int Columns => TableSize.Y;
        public int BoxCount => TableSize.X * TableSize.Y;
        public Point Location
        {
            get => location;
            set
            {
                location = value;
                GenerateBoxes();
            }
        }
        public Rectangle Bounds => new(Location, new(boxTexture.Width * Columns + boxSpacing.X * (Columns - 1), boxTexture.Height * Rows + boxSpacing.Y * (Rows - 1)));
        public Sprite this[int number] => boxes[number];
        public Sprite this[int row, int column] => this[row * Columns + column];
        public Sprite this[Point location] => this[location.X, location.Y];

        public void OffsetLocation(int x_offset, int y_offset) => OffsetLocation(new(x_offset, y_offset));
        public void OffsetLocation(Point offset)
        {
            Location += offset;
        }

        private void GenerateBoxes()
        {
            for (int i = 0; i < BoxCount; i++)
            {
                boxes[i] = new(boxTexture, Location + boxLocations[i]);
            }
        }

        public IEnumerator GetEnumerator()
        {
            return boxes.GetEnumerator();
        }
    }
}
