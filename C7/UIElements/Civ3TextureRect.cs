using Godot;
using System.Collections.Generic;

[GlobalClass]
[Tool]
public partial class Civ3TextureRect : TextureRect {
	public override void _ValidateProperty(Godot.Collections.Dictionary property) {
		Util.ApplyNoSaveFlag(property, [PropertyName.Texture]);
	}
}
