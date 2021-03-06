﻿#region License

// 
// Copyright (c) 2011, PowerStudio Project Contributors
// 
// Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
// See the file LICENSE.txt for details.
// 

#endregion

#region Using Directives

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using PowerStudio.LanguageServices.Intellisense.Completion;

#endregion

namespace PowerStudio.LanguageServices.PowerShell.Intellisense.Completion
{
    [Export( typeof (ICompletionSourceProvider) )]
    [ContentType( LanguageConfiguration.Name )]
    [Name( "built-in completion" )]
    public class BuiltInCompletionSourceProvider : CompletionSourceProvider
    {
        /// <summary>
        ///   Creates a completion provider for the given context.
        /// </summary>
        /// <param name = "textBuffer">The text buffer over which to create a provider.</param>
        /// <returns>
        ///   A valid <see cref = "T:Microsoft.VisualStudio.Language.Intellisense.ICompletionSource" /> instance, or null if none could be created.
        /// </returns>
        public override ICompletionSource TryCreateCompletionSource( ITextBuffer textBuffer )
        {
            return new BuiltInCompletionSource( this, textBuffer );
        }
    }
}