<?php
	include "Kernmodule4_Connection.php";	

    $queryTop = "SELECT user_id, AVG(score), date_time as avg_score FROM scores WHERE date_time > date_sub(now(), interval 1 month) GROUP BY score ORDER BY AVG(score) DESC LIMIT 5";
              
    if (!($result = $mysqli->query($queryTop))) 
    {
        showerror($mysqli->errno,$mysqli->error);
    }   
       
    while ($row = $result->fetch_assoc())
    {
            echo json_encode($row);
            echo "<br>";
    }
    
    $queryPlays = "SELECT COUNT(*) FROM (SELECT date_time FROM scores WHERE date_time > date_sub(now(), interval 1 month)) AS subquery";     
    if (!($result2 = $mysqli->query($queryPlays))) 
    {
        echo '0';
    }  
    
    echo ('{"monthlyplays:' . json_encode($result2->fetch_assoc()) . '}');
?>