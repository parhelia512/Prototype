using System;
using System.Collections.Generic;
using System.Linq;
using C7Engine;
using C7GameData.Save;

namespace C7GameData;

public enum InflowYield {
    Commerce,
    Culture,
    Science,
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
		return this.localYield.FirstOrDefault(y => y.yieldType == InflowYield.Commerce).yieldCalculation;
	}
	public Func<ScriptContext, int> GetCultureYieldFunc() {
		return this.localYield.FirstOrDefault(y => y.yieldType == InflowYield.Culture).yieldCalculation;
	}
	public Func<ScriptContext, int> GetScienceYieldFunc() {
		return this.localYield.FirstOrDefault(y => y.yieldType == InflowYield.Science).yieldCalculation;
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

public struct ScriptContext(int input, List<Tech> techs) {
	public int input = input;
	public List<Tech> techs = techs;
}
