$ErrorActionPreference = "Stop"

function LogOperationStart($message) {
    Write-Host $message -NoNewline
}

function LogSuccess() {
    Write-Host -ForegroundColor Green "...Done"
}

function LogErrorAndExit($errorMessage, $exception) {
    Write-Host -ForegroundColor Red "...Failed" 
    if ($exception -ne $null) {
        Write-Host $exception -ForegroundColor Red | format-list -force
    } 
    throw $errorMessage
}

Function validateFeedJson($fileName)
{
    try {
        $file = Get-Content $fileName -Raw
        $jsonObj = ConvertFrom-Json $file
    } catch  {
        LogErrorAndExit "The feed content for $fileName is not valid JSON"  $_.Exception
    }
}

# start tests

LogOperationStart "Checking if the feed content is valid json"
$files = Get-ChildItem -Filter "cli-feed*.json"
foreach ($file in $files) {
    validateFeedJson $file.Name
}
LogSuccess 