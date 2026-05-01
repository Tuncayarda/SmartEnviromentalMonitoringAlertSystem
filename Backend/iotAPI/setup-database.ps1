# PostgreSQL Veritabanı Kurulum Scripti
# Bu script veritabanını oluşturur ve migration'ları uygular

Write-Host "IoT API - PostgreSQL Veritabanı Kurulumu" -ForegroundColor Cyan
Write-Host "==========================================`n" -ForegroundColor Cyan

# 1. Connection string kontrolü
Write-Host "1. appsettings.json dosyasındaki PostgreSQL connection string'i kontrol edin" -ForegroundColor Yellow
Write-Host "   Örnek: Host=localhost;Port=5432;Database=iotdb;Username=postgres;Password=yourpassword`n" -ForegroundColor Gray

# 2. Entity Framework Core tools kontrolü
Write-Host "2. EF Core Tools yükleniyor..." -ForegroundColor Yellow
dotnet tool install --global dotnet-ef --version 9.0.0 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "   EF Core Tools zaten yüklü." -ForegroundColor Gray
}
Write-Host ""

# 3. Migration oluşturma
Write-Host "3. Veritabanı migration'ı oluşturuluyor..." -ForegroundColor Yellow
dotnet ef migrations add InitialCreate --project .\iotAPI.csproj
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✓ Migration başarıyla oluşturuldu!" -ForegroundColor Green
} else {
    Write-Host "   ✗ Migration oluşturulamadı!" -ForegroundColor Red
    exit 1
}
Write-Host ""

# 4. Migration uygulama
Write-Host "4. Migration veritabanına uygulanıyor..." -ForegroundColor Yellow
dotnet ef database update --project .\iotAPI.csproj
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✓ Veritabanı başarıyla oluşturuldu!" -ForegroundColor Green
} else {
    Write-Host "   ✗ Veritabanı oluşturulamadı! PostgreSQL sunucusunun çalıştığından emin olun." -ForegroundColor Red
    exit 1
}
Write-Host ""

Write-Host "==========================================`n" -ForegroundColor Cyan
Write-Host "Kurulum tamamlandı! Uygulamayı 'dotnet run' ile başlatabilirsiniz." -ForegroundColor Green
Write-Host ""
