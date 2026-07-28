using BaseLib.Extensions;
using BaseLib.Patches.UI;
using MegaCrit.Sts2.Core.Localization;

namespace BaseLib.Utils;

/// <summary>
/// Public registry for adding mod credit sections to the vanilla credits screen.
/// Mods call one of the <c>Register</c> overloads at load time; the sections are
/// rendered later by <see cref="NCreditsScreenPatch"/> when the screen opens.
/// </summary>
/// <remarks>
/// All text is resolved from the vanilla <c>credits</c> loc table, namespaced by
/// mod id: <c>&lt;MODID&gt;-&lt;SECTION&gt;.header</c> and
/// <c>&lt;MODID&gt;-&lt;SECTION&gt;.names</c>.
/// </remarks>
public static class ModCredits
{
    /// <summary>Body layout of a section, determining how its <c>.names</c> value is parsed.</summary>
    public enum Layout
    {
        /// <summary>One name per line.</summary>
        Names = 1,
        /// <summary>One <c>Role||Name</c> pair per line, rendered as two columns.</summary>
        Roles = 2,
        /// <summary>One name per line, dealt round-robin across three columns.</summary>
        Columns3 = 3,
    }

    /// <summary>
    /// A credits section. A <b>leaf</b> renders a gold header plus its names. A
    /// <b>group</b> (when <paramref name="Children"/> is non-empty) renders a green
    /// header, then — if a <c>.names</c> key exists — its own names, then its child
    /// sections. Groups may nest arbitrarily.
    /// </summary>
    /// <param name="Name">
    /// Section id; combined with the mod id to form the loc keys
    /// <c>&lt;MODID&gt;-&lt;NAME&gt;.header</c> / <c>.names</c>.
    /// </param>
    /// <param name="Kind">How this section's body is laid out (applies to a group's own names too).</param>
    /// <param name="Children">Child sections; supplying any makes this a group.</param>
    public record Section(string Name, Layout Kind = Layout.Names, Section[]? Children = null)
    {
        /// <summary>True when this section owns child sections (renders as a green group header).</summary>
        public bool IsGroup => Children is { Length: > 0 };
    }

    /// <summary>A registered mod and its sections, in registration order.</summary>
    internal record Entry(string ModId, List<Section> Sections);

    /// <summary>All registered mods, rendered in the order they registered.</summary>
    internal static readonly List<Entry> Entries = [];

    /// <summary>
    /// Registers sections for the mod whose root namespace matches <typeparamref name="TFromMod"/>.
    /// Pass any type from your mod assembly (plugin class, a card model, etc.).
    /// </summary>
    /// <typeparam name="TFromMod">A type in your mod; its root namespace becomes the mod id.</typeparam>
    /// <param name="sections">The sections to display for this mod.</param>
    public static void Register<TFromMod>(params Section[] sections)
        => Entries.Add(new Entry(IdOf(typeof(TFromMod)), [.. sections]));

    /// <summary>Registers sections under an explicit, upper-cased mod id.</summary>
    /// <param name="modId">The mod id used to namespace loc keys.</param>
    /// <param name="sections">The sections to display for this mod.</param>
    public static void Register(string modId, params Section[] sections)
        => Entries.Add(new Entry(modId.ToUpperInvariant(), [.. sections]));

    /// <summary>
    /// Derives the mod id from a type's root namespace, matching the prefix
    /// BaseLib assigns to content ids (minus its trailing dash).
    /// </summary>
    private static string IdOf(Type type)
        => type.GetRootNamespace().ToUpperInvariant();

    /// <summary>Looks up a key in the vanilla <c>credits</c> loc table.</summary>
    internal static string Resolve(string key)
        => new LocString("credits", key).GetRawText();

    /// <summary>True when <paramref name="key"/> exists in the credits table.</summary>
    internal static bool Has(string key)
        => LocString.Exists("credits", key);
}