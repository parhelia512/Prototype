using Godot;

[GlobalClass]
public partial class Civ3FileDialog : FileDialog {
	// Use this instead of a scene-based FileDialog to avoid it saving the local dev's last browsed folder in the repo
	// While instantiated it will return to the last-accessed folder when reopened
	public Civ3FileDialog() {
		FileMode = FileDialog.FileModeEnum.OpenFile;
	}

	public override void _Ready() {
		Access = AccessEnum.Filesystem;
		base._Ready();
	}

	public void SetDirectory(string RelPath) {
		CurrentDir = Util.Civ3Root + "/" + RelPath;
	}
}
