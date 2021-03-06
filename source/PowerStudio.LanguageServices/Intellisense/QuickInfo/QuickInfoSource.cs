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
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using PowerStudio.LanguageServices.Tagging;

#endregion

namespace PowerStudio.LanguageServices.Intellisense.QuickInfo
{
    public abstract class QuickInfoSource<TTokenTag,TToken> : IQuickInfoSource
            where TTokenTag : ITokenTag<TToken>
    {
        private bool _Disposed;

        /// <summary>
        ///   Initializes a new instance of the <see cref = "QuickInfoErrorSource" /> class.
        /// </summary>
        /// <param name = "buffer">The buffer.</param>
        /// <param name = "aggregator">The aggregator.</param>
        /// <param name = "quickInfoSourceProvider">The quick info source provider.</param>
        protected QuickInfoSource( ITextBuffer buffer,
                                   ITagAggregator<TTokenTag> aggregator,
                                   QuickInfoSourceProvider<TTokenTag,TToken> quickInfoSourceProvider)
        {
            Aggregator = aggregator;
            QuickInfoSourceProvider = quickInfoSourceProvider;
            Buffer = buffer;
        }

        protected ITagAggregator<TTokenTag> Aggregator { get; private set; }
        protected ITextBuffer Buffer { get; private set; }
        protected QuickInfoSourceProvider<TTokenTag,TToken> QuickInfoSourceProvider { get; private set; }

        #region IQuickInfoSource Members

        /// <summary>
        ///   Determines which pieces of QuickInfo content should be part of the specified <see cref = "T:Microsoft.VisualStudio.Language.Intellisense.IQuickInfoSession" />.
        /// </summary>
        /// <param name = "session">The session for which completions are to be computed.</param>
        /// <param name = "quickInfoContent">The QuickInfo content to be added to the session.</param>
        /// <param name = "applicableToSpan">The <see cref = "T:Microsoft.VisualStudio.Text.ITrackingSpan" /> to which this session applies.</param>
        /// <remarks>
        ///   Each applicable <see cref = "M:Microsoft.VisualStudio.Language.Intellisense.IQuickInfoSource.AugmentQuickInfoSession(Microsoft.VisualStudio.Language.Intellisense.IQuickInfoSession,System.Collections.Generic.IList{System.Object},Microsoft.VisualStudio.Text.ITrackingSpan@)" /> instance will be called in-order to (re)calculate a
        ///   <see cref = "T:Microsoft.VisualStudio.Language.Intellisense.IQuickInfoSession" />. Objects can be added to the session by adding them to the quickInfoContent collection
        ///   passed-in as a parameter.  In addition, by removing items from the collection, a source may filter content provided by
        ///   <see cref = "T:Microsoft.VisualStudio.Language.Intellisense.IQuickInfoSource" />s earlier in the calculation chain.
        /// </remarks>
        public void AugmentQuickInfoSession( IQuickInfoSession session,
                                             IList<object> quickInfoContent,
                                             out ITrackingSpan applicableToSpan )
        {
            if ( _Disposed )
            {
                throw new ObjectDisposedException( "QuickInfoErrorSource" );
            }

            ITextSnapshot currentSnapshot = Buffer.CurrentSnapshot;
            SnapshotPoint? triggerPoint = session.GetTriggerPoint( currentSnapshot );

            if ( !triggerPoint.HasValue )
            {
                applicableToSpan = null;
                return;
            }

            var bufferSpan = new SnapshotSpan( currentSnapshot, 0, currentSnapshot.Length );
            foreach ( var tagSpan in Aggregator.GetTags( bufferSpan ) )
            {
                foreach ( SnapshotSpan span in tagSpan.Span
                        .GetSpans( Buffer )
                        .Where( span => span.Contains( triggerPoint.Value ) ) )
                {
                    applicableToSpan = currentSnapshot.CreateTrackingSpan( span, SpanTrackingMode.EdgeExclusive );
                    quickInfoContent.Add( GetToolTip( tagSpan.Tag ) );
                    return;
                }
            }
            applicableToSpan = null;
        }

        /// <summary>
        ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        #endregion

        /// <summary>
        ///   Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name = "disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected void Dispose( bool disposing )
        {
            if ( !_Disposed && disposing )
            {
                if ( Aggregator != null )
                {
                    Aggregator.Dispose();
                }
            }

            _Disposed = true;
        }

        protected abstract object GetToolTip( TTokenTag tokenTag );
    }
}