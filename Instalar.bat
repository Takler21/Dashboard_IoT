@echo off
echo Limpiando contenedores y volumenes anteriores...
docker compose down -v

echo Construyendo imagenes y arrancando contenedores...
docker compose up -d --build

echo.
echo Hecho. Ejecuta arrancar.bat para abrir la aplicacion.
pause