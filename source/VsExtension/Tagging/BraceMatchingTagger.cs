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
using System.Management.Automation;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

#endregion

namespace PowerStudio.VsExtension.Tagging
{
    public class BraceMatchingTagger : TaggerBase<BraceMatchingTag>
    {
        public BraceMatchingTagger( ITextView view, ITextBuffer buffer )
                : base( buffer )
        {
            View = view;
            View.Caret.PositionChanged += CaretPositionChanged;
            View.LayoutChanged += ViewLayoutChanged;
            Parse();
        }

        private ITextView View { get; set; }
        private SnapshotPoint? CurrentChar { get; set; }

        /// <summary>
        ///   Determines whether given token is in the target span translating to the current snaphot
        ///   if needed.
        /// </summary>
        /// <param name = "tag">The tag.</param>
        /// <param name = "snapshot">The snapshot.</param>
        /// <param name = "span">The span.</param>
        /// <returns>
        ///   <c>true</c> if the tag is in the span for the snapshot; otherwise, <c>false</c>.
        /// </returns>
        protected override bool IsTokenInSpan( BraceMatchingTag tag, ITextSnapshot snapshot, SnapshotSpan span )
        {
            if ( !CurrentChar.HasValue )
            {
                return false;
            }

            SnapshotPoint currentChar = CurrentChar.Value;

            if ( IsSnapshotPointContainedInSpan( snapshot, currentChar, tag.Span ) ||
                 IsSnapshotPointContainedInSpan( snapshot, currentChar, tag.Match.Span ) )
            {
                return base.IsTokenInSpan( tag, snapshot, span ) ||
                       base.IsTokenInSpan( tag.Match, snapshot, span );
            }
            return false;
        }

        protected override List<BraceMatchingTag> GetTags( ITextSnapshot snapshot )
        {
            var braces = new List<BraceMatchingTag>();
            var stack = new Stack<PSToken>();
            IEnumerable<PSToken> tokens = GetTokens( snapshot, true );
            foreach ( PSToken token in tokens )
            {
                switch ( token.Type )
                {
                    case PSTokenType.GroupStart:
                    {
                        stack.Push( token );
                        break;
                    }
                    case PSTokenType.GroupEnd:
                    {
                        if ( stack.Count == 0 )
                        {
                            continue;
                        }
                        PSToken startToken = stack.Pop();
                        var start = new BraceMatchingTag( PredefinedTextMarkerTags.BraceHighlight )
                                    {
                                            Span = CreateSnapshotSpan( snapshot, startToken ),
                                    };

                        var end = new BraceMatchingTag( PredefinedTextMarkerTags.BraceHighlight )
                                  {
                                          Span = CreateSnapshotSpan( snapshot, token ),
                                          Match = start
                                  };
                        start.Match = end;
                        braces.Add( start );
                        braces.Add( end );
                    }
                        break;
                    default:
                        break;
                }
            }
            return braces;
        }

        private void ViewLayoutChanged( object sender, TextViewLayoutChangedEventArgs e )
        {
            if ( e.NewSnapshot !=
                 e.OldSnapshot ) //make sure that there has really been a change
            {
                UpdateAtCaretPosition( View.Caret.Position );
            }
        }

        private void CaretPositionChanged( object sender, CaretPositionChangedEventArgs e )
        {
            UpdateAtCaretPosition( e.NewPosition );
        }

        private void UpdateAtCaretPosition( CaretPosition caretPosition )
        {
            CurrentChar = caretPosition.Point.GetPoint( Buffer, caretPosition.Affinity );

            if ( !CurrentChar.HasValue )
            {
                return;
            }

            var span = new SnapshotSpan( Buffer.CurrentSnapshot, 0, Buffer.CurrentSnapshot.Length );
            OnTagsChanged( new SnapshotSpanEventArgs( span ) );
        }
    }
}