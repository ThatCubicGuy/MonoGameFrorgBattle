using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FrogBattle.UI
{
    internal record class SelectableCell(Texture2D Texture, Texture2D CursorTexture) : ISelectableCell
    {
        public Point Size => Texture.Bounds.Size;
        public Point CursorSize => CursorTexture.Bounds.Size;
        public virtual void Draw(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.Draw(Texture, bounds, Color.White);
        }
        public virtual void DrawCursorOverlay(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.Draw(CursorTexture, bounds, Color.White);
        }
    }
}
