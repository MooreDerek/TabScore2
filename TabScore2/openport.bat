@echo off
netsh advfirewall firewall add rule name="TabScore2" dir=in program="C:\Temp\Test\TabScore2.exe" action=allow protocol=TCP enable=yes profile=private