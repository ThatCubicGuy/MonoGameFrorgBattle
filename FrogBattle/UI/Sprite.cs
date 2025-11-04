using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FrogBattle.UI
{
    public class Sprite
    {
        public Texture2D texture;
        public Rectangle position;
        public Sprite(Texture2D texture, Rectangle position)
        {
            this.texture = texture;
            this.position = position;
        }
        public Sprite(Texture2D texture, Point position) : this(texture, new Rectangle(position, texture.Bounds.Size)) { }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, Color.White);
        }
    }

    public class ColoredSprite : Sprite
    {
        public Color color;
        public ColoredSprite(Texture2D texture, Rectangle position, Color color) : base(texture, position)
        {
            this.color = color;
        }
        public ColoredSprite(Texture2D texture, Point position, Color color) : this(texture, new Rectangle(position, texture.Bounds.Size), color) { }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, color);
        }
    }
}
