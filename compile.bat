@echo off
echo ========================================
echo       Construyendo ZenCraft...
echo ========================================
echo.

echo [1/2] Compilando main.zc a C...
..\zc.com build main.zc --emit-c


echo.
echo [2/2] Compilando out.c con GCC...

REM Crear la carpeta bin si no existe para evitar errores
if not exist bin mkdir bin

gcc out.c -o ./bin/zencraft.exe -L. -lraylib -lopengl32 -lgdi32 -lwinmm

REM Comprobar si GCC fallo
if %errorlevel% neq 0 (
    echo.
    echo [ERROR] Fallo la compilacion de GCC.
    pause
    exit /b %errorlevel%
)

echo.
echo ========================================
echo   ¡Construccion completada con exito!
echo   Ejecutable creado en: bin\zencraft.exe
echo ========================================
pause