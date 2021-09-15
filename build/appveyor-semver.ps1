$version=[Version]$Env:APPVEYOR_BUILD_VERSION
$version_suffix=$Env:version_suffix

$basever=$version.Major.ToString() + "." + $version.Minor.ToString() + "." + $version.Build.ToString()

$paddedRevision = $version.Revision.ToString().PadLeft(6,"0")

$semver = $basever + "-" + $version_suffix + "." + $version.Revision.ToString()
$mssemver = $basever + "-" + $version_suffix + "-" + $paddedRevision
$appveyor_version = $mssemver

$Env:semver = $semver
$Env:mssemver = $mssemver
$Env:appveyor_version = $appveyor_version

$Env:appveyor_file_version = $Env:APPVEYOR_BUILD_VERSION

$Env:ms_file_version = $version.ToString()
$Env:padded_build_revision = $paddedRevision

"Envrionment variable 'semver' set:" + $Env:semver
"Envrionment variable 'mssemver' set:" + $Env:mssemver
"Envrionment variable 'appveyor_version' set:" + $Env:appveyor_version
"Envrionment variable 'padded_build_revision' set:" + $Env:padded_build_revision