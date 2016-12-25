param (
    [Parameter(Mandatory=$true)][string]$version = 0
)

# projects to build
$projects = @(
    "Kronos.sln"
)

# build function for project
function Build($path) {
    dotnet build $path -c Release --version-suffix $version --no-incremental
}

function RestorePackages(){
    dotnet restore
}

write-host "Build started"

# restore packages
write-host "Restoring packages"
RestorePackages

# build each project
foreach ($project in $projects){
    write-host "Building " $project "with suffix version " $version
    Build($project)
}

# Set build as failed if any error occurred
if($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode )  }

write-host "Build finished"