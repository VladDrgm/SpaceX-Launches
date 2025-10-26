using System.ComponentModel;

namespace SpaceXLaunchDataService.Data.Models.Enums;

/// <summary>
/// Sort order options
/// </summary>
public enum SortOrder
{
    /// <summary>
    /// Ascending order (A-Z, 0-9, oldest first)
    /// </summary>
    [Description("Ascending order (A-Z, 0-9, oldest first)")]
    Asc,

    /// <summary>
    /// Descending order (Z-A, 9-0, newest first)
    /// </summary>
    [Description("Descending order (Z-A, 9-0, newest first)")]
    Desc
}