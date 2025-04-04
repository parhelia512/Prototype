using System.Collections.Generic;
using Godot;
using C7GameData;
using C7Engine;
using System;

[Tool]
[GlobalClass]
public partial class ProductionMenu : Civ3TextureRect {
	private Dictionary<TreeItem, IProducible> itemMapping = new();

	Tree tree = new();
	Theme fontTheme = new();

	public ProductionMenu() {
		this.Texture = Util.LoadTextureFromPCX("Art/city screen/ProductionQueueBox.pcx");

		// Load the font we'll use.
		FontFile font = ResourceLoader.Load<FontFile>("res://Fonts/NotoSans-Regular.ttf", null, ResourceLoader.CacheMode.Ignore);
		font.FixedSize = 12;
		fontTheme.DefaultFont = font;

		// Set up the tree of items. We use a tree so we get a scroll bar and
		// other niceties.
		AddChild(tree);
		tree.Columns = 2;

		// Match the size of the texture used in the deal screen.
		tree.Size = new Vector2(203, 360);

		TradingTree.ConfigureTreeTheme(tree, fontTheme);
	}

	public void AddItems(City city, Action<IProducible> chooseProduction) {
		tree.Clear();

		TreeItem root = TradingTree.CreateTreeRoot(tree);

		foreach (IProducible option in city.ListProductionOptions()) {
			int buildTime = city.TurnsToProduce(option);

			TreeItem child = tree.CreateItem(root);
			string text = $"{option.name}";
			if (option is UnitPrototype proto) {
				text += $" {proto.attack}.{proto.defense}.{proto.movement}";
			}
			child.SetText(0, text);
			child.SetText(1, $"{buildTime} turns");
			child.SetIcon(0, RightClickChooseProductionMenu.GetProducibleIcon(option));
			itemMapping[child] = option;
		}

		// We only want clickable behavior, not selectable behavior.
		tree.ItemSelected += () => {
			TreeItem ti = tree.GetSelected();
			ti.Deselect(0);
			chooseProduction(itemMapping[ti]);
		};
	}

	private void AddItem(string text, System.Action action, Texture2D icon = null) {
		Button button = new Button();
		button.Text = text;
		if (icon != null) {
			button.Icon = icon;
		}
		button.Alignment = HorizontalAlignment.Left;
		if (action != null) {
			button.Pressed += action;
		}
		this.AddChild(button);
	}
}
