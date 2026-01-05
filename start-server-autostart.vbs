Set WshShell = CreateObject("WScript.Shell")
WshShell.Run "powershell.exe -WindowStyle Hidden -ExecutionPolicy Bypass -File ""C:\Users\ASUS\Desktop\AdminServer\start-server.ps1""", 0
Set WshShell = Nothing
