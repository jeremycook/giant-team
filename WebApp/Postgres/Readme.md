# PostgreSQL

* About: https://www.postgresql.org/
* Docs: https://www.postgresql.org/docs/
* License:
  * Type: postgresql
  * URL: https://www.postgresql.org/about/licence/
* Source: https://git.postgresql.org/gitweb/?p=postgresql.git

# Npgsql

Npgsql is the .NET client library that talks to PostgreSQL.

* Docs: https://www.npgsql.org/doc/index.html
* Connection string: https://www.npgsql.org/doc/connection-string-parameters.html

# Windows Setup

1. Download the latest stable version of the PostgreSQL installer from https://www.postgresql.org/download/
2. When using the EDB installer, avoid installing pgAdmin (it is usually out of date)
3. Download and install the latest version of pgAdmin from https://www.pgadmin.org/download/

## Trusted authentication on Windows

Enable local, trusted authentication by adding the following line above other lines in **pg_hba.conf**. Add a user to the database that matches your Windows username. **The casing of the user and computer names matter.**

```
# SSPI local connections (casing matters)
host    all             MyUsername          127.0.0.1/32                sspi include_realm=0 krb_realm=MY-DESKTOP
host    all             MyUsername          ::1/128                     sspi include_realm=0 krb_realm=MY-DESKTOP
```

> **TIP!** The pg_hba.conf is commonly located in a folder like "C:\Program Files\PostgreSQL\14\data" where the number is the major version you have installed.

Display your username and computer name with this Windows command.

```cmd
echo %username% %computername%
```
