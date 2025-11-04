using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FrogBattle.UI
{
    internal class TextBox
    {
        private Sprite boxSprite;
        public TextBox(Sprite boxSprite)
        {
            this.boxSprite = boxSprite;
        }
        public virtual string Text { get; set; }
        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            spriteBatch.Draw(boxSprite.texture, boxSprite.position, Color.White);
            spriteBatch.DrawString(font, Text, boxSprite.position.Location.ToVector2(), Color.White);
        }
    }
}
