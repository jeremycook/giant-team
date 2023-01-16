namespace GiantTeam.Organization.Etc.Models;

public static class PermissionId
{
    /// <summary>
    /// Read file, list directory contents, select rows from table
    /// </summary>
    public const char Read = 'r';

    /// <summary>
    /// Append data to end of file, create file in directory, insert rows into table
    /// </summary>
    public const char Append = 'a';

    /// <summary>
    /// Write data into file, create subdirectory in directory, update rows in table
    /// </summary>
    public const char Write = 'w';

    /// <summary>
    /// Delete file, delete directory, delete rows from table
    /// </summary>
    public const char Delete = 'd';

    /// <summary>
    /// Remove children from the directory, drop table
    /// </summary>
    public const char Drop = 'D';

    /// <summary>
    /// Execute file, change directory
    /// </summary>
    public const char Execute = 'x';

    /// <summary>
    /// Write the named attributes of the file/directory, alter the table
    /// </summary>
    public const char Alter = 'N';

    /// <summary>
    /// Modify access
    /// </summary>
    public const char ControlAccess = 'C';

    /// <summary>
    /// Change ownership
    /// </summary>
    public const char Ownership = 'o';
}
