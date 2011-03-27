﻿#region License

// 
// Copyright (c) 2011, PowerStudio Project Contributors
// 
// Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
// See the file LICENSE.txt for details.
// 

#endregion

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Project;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace PowerStudio.VsExtension.Project
{
    [Guid( PsConstants.PsProjectFactoryGuidString )]
    public class PowerShellProjectFactory : ProjectFactory
    {
        private readonly PowerShellPackage _Package;

        public PowerShellProjectFactory( PowerShellPackage package )
                : base( package )
        {
            _Package = package;
        }

        protected override ProjectNode CreateProject()
        {
            var project = new PowerShellProjectNode( _Package );
            project.SetSite(
                    (IOleServiceProvider) ( (IServiceProvider) _Package ).GetService( typeof (IOleServiceProvider) ) );
            return project;
        }
    }
}