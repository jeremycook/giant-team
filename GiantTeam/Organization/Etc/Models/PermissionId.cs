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
    /// Remove children from the directory, drop table
    /// </summary>
    D = 'D',

    /// <summary>
    /// Execute file, change directory
    /// </summary>
    x = 'x',

    /// <summary>
    /// Write the named attributes of the file/directory, alter the table, alter the schema
    /// </summary>
    N = 'N',

    /// <summary>
    /// Control access
    /// </summary>
    C = 'C',

    /// <summary>
    /// Change ownership
    /// </summary>
    o = 'o',
}
