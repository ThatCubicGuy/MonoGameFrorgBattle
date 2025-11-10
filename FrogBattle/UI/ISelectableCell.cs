using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace FrogBattle.UI
{
    public interface ISelectableCell
    {
        void Draw(SpriteBatch spriteBatch, Point location);
        void DrawCursorOverlay(SpriteBatch spriteBatch, Point location);
    }
}
