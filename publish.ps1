$YearMonth = $(Get-Date -f yyMMdd).substring(1);
$Hour = "1" + $(Get-Date -f HHmm)
$Second = "1" + $(Get-Date -f ss)
$Version = '1.' + $YearMonth + '.' + $Hour + '.' + $Second;
$AssemblyName = Split-Path -Path (Get-Location) -Leaf
$Package = $AssemblyName + '\bin\Release\' + $AssemblyName +'.' + $Version + '.nupkg';
dotnet build CMP.ManipuladorPDF\CMP.ManipuladorPDF.csproj -c Release -v minimal
dotnet pack --configuration Release /p:Version=$Version
dotnet nuget push -s https://sistemas.camarapiracicaba.sp.gov.br/nuget/v3/index.json -k KwYQg754C66caiZltP4D6nLJiJS72tUc $Package