﻿using OwlCore.AbstractUI.Models;
using System.ComponentModel;

namespace OwlCore.AbstractUI.ViewModels
{
    /// <summary>
    /// ViewModel for <see cref="AbstractRichTextBlock"/>.
    /// </summary>
    [Bindable(true)]
    public class AbstractRichTextBlockViewModel : AbstractUIViewModelBase
    {
        private readonly AbstractRichTextBlock _model;

        /// <summary>
        /// Creates a new instance of <see cref="AbstractRichTextBlockViewModel"/>.
        /// </summary>
        /// <param name="model"></param>
        public AbstractRichTextBlockViewModel(AbstractRichTextBlock model) : base(model)
        {
            _model = model;
        }

        /// <summary>
        /// Rich text to display to the user.
        /// </summary>
        public string RichText
        {
            get => _model.RichText;
            set => SetProperty(_model.RichText, value, _model, (u, n) => _model.RichText = n);
        }
    }
}