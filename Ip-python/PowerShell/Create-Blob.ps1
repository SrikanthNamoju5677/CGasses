<#
    .SYNOPSIS
        Creates a new Blob Container within a Storage Account

    .DESCRIPTION
        Creates a new Blob Container within the specifiied Azure Storage Account

    .PARAMETER ResourceGroupName
        Name of the Resource Group where the Storage Account is located

    .PARAMETER StorageAccountName
        Name of the Storage Account where the Blob Container should be created

    .PARAMETER ContainerName
        Name of the Container that should be created

    .EXAMPLE
        .\Create-BlobContainer.ps1 -ResourceGroupName demo01-rg -StorageAccountName demo01-sa -ContainerName demo01-container
#>
Param (
    [Parameter(Mandatory = $true)]
    [string] $StorageAccountName,
    [Parameter(Mandatory = $true)]
    [ValidateScript( {
            $ContainerName = $_
            if ($ContainerName -cmatch '^[a-z0-9]{1}[a-z0-9-]{1,61}[a-z0-9]{1}$') {
                Return $True
            }
            else {
                Throw "Container name '$ContainerName' is invalid. Valid names start and end with a lower case letter or a number and has in between a lower case letter, number or dash with no consecutive dashes and is 3 through 63 characters long."
            }
        })]
    [string] $ContainerName
)

[int]$Retry = 12
[int]$SleepInSeconds = 15

Write-Host "Start creation of storage account container '$ContainerName'"

Write-Host 'Specifying storage account context'
$Context = New-AzStorageContext -StorageAccountName $StorageAccountName -UseConnectedAccount
Write-Verbose ($Context | ConvertTo-Json)

Write-Host "Verifying if container '$ContainerName' is already present"
$Container = Get-AzStorageContainer -Context $Context -Name $ContainerName -ErrorAction SilentlyContinue

if ($null -eq $Container) {
    [int]$Count = 1
    do {
        Write-Host "Creating blob storage container '$ContainerName' (Attempt '$Count' of $Retry)"
        $Container = New-AzStorageContainer -Context $Context -Name $ContainerName -ErrorAction SilentlyContinue -ErrorVariable Failed

        if ($Failed) {
            $Failed | Out-Host
        }

        if ($null -ne $Container) {
            break
        }

        $Count++

        if ($Count -le $Retry) {
            Write-Host "Pausing execution for $SleepInSeconds seconds"
            Start-Sleep -Seconds $SleepInSeconds
            $SleepInSeconds += 15

            Write-Host "Refreshing storage context"
            Remove-Variable -Name Context
            $Context = New-AzStorageContext -StorageAccountName $StorageAccountName -UseConnectedAccount
        }
    } while ($Count -le $Retry)

    if ($Count -gt $Retry -or $null -eq $Container) {
        Write-Error "Failed to create blob storage container '$ContainerName'"
        #throw $Failed
    }
}

Write-Host "Storage account container present"

@{
    ContainerName     = $Container.Name
    ContainerEndPoint = $Container.CloudBlobContainer.Uri
}

Write-Host "Storage account container creation completed"