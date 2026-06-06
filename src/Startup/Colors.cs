using System.Drawing;
namespace Blackwood;

/// <summary>
/// A class that provides a dictionary of RGB colors, mapping color names to Color objects.
/// </summary>
static partial class RGBColors
{
    /// <summary>
    /// A dictionary of RGB colors, mapping color names to Color objects.
    /// </summary>
    internal static readonly Dictionary<uint,Color> RGBColor_ = [];
    
    /// <summary>
    /// Gets the RGB color for a given name.
    /// </summary>
    /// <param name="name">The name of the color.</param>
    /// <param name="color">The RGB color.</param>
    /// <returns>True if the color was found, false otherwise.</returns>
    internal static bool RGBColor(string name, out Color color)
    {
        return RGBColor_.TryGetValue(Match.FNV1a(name.ToUpper()), out color);
    }


    /// <summary>
    /// Loads the RGB colors from the resource file.
    /// </summary>
    static RGBColors()
    {
        // Load the Colors from the resource file
        Table.LoadTableResource("Resources/colors"
                               , (headerMap, cells) =>
        {
            // Add the color to the dictionary
            RGBColor_[Match.FNV1a(cells[headerMap["COLOR"]].ToUpper())] = ColorTranslator.FromHtml("#" + (cells[headerMap["RGB"]].Substring(2)));
            return true;
        });
    }
}
