using System.Globalization;
using System.Text;

namespace AspireApp.Tools.Generator.Generator.Mutators;

internal sealed class NavMenuMutator(string targetPath) : IFileMutator
{
    public string TargetPath { get; } = targetPath;

    public MutationResult Mutate(string source, EntitySpec entity)
    {
        var marker = $"href=\"{entity.Lower}\"";
        if (source.Contains(marker, StringComparison.Ordinal))
            return new MutationResult(source, false, "already up to date");

        var newline = source.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var navItem = BuildNavItem(entity, newline);

        var anchorIdx = source.LastIndexOf("</nav>", StringComparison.Ordinal);
        if (anchorIdx < 0)
            throw new InvalidOperationException("Could not find </nav> in NavMenu.razor.");

        // Walk back to start of the line that contains </nav>
        var lineStart = source.LastIndexOf('\n', anchorIdx) + 1;
        var insertion = navItem + newline + newline;
        var updated = source[..lineStart] + insertion + source[lineStart..];

        return new MutationResult(updated, true, "+ NavLink");
    }

    private static string BuildNavItem(EntitySpec entity, string newline)
    {
        var sb = new StringBuilder();
        sb.Append("        <div class=\"nav-item px-3\">").Append(newline);
        sb.Append(CultureInfo.InvariantCulture, $"            <NavLink class=\"nav-link\" href=\"{entity.Lower}\">").Append(newline);
        sb.Append(CultureInfo.InvariantCulture, $"                <span class=\"bi bi-list-nested\" aria-hidden=\"true\"></span> {entity.Plural}").Append(newline);
        sb.Append("            </NavLink>").Append(newline);
        sb.Append("        </div>");
        return sb.ToString();
    }
}
