﻿using GiantTeam.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.Cluster.Directory.Data;

public class User
{
    [Obsolete("Runtime only", true)]
    public User() { }

    public User(DateTime created)
    {
        Created = created;
    }

    [Key]
    public Guid UserId { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = null!;

    [Required, StringLength(50), Username]
    public string Username { get; set; } = null!;

    [Required, StringLength(60), RegularExpression("^u:" + UsernameAttribute.UsernamePattern + "$")]
    public string DbUser { get; set; } = null!;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = null!;

    public bool EmailVerified { get; set; }

    public DateTime Created { get; private set; }
}