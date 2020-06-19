<?php
	include "Kernmodule4_Connection.php";
	include "Kernmodule4_Globals.php";

	$user_id = null;
	$user_password = htmlspecialchars( GetURLVariable("password", -1, -1, ""));
	$user_email = htmlspecialchars( GetURLVariable("email", -1 -1, ""));

	$user_result = 0;
	if (filter_var($user_email, FILTER_VALIDATE_EMAIL)) 
    {
		$query = "SELECT id, first_name, last_name FROM `users` WHERE password = '$user_password' and `email` = '$user_email' limit 1"; 
		$result = mysqli_query($mysqli, $query);
    
		if (mysqli_num_rows($result) > 0) 
        {
			while($row = mysqli_fetch_assoc($result)) 
            {
				$user_result = json_encode($row);
				$user_id = $row["id"];
				$_SESSION["user_id"] = $user_id;
			}
		}
	}

	echo $user_result;
?>