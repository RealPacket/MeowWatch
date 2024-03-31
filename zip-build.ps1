$InputFolder = "build"

$InputFolderName = Split-Path -Path $InputFolder -Leaf
$ZipFileName = $InputFolderName + ".zip"
$ZipFilePath = $ZipFileName
$ZIP_BYTES = (80, 75, 05, 06, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 001)


# Check if the zip file exists and create it if not
if (-Not (Test-Path -Path $ZipFilePath)) {
    New-Item -Path $ZipFilePath -ItemType File
    [IO.File]::WriteAllBytes($ZipFilePath, $ZIP_BYTES)
}

# Loop through each subfolder in the input folder
$Subfolders = Get-ChildItem $InputFolder | Where-Object { $_.PSIsContainer }
ForEach ($Subfolder in $Subfolders) {
    # Get the subfolder path
    $SubfolderPath = $Subfolder.FullName

    # Add the subfolder to the zip file
    Compress-Archive -Path $SubfolderPath -DestinationPath $ZipFilePath -Update
}