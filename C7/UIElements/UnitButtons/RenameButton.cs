using Godot;
using ConvertCiv3Media;

[Tool]
public partial class RenameButton : Civ3TextureButton {
	public override void _Ready() {
		ImageTexture menuTexture = TextureLoader.Load("ui.rename");
		this.TextureNormal = menuTexture;
	}
}
