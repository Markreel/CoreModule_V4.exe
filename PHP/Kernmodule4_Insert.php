<?php
	include "Kernmodule4_Connection.php";
	include 'Kernmodule4_Globals.php';

	SetExistingSession();
    session_start();

    if (!CheckServerID()) 
    {
        echo "0"; 
        return;
    }

    $userID = GetURLVariable("user", -1, -1, 0);
    $value = GetURLVariable("score", 0, 99999, 0);

    if ($userID == 0) 
    {
        echo "0";
    } 

    else 
    {
        insertScore($mysqli, $userID,$value);
    }

    function insertScore($mysqli, $userID, $value) 
    {
        $query = "INSERT INTO `scores` (`id`, `game_id`, `user_id`, `date_time`, `score`) VALUES (NULL, '1', '$userID', now(), '$value');";
        
        if (!($result = $mysqli->query($query))) 
        {
            echo "0"; 
            showerror($mysqli->errno,$mysqli->error);
        } 
        
        else 
        {
            echo "1"; 
        }
    }
?>