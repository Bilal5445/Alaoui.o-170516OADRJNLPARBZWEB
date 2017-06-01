<?php 

	$method = $_SERVER['REQUEST_METHOD'];

	$serverRoot = $_SERVER['DOCUMENT_ROOT'];
	
	$postData = file_get_contents('php://input');

	$inputFilePath = $serverRoot . '/namatedev-17028oadrjnlparbz-991d3268755f/example/small-example.arabizi';
	$inputTranslFile = fopen($inputFilePath, "w");
	fwrite($inputTranslFile, $postData);
	fclose($inputTranslFile);

	shell_exec('sh ' . $serverRoot . '/namatedev-17028oadrjnlparbz-991d3268755f/RUN_transl_pipeline.sh');

	$outputFilePath = $serverRoot . '/namatedev-17028oadrjnlparbz-991d3268755f/example/small-example.7.charTransl';
	$outputTranslFile = fopen($outputFilePath, "r");
	$arabicOutput = fread($outputTranslFile, filesize($outputFilePath));
	fclose($outputTranslFile);

	echo $postData;
	//echo $arabicOutput;
 ?>