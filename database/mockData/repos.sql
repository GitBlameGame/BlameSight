--changeset karl:dml:mockData:repos
insert into repos (repo_owner_id, repo_name) values
(1,'BeanStalk'),
(2,'Bean-Enthusiasts'),
(3,'BlameSight');
--rollback DELETE FROM "repos";
