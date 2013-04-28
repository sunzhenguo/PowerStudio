#Requires -Version 2.0

# 
# Copyright (c) 2011, Toji Project Contributors
# 
# Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
# See the file LICENSE.txt for details.
# 

param(
  [Parameter( Position = 0, Mandatory = 0 )]
  [string] $path = ((Resolve-Path "$(Split-Path -parent $MyInvocation.MyCommand.Definition)\..\").Path),
  [Parameter( Position = 1, Mandatory = 0 )]
  [string] $install_to = 'Packages'
)

#引导
function script:BootStrap-Chewie {
   #如果当前脚本目录没有 .NugetFile 文件则创建一个
  if(!(test-path $pwd\.NugetFile)) {
    new-item -path $pwd -name .NugetFile -itemtype file
    add-content $pwd\.NugetFile "install_to '.'"
    add-content $pwd\.NugetFile "chew 'psake' '4.0.1.0'"
  }
}

#这个函数会遍历环境变量中 Path 的所有路径下的 $fileName 文件，如果存在一个或一个以上的文件则返回 true, 否则返回 false。
function script:FileExistsInPath {
  param (
    [Parameter(Position=0,Mandatory=$true)]
    [string] $fileName = $null
  )
	
	#获取 env:Path 路径的集合
  $path = Get-Childitem Env:Path

  $found = $false
  foreach ($folder in $path.Value.Split(";")) { if (Test-Path "$folder\$fileName") { $found = $true; break } }
  return $found
}

#这个函数获取 nuGet.exe 的全路径。
#首先在环境变量中路径中寻找 nuGet.exe,如果没有找到，则搜索整个文件夹,如果还没有找到则返回错误，如果找到了返回 nuGet.exe 的完整路径。
function global:Resolve-NuGet {
  $nuGetIsInPath = (FileExistsInPath "NuGet.exe") -or (FileExistsInPath "NuGet.bat")
  
  $nuget = "NuGet"
  
  if($nuGetIsInPath) {
  	#上面两个文件存在
    $nuget = (@(get-command nuget) | % {$_.Definition} | ? { (Test-Path $_) } | Select-Object -First 1)
  } else {  
    $nugets = @(Get-ChildItem "..\*" -recurse -include NuGet.exe)
    if ($nugets.Length -le 0) { 
      Write-Output "No NuGet executables found."
      return
    }
    $nuget = (Resolve-Path $nugets[0]).Path
    $env:Path = $env:Path + ";" + (Split-Path $nuget)
  }
  return $nuget
}

Push-Location $path #将当前路径压入栈中
try {
  Write-Output "加载 Nuget 依赖关系"
  $nuget = Resolve-NuGet #获取 nuget.exe 的完整路径
  BootStrap-Chewie
  Write-Output "Nuget = $nuget"
  if(Test-Path ".NugetFile") {
    Write-Output "Loading chewie"
    Import-Module "$pwd\chewie.psm1"
    Write-Output "Running chewie"
    Invoke-Chewie
  } else {
    $package_files = Get-ChildItem . -recurse -include packages.config
    $package_files | % { & $nuget i $_ -OutputDirectory $install_to }
  }
} finally { Pop-Location } #完成任务后出栈并设置为当前路径
