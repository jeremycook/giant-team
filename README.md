# Giant Team

# Getting Started

```sh
# Deploy a local Docker registry
# https://docs.docker.com/registry/deploying/
docker run -d -p localhost:5000:5000 --restart=always --name registry registry:2

# Example starting folder
cd ~/

# Clone the repo
git clone git@github.com:cosoftus/giant-team.git

# Build Dockerfile and push to local registry
cd giant-team
git fetch && git reset --hard && git clean -fxd && git pull
docker build -t localhost:5000/giantteam:latest .
docker push localhost:5000/giantteam:latest

# Copy the stacks folder for editing
cd ~/
cp giant-team/Doc/Stacks stacks

# Prepare pg-stack
cd ~/stacks/pg-stack

# Generate Docker secrets
cat /dev/urandom | tr -dc '[:alnum:]' | fold -w ${1:-25} | head -n 1 > POSTGRES_PASSWORD_FILE

# Deploy pg-stack
docker stack deploy --compose-file compose.yaml pg-stack

# TODO: Symlink the db-data volume to a secondary drive

# Prepare web-stack
cd ~/stacks/web-stack

# Generate Docker secrets

# Certificate Data Protection
openssl req -x509 -sha256 -newkey rsa:4096 -nodes -days 3650 -subj "/C=/ST=/L=/O=/CN=Data Protection Encryption Certificate" -keyout DataProtectionCertificate

# Database passwords
cat /dev/urandom | tr -dc '[:alnum:]' | fold -w ${1:-25} | head -n 1 > DataProtectionConnection__Password
cat /dev/urandom | tr -dc '[:alnum:]' | fold -w ${1:-25} | head -n 1 > MgmtConnection__Password
cat /dev/urandom | tr -dc '[:alnum:]' | fold -w ${1:-25} | head -n 1 > SecConnection__Password
cat /dev/urandom | tr -dc '[:alnum:]' | fold -w ${1:-25} | head -n 1 > MigratorConnection__Password

# Create databases and users and then come back here

# Deploy web-stack
docker stack deploy --compose-file compose.yaml web-stack
```

# Create databases and users

```sql
CREATE ROLE "giantteam:owner" WITH LOGIN NOINHERIT;
CREATE ROLE "giantteam:dp" WITH LOGIN NOINHERIT;
CREATE ROLE "giantteam:mgmt" WITH LOGIN NOINHERIT;
CREATE ROLE "giantteam:sec" WITH CREATEROLE LOGIN NOINHERIT;

CREATE DATABASE "giantteam:dp" OWNER "giantteam:owner";
CREATE DATABASE "giantteam:mgmt" OWNER "giantteam:owner";

SET ROLE "giantteam:owner";
REVOKE ALL ON DATABASE "giantteam:dp" FROM PUBLIC;
GRANT CONNECT ON DATABASE "giantteam:dp" TO "giantteam:dp";
ALTER DEFAULT PRIVILEGES FOR ROLE "giantteam:owner"
	GRANT SELECT, UPDATE, DELETE, INSERT ON TABLES TO "giantteam:dp";
ALTER DEFAULT PRIVILEGES FOR ROLE "giantteam:owner"
	GRANT USAGE ON SEQUENCES TO "giantteam:dp";

SET ROLE "giantteam:owner";
REVOKE ALL ON DATABASE "giantteam:mgmt" FROM PUBLIC;
GRANT CONNECT ON DATABASE "giantteam:mgmt" TO "giantteam:mgmt";
ALTER DEFAULT PRIVILEGES FOR ROLE "giantteam:owner"
	GRANT SELECT, UPDATE, DELETE, INSERT ON TABLES TO "giantteam:mgmt";
ALTER DEFAULT PRIVILEGES FOR ROLE "giantteam:owner"
	GRANT USAGE ON SEQUENCES TO "giantteam:mgmt";
```

```sh
# Redeploy
cd ~/

cd giant-team
git fetch && git reset --hard && git clean -fxd && git pull
docker build -t localhost:5000/giantteam:latest .
docker push localhost:5000/giantteam:latest

cd ../web-stack
docker stack deploy --compose-file compose.yaml web-stack
```
