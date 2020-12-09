// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Broken Rule doesn't pick up on being set by razor page.", Scope = "namespaceanddescendants", Target = "Hashgraph.Portal.Pages")]
[assembly: SuppressMessage("Performance", "CA1805:Do not initialize unnecessarily", Justification = "Prefer this style", Scope = "namespaceanddescendants", Target = "Hashgraph.Portal.Pages")]
