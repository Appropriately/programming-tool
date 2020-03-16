# Script for handling final submission

$InputDirectory = "C:\Users\Sean Lewis\Documents\Unity\programming-tool"
$OutputDirectory = "C:\Users\Sean Lewis\Documents\Unity\programming-tool\COMP30040_9610214.zip"
$Exclude = @("Logs", "Packages", ".git", ".vscode", "Library", "ProjectSettings", "Temp", "obj")
$ExcludeFiles = @("*.csproj", ".git*", "*.zip", "*.sln")
$Temp = "C:\Temp"

# Temporarily move the /Plugins directory, because Powershell can't elegantly handle exluding subdirectories
$Path = -join($InputDirectory, "\Assets\Plugins")
Move-Item -Path $Path -Destination $Temp

Get-ChildItem -Path $InputDirectory -Exclude $ExcludeFiles |
   Where-Object { $_.Name -notin $Exclude} |
      Compress-Archive -DestinationPath $OutputDirectory -Update -Verbose

$Path = -join($InputDirectory, "\Assets")
Move-Item -Path "C:\Temp\Plugins" -Destination $Path