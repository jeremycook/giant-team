SELECT
	(SELECT string_agg(upper(substring(q, 1, 1)) ||substring(q, 2), '') FROM regexp_split_to_table(table_name, '_') q),
	format('%s%s%spublic %s%s %s { get; %sset; }%s', 
		CASE WHEN column_name = table_name || '_id' THEN format('[Key]%s', chr(13)) ELSE '' END,
		CASE WHEN character_maximum_length IS NOT NULL THEN format('[StringLength(%s)]%s', character_maximum_length, chr(13)) ELSE '' END,
		CASE WHEN generation_expression IS NOT NULL THEN format('[DatabaseGenerated(DatabaseGeneratedOption.Computed)]%s', chr(13)) ELSE '' END,
		CASE udt_name
			WHEN 'uuid' THEN 'Guid'
			WHEN 'bytea' THEN 'byte[]'
			WHEN 'timestamp' THEN 'DateTime'
			WHEN 'json' THEN 'JsonDocument'
			WHEN 'text' THEN 'string'
			WHEN 'varchar' THEN 'string'
			ELSE udt_name
		END,
		CASE is_nullable WHEN 'YES' THEN '?' ELSE '' END,
		(SELECT string_agg(upper(substring(q, 1, 1)) ||substring(q, 2), '') FROM regexp_split_to_table(column_name, '_') q),
		CASE WHEN generation_expression IS NOT NULL OR is_updatable <> 'YES' THEN 'private ' ELSE '' END,
		CASE WHEN is_nullable <> 'YES' AND udt_name IN ('text','varchar') THEN ' = null!;' ELSE '' END
	),
	*
FROM information_schema.columns
WHERE table_schema = 'etc'
ORDER BY table_name, ordinal_position;
