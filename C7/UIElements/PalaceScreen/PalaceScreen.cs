using Godot;

[GlobalClass]
[Tool]
public partial class PalaceScreen : Control {
	[Export] TextureRect background;
	[Export] TextureButton close;

	public override void _Ready() {
		background.Texture = TextureLoader.Load("palace.background");

		close.TextureNormal = TextureLoader.Load("city_screen.buttons.close.normal");
		close.TextureHover = TextureLoader.Load("city_screen.buttons.close.hover");
		close.TexturePressed = TextureLoader.Load("city_screen.buttons.close.pressed");

		close.Pressed += Hide;
	}
}
