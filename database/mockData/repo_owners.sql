--changeset karl:dml:mockData:repo_owners
insert into repo_owners (repo_owner_name) values
('BeanStalk-BBD'),
('bbd-grad-levelups'),
('GitBlameGame');
--rollback DELETE FROM "repo_owners";