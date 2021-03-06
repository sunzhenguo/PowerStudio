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
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using NLog;

#endregion

namespace PowerStudio.LanguageServices.Tagging.Taggers
{
    public abstract class TaggerBase<TTokenTag, TToken> : ITagger<TTokenTag>
            where TTokenTag : ITokenTag<TToken>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected TaggerBase( ITextBuffer buffer )
        {
            if ( buffer == null )
            {
                throw new ArgumentNullException( "buffer" );
            }
            Buffer = buffer;
            Snapshot = Buffer.CurrentSnapshot;
            Buffer.Changed += BufferChanged;
            Tags = Enumerable.Empty<TTokenTag>().ToList().AsReadOnly();
        }

        protected ITextBuffer Buffer { get; private set; }
        protected ITextSnapshot Snapshot { get; set; }
        protected ReadOnlyCollection<TTokenTag> Tags { get; set; }

        #region ITagger<TTokenTag> Members

        /// <summary>
        ///   Gets all the tags that overlap the <paramref name = "spans" />.
        /// </summary>
        /// <param name = "spans">The spans to visit.</param>
        /// <returns>
        ///   A <see cref = "T:Microsoft.VisualStudio.Text.Tagging.ITagSpan`1" /> for each tag.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///     Taggers are not required to return their tags in any specific order.
        ///   </para>
        ///   <para>
        ///     The recommended way to implement this method is by using generators ("yield return"),
        ///     which allows lazy evaluation of the entire tagging stack.
        ///   </para>
        /// </remarks>
        public virtual IEnumerable<ITagSpan<TTokenTag>> GetTags( NormalizedSnapshotSpanCollection spans )
        {
            if ( spans == null )
            {
                throw new ArgumentNullException( "spans" );
            }
            if ( spans.Count == 0 ||
                 Buffer.CurrentSnapshot.Length == 0 )
            {
                //there is no content in the buffer
                yield break;
            }
            ReadOnlyCollection<TTokenTag> tags = Tags;
            ITextSnapshot currentSnapshot = Snapshot;
            SnapshotSpan span =
                    new SnapshotSpan( spans[0].Start, spans[spans.Count - 1].End )
                            .TranslateTo( currentSnapshot, SpanTrackingMode.EdgeExclusive );

            foreach ( TTokenTag tag in from tag in tags
                                       where IsTokenInSpan( tag, currentSnapshot, span )
                                       select tag )
            {
                yield return new TokenTagSpan<TTokenTag, TToken>( tag );
            }
        }

        /// <summary>
        ///   Occurs when [tags changed].
        /// </summary>
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        #endregion

        /// <summary>
        ///   Occurs when a non-empty text edit is applied. If the snapshot changes then we need to handle it.
        /// </summary>
        /// <param name = "sender">The sender.</param>
        /// <param name = "e">The <see cref = "Microsoft.VisualStudio.Text.TextContentChangedEventArgs" /> instance containing the event data.</param>
        protected virtual void BufferChanged( object sender, TextContentChangedEventArgs e )
        {
            // If this isn't the most up-to-date version of the buffer, then ignore it for now (we'll eventually get another change event).
            if ( Buffer.CurrentSnapshot !=
                 e.After )
            {
                return;
            }
            Parse();
        }

        /// <summary>
        ///   Raises the <see cref = "E:TagsChanged" /> event.
        /// </summary>
        /// <param name = "args">The <see cref = "Microsoft.VisualStudio.Text.SnapshotSpanEventArgs" /> instance containing the event data.</param>
        protected virtual void OnTagsChanged( SnapshotSpanEventArgs args )
        {
            EventHandler<SnapshotSpanEventArgs> handler = TagsChanged;
            if ( handler != null )
            {
                handler( this, args );
            }
        }

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
        protected virtual bool IsTokenInSpan( TTokenTag tag, ITextSnapshot snapshot, SnapshotSpan span )
        {
            return IsSpanContainedInTargetSpan( snapshot, span, tag.Span );
        }

        /// <summary>
        ///   Parses this instance for the current buffer. All tags are parsed and then any
        ///   changes are published through the <see cref = "E:TagsChanged" /> event. This also
        ///   updates the current snapshot for this tagger.
        /// </summary>
        protected virtual void Parse()
        {
            ITextSnapshot newSnapshot = Buffer.CurrentSnapshot;
            List<TTokenTag> tags;
            try
            {
                tags = GetTags( newSnapshot );
            }
            catch ( Exception ex )
            {
                Logger.ErrorException( "Failed to parse buffer.", ex );
                tags = Enumerable.Empty<TTokenTag>().ToList();
            }
            PublishTagChanges( newSnapshot, tags );
        }

        protected abstract List<TTokenTag> GetTags( ITextSnapshot snapshot );

