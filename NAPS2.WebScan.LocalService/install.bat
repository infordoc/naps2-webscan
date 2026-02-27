@echo off
echo ========================================
echo  NAPS2 WebScan Server - Instalador
echo ========================================
echo.

REM Verificar administrador
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERRO: Execute como Administrador
    pause
    exit /B 1
)

:inicio
echo Escolha uma opcao:
echo.
echo 1. Instalar como servico do Windows
echo 2. Desinstalar servico
echo 3. Iniciar servico
echo 4. Parar servico
echo 5. Executar em modo console (teste)
echo 0. Sair
echo.

set /p opcao="Digite sua opcao (0-5): "

if "%opcao%"=="1" goto instalar
if "%opcao%"=="2" goto desinstalar
if "%opcao%"=="3" goto iniciar
if "%opcao%"=="4" goto parar
if "%opcao%"=="5" goto console
if "%opcao%"=="0" goto sair

:instalar
echo.
echo Instalando NAPS2 WebScan Server como servico...
sc create "NAPS2WebScanServer" binPath= "%~dp0NAPS2.WebScan.LocalService.exe --service" DisplayName= "NAPS2 WebScan Server" start= auto
if %errorLevel% equ 0 (
    echo Servico instalado com sucesso!
    echo - Nome: NAPS2WebScanServer
    echo - Inicio: Automatico
    echo - Porta: 8080
    echo.
    echo Deseja iniciar o servico agora? (S/N)
    set /p start="Digite S ou N: "
    if /i "%start%"=="S" (
        sc start "NAPS2WebScanServer"
    )
)
pause
goto sair

:desinstalar
echo.
echo Parando servico...
sc stop "NAPS2WebScanServer" >nul 2>&1
timeout /t 3 >nul
echo Desinstalando NAPS2 WebScan Server...
sc delete "NAPS2WebScanServer"
pause
goto sair

:iniciar
sc start "NAPS2WebScanServer"
pause
goto sair

:parar
sc stop "NAPS2WebScanServer"
pause
goto sair

:console
echo Executando em modo console...
NAPS2.WebScan.LocalService.exe
pause
goto sair

:sair
