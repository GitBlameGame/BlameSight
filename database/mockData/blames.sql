--changeset karl:dml:mockData:blames
insert into blames (blamer_id, blamed_id, urgency_descriptor_id, repo_id, blame_path, blame_line, blame_message, blame_viewed, blame_complete, blame_timestamp) values
(1,2,1,1,'README.md',41,'Messy','f','f','2024-04-06 15:23:54.918922'),
(1,4,4,1,'README.md',1,'Just EW','f','f','2024-04-07 10:00:47.710213'),
(1,1,2,1,'main/README.md',22,'Just EW','t','t','2024-04-07 09:59:04.73573'),
(3,5,4,2,'Server/src/main/java/com/bbd/BeanServer/AuthInt.java',8,'not cool bro','f','f','2024-04-07 11:53:53.648362'),
(3,6,4,3,'Frontend/BlameSightFrontend/BlameSightFrontend/UserInput.cs',56,'more modularity man','f','f','2024-04-07 12:18:42.833292'),
(3,1,4,3,'Backend/BlameSightBackend/BlameSightBackend/Controllers/BlameController.cs',15,'test','f','t','2024-04-07 12:04:05.272948'),
(1,2,4,1,'main/README.md',41,'That is bizzare','f','f','2024-04-07 13:50:30.626266'),
(3,1,4,3,'Backend/BlameSightBackend/BlameSightBackend/Controllers/BlameController.cs',23,'testing','f','t','2024-04-07 12:08:14.549602'),
(3,1,4,3,'Backend/BlameSightBackend/BlameSightBackend/Models/newBlame.cs',2,'tetsing','f','t','2024-04-07 12:36:41.779073'),
(3,1,5,3,'Backend/BlameSightBackend/BlameSightBackend/Controllers/BlameController.cs',47,'was submitted broken ! :/','t','f','2024-04-07 12:00:01.210679'),
(3,1,3,3,'README.md',1,'t','t','f','2024-04-07 12:10:55.320908'),
(3,1,4,3,'README.md',1,'Hi','t','f','2024-04-07 12:16:50.719346'),
(1,1,2,1,'main/README.md',20,'SUS','t','f','2024-04-07 13:50:48.017324'),
(1,1,1,1,'main/README.md',20,'Why tab spaces?','t','f','2024-04-07 13:51:28.955194'),
(1,1,1,1,'main/README.md',20,'Why tab spaces?','t','f','2024-04-07 13:52:10.12861'),
(1,1,2,3,'main/Backend/BlameSightBackend/BlameSightBackend/Utils/JWTUtils.cs',3,'Why tab spaces?','t','f','2024-04-07 14:06:48.514997');
--rollback DELETE FROM "blames";