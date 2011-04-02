﻿#region License

// 
// Copyright (c) 2011, PowerStudio Project Contributors
// 
// Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
// See the file LICENSE.txt for details.
// 

#endregion

using System.Management.Automation;
using Microsoft.VisualStudio.Text.Tagging;

namespace PowerStudio.VsExtension.Tagging
{
    public class PowerShellTokenTag : ITag
    {
        public PSTokenType TokenType { get; set; }
    }
}