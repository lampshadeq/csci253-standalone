<?php
  // Allow this file access to the AJAX data
  header("Access-Control-Allow-Origin: *");
  
  // Grab the data
  $like    = $_GET["like"];
  $comment = $_GET["comment"];
  $tag     = $_GET["tag"];
  $group   = $_GET["group"];
  $bday    = $_GET["bday"];
  $msg     = $_GET["msg"];
  $friend  = $_GET["friend"];
  
  // Create the file
  $file = fopen("notif.txt", "w");
  
  // Write number
  fwrite($file, "$like\r\n");
  fwrite($file, "$comment\r\n");
  fwrite($file, "$tag\r\n");
  fwrite($file, "$group\r\n");
  fwrite($file, "$bday\r\n");
  fwrite($file, "$msg\r\n");
  fwrite($file, $friend);
  
  // Close the file
  fclose($file);
?>