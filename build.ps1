$ErrorActionPreference = "Stop"

function LogOperationStart($message) {
    Write-Host $message -NoNewline
}

function LogSuccess() {
    Write-Host -ForegroundColor Green "...sssDone"
}

function LogErrorAndExit($errorMessage, $exception) {
    Write-Host -ForegroundColor Red "...Failed" 
    if ($exception -ne $null) {
        Write-Host $exception -ForegroundColor Red | format-list -force
    } 
    throw $errorMessage
}

Function validateFeedJson()
{
    try {
        $file = Get-Content ".\cli-feed-v3.json" -Raw
        $jsonObj = ConvertFrom-Json $file
    } catch  {
        LogErrorAndExit "The feed content is not valid JSON"  $_.Exception
    }
}

# start tests

LogOperationStart "Checking if the feed content is valid json"
validateFeedJson
LogSuccess 