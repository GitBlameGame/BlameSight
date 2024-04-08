--changeset karl:dml:mockData:users
insert into users (user_name) values
('JustinBBD'),
('Lior-Becker'),
('DerrykBBD'),
('Christo-Kruger-BBD'),
('jhviljoen'),
('Derryk Fivaz');
--rollback DELETE FROM "users";