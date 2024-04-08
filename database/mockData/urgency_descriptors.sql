--changeset karl:dml:mockData:urgency_descriptors
insert into urgency_descriptors (urgency_descriptor_id, urgency_descriptor_name) values
(1,'Formatting Issues ☕'),
(2,'Mini Bug 🐞'),
(3,'Medium Priority'),
(4,'Urgent🚨'),
(5,'PROD on Fire! 🔥');
--rollback DELETE FROM "urgency_descriptors";