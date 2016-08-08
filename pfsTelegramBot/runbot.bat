@ECHO off
ECHO Restart Application
ECHO Written by: Jason Faulkner
ECHO SysadminGeek.com
ECHO.
ECHO.

SETLOCAL EnableExtensions

REM Enter the application information.
SET AppName=PerthFurStats Chat Command Bot
SET ExeFile=pfsTelegramBot.exe
SET ExePath=F:\Users\AndyDingo\Source\Repos\PerthFurStats_TelegramBot\Artifacts\Debug

REM Select the conditions to kill the application.
REM A value of 1 = Yes, 0 = No
SET KillIfRunning=1
SET KillIfNotResponding=1
SET KillIfUnknownStatus=1

REM Specify when to start the application:
REM 1 = Start only if the process was previous killed.
REM 0 = Start the application regardless.
SET StartOnlyIfKilled=1

SET KillStatus="%TEMP%KillStatus.tmp.txt"
SET Success=0

ECHO Killing existing %AppName% instance...
IF {%KillIfRunning%}=={1} CALL :CheckKillStatus "%ExeFile%" "RUNNING"
IF {%KillIfNotResponding%}=={1} CALL :CheckKillStatus "%ExeFile%" "NOT RESPONDING"
IF {%KillIfUnknownStatus%}=={1} CALL :CheckKillStatus "%ExeFile%" "UNKNOWN"
ECHO.

IF {%StartOnlyIfKilled%}=={1} (
	IF {%Success%}=={0} GOTO End
)
ECHO Restarting %AppName%...
START "%ExeFile%" "%ExePath%\%ExeFile%"
ECHO.

IF EXIST %KillStatus% DEL /F /Q %KillStatus%

ENDLOCAL


:CheckKillStatus
ECHO Killing with status: %~2
TASKKILL /FI "STATUS eq %~2" /IM "%~1" /F > %KillStatus%
SET /P KillResult= < %KillStatus%
FOR /F "tokens=1,* delims=:" %%A IN ("%KillResult%") DO (
	ECHO %%A:%%B
	IF /I {%%A}=={SUCCESS} SET /A Success=%Success%+1
)


:End