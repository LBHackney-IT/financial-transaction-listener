[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [String]$name,

    [Parameter(Mandatory = $true)]
    [String]$alternateName
)

if ($name -match '-') {
    Write-Host "`nModifying Input to avoid C# namespaces naming error ('-' not allowed)."
    Write-Host $("Changing input from '{0}' to '{1}'.`n" -f $name, $name.replace('-', '_'))
    $name = $name.replace('-', '_')
}

Write-Host "`nScanning files...`n"

Get-ChildItem -Path $PSScriptRoot -File -Recurse -exclude *.ps1 | % {
    $contents = (Get-Content $_.PSPath)
    $fileName = $_.Name

    if ($contents -match "BaseListener") {
        $contents -replace 'BaseListener', $name | Set-Content $_.PSPath
        Write-Host $("'{0}': contents changed." -f $fileName)

		$contents = (Get-Content $_.PSPath)
    }
	
    if ($contents -match "base-listener") {
        $contents -replace 'base-listener', $alternateName | Set-Content $_.PSPath
        Write-Host $("'{0}': contents changed." -f $fileName)
    }

    if ($fileName -match 'BaseListener') {
        $newName = $_.Name -replace 'BaseListener', $name
        Rename-Item -Path $_.PSPath -NewName $newName
        Write-Host $("File renamed from '{0}' to '{1}'." -f $fileName, $newName)
    }
}

Write-Host "`nScanning directories...`n"

Get-ChildItem -Path $PSScriptRoot -Directory -Recurse |
Sort-Object -Descending FullName |
Where-Object { $_.Name -match 'BaseListener' } | % {
    Write-Host $("Editing directory: '{0}'." -f $_.FullName)
    $newDirName = $_.Name -replace 'BaseListener', $name
    Rename-Item -Path $_.FullName -NewName $newDirName
    Write-Host $("Directory renamed from '{0}' to '{1}'." -f $_.Name, $newDirName)
}

Write-Host "Renaming done.`n`n"

Write-Host $("Do you want to delete Renamer script file?`nTarget filepath: '$PSCommandPath'.")

do { $myInput = (Read-Host 'Delete Script? (Y/N)').ToLower() } while ($myInput -notin @('y', 'n'))
if ($myInput -eq 'y') {
    Remove-Item -Force $PSCommandPath
    Write-Host "Script file was deleted."
}
else {
    Write-Host "Keeping the script file."
}
