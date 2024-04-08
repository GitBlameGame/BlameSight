--changeset karl:ddl:createTable:Users
CREATE TABLE users(
	user_id SERIAL PRIMARY KEY NOT NULL,
	user_name VARCHAR(39) NOT NULL UNIQUE
);
--rollback DROP TABLE "Users";

--changeset karl:ddl:createTable:UrgencyDescriptors
CREATE TABLE urgency_descriptors(
	urgency_descriptor_id INT PRIMARY KEY NOT NULL,
	urgency_descriptor_name VARCHAR(30) NOT NULL UNIQUE
);
--rollback DROP TABLE "UrgencyDescriptors";

--changeset karl:ddl:createTable:RepoOwners
CREATE TABLE repo_owners(
	repo_owner_id SERIAL PRIMARY KEY NOT NULL,
	repo_owner_name VARCHAR(1000) NOT NULL UNIQUE
);
--rollback DROP TABLE "RepoOwners";

--changeset karl:ddl:createTable:Repos
CREATE TABLE repos(
	repo_id SERIAL PRIMARY KEY NOT NULL,
	repo_owner_id INT NOT NULL,
	repo_name VARCHAR(1000) NOT NULL,
	UNIQUE(repo_owner_id, repo_name)
);
--rollback DROP TABLE "Repos";

--changeset karl:ddl:createTable:Blames
CREATE TABLE blames(
	blame_id SERIAL PRIMARY KEY NOT NULL,
	blamer_id INT NOT NULL,
	blamed_id INT NOT NULL,
	urgency_descriptor_id INT NOT NULL,
	repo_id INT NOT NULL,
	blame_path VARCHAR(4096) NOT NULL,
	blame_line INT NOT NULL,
	blame_message VARCHAR(256) NOT NULL,
	blame_viewed BOOLEAN NOT NULL DEFAULT false,
	blame_complete BOOLEAN NOT NULL DEFAULT false,
	blame_timestamp TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
--rollback DROP TABLE "Blames";