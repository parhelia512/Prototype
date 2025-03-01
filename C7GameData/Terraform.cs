using System.Collections.Generic;

namespace C7GameData;

public class Terraform {
	public ID Id;

	public string Name;

	public string CivilopediaEntry;

	public int TurnsToComplete;

	public ID RequiredTech;

	public List<ID> RequiredResources = new();

	public string Action;
}
