Param (
    [Parameter(Mandatory = $true)]
    [string] $ResourceGroupName,

    [Parameter(Mandatory = $true)]
    [string] $StorageAccountName,

    # Should be a comma delimited string such as: "8.8.8.8,28.1.0.0/16"
    [Parameter(Mandatory = $false)]
    [string] $IPAddressOrRange
)

# Constants
$Url = "http://ifconfig.me/ip"

if ([string]::IsNullOrEmpty($IPAddressOrRange)) {
    Write-Host "Obtaining public IP address of the agent"
    $IPAddressOrRangeArray = (Invoke-WebRequest -Uri $Url -ErrorAction SilentlyContinue).Content
    Write-Host "Found IP address: '$IPAddressOrRangeArray'"
}
else {
    # Check if IPAddressOrRange is a valid IP address or CIDR range
    $IPAddressOrRangeArray = ($IPAddressOrRange.split(",")).replace(' ', '')
    if (!([string]::IsNullOrEmpty($IPAddressOrRange.Trim()))) {
        foreach ($IPAddressOrRange In $IPAddressOrRangeArray) {
            Write-Host "Validating IP address '$IPAddressOrRange'"
            [bool] $IsValidIP = $IPAddressOrRange -Match '^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])(\/([0-9]|[1-2][0-9]|3[0-2]))?$'

            if (!$IsValidIP) {
                Write-Error "The specified IP address is not a valid IPv4 address or CIDR range."
                $HasError = $true
            }
        }
    }
}

if ($HasError) {
    return
}

# Get the current IP rules:
Write-Host "Retrieving existing storage account IP address rules"
$CurrentRuleSet = (Get-AzStorageAccountNetworkRuleSet -ResourceGroupName $ResourceGroupName -AccountName $StorageAccountName).IPRules.IPAddressOrRange

foreach ($IPAddressOrRangeItem in $IPAddressOrRangeArray) {
    Write-Host "Handling IP address (range): '$IPAddressOrRangeItem'"
    If ($CurrentRuleSet -notcontains $IPAddressOrRangeItem) {
        [int]$Count = 0
        do {
            Write-Host "Adding IP address (range) to storage account IP address allow list"
            Add-AzStorageAccountNetworkRule -ResourceGroupName $ResourceGroupName -AccountName $StorageAccountName -IPAddressOrRange $IPAddressOrRangeItem

            Write-Host "Validating if IP address (range) is added successfully"
            $CheckNetworkRule = (Get-AzStorageAccountNetworkRuleSet -ResourceGroupName $ResourceGroupName -AccountName $StorageAccountName).IPRules.IPAddressOrRange
            Write-Host "$($CheckNetworkRule | Out-String)"
            $Failed = $false
            if ($CheckNetworkRule -notcontains $IPAddressOrRangeItem) {
                $Failed = $true
                $Count++
                Write-Host "Failed to add IP address (range) to the firewall rules - pausing for 10 seconds before next attempt"
                Start-Sleep -Seconds 10
            }
            else {
                Write-Host 'IP address (range) successfully added to the allow list'
            }
        } while ($Failed -or $Count -gt 12)
    }
    else {
        Write-Host 'IP address (range) already present to the allow list'
    }
}