using GiantTeam.ComponentModel;
using GiantTeam.Postgres;
using System.ComponentModel.DataAnnotations;

namespace GiantTeam.WorkspaceInteraction.Services
{
    public class FetchRecordsInput
    {
        /// <summary>
        /// Output generated SQL into <see cref="FetchRecords.Sql"/>.
        /// </summary>
        public bool? Verbose { get; set; } = false;

        [Required]
        [Identifier]
        [StringLength(50, MinimumLength = 3)]
        public string Database { get; set; } = null!;

        [Required]
        [Identifier]
        [StringLength(100)]
        public string Schema { get; set; } = null!;

        [Required]
        [Identifier]
        [StringLength(100)]
        public string Table { get; set; } = null!;

        public List<FetchRecordsInputColumn>? Columns { get; set; }

        public List<FetchRecordsInputRangeFilter>? Filters { get; set; }

        public int? Skip { get; set; } = 0;

        [Range(1, 10000)]
        public int? Take { get; set; } = 100;
    }

    public class FetchRecordsInputColumn
    {
        [Required]
        [Identifier]
        [StringLength(100)]
        public string Name { get; set; } = null!;
        public Sort Sort { get; set; }
        public int? Position { get; set; }
    }

    public abstract class FetchRecordsInputFilter
    {
        public FetchRecordsInputFilter(string discriminator)
        {
            Discriminator = discriminator;
        }

        public string Discriminator { get; set; }

        [Required]
        [Identifier]
        [StringLength(100)]
        public string Column { get; set; } = null!;
    }

    public class FetchRecordsInputRangeFilter : FetchRecordsInputFilter
    {
        public FetchRecordsInputRangeFilter() : base(nameof(FetchRecordsInputRangeFilter))
        {
        }

        [Required]
        public string LowerValue { get; set; } = null!;

        [Required]
        public string UpperValue { get; set; } = null!;
    }

    public class FetchRecordsInputOrder
    {
        [Required]
        [Identifier]
        [StringLength(100)]
        public string Column { get; set; } = null!;

        public bool? Desc { get; set; } = false;

        public string ToSql()
        {
            return $"{PgQuote.Identifier(Column)}{(Desc == true ? " DESC" : string.Empty)}";
        }
    }

    public class FetchRecords
    {
        public string? Sql { get; set; }
        public List<FetchRecordsColumn> Columns { get; } = new();
        public List<object?[]> Records { get; } = new();
    }

    public class FetchRecordsColumn
    {
        [Required]
        [Identifier]
        [StringLength(100)]
        public string Name { get; set; } = null!;
        public string DataType { get; set; } = null!;
        public bool Nullable { get; set; }
    }
}
