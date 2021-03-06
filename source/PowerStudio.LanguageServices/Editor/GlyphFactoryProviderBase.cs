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
using Microsoft.VisualStudio.Text.Editor;
using PowerStudio.LanguageServices.Tagging.Tags;

#endregion

namespace PowerStudio.LanguageServices.Editor
{
    public abstract class GlyphFactoryProviderBase<TTokenTag,TToken> : IGlyphFactoryProvider
            where TTokenTag : GlyphTag<TToken>
    {
        #region IGlyphFactoryProvider Members

        /// <summary>
        ///   Gets the <see cref = "T:Microsoft.VisualStudio.Text.Editor.IGlyphFactory" /> for the given text view and margin.
        /// </summary>
        /// <param name = "view">The view for which the factory is being created.</param>
        /// <param name = "margin">The margin for which the factory will create glyphs.</param>
        /// <returns>
        ///   An <see cref = "T:Microsoft.VisualStudio.Text.Editor.IGlyphFactory" /> for the given view and margin.
        /// </returns>
        public IGlyphFactory GetGlyphFactory( IWpfTextView view, IWpfTextViewMargin margin )
        {
            return view.Properties.GetOrCreateSingletonProperty( GetFactory( view, margin ) );
        }

        #endregion

        protected abstract Func<IGlyphFactory<TTokenTag, TToken>> GetFactory( IWpfTextView view,
                                                                              IWpfTextViewMargin margin );
    }
}