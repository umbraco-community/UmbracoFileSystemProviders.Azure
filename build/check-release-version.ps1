$tagged=$Env:APPVEYOR_REPO_TAG
$tag_name=$Env:APPVEYOR_REPO_TAG_NAME
$version=[Version]$Env:APPVEYOR_BUILD_VERSION
$branch=$Env:APPVEYOR_REPO_BRANCH

$Env:tagged_release_build = "false"

#$tagged="true"
#$tag_name="v2.1.2-beta"
#$version=[Version]"2.1.2.12"

$has_suffix = "false"
$suffix = ""

"APPVEYOR_REPO_TAG:" + $tagged
"APPVEYOR_REPO_TAG_NAME:" + $tag_name

If ($tagged -eq "true" -and $tag_name.StartsWith("v")){

	# This is to cut out the beta etc
	if ($tag_name.Contains("-")){
		$has_suffix = "true"
		$index = $tag_name.indexof("-");
		$suffix = $tag_name.Substring($index + 1, $tag_name.Length - ($index + 1))
		$tag_name = $tag_name.Substring(0, $index)
	}

	$tagged_version=[Version]$tag_name.Substring(1)

	if ($tagged_version.Major -eq $version.Major -and $tagged_version.Minor -eq $version.Minor -and $tagged_version.Build -eq $version.Build){
		"** THIS IS A TAGGED RELEASE BUILD:" + $tagged_version.ToString() + " **"

		$Env:tagged_release_build = "true"

		$basever=$tagged_version.Major.ToString() + "." + $tagged_version.Minor.ToString() + "." + $tagged_version.Build.ToString()

		if ($has_suffix -eq "true"){
			$Env:version_suffix = $suffix

			$Env:mssemver = $basever + "-" + $suffix
			$Env:appveyor_version = $basever + "-" + $suffix
			$Env:semver = $basever + "-" + $suffix
		} Else {
			$Env:mssemver = $basever
			$Env:appveyor_version = $basever
			$Env:semver = $basever
		}

		"Envrionment variable 'semver' set:" + $Env:semver
		"Envrionment variable 'mssemver' set:" + $Env:mssemver
		"Envrionment variable 'appveyor_version' set:" + $Env:appveyor_version
	}Else{
		"** Naughty, naughty, very naughty, tagged version: " + $tagged_version.ToString() + " doesn't match build version:" + $version.ToString() + " **"
		"** If this was intentional you should manually bump the version in appveyor.yml **"
		$host.SetShouldExit(1)
	}
}