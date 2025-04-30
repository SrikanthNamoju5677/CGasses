<#
    .SYNOPSIS
        Add Role assignements to the created storage account

    .DESCRIPTION
        This function will add any role assignment selected by the user to the storage account that is being created by the extension

    .PARAMETER ResourceGroupName
        Name of the resource group that contains the Storage Account

    .PARAMETER StorageAccountName
        Name of Storage Account that was created

    .PARAMETER ServicePrincipalId
        Id of the user Service Principal (This is the user running the extension context)

    .PARAMETER Roles
        Comma-seperated list of roles to be added to the Service Principal for this Storage Account

    .EXAMPLE
        Add-RoleAssignments -ResourceGroupName ResourceGroupA -StorageAccountName appstorage -ServicePrincipalId user@asml.com -Roles "Storage Blob Data Contributor,Storage Blob Data Owner"
#>
[CmdLetBinding()]
Param(
    [Parameter(Mandatory = $true)]
    [String] $ResourceGroupName,
    [Parameter(Mandatory = $true)]
    [String] $StorageAccountName,
    [Parameter(Mandatory = $true)]
    [String] $Roles,
    [Parameter(Mandatory = $false)]
    [String] $ServicePrincipalId
)

# Retrieve the ServicePrincipalId from the current context
if ([string]::IsNullOrWhiteSpace($ServicePrincipalId)) {
    $ContextAccountId = (Get-AzContext).Account.Id
    $ServicePrincipal = Get-AzADServicePrincipal -ServicePrincipalName $ContextAccountId
    $ServicePrincipalId = $ServicePrincipal.Id
}

Function Wait-StorageAccountProvisioning {
    <#
        .SYNOPSIS
            Wait for the provisioning of the Storage Account
        .DESCRIPTION
            Waits for the provisioning of the Storage Account. Max waiting time is 2 minutes.
        .PARAMETER StorageAccountName
            Name of the storage account being provisioned
        .EXAMPLE
            Wait-StorageAccountProvisioning -StorageAccountName mystorageaccount
    #>
    [CmdLetBinding()]
    Param(
        [Parameter(Mandatory = $true)]
        [String]$StorageAccountName
    )
    $StorageAccount = (Get-AzStorageAccount | Where-Object { $_.StorageAccountName -eq $StorageAccountName })
    $SleepInSeconds = 2

    While ($null -ne $StorageAccount) {
        Write-Verbose "Waiting for $SleepInSeconds seconds to get the Storage Account '$StorageAccountName'..."
        Start-Sleep -Seconds $SleepInSeconds

        $StorageAccount = (Get-AzStorageAccount | Where-Object { $_.StorageAccountName -eq $StorageAccountName })

        $SleepInSeconds = $SleepInSeconds * 2
        if ($SleepInSeconds -gt 32) {
            break
        }
    }
}

Wait-StorageAccountProvisioning -StorageAccountName $StorageAccountName

$AvailableRoleAssignments = @(
    "Storage Account Contributor",
    "Storage Account Key Operator Service Role",
    "Storage Blob Data Contributor",
    "Storage Blob Data Owner",
    "Storage Blob Data Reader",
    "Storage Queue Data Contributor",
    "Storage Queue Data Message Processor",
    "Storage Queue Data Message Sender",
    "Storage Queue Data Reader"
)

# Set Role Assignment if selected in task
Foreach ($Role in $AvailableRoleAssignments) {
    $RoleName = $Role.Replace(' ', '')

    $RoleSelected = $Roles.Replace(' ', '').Contains($RoleName)
    If ($RoleSelected) {
        # Apply Role Assignment
        Write-Verbose -Message "Applying Role Assignment '$Role'"

        $Scope = (Get-AzResource -Name $StorageAccountName -ResourceGroupName $ResourceGroupName).ResourceId
        $RoleAssignments = Get-AzRoleAssignment -ObjectId $ServicePrincipalId -Scope $Scope
        If (($RoleAssignments).RoleDefinitionName -notcontains $Role) {
            # Set role on service principal
            $RoleAssignmentParams = @{
                RoleDefinitionName = $Role
                Scope              = $Scope
                ObjectId           = $ServicePrincipalId
            }

            Write-Verbose ($RoleAssignmentParams | ConvertTo-Json)
            New-AzRoleAssignment @RoleAssignmentParams -ErrorAction Stop

            Write-Output "Role '$Role' applied for '$ServicePrincipalId' on Storage Account '$StorageAccountName'"
        }
        Else {
            Write-Output "Role '$Role' already applied to '$ServicePrincipalId' on Storage Account '$StorageAccountName'"
        }
    }
}