﻿#region License

// 
// Copyright (c) 2011, PowerStudio Project Contributors
// 
// Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
// See the file LICENSE.txt for details.
// 

#endregion

#region Using Directives

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;

#endregion

namespace PowerStudio.VsExtension.Tagging
{
    public class PowerShellErrorTokenTagger : PowerShellTagger<ErrorTag>
    {
        public PowerShellErrorTokenTagger( ITextBuffer buffer )
                : base( buffer )
        {
        }

        public override IEnumerable<ITagSpan<ErrorTag>> GetTags( NormalizedSnapshotSpanCollection spans )
        {
            if ( spans.Count == 0 ||
                 Buffer.CurrentSnapshot.Length == 0 )
            {
                //there is no content in the buffer
                yield break;
            }
            foreach ( SnapshotSpan currentSpan in spans )
            {
                int curLoc = currentSpan.Start.Position;
                string text = currentSpan.GetText();
                Collection<PSParseError> errors;
                Collection<PSToken> tokens = PSParser.Tokenize( text, out errors );

                foreach ( PSParseError error in errors )
                {
                    // HACK: Need to figure out why we are getting false positives.

                    if ( error.Token.Type == PSTokenType.Operator ||
                         error.Token.Type == PSTokenType.Position ||
                         error.Token.Type == PSTokenType.GroupStart ||
                         error.Token.Type == PSTokenType.GroupEnd )
                    {
                        continue;
                    }
                    var tokenSpan = new SnapshotSpan( currentSpan.Snapshot,
                                                      new Span( error.Token.Start + curLoc, error.Token.Length ) );
                    var errorTag = new ErrorTag( PredefinedErrorTypeNames.SyntaxError, error.Message );
                    yield return new TagSpan<ErrorTag>( tokenSpan, errorTag );
                }
            }
        }
    }
}