function Get-ScriptDirectory
{
    $invocation = (Get-Variable MyInvocation -Scope 1).Value
    Split-Path $invocation.MyCommand.Path
}

function ZipFiles($zipfilename, $sourceDir)
{
    Add-Type -Assembly System.IO.Compression.FileSystem
    $compressionLevel = [System.IO.Compression.CompressionLevel]::Optimal
    [System.IO.Compression.ZipFile]::CreateFromDirectory($sourceDir, $zipfilename, $compressionLevel, $false)
}

# $zipFileName --> The name of the zip file to create, without the .zip file extension.
# $projectRelativePath --> The path of the worker project's release output directory to be zipped, relative to the directory of this script.
function CreateApplicationPackage($zipfilename, $projectRelativePath)
{
    # Get the folder for storing the zip files.
    $appPkgZipsDir = join-path -path $scriptDir -childpath "\AppPkgZips"
    Write-Output "AppPkgZipsDir is $appPkgZipsDir"

    # Get the temp/staging folder to copy binaries into for zipping.
    $appPkgTmpDir = join-path -path $appPkgZipsDir -childpath ([IO.Path]::Combine("\~tmp", $zipfilename))
    Write-Output "AppPkgTmpDir is $appPkgTmpDir"

    # Get the release build of the specified project's files.
    $projectReleaseDir = [IO.Path]::GetFullPath((join-path $scriptDir $projectRelativePath))
    Write-Output "ProjectReleaseDir is $projectReleaseDir"

    # Get the application package zip file path.
    $appPkgZip = join-path -path $appPkgZipsDir -childpath ($zipfilename + ".zip")
    Write-Output "Application package is $appPkgZip"

    # Delete the app package's staging/temp directory if it exists.
    if (Test-Path $appPkgTmpDir)
    {
        Write-Output "Deleting $appPkgTmpDir"
        remove-item $appPkgTmpDir -Recurse
        Write-Output "Deleted $appPkgTmpDir"
    }

    # Delete the existing application package zip file if it exists.
    if (Test-Path $appPkgZip)
    {
        Write-Output "Deleting $appPkgZip"
        remove-item $appPkgZip
        Write-Output "Deleted $appPkgZip"
    }

    # Create the app package staging/temp directory.
    Write-Output "Creating $appPkgTmpDir"
    new-item -Path $appPkgTmpDir -ItemType Directory
    Write-Output "Created $appPkgTmpDir"

    # Copy the release build of the specified project's files.
    robocopy $projectReleaseDir $appPkgTmpDir /XF "*.pdb" "*.tmp"

    Write-Output "Creating $appPkgZip"
    ZipFiles $appPkgZip $appPkgTmpDir
    Write-Output "Created $appPkgZip"
}

# Get the root directory for creating the application package.
$scriptDir = Get-ScriptDirectory


CreateApplicationPackage "PiEstimatorWorker" ".\bin\Release\net461"
