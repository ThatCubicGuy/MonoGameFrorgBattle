using FrogBattle.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FrogBattle.UI.State;

public interface IUIState
{
    void HandleInput(InputTypes input);
    void Update(GameTime gameTime);
    void Draw(SpriteBatch spriteBatch);
}