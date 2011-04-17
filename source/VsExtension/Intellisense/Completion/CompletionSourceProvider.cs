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
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

#endregion

namespace PowerStudio.VsExtension.Intellisense.Completion
{
    [Export( typeof (ICompletionSourceProvider) )]
    [ContentType( LanguageConfiguration.Name )]
    [Name( "token completion" )]
    internal class CompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        #region Implementation of ICompletionSourceProvider

        /// <summary>
        /// Creates a completion provider for the given context.
        /// </summary>
        /// <param name="textBuffer">The text buffer over which to create a provider.</param>
        /// <returns>
        /// A valid <see cref="T:Microsoft.VisualStudio.Language.Intellisense.ICompletionSource"/> instance, or null if none could be created.
        /// </returns>
        public ICompletionSource TryCreateCompletionSource( ITextBuffer textBuffer )
        {
            return new CompletionSource( this, textBuffer );
        }

        #endregion
    }
}