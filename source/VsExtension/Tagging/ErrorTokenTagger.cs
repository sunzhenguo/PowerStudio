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
using System.Linq;
using Microsoft.VisualStudio.Text;

#endregion

namespace PowerStudio.VsExtension.Tagging
{
    public class ErrorTokenTagger : TaggerBase<ErrorTokenTag>
    {
        public ErrorTokenTagger( ITextBuffer buffer )
                : base( buffer )
        {
            Parse();
        }

        protected override void Parse()
        {
            ITextSnapshot newSnapshot = Buffer.CurrentSnapshot;

            List<ErrorTokenTag> tags = ( from error in GetErrorTokens( newSnapshot )
                                         select new ErrorTokenTag( error.Message )
                                                {
                                                        TokenType = error.Token.Type,
                                                        Span = AsSnapshotSpan( newSnapshot, error.Token )
                                                } )
                    .ToList();

            Snapshot = newSnapshot;
            Tags = tags.AsReadOnly();
            OnTagsChanged(
                    new SnapshotSpanEventArgs( new SnapshotSpan( newSnapshot, Span.FromBounds( 0, newSnapshot.Length ) ) ) );
        }
    }
}