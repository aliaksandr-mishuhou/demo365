CREATE DATABASE `demo365`;

USE `demo365`;

CREATE TABLE `games` (
  `id` bigint(10) NOT NULL AUTO_INCREMENT,
  `sport` varchar(30) DEFAULT NULL,
  `competition` varchar(30) DEFAULT NULL,
  `team1` varchar(30) DEFAULT NULL,
  `team2` varchar(30) DEFAULT NULL,
  `time` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_games_sport_competition_team1_team2_time` (`sport`,`competition`,`team1`,`team2`,`time`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;