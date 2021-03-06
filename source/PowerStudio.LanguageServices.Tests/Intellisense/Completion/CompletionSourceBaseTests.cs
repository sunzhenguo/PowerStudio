﻿#region License

// 
// Copyright (c) 2011, PowerStudio Project Contributors
// 
// Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
// See the file LICENSE.txt for details.
// 

#endregion

#region Using Directives

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Moq;
using PowerStudio.LanguageServices.Intellisense.Completion;
using PowerStudio.LanguageServices.Tests.Mocks;
using Xunit;

#endregion

namespace PowerStudio.LanguageServices.Tests.Intellisense.Completion
{
    public class CompletionSourceBaseTests
    {
        [Fact]
        public void WhenANullSourceProviderIsPassedIntoTheCtor_ThenAnExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(
                    () => new CompletionSourceImpl( null, new TextBufferMock( string.Empty ) )
                    );
        }

        [Fact]
        public void WhenNullIsPassedIntoTheCtor_ThenAnExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(
                    () => new CompletionSourceImpl( new Mock<CompletionSourceProvider>().Object, null )
                    );
        }

        #region Nested type: CompletionSourceImpl

        private class CompletionSourceImpl : CompletionSourceBase
        {
            public CompletionSourceImpl( CompletionSourceProvider sourceProvider, ITextBuffer textBuffer )
                    : base( sourceProvider, textBuffer )
            {
            }

            public override void Dispose()
            {
            }

            public override void AugmentCompletionSession( ICompletionSession session,
                                                           IList<CompletionSet> completionSets )
            {
            }
        }

        #endregion
    }
}