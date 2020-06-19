-- phpMyAdmin SQL Dump
-- version 4.9.4
-- https://www.phpmyadmin.net/
--
-- Host: localhost
-- Gegenereerd op: 19 jun 2020 om 21:45
-- Serverversie: 10.2.31-MariaDB
-- PHP-versie: 5.5.14

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET AUTOCOMMIT = 0;
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `markmeerssman`
--

-- --------------------------------------------------------

--
-- Tabelstructuur voor tabel `games`
--

CREATE TABLE `games` (
  `id` int(11) NOT NULL,
  `name` varchar(25) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Gegevens worden geëxporteerd voor tabel `games`
--

INSERT INTO `games` (`id`, `name`) VALUES
(1, 'MiniBall'),
(2, 'MaxiBall'),
(3, 'MiniBlox'),
(4, 'MaxiBlox'),
(5, 'BallBlox');

-- --------------------------------------------------------

--
-- Tabelstructuur voor tabel `scores`
--

CREATE TABLE `scores` (
  `id` int(11) NOT NULL,
  `user_id` int(11) NOT NULL,
  `game_id` int(11) NOT NULL,
  `score` int(11) NOT NULL,
  `date_time` datetime NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Gegevens worden geëxporteerd voor tabel `scores`
--

INSERT INTO `scores` (`id`, `user_id`, `game_id`, `score`, `date_time`) VALUES
(1, 1, 0, 420, '2020-05-08 10:25:56'),
(2, 1, 0, 5123, '2020-05-08 10:25:56'),
(3, 2, 1, 1, '2020-05-08 10:25:56'),
(4, 3, 1, 25, '2020-05-08 10:25:56'),
(5, 1, 5, 740, '2020-05-08 10:42:17'),
(6, 5, 5, 420, '2020-05-08 10:42:17'),
(7, 1, 1, 80085, '2020-05-12 12:32:48'),
(8, 0, 0, 0, '2020-05-12 12:40:56'),
(9, 2, 0, 1, '2020-05-12 12:41:32'),
(10, 2, 1, 1, '2020-05-12 12:42:10'),
(11, 430, 4, 0, '2020-05-18 14:56:07'),
(12, 430, 4, 0, '2020-05-18 14:56:40'),
(13, 430, 4, 5, '2020-05-18 14:56:47'),
(14, 1, 1, 5000, '2020-06-19 21:59:07');

-- --------------------------------------------------------

--
-- Tabelstructuur voor tabel `servers`
--

CREATE TABLE `servers` (
  `id` int(11) NOT NULL,
  `password` varchar(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Gegevens worden geëxporteerd voor tabel `servers`
--

INSERT INTO `servers` (`id`, `password`) VALUES
(1, 'WaitWord123'),
(2, '123');

-- --------------------------------------------------------

--
-- Tabelstructuur voor tabel `users`
--

CREATE TABLE `users` (
  `id` int(11) NOT NULL,
  `first_name` varchar(25) NOT NULL,
  `last_name` varchar(35) NOT NULL,
  `email` varchar(60) NOT NULL,
  `password` varchar(100) NOT NULL,
  `registered_date` date NOT NULL,
  `birthday` date NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Gegevens worden geëxporteerd voor tabel `users`
--

INSERT INTO `users` (`id`, `first_name`, `last_name`, `email`, `password`, `registered_date`, `birthday`) VALUES
(1, 'Mark', 'Meerssman', 'mark.meerssman@student.hku.nl', '12345', '2020-04-24', '1998-04-18'),
(2, 'Mark2', 'Meerssman', 'mark.meerssman@student.hku.nl', '54321', '2020-04-24', '1999-04-18'),
(3, 'Pieter', 'Bobszoon', 'Pieter.B@gmail.com', 'HetBestWachtwoordOoit2468', '2020-05-08', '1954-11-23'),
(4, 'Karel', 'Achternaam', 'Karel.Achternaam@gmail.com', 'mindergoedwachtwoor123', '2020-05-08', '1994-10-07'),
(5, 'Dennis', 'Brost', 'DanisBarsh@mail.nl', '321123', '2020-05-08', '2001-04-20');

--
-- Indexen voor geëxporteerde tabellen
--

--
-- Indexen voor tabel `games`
--
ALTER TABLE `games`
  ADD PRIMARY KEY (`id`);

--
-- Indexen voor tabel `scores`
--
ALTER TABLE `scores`
  ADD PRIMARY KEY (`id`);

--
-- Indexen voor tabel `servers`
--
ALTER TABLE `servers`
  ADD PRIMARY KEY (`id`);

--
-- Indexen voor tabel `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`id`);

--
-- AUTO_INCREMENT voor geëxporteerde tabellen
--

--
-- AUTO_INCREMENT voor een tabel `games`
--
ALTER TABLE `games`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=11;

--
-- AUTO_INCREMENT voor een tabel `scores`
--
ALTER TABLE `scores`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=15;

--
-- AUTO_INCREMENT voor een tabel `servers`
--
ALTER TABLE `servers`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT voor een tabel `users`
--
ALTER TABLE `users`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
