Param (
    [Parameter(Mandatory = $true)]
    [String] $ResourceGroupName,

    [Parameter(Mandatory = $true)]
    [String] $StorageAccountName,

    [Parameter(Mandatory = $true)]
    [String] $ContainerName,

    [Parameter(Mandatory = $true)]
    [String] $FileName,

    [Parameter(Mandatory = $false)]
    [String] $DirectoryName,

    [Parameter(Mandatory = $false)]
    [string]
    [ValidateLength(1, 7)]
    [ValidatePattern("^[a-zA-Z]+$")]
    $Permissions
)

# Setting default values
if ([string]::IsNullOrEmpty($Permissions)) {
    $Permissions = 'r'
    Write-Output "Permissions set to default value '$Permissions'."
}
else {
    Write-Output "Permissions '$Permissions' used from parameter"
}

if ([string]::IsNullOrEmpty($DirectoryName)) {
    $Blob = $FileName
}
else {
    $Blob = '{0}\{1}' -f $DirectoryName, $Filename
}
Write-Output ('SasToken generated for file {0}.' -f $Blob)

try {

    $Key0 = Get-AzStorageAccountKey -ResourceGroupName $ResourceGroupName -Name $StorageAccountName | Select-Object -First 1 -ExpandProperty Value
    $Context = New-AzStorageContext -StorageAccountName $StorageAccountName -StorageAccountKey $Key0

    # To generate sastokens accountkey is needed. Below option is not possible in this situation
    # $Context = New-AzStorageContext -StorageAccountName $StorageAccountName -UseConnectedAccount

    $Params = @{
        'Container'  = $ContainerName
        'Blob'       = $Blob
        'Permission' = $Permissions
        'Context'    = $Context
        'ExpiryTime' = (Get-Date).AddDays(1).ToUniversalTime()
    }
    $SASToken = New-AzStorageBlobSASToken @Params
    Write-output ('New SasToken is generated valid for 24h.')

    # Convert SasToken to securestring
    # $SecureSaSToken = ConvertTo-SecureString -String ($SASToken) -AsPlainText -Force
    Write-Output -InputObject ('##vso[task.setvariable variable=SASToken;issecret=true;]{0}' -f $SaSToken)
    return $SasToken
}
catch {
    Write-Error $_
}