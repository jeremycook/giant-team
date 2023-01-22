namespace GiantTeam.Organization.Etc.Models;

public enum PermissionId
{
    /// <summary>
    /// Read file, list directory contents, select rows from table, use the schema
    /// </summary>
    r = 'r',

    /// <summary>
    /// Append data to end of file, create file in directory, insert rows into table, create objects in the schema
    /// </summary>
    a = 'a',

    /// <summary>
    /// Write data into file, create subdirectory in directory, update rows in table
    /// </summary>
    w = 'w',

    /// <summary>
    /// Delete file, delete directory, delete rows from table
    /// </summary>
    d = 'd',

    /// <summary>
    /// Manage file, manage directory, alter table, alter schema
    /// </summary>
    m = 'm',
}
