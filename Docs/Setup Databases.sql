DO
$$
DECLARE
_password text := array_to_string(array(select substr('0123456789ABCDEFGHJKMNPQRSTVWXYZ',((random()*(32-1)+1)::integer),1) from generate_series(1,25)),'');
BEGIN

RAISE NOTICE 'Password %', _password;
RAISE NOTICE '%', format('CREATE ROLE %I;', 'giantteam_records_1');
RAISE NOTICE '%', format('CREATE LOGIN %I MEMBER OF %I;', 'giantteam_records', 'giantteam_records_1');

END$$;