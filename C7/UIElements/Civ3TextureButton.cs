using Godot;
using System.Collections.Generic;

[GlobalClass]
[Tool]
public partial class Civ3TextureButton : TextureButton {
	public override void _ValidateProperty(Godot.Collections.Dictionary property) {
		Util.ApplyNoSaveFlag(property, [PropertyName.TextureNormal, PropertyName.TextureHover, PropertyName.TexturePressed]);
	}
}
