-- Usage: CALL gt.set_user_password('jdoe', '123abc', current_timestamp + interval '1 hour');
CREATE OR REPLACE PROCEDURE gt.set_user_password(_user text, _pass text, _valid_until timestamp with time zone) AS $$
BEGIN
	EXECUTE format('ALTER USER %I PASSWORD %L VALID UNTIL %L'
		, _user
		, _pass
		, _valid_until
	);
END;
$$ LANGUAGE plpgsql;
DROP PROCEDURE IF EXISTS gt.set_user_password(text, text);
