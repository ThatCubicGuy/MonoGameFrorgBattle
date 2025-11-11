using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;

namespace FrogBattle.UI
{
    public class SelectableTable : IGameEntity, IEnumerable
    {
        /// <summary>
        /// An array containing the locations of each cell, relative to the table.
        /// </summary>
        private readonly Point[] boxLocations;
        private readonly ISelectableCell[] boxes;
        //private readonly Texture2D boxTexture;
        //private readonly Texture2D cursorTexture;
        private readonly Point boxSpacing;
        private Point selectedItem;

        public SelectableTable(ISelectableCell[] cells, Point location, Point menuSize, Point box_spacing)
        {
            if (menuSize.X <= 0 || menuSize.Y <= 0) throw new ArgumentException("Cannot create a menu with less than 1 box!", nameof(menuSize));
            Location = location;
            TableSize = menuSize;
            boxSpacing = box_spacing;
            Size = new(Columns + boxSpacing.X * (Columns - 1), Rows + boxSpacing.Y * (Rows - 1));
            selectedItem = Point.Zero;
            boxLocations = new Point[BoxCount];
            boxes = new ISelectableCell[cells.Length];
            Array.Copy(cells, boxes, cells.Length);
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    // Create a static list of the locations of each box, relative to the table's location.
                    boxLocations[i * Columns + j] = new Point(boxSpacing.X * j, boxSpacing.Y * i);
                }
            }
        }

        public ISelectableCell Cursor => boxes[SelectedItemID];
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
        public Point Location { get; set; }
        public Point Size { get; }
        public Rectangle Bounds => new(Location, Size);
        public ISelectableCell this[int number] => boxes[number];
        public ISelectableCell this[int row, int column] => this[row * Columns + column];
        public ISelectableCell this[Point location] => this[location.X, location.Y];

        public void OffsetLocation(int x_offset, int y_offset) => OffsetLocation(new(x_offset, y_offset));
        public void OffsetLocation(Point offset)
        {
            Location += offset;
        }
        public void MoveCursorLeft() => --CursorColumn;
        public void MoveCursorRight() => ++CursorColumn;
        public void MoveCursorUp() => --CursorRow;
        public void MoveCursorDown() => ++CursorRow;

        public void Draw(SpriteBatch spriteBatch)
        {
            for (uint i = 0; i < BoxCount; i++)
            {
                boxes[i].Draw(spriteBatch, Location + boxLocations[i]);
            }
            Cursor.DrawCursorOverlay(spriteBatch, Location + boxLocations[SelectedItemID]);
        }

        public IEnumerator GetEnumerator()
        {
            return boxes.GetEnumerator();
        }
    }
}
