using System.Reflection;
using System.Text;
namespace Blackwood;

/// <summary>
/// Loads a table from a resource or stream and processes each row with a given action.
/// </summary>
public partial class Table
{
    /// <summary>
    /// Loads a table from a resource and processes each row with a given action.
    /// </summary>
    /// <param name="resourceName">The name of the resource to load the table from.</param>
    /// <param name="processRow">The action to process each row with.</param>
    public static void LoadTableResource(string resourceName, d_processRow processRow)
    {
        var assembly = Assembly.GetCallingAssembly();

        // Load the table from the resource in the calling assembly
        using var stream = new EmbeddedResources(assembly).Stream(resourceName);

        // Load the table from the stream, assume each row is processed cleanly
        if (null != stream)
            LoadTable(stream, processRow);
    }

    /// <summary>
    /// Delegate for processing a row of a table.
    /// </summary>
    /// <param name="nameToColumnIndex">A dictionary mapping header names to column indices.</param>
    /// <param name="row">An array of strings representing the row's cells.</param>
    /// <returns>True if the row was processed successfully, false if it should be deferred.</returns>
    public delegate bool d_processRow(Dictionary<string, int> nameToColumnIndex, string[] row);

    /// <summary>
    /// Loads a table from a stream and processes each row with a given action.
    /// </summary>
    /// <param name="stream">The stream to load the table from.</param>
    /// <param name="processRow">The action to process each row with.</param>
    internal static void LoadTable(Stream stream, d_processRow processRow)
    {
        // Deferred rows are rows that could not be processed due to circular dependencies
        List<string[]> deferred = [];

        // The delimiter is a pipe character
        char[] A = ['|'];

        // Create a mapping of headers to column indices
        Dictionary<string, int> headerMap = [];

        // Each line is a row in the table
        using var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true);
        if (null == reader)
            return;
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            line = line.Trim();
            // Skip non-row lines, including comments
            if (!line.StartsWith('|'))
                continue;

            // Split the row into cells
            var cells = line.Split(A).Skip(1).SkipLast(1).ToArray();
            if (cells.Length < 0)
                return;

            // Skip separator rows
            if (cells[0].StartsWith(':') || cells[0].StartsWith('-'))
            {
                continue;
            }

            // Trim whitespace from each cell
            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = cells[i].Trim();
            }

            // Create mapping of headers to column indices on first row
            if (headerMap.Count == 0)
            {
                // Create a mapping of headers to column indices
                for (var index = 0; index < cells.Length; index++)
                {
                    var header = cells[index].ToUpper();
                    if (!string.IsNullOrEmpty(header))
                    {
                        headerMap[header] = index;
                    }
                }
                continue;
            }

            // Pass the header map and the cells to the process row action
            if (!processRow(headerMap, cells))
            {
                // Put the row into the deferred column
                deferred.Add(cells);
            }
        }

        // Repeat applying the deferred items until all have resolved
        while (deferred.Count > 0)
        {
            // Prepare a new deferred list
            var oldDeferred = deferred;
            deferred = [];
            // Attempt to process the deferred rows
            foreach (var row in oldDeferred)
            {
                // Pass the header map and the cells to the process row action
                if (!processRow(headerMap, row))
                {
                    // Put the row into the deferred column
                    deferred.Add(row);
                }
            }

            // If the number of deferred rows is the same as the number of old deferred rows,
            // there is a circular dependency in the table
            if (oldDeferred.Count == deferred.Count)
                throw new Exception("Cant complete table load: perhaps circular dependency in table");
        }
    }
}
