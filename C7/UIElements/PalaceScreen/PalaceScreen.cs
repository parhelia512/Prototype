using Godot;

[GlobalClass]
[Tool]
public partial class PalaceScreen : Control {
	[Export] TextureRect background;
	[Export] TextureButton close;

	public override void _Ready() {
		background.Texture = Util.LoadTextureFromPCX("Art/PalaceView/bkgr.pcx");

		close.TextureNormal = Util.LoadTextureFromPCX("Art/city screen/cityMgmtButtons.pcx", 155, 1, 38, 48);
		close.TextureHover = Util.LoadTextureFromPCX("Art/city screen/cityMgmtButtons.pcx", 155, 50, 38, 48);
		close.TexturePressed = Util.LoadTextureFromPCX("Art/city screen/cityMgmtButtons.pcx", 155, 99, 38, 48);

		close.Pressed += Hide;
	}
}
