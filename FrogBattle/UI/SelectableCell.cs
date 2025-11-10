using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FrogBattle.UI
{
    internal record class SelectableCell(Texture2D Texture) : ISelectableCell
    {
        public void Draw(SpriteBatch spriteBatch, Point location)
        {
            spriteBatch.Draw(Texture, new Rectangle(location, Texture.Bounds.Size), Color.White);
        }
    }
}
