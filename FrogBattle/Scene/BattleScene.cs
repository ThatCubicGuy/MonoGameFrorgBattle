using FrogBattle.Classes;
using FrogBattle.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Collections;
using FrogBattle.Input;
using FrogBattle.UI.State;

namespace FrogBattle.Scene
{
	internal class BattleScene : IGameScene, IEnumerable<IGameEntity>
	{
		private readonly BattleManager _battle;
		private readonly Game _game;
		private readonly Texture2D _abilityMenuCellTexture;
		private readonly Texture2D _abilityMenuCursorTexture;
		private readonly SelectableTable _abilityMenu;
		private IUIState uiState;

		public BattleScene(FrogBattle game)
		{
			//_battle = new BattleManager();
			_game = game;
			_abilityMenuCellTexture = _game.Content.Load<Texture2D>("UI/AbilityMenu/Cell");
			_abilityMenuCursorTexture = _game.Content.Load<Texture2D>("UI/AbilityMenu/Cursor");
			ISelectableCell[] selectableCells =
			[
				new SelectableCell(_abilityMenuCellTexture, _abilityMenuCursorTexture),
                new SelectableCell(_abilityMenuCellTexture, _abilityMenuCursorTexture),
                new SelectableCell(_abilityMenuCellTexture, _abilityMenuCursorTexture),
                new SelectableCell(_abilityMenuCellTexture, _abilityMenuCursorTexture),
                new SelectableCell(_abilityMenuCellTexture, _abilityMenuCursorTexture),
                new SelectableCell(_abilityMenuCellTexture, _abilityMenuCursorTexture)
			];
			var emptyCell = new SelectableCell(_game.Content.Load<Texture2D>("Big_chungus"), _game.Content.Load<Texture2D>("raiden-hd"));
			_abilityMenu = new SelectableTable(selectableCells, new Point(4, 3), Point.Zero, _abilityMenuCellTexture.Bounds.Size, new Point(20, 20));
		}
		public Character ActiveCharacter { get; set; }

		public void Update(GameTime gameTime)
		{
			uiState.Update(gameTime);
        }

		public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
		{
			uiState.Draw(spriteBatch);
		}

		public IEnumerator<IGameEntity> GetEnumerator()
		{
			yield return _abilityMenu;
			yield break;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
