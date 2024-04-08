--changeset karl:ddl:repos:fk_repo_owner
ALTER TABLE repos
ADD CONSTRAINT fk_repo_owner
FOREIGN KEY (repo_owner_id) REFERENCES repo_owners(repo_owner_id);
--rollback ALTER TABLE "repos" DROP CONSTRAINT fk_repo_owner

--changeset karl:ddl:blames:fk_blamer
ALTER TABLE blames
ADD CONSTRAINT fk_blamer
FOREIGN KEY (blamer_id) REFERENCES users(user_id);
--rollback ALTER TABLE "blames" DROP CONSTRAINT fk_blamer

--changeset karl:ddl:blames:fk_urgency_descriptor
ALTER TABLE blames
ADD CONSTRAINT fk_urgency_descriptor
FOREIGN KEY (urgency_descriptor_id) REFERENCES urgency_descriptors(urgency_descriptor_id);
--rollback ALTER TABLE "blames" DROP CONSTRAINT fk_urgency_descriptor

--changeset karl:ddl:blames:fk_repo
ALTER TABLE blames
ADD CONSTRAINT fk_repo
FOREIGN KEY (repo_id) REFERENCES repos(repo_id);
--rollback ALTER TABLE "blames" DROP CONSTRAINT fk_repo

--changeset karl:ddl:blames:fk_blamed
ALTER TABLE blames
ADD CONSTRAINT fk_blamed
FOREIGN KEY (blamed_id) REFERENCES users(user_id);
--rollback ALTER TABLE "blames" DROP CONSTRAINT fk_blamed
