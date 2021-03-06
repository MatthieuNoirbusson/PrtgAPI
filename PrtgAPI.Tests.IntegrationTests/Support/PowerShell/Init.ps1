﻿. "$PSScriptRoot\..\..\..\PrtgAPI.Tests.UnitTests\Support\PowerShell\Init.ps1"

if(!(Get-Module -ListAvailable Assert))
{
    Install-Package Assert -ForceBootstrap -Force | Out-Null
}

function ServerManager
{
    return [PrtgAPI.Tests.IntegrationTests.BasePrtgClientTest]::ServerManager
}

function Startup($testName)
{
    StartupSafe $testName

    (ServerManager).Initialize();
}

function Log($message, $error = $false)
{
    [PrtgAPI.Tests.IntegrationTests.Logger]::Log($message, $error, "PS")
    Write-Host $message
}

function LogTest($message, $error)
{
    if($error -ne $true)
    {
        $error = $false
    }

    [PrtgAPI.Tests.IntegrationTests.Logger]::LogTest($message, $error, "PS")
    Write-Host $message
}

function LogTestName($message, $error = $false)
{
    [PrtgAPI.Tests.IntegrationTests.Logger]::LogTestDetail($message, $error, "PS")
}

function LogTestDetail($message, $error = $false)
{
    LogTestName "    $message" $error
}

function StartupSafe($testName)
{
    Write-Host "Performing startup tasks"
    InitializeModules "PrtgAPI.Tests.IntegrationTests" $PSScriptRoot

    $path = (Get-Module "PrtgAPI.Tests.IntegrationTests").Path
    $slash = $path.LastIndexOf("\")
    $substr = $path.Substring(0, $slash + 1)
    $psd1 = $substr + "PrtgAPI.Tests.psd1"
    ipmo $psd1

    SetTestName $testName

    if(!(Get-PrtgClient))
    {
        InitializePrtgClient
    }

    if($global:PreviousTest -and !$psISE)
    {
        Log "Sleeping for 30 seconds as tests have run previously"
        Sleep 30

        Log "Refreshing objects"

        Get-Device | Refresh-Object

        Log "Waiting for refresh"
        Sleep 30
    }
    else
    {
        try
        {
            Get-SensorTotals
        }
        catch [exception]
        {
            (ServerManager).StartServices()
            (ServerManager).ValidateSettings()

            Log "PRTG service may still be starting up; pausing 15 seconds"
            Sleep 10

            try
            {
                Get-Sensor | Refresh-Object
                Sleep 5
            }
            catch
            {
                LogTest "Failed to refresh objects: $($_.Exception.Message)"
            }
        }
    }

    (ServerManager).RepairState()
    (ServerManager).WaitForObjects()
}

function SetTestName($name)
{
    [PrtgAPI.Tests.IntegrationTests.Logger]::PSTestName = $name
}

function ClearTestName
{
    try
    {
        SetTestName $null
    }
    catch
    {
    }
}

function InitializePrtgClient
{
    Log "Starting PowerShell Tests"

    (ServerManager).ValidateSettings()
    (ServerManager).StartServices()

    try
    {
        Log "Connecting to PRTG Server"
        Connect-PrtgServer (Settings ServerWithProto) (New-Credential prtgadmin prtgadmin) -Force
    }
    catch [exception]
    {
        Log $_.Exception.Message

        if(!($Global:FirstRun))
        {
            $Global:FirstRun = $true
            Log "Sleeping for 30 seconds as its our first test and we couldn't connect..."
            Sleep 30
            Log "Attempting second connection"

            try
            {
                Connect-PrtgServer (Settings ServerWithProto) (New-Credential prtgadmin prtgadmin) -Force

                Log "Connection successful"
            }
            catch [exception]
            {
                Log $_.Exception.Message $true
                throw
            }

            Log "Refreshing all sensors"

            Get-Sensor | Refresh-Object

            Log "Sleeping for 30 seconds"

            Sleep 30
        }
        else
        {
            throw
        }
    }
}

function Shutdown
{
    Log "Performing cleanup tasks"
    (ServerManager).Cleanup()

    $global:PreviousTest = $true
}

function Settings($property)
{
    $val = [PrtgAPI.Tests.IntegrationTests.Settings]::$property

    if($val -eq $null)
    {
        throw "Setting '$property' could not be found."
    }

    return $val
}

function It {
    [CmdletBinding(DefaultParameterSetName = 'Normal')]
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$name,

        [Parameter(Mandatory = $true, Position = 1)]
        [ScriptBlock] $script,

        [Parameter(Mandatory = $false)]
        [System.Collections.IDictionary[]] $TestCases
    )

    Pester\It $name {

        try
        {
            if($null -eq $TestCases)
            {
                LogTestName "Running test '$name'"

                & $script
            }
            else
            {
                foreach($test in $TestCases)
                {
                    LogTestName "Running test '$($name): $($test["name"])'"

                    & $script @test
                }
            }
        }
        catch [exception]
        {
            $messages = @($_.Exception.Message -split "`n") -replace "`r",""

            foreach($message in $messages)
            {
                LogTestDetail $message $true
            }

            if($_.Exception.StackTrace -ne $null)
            {
                $stackTrace = ($_.Exception.StackTrace -split "`n") -replace "`r",""

                foreach($line in $stackTrace)
                {
                    LogTestDetail " $line" $true
                }
            }

            throw
        }
    }
}