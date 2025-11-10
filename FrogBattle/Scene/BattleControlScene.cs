using FrogBattle.Classes;
using FrogBattle.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Collections.Generic;

namespace FrogBattle.Scene
{
    internal class BattleControlScene : IGameScene
    {
        private readonly BattleManager _battle;
        private readonly Texture2D _abilityMenuCellTexture;
        private readonly Texture2D _abilityMenuCursorTexture;
        private readonly SelectableTable _abilityMenu;
        private BattleUIState uiState;

        public BattleControlScene(ContentManager contentManager, BattleManager battleManager)
        {
            _battle = battleManager;
            _abilityMenuCellTexture = contentManager.Load<Texture2D>("UI/AbilityMenu/Cell");
            _abilityMenuCursorTexture = contentManager.Load<Texture2D>("UI/AbilityMenu/Cursor");
        }
        public Character ActiveCharacter { get; set; }

        public void Update(GameTime gameTime)
        {
            throw new System.NotImplementedException();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator<IGameEntity> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public enum BattleUIState
        {
            SelectingAbility,
            SelectingTarget,
            ProcessingAbility
        }
    }
}
