INSERT INTO Users (user_name)
VALUES
('JustinBBD'),
('BESTCoderEVA'),
('John'),
('MysteriousSalamader'),
('23vfdr4et'),
('CodeDevoid');

INSERT INTO UrgencyDescriptors (urgency_descriptor_id,urgency_descriptor_name) VALUES
(1,'Formatting Issues ☕'),
(2,'Mini Bug 🐞'),
(3,'Medium Priority'),
(4,'Urgent🚨'),
(5,'PROD on Fire! 🔥');

INSERT INTO RepoOwners (repo_owner_name) VALUES
('CodeCrusaders'),
('PixelPioneers'),
('StreamlineStudios'),
('QuantumQuartet'),
('NeuralNinjas'),
('BlockchainBrigade'),
('CloudCommanders'),
('KernelKollectors'),
('DataDynamos'),
('InnovateInfinity');

INSERT INTO Repos (repo_owner_id, repo_name) VALUES
((SELECT repo_owner_id FROM RepoOwners WHERE repo_owner_name = 'CodeCrusaders'), 'code-crusaders-website'),
((SELECT repo_owner_id FROM RepoOwners WHERE repo_owner_name = 'PixelPioneers'), 'pixel-pioneers-app'),
((SELECT repo_owner_id FROM RepoOwners WHERE repo_owner_name = 'StreamlineStudios'), 'streamline-studios-api'),
((SELECT repo_owner_id FROM RepoOwners WHERE repo_owner_name = 'QuantumQuartet'), 'quantum-quartet-database'),
((SELECT repo_owner_id FROM RepoOwners WHERE repo_owner_name = 'NeuralNinjas'), 'neural-ninjas-machine-learning'),
((SELECT repo_owner_id FROM RepoOwners WHERE repo_owner_name = 'BlockchainBrigade'), 'blockchain-brigade-contracts'),
((SELECT repo_owner_id FROM RepoOwners WHERE repo_owner_name = 'CloudCommanders'), 'cloud-commanders-storage'),
((SELECT repo_owner_id FROM RepoOwners WHERE repo_owner_name = 'KernelKollectors'), 'kernel-kollectors-os'),
((SELECT repo_owner_id FROM RepoOwners WHERE repo_owner_name = 'DataDynamos'), 'data-dynamos-analytics'),
((SELECT repo_owner_id FROM RepoOwners WHERE repo_owner_name = 'DataDynamos'), 'data-dynamos-statistics'),
((SELECT repo_owner_id FROM RepoOwners WHERE repo_owner_name = 'InnovateInfinity'), 'innovate-infinity-projects');

WITH justinBBd_id AS (
    SELECT user_id FROM Users WHERE user_name = 'JustinBBD'
)

INSERT INTO Blames (blamer_id, blamed_id, urgency_descriptor_id, repo_id, blame_path, blame_line, blame_message, blame_viewed) VALUES
((SELECT user_id FROM justinBBd_id), (SELECT user_id FROM Users WHERE user_name = 'MysteriousSalamader'), 3, 1, '/project/src/main.js', 15, 'Variable not defined', FALSE),
((SELECT user_id FROM Users WHERE user_name = 'MysteriousSalamader'), (SELECT user_id FROM justinBBd_id), 2, 2, '/project/src/utils/helpers.js', 42, 'Helper function deprecated', TRUE),
((SELECT user_id FROM justinBBd_id), (SELECT user_id FROM Users WHERE user_name = 'CodeDevoid'), 5, 3, '/project/assets/images/logo.png', 1, 'Image not optimized', TRUE),
((SELECT user_id FROM Users WHERE user_name = 'BESTCoderEVA'), (SELECT user_id FROM justinBBd_id), 4, 4, '/project/tests/test_auth.py', 101, 'Test case missing', FALSE),
((SELECT user_id FROM justinBBd_id), (SELECT user_id FROM Users WHERE user_name = 'MysteriousSalamader'), 1, 5, '/project/docs/readme.md', 25, 'Documentation outdated',TRUE);


SELECT * FROM Blames;