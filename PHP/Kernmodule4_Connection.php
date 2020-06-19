<?php
   $db_user = '';
   $db_pass = '';
   $db_host = '';
   $db_name = '';

/* Open a connection */
$mysqli = new mysqli("$db_host","$db_user","$db_pass","$db_name");

function showerror($error,$errornr) 
{
    die("Error (" . $errornr . ") " . $error);
}

/* check connection */
if ($mysqli->connect_errno) 
{
   echo "Failed to connect to MySQL: (" . $mysqli->connect_errno() . ") " . $mysqli->connect_error();
   exit();
}
else 
{
	echo("Succesfully connected to the database <BR>");
}
?>