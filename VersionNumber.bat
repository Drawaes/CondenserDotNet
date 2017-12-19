IF "%APPVEYOR_REPO_TAG_NAME%"=="" (
ECHO ^<Project^>^<PropertyGroup^>^<VersionSuffix^>%APPVEYOR_BUILD_NUMBER%^</VersionSuffix^>^</PropertyGroup^>^</Project^> > version.props
) ELSE (
ECHO ^<Project^>^<Import Project=^"./releasenotes/%APPVEYOR_REPO_TAG_NAME%.props^" /^>^<PropertyGroup^>^<VersionPrefix^>%APPVEYOR_REPO_TAG_NAME%^</VersionPrefix^>^</PropertyGroup^>^</Project^> > version.props
)
