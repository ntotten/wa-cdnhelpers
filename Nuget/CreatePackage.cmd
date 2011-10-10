del ASPX\lib\Net40\CdnHelpers.dll
copy ..\CdnHelpers\bin\Release\CdnHelpers.dll ASPX\lib\Net40
copy ..\WebRole\cdn\Web.config ASPX\content\cdn
copy ..\WebRole\App_Data\CdnHelpers.xml ASPX\content\App_Data\CdnHelpers.xml
..\packages\NuGet.CommandLine.1.3.20425.372\tools\nuget.exe pack ASPX\CdnHelpers.ASPX.nuspec
del Razor\lib\Net40\CdnHelpers.dll
copy ..\CdnHelpers\bin\Release\CdnHelpers.dll Razor\lib\Net40
copy ..\WebRole\cdn\Web.config Razor\content\cdn
copy ..\WebRole\App_Data\CdnHelpers.xml Razor\content\App_Data\CdnHelpers.xml
..\packages\NuGet.CommandLine.1.3.20425.372\tools\nuget.exe pack Razor\CdnHelpers.Razor.nuspec