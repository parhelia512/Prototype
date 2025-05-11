using System.Collections.Generic;

namespace C7GameData.Save;

public class SaveTerraform {
	public ID Id;
	public string Name;
	public string CivilopediaEntry;
	public int TurnsToComplete;
	public ID RequiredTech;
	public List<string> RequiredResources = [];
	public UnitAction Action;

	public SaveTerraform() { }
}
