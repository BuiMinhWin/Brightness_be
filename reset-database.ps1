# Reset database for fresh start
# Run this if you get "relation already exists" errors

Write-Host "Stopping services..." -ForegroundColor Yellow
docker-compose down

Write-Host "`nRemoving database volume..." -ForegroundColor Yellow
docker volume rm brightness_be_postgres_data -ErrorAction SilentlyContinue

Write-Host "`nStarting fresh database..." -ForegroundColor Green
docker-compose up -d postgres

Write-Host "`nWaiting for database to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

Write-Host "`nStarting all services..." -ForegroundColor Green
docker-compose up --build -d

Write-Host "`nDatabase reset complete! All services starting with fresh database." -ForegroundColor Green
Write-Host "Wait ~30 seconds for migrations to complete, then check logs:" -ForegroundColor Cyan
Write-Host "   docker-compose logs -f personal-coach-api" -ForegroundColor White

Write-Host "`nPress any key to exit..." -ForegroundColor Yellow
[void][System.Console]::ReadKey($true)
