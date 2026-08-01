using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using Godot;
using Valencina.ValencinaCode;
using Valencina.ValencinaCode.Backgrounds;
using Valencina.ValencinaCode.Character;
using Valencina.ValencinaCode.Monsters;
using Valencina.ValencinaCode.UI;
using Valencina.ValencinaCode.Vfx;

[assembly: AssemblyCompany("Valencina")]
[assembly: AssemblyConfiguration("ExportRelease")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0")]
[assembly: AssemblyProduct("Valencina")]
[assembly: AssemblyTitle("Valencina")]
[assembly: AssemblyHasScripts(new Type[]
{
	typeof(Act4EliteCombatBackground),
	typeof(UngezieferKaiserCombatBackground),
	typeof(WarAmbushCombatBackground),
	typeof(ValencinaCreatureVisuals),
	typeof(ValencinaRestSiteCharacterNode),
	typeof(MainFile),
	typeof(Act4EliteCreatureVisuals),
	typeof(GCompanyCreatureVisuals),
	typeof(UngezieferKaiserVisuals),
	typeof(ValencinaSelectionReticle),
	typeof(ShinAuraSceneNode)
})]
[assembly: AssemblyVersion("1.0.0.0")]
[module: RefSafetyRules(11)]
