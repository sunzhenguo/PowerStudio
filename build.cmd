@echo off

powershell -NoProfile -ExecutionPolicy Unrestricted -Command "& { .\build.ps1 %* }"