        /// <summary>
        ///   Publishes the tag changes.
        /// </summary>
        /// <remarks>
        ///   Once the tags are parsed from the current document, they are passed into this method
        ///   in order to determine if anything has changed. Once the change detection is done,
        ///   it replaces <see cref = "Snapshot" /> for the tagger with the parameter passed in as
        ///   well as the <see cref = "Tags" />. If there were any changes, the <see cref = "E:TagsChanged" />
        ///   is raised with the combined snapshot of all changes made.
        /// </remarks>
        /// <param name = "newSnapshot">The new snapshot.</param>
        /// <param name = "newTags">The new tags.</param>
        protected virtual void PublishTagChanges( ITextSnapshot newSnapshot, List<TTokenTag> newTags )
        {
            ReadOnlyCollection<TTokenTag> currentTags = Tags;
            var oldSpans =
                    new List<Span>( currentTags.Select( tag => tag.Span.TranslateTo( newSnapshot,
                                                                                     SpanTrackingMode.EdgeExclusive )
                                                                       .Span ) );
            var newSpans =
                    new List<Span>( newTags.Select( tag => tag.Span.Span ) );

            var oldSpanCollection = new NormalizedSpanCollection( oldSpans );
            var newSpanCollection = new NormalizedSpanCollection( newSpans );

            //the changed regions are regions that appear in one set or the other, but not both.
            NormalizedSpanCollection removed =
                    NormalizedSpanCollection.Difference( oldSpanCollection, newSpanCollection );

            int changeStart = int.MaxValue;
            int changeEnd = -1;

            if ( removed.Count > 0 )
            {
                changeStart = removed[0].Start;
                changeEnd = removed[removed.Count - 1].End;
            }

            if ( newSpans.Count > 0 )
            {
                changeStart = Math.Min( changeStart, newSpans[0].Start );
                changeEnd = Math.Max( changeEnd, newSpans[newSpans.Count - 1].End );
            }

            Snapshot = newSnapshot;
            Tags = newTags.AsReadOnly();

            if ( changeStart <= changeEnd )
            {
                var snapshot = new SnapshotSpan( newSnapshot, Span.FromBounds( changeStart, changeEnd ) );
                OnTagsChanged( new SnapshotSpanEventArgs( snapshot ) );
            }
        }

        /// <summary>
        ///   Gets the tokens by parsing the text snapshot, optionally including any errors.
        /// </summary>
        /// <param name = "textSnapshot">The text snapshot.</param>
        /// <param name = "includeErrors">if set to <c>true</c> [include errors].</param>
        /// <returns></returns>
        protected abstract IEnumerable<TToken> GetTokens( ITextSnapshot textSnapshot, bool includeErrors );

        protected virtual bool IsSpanContainedInTargetSpan( ITextSnapshot snapshot,
                                                            SnapshotSpan sourceSpan,
                                                            SnapshotSpan targetSpan )
        {
            if ( snapshot != sourceSpan.Snapshot )
            {
                // need to map to the new snapshot before we can detect overlap
                sourceSpan = sourceSpan.TranslateTo( snapshot, SpanTrackingMode.EdgeExclusive );
            }
            if ( snapshot != targetSpan.Snapshot )
            {
                // need to map to the new snapshot before we can detect overlap
                targetSpan = targetSpan.TranslateTo( snapshot, SpanTrackingMode.EdgeExclusive );
            }
            return sourceSpan.Contains( targetSpan );
        }

        protected virtual bool IsSnapshotPointContainedInSpan( ITextSnapshot snapshot,
                                                               SnapshotPoint snapshotPoint,
                                                               SnapshotSpan sourceSpan )
        {
            if ( snapshot != sourceSpan.Snapshot )
            {
                // need to map to the new snapshot before we can detect overlap
                sourceSpan = sourceSpan.TranslateTo( snapshot, SpanTrackingMode.EdgeExclusive );
            }
            if ( snapshot != snapshotPoint.Snapshot )
            {
                // need to map to the new snapshot before we can detect overlap
                snapshotPoint = snapshotPoint.TranslateTo( snapshot, PointTrackingMode.Positive );
            }
            return sourceSpan.Contains( snapshotPoint );
        }

        /// <summary>
        ///   Creates a <see cref = "SnapshotSpan" /> for the given text snapshot and token.
        /// </summary>
        /// <param name = "snapshot">The snapshot.</param>
        /// <param name = "token">The token.</param>
        /// <returns></returns>
        protected abstract SnapshotSpan CreateSnapshotSpan( ITextSnapshot snapshot, TToken token );

        /// <summary>
        ///   Creates a <see cref = "SnapshotSpan" /> for the given text snapshot covering all text between
        ///   the start and end tokens - (startToken, endToken)
        /// </summary>
        /// <param name = "snapshot">The current snapshot.</param>
        /// <param name = "startToken">The start token of the span.</param>
        /// <param name = "endToken">The end token of the span.</param>
        /// <returns></returns>
        protected abstract SnapshotSpan CreateSnapshotSpan( ITextSnapshot snapshot, TToken startToken, TToken endToken );
    }
}