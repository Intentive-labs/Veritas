@echo off
cd /d C:\Users\inten\src\Veritas\Veritas

REM Create main project directories
echo Creating Veritas.Core...
dotnet new classlib -n Veritas.Core -o src\Veritas.Core --no-restore

echo Creating Veritas.DomainPacks...
dotnet new classlib -n Veritas.DomainPacks -o src\Veritas.DomainPacks --no-restore

echo Creating Veritas.Storage...
dotnet new classlib -n Veritas.Storage -o src\Veritas.Storage --no-restore

echo Creating Veritas.Corpora...
dotnet new classlib -n Veritas.Corpora -o src\Veritas.Corpora --no-restore

echo Creating Veritas.Rag...
dotnet new classlib -n Veritas.Rag -o src\Veritas.Rag --no-restore

echo Creating Veritas.Extraction...
dotnet new classlib -n Veritas.Extraction -o src\Veritas.Extraction --no-restore

echo Creating Veritas.Api...
dotnet new webapi -n Veritas.Api -o src\Veritas.Api --no-restore

REM Create subdirectories using mkdir
echo Creating subdirectories...
mkdir src\Veritas.Core\Models 2>nul
mkdir src\Veritas.Core\Contracts 2>nul
mkdir src\Veritas.DomainPacks\Models 2>nul
mkdir src\Veritas.Rag\Contracts 2>nul
mkdir src\Veritas.Rag\Models 2>nul
mkdir src\Veritas.Rag\Implementation 2>nul
mkdir src\Veritas.Rag\Api 2>nul
mkdir src\Veritas.Extraction\Agents 2>nul
mkdir src\Veritas.Extraction\Pipeline 2>nul
mkdir src\Veritas.Api\Controllers 2>nul
mkdir domain-pack-schema\schema-v1 2>nul
mkdir domain-pack-schema\examples\lenr-pack-example\hypotheses 2>nul
mkdir mock-data\sample-documents 2>nul
mkdir mock-data\mock-corpora 2>nul

REM Create .gitkeep files to preserve directories
echo Creating .gitkeep files...
type nul > src\Veritas.Core\.gitkeep
type nul > src\Veritas.Core\Models\.gitkeep
type nul > src\Veritas.Core\Contracts\.gitkeep
type nul > src\Veritas.DomainPacks\.gitkeep
type nul > src\Veritas.DomainPacks\Models\.gitkeep
type nul > src\Veritas.Storage\.gitkeep
type nul > src\Veritas.Corpora\.gitkeep
type nul > src\Veritas.Rag\.gitkeep
type nul > src\Veritas.Rag\Contracts\.gitkeep
type nul > src\Veritas.Rag\Models\.gitkeep
type nul > src\Veritas.Rag\Implementation\.gitkeep
type nul > src\Veritas.Rag\Api\.gitkeep
type nul > src\Veritas.Extraction\.gitkeep
type nul > src\Veritas.Extraction\Agents\.gitkeep
type nul > src\Veritas.Extraction\Pipeline\.gitkeep
type nul > src\Veritas.Api\.gitkeep
type nul > src\Veritas.Api\Controllers\.gitkeep
type nul > domain-pack-schema\schema-v1\.gitkeep
type nul > domain-pack-schema\examples\lenr-pack-example\hypotheses\.gitkeep
type nul > mock-data\sample-documents\.gitkeep
type nul > mock-data\mock-corpora\.gitkeep

echo.
echo Project setup complete!
echo.
pause
