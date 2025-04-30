<#
.SYNOPSIS
Creates an Azure Storage Account.

.DESCRIPTION
Creates a Storage Account with the specified name and settings.

.OUTPUTS
HashTable
Information on the Created Resource"

.EXAMPLE
Create-StorageAccount.ps1 -ResourceGroupName ccoe-dta01-rg -StorageAccountName ccoedta01tstsa -StorageAccountSku Standard_LRS -StorageAccountKind BlobStorage -AccessTier Hot -IpRules '84.24.135.30'

This command will create storage account ccoedta01tstsa in the same Azure region as the resource group.
Public IP address 84.24.135.30 is configured in the firewall to allow access.
#>
[CmdletBinding()]
Param (
    [Parameter(
        Mandatory = $true,
        HelpMessage = "Name of the Resource Group where the Storage Account will be created")]
    [String]$ResourceGroupName,

    [Parameter(
        Mandatory = $true,
        HelpMessage = "Storage Account Name that will be created in the Resource Group name")]
    [ValidateLength(3, 24)]
    [ValidatePattern("^[a-z0-9]+$")]
    [String]$StorageAccountName,

    [Parameter(
        Mandatory = $true,
        HelpMessage = "Sku for the Storage Account being created")]
    [ValidateSet('Standard_LRS', 'Standard_GRS', 'Standard_RAGRS', 'Standard_ZRS', 'Standard_GZRS', 'Standard_RAZRS', 'Premium_LRS')]
    [String]$StorageAccountSku,

    [Parameter(
        Mandatory = $true,
        HelpMessage = "Kind of Storage Account being created (Blob Storage, Storage Account v2 or Data Lake Store)")]
    [ValidateSet('BlobStorage', 'BlockBlobStorage', 'FileStorage', 'StorageV2', 'DataLakeStoreGen2')]
    [String]$StorageAccountKind,

    [Parameter(
        Mandatory = $false,
        HelpMessage = "Access tier for the Storage Account being created")]
    [ValidateSet('Hot', 'Cool')]
    [String]$AccessTier,

    [Parameter(
        Mandatory = $false,
        HelpMessage = "Public IP addresses and/or IP address ranges which should be granted explicit permission to the storage account.")]
    [Array]$IpRules,

    [Parameter(
        Mandatory = $false,
        HelpMessage = "To Enable Nfsv3 protocol for storage account.")]
    [Boolean]$isNfsV3Enabled,

    [Parameter(
        Mandatory = $false,
        HelpMessage = "Provide the Azure location in where the storage account is to be provisioned")]
    [String]$Location,

    [Parameter(Mandatory = $false)]
    [Switch]$DoNotSendMetrics,

    [Parameter(
        Mandatory = $false,
        HelpMessage = "To enable/disable usage of shared access key. Default is disabled")]
    [Boolean]$SharedAccessKeyEnabled = $false

)
Function Test-IpRules {
    <#
    .DESCRIPTION
    Validate the informed IP rules, if any, and return a concatenated string with all the values.
    #>
    [CmdLetBinding()]
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSUseSingularNouns", "")]
    Param(
        [Parameter(Mandatory = $false)]
        [Array] $IpRules
    )
    $PrivateIpAddressPattern = '(^127\.)|(^192\.168\.)|(^10\.)|(^172\.1[6-9]\.)|(^172\.2[0-9]\.)|(^172\.3[0-1]\.)|(^::1$)|(^[fF][cCdD])'
    $IPAddressPattern = '^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$|^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])(\/([0-9]|[1-2][0-9]|3[0]))$'
    if (-Not [string]::IsNullOrWhiteSpace($IpRules)) {
        foreach ($IpRangeOrAddress In $IpRules) {
            if ($IpRangeOrAddress -match $PrivateIpAddressPattern) {
                throw "Only public IP addresses are allowed"
            }
            elseif ($IpRangeOrAddress -notmatch $IPAddressPattern) {
                throw "'$IpRangeOrAddress' is not a valid IPv4 address or CIDR range (CIDR must be smaller than or equal to 30)."
            }
        }
    }

    $IpRulesString = $IpRules -Join ","

    $IpRulesString
}

$IpRulesString = Test-IpRules -IpRules $IpRules

