using System;

namespace C7Engine;

/// <summary>
/// An attribute to mark methods that are exposed and used (or are intended to be used) in the Lua layer.<br/>
/// This is intended to be primarily a visual aid, especially for methods that seem unused by the native c# code.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class LuaMethodAttribute : Attribute { }
