<?php
	include "Kernmodule4_Connection.php";
	include "Kernmodule4_Globals.php";

	$session_id = 0;
	$server_id = htmlspecialchars(GetURLVariable("id", -1 -1, ""));
	$server_password = htmlspecialchars(GetURLVariable("password", -1, -1, ""));

    $PasswordResult = $mysqli->query("SELECT id FROM servers WHERE id ='" . $server_id . "' AND password ='". $server_password . "'");
    $row = $PasswordResult->fetch_assoc();
	
	if($row["id"] == $server_id)
    {
        session_start();
        $_SESSION["server_id"] = $server_id;
        echo(session_id());
    }

    else
    {
        echo("0");
    }	
?>