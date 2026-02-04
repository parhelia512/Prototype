using System;
using System.Collections.Generic;
using System.Linq;
using C7Engine;
using C7GameData.Save;

namespace C7GameData;

// TODO: ideas for other city related stuff that are not yet implemented;
// pollution + -, luxury happy face + -, some tile modifier? ,
// more/less units in  the city -> happier/sadder population

// We should keep these names in all lower case form
// to avoid any case mismatch between this code, the json and lua.
// It's not pretty but at least it will be consistent across all these.
public enum InflowYield {
	commerce,
	culture,
	science,
	happiness,
	maintenance,
	unitsupport,
	corruption,
}
public class Inflow : IProducible {
	public string name { get; set; }
	public int populationCost { get; set; }
	public Tech requiredTech { get; set; }
	public HashSet<Resource> requiredResources { get; set; }

	public int iconRowIndex;
	public List<LocalYield> localYield { get; set; }

	public int ShieldCost(HashSet<Civilization.Trait> civTraits, float costFactor) {
		return 0;
	}

	public bool CanProduce(City city, HashSet<Resource> accessibleResources) {
		return true;
	}

	public Inflow(SaveInflow saveInflow, LuaRulesEngine luaRulesEngine) {
		this.name = saveInflow.name;
		this.iconRowIndex = saveInflow.iconRowIndex;
		this.localYield = saveInflow.localYield.ConvertAll(y => new LocalYield(y.yieldType, luaRulesEngine, y.yieldCalculation));
	}

	public Func<ScriptContext, int> GetCommerceYieldFunc() {
		return this.localYield.FirstOrDefault(y => y.yieldType == InflowYield.commerce).yieldCalculation;
	}
	public Func<ScriptContext, int> GetCultureYieldFunc() {
		return this.localYield.FirstOrDefault(y => y.yieldType == InflowYield.culture).yieldCalculation;
	}
	public Func<ScriptContext, int> GetScienceYieldFunc() {
		return this.localYield.FirstOrDefault(y => y.yieldType == InflowYield.science).yieldCalculation;
	}
	public Func<ScriptContext, int> GetHappinessYieldFunc() {
		return this.localYield.FirstOrDefault(y => y.yieldType == InflowYield.happiness).yieldCalculation;
	}
	public Func<ScriptContext, int> GetMaintenanceYieldFunc() {
		return this.localYield.FirstOrDefault(y => y.yieldType == InflowYield.maintenance).yieldCalculation;
	}
	public Func<ScriptContext, int> GetUnitSupportYieldFunc() {
		return this.localYield.FirstOrDefault(y => y.yieldType == InflowYield.unitsupport).yieldCalculation;
	}
	public Func<ScriptContext, int> GetCorruptionYieldFunc() {
		return this.localYield.FirstOrDefault(y => y.yieldType == InflowYield.corruption).yieldCalculation;
	}
}

public struct LocalYield {
	public InflowYield yieldType { get; set; }
	public readonly Func<ScriptContext, int> yieldCalculation;

	public LocalYield() { }
	public LocalYield(InflowYield type, LuaRulesEngine rulesEngine, string yieldCalculation = null) {
		this.yieldType = type;
		if (yieldCalculation != null) {
			this.yieldCalculation = rulesEngine.ImportFunc<Func<ScriptContext, int>>(yieldCalculation);
		}
	}
}
public struct SaveLocalYield {
	public InflowYield yieldType { get; set; }
	public string yieldCalculation;

	public SaveLocalYield() { }
	public SaveLocalYield(InflowYield type, string yieldCalculation) {
		this.yieldType = type;
		this.yieldCalculation = yieldCalculation;
	}
}

public struct ScriptContext(Player player, City city) {
	public Player player = player;
	public City city = city;
}
