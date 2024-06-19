$Major = $(Get-Date -f yyyyMMdd)
$Minor = $(Get-Date -f HHmmss)
$Version = '1.1.' + $Major + '.' + $Minor
$AssemblyName = Split-Path -Path (Get-Location) -Leaf
$Package = $AssemblyName + '\bin\Release\' + $AssemblyName +'.' + $Version + '.nupkg';
dotnet pack --configuration Release /p:Version=$Version
dotnet nuget push -s https://sistemas.camarapiracicaba.sp.gov.br/nuget/v3/index.json -k KwYQg754C66caiZltP4D6nLJiJS72tUc $Package