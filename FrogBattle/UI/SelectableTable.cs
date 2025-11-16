using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;

namespace FrogBattle.UI
{
	public class SelectableTable : IGameEntity, IEnumerable<ISelectableCell>
	{
		/// <summary>
		/// An array containing the locations of each cell, relative to the table.
		/// </summary>
		private readonly ISelectableCell[] cells;
		private readonly Point[] cellLocations;
		private readonly Point cellSize;
		private readonly Point cellSpacing;
		private Point selectedItem;

		/// <summary>
		/// Creates a new selectable table with the given elements.
		/// </summary>
		/// <param name="cells">An <see cref="IEnumerable{T}"/> containing every <see cref="ISelectableCell"/> that this table should have.</param>
		/// <param name="defaultValue">The default value to fill the rest of the array of boxes with.</param>
		/// <param name="grid_size">The maximum capacity of the table (columns, rows).</param>
		/// <param name="location">The top-left coordinates of the table.</param>
		/// <param name="cell_size">The bounds within which each cell can draw itself.</param>
		/// <param name="cell_spacing">The (horizontal, vertical) distance in pixels between each cell rectangle.</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public SelectableTable(ISelectableCell[] baseCells, Point grid_size, Point location, Point cell_size, Point cell_spacing = new())
		{
			if (grid_size.X <= 0 || grid_size.Y <= 0) throw new ArgumentOutOfRangeException(nameof(grid_size), "Cannot create a menu with less than 1 box!");
            Location = location;
			GridSize = grid_size;
			cellSize = cell_size;
			cellSpacing = cell_spacing;
			Size = new(cellSize.X * Columns + cellSize.X * (Columns - 1), Rows + cellSize.Y * (Rows - 1));
			selectedItem = Point.Zero;
			cellLocations = new Point[MaximumCellCount];
			cells = new ISelectableCell[MaximumCellCount];
			SetCells(baseCells);
        }

		public ISelectableCell Cursor => cells[SelectedItemID];
		public int CursorRow
		{
			get => selectedItem.Y;
			set
			{
				selectedItem.Y = value;
				if (value > Rows - 1) selectedItem.Y = 0;
				if (value < 0) selectedItem.Y = Rows - 1;

                if (!IsCellValid(CursorRow, CursorColumn))
                {
                    --CursorRow;
                }
            }
		}
		public int CursorColumn
		{
			get => selectedItem.X;
			set
			{
				selectedItem.X = value;
				if (value > Columns - 1) selectedItem.X = 0;
				if (value < 0) selectedItem.X = Columns - 1;

				if (!IsCellValid(CursorRow, CursorColumn))
				{
					--CursorColumn;
				}
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
		public Point GridSize { get; }
		public int Rows => GridSize.Y;
		public int Columns => GridSize.X;
		public int MaximumCellCount => Rows * Columns;
		public int CellCount { get; private set; }
		public Point Location { get; set; }
		public Point Size { get; }
		public Rectangle Bounds => new(Location, Size);
		public ISelectableCell this[int number] => cells[number];
		public ISelectableCell this[int column, int row] => this[row * Columns + column];
		/// <summary>
		/// Get the cell at the (column, row) position given.
		/// </summary>
		/// <param name="location">X means columns, Y means rows.</param>
		/// <returns>An ISelectableCell whose coordinates in this table are equal to <paramref name="location"/>.</returns>
		public ISelectableCell this[Point location] => this[location.X, location.Y];
        
		private bool IsCellValid(int row, int col)
        {
            int index = row * Columns + col;
            return index >= 0 && index < CellCount && cells[index] != null;
        }

        public void OffsetLocation(int x_offset, int y_offset) => OffsetLocation(new(x_offset, y_offset));
		public void OffsetLocation(Point offset) => Location += offset;
        public void MoveCursorLeft() => --CursorColumn;
		public void MoveCursorRight() => ++CursorColumn;
		public void MoveCursorUp() => --CursorRow;
		public void MoveCursorDown() => ++CursorRow;

		public void SetCells(ISelectableCell[] newCells)
		{
            CellCount = newCells.Length;
			Array.Clear(cells);
			Array.Copy(newCells, cells, CellCount);
            for (int i = 0; i < CellCount; i++)
            {
                // Create a list of the locations of each box, relative to the table's location.
                cellLocations[i] = new Point((cellSize.X + cellSpacing.X) * (i % Columns), (cellSize.Y + cellSpacing.Y) * (i / Columns));
            }
        }

		public void Draw(SpriteBatch spriteBatch)
		{
			for (uint i = 0; i < cells.Length; i++)
			{
				cells[i].Draw(spriteBatch, new(Location + cellLocations[i], cellSize));
			}
			Cursor.DrawCursorOverlay(spriteBatch, new(Location + cellLocations[SelectedItemID], cellSize));
		}

		IEnumerator<ISelectableCell> IEnumerable<ISelectableCell>.GetEnumerator() => (IEnumerator<ISelectableCell>)GetEnumerator();

		public IEnumerator GetEnumerator()
		{
			return cells.GetEnumerator();
		}
	}
}