Write-Verbose "Get template for storage account"
if ($StorageAccountKind -eq 'DataLakeStoreGen2') {
    $StorageAccountKind = 'StorageV2'

    if ($StorageAccountSku -match "Premium") {
        throw "Data Lake Storage Gen2 does not support the premium storage sku."
    }

    $TemplateFile = Join-Path -Path $PSScriptRoot -ChildPath "..\arm\dls2.json"
    [hashtable]$ParametersARM = @{
        storageAccountName = $StorageAccountName
        storageAccountSku  = $StorageAccountSku
        storageAccountKind = $StorageAccountKind
        ipRange            = $IpRulesString
        isNfsV3Enabled     = $isNfsV3Enabled
    }
}
else {

    if ($StorageAccountKind -eq 'BlockBlobStorage' -or $StorageAccountKind -eq 'FileStorage') {
        if ($StorageAccountSku -ne 'Premium_LRS') {
            throw "Storage account kind '$($StorageAccountKind)' is only available on Premium Locally-redundant storage (Premium_LRS). `n You selected: '$($StorageAccountSku)'"
        }
    }
    if ($isNfsV3Enabled -eq 'true') {
        if ($StorageAccountKind -ne 'DataLakeStoreGen2') {
            throw "Storage account NfsV3 protocol feature only available with StorageAccountKind (DataLakeStoreGen2)."
        }
    }

    $TemplateFile = Join-Path -Path $PSScriptRoot -ChildPath "..\arm\storageAccount.json"
    [hashtable]$ParametersARM = @{
        storageAccountName = $StorageAccountName
        storageAccountSku  = $StorageAccountSku
        storageAccountKind = $StorageAccountKind
        ipRange            = $IpRulesString
    }
}

#Below is to make sure InfraEncryption is enabled on new Storage Account(SA)
$SA = Get-AzStorageAccount -ResourceGroupName $ResourceGroupName -StorageAccountName $StorageAccountName -ErrorAction 'SilentlyContinue'
if (-Not $SA) {
    $ParametersARM.Add('newOrExisting', "new")
}
else {
    $ParametersARM.Add('newOrExisting', "existing")
}

if (-Not ([string]::IsNullOrWhiteSpace($Location))) {
    $ParametersARM.Add('location', $Location)
}

if (-Not ([string]::IsNullOrWhiteSpace($AccessTier))) {
    $ParametersARM.Add('accessTier', $AccessTier)
}

#Limiting Deployment name to 64 characters
$DeploymentName = "ARM-deployment-$( -join ((0x30..0x39) + ( 0x41..0x5A) + ( 0x61..0x7A) | Get-Random -Count 48  | ForEach-Object {[char]$_}) )"

Write-Host "Deploy storage account ARM template"
Write-Verbose $TemplateFile
try {
    [hashtable]$DeploymentParameters = @{
        Name                    = $DeploymentName
        ResourceGroupName       = $ResourceGroupName
        TemplateFile            = $TemplateFile
        TemplateParameterObject = $ParametersARM
    }
    Write-Verbose ($DeploymentParameters | ConvertTo-Json -Depth 4)
    $Result = New-AzResourceGroupDeployment @DeploymentParameters
}
catch {
    $ErrorMessage = $_.Exception.Message
    Write-Verbose ($ErrorMessage | ConvertTo-Json -Depth 10)
    if ($ErrorMessage.Contains('isHnsEnabled') -and $ErrorMessage.Contains('AccountPropertyCannotBeUpdated')) {

        Write-Error $_.Exception -ErrorAction Continue
        throw 'Data Lake Store Gen2 features can only be enabled/disabled during the creation of a Storage Account.'
    }

    # When the ARM template is not valid, the details are logged in the Azure Activity Logs, but not returned by New-AzResourceGroupDeployment
    # Execute a Test-AzResourceGroupDeployment to try to get the actual error message.
    if ($_.Exception.Message.Contains('Code=InvalidTemplateDeployment')) {
        Write-Host 'Code=InvalidTemplateDeployment'
        $DeploymentParameters.Remove('Name')
        $DetailsList = (Test-AzResourceGroupDeployment @DeploymentParameters)
        $DetailsItem = $DetailsList[0]
        $lastErrorMessage = $_.Exception.Message
        while ($DetailsItem) {
            $errorMessage = "$($DetailsItem.Code): $($DetailsItem.Message)"
            Write-Warning $errorMessage -ErrorAction Continue
            $lastErrorMessage = $errorMessage
            # It's a chained list of detail objects.
            $DetailsItem = $DetailsItem.Details
        }
        throw $lastErrorMessage
    }

    throw $_
}

if ($SharedAccessKeyEnabled -eq $true) {
    Write-Host "Enabling shared access key for storage account with SasExpirationPeriod 7 days."
    Set-AzStorageAccount -ResourceGroupName $ResourceGroupName -AccountName $StorageAccountName -AllowSharedKeyAccess $SharedAccessKeyEnabled -SasExpirationPeriod 7.00:00:00
}
else {
    Set-AzStorageAccount -ResourceGroupName $ResourceGroupName -AccountName $StorageAccountName -AllowSharedKeyAccess $SharedAccessKeyEnabled
}

$output = [HashTable] @{
    ResourceID   = $Result.Outputs['resourceID'].Value
    ResourceName = $Result.Outputs['resourceName'].Value
}

if ($OutputVariableName) {
    Write-Host "Writing output variable:"
    $output.Keys | ForEach-Object {
        Write-Host "- $OutputVariableName.$($_) = $($output[$_])"
        Write-Host "##vso[task.setVariable variable=$OutputVariableName.$($_)]$($output[$_])"
        Write-Host "##vso[task.setVariable variable=$OutputVariableName.$($_);isOutput=true]$($output[$_])"
    }
}

Write-Output $output
