﻿using System;
using System.Threading.Tasks;

namespace OwlCore.AbstractUI
{
    /// <summary>
    /// Presents a text box to the user, with actions for saving any entered data.
    /// </summary>
    public class AbstractTextBox : AbstractUIElement
    {
        /// <summary>
        /// Creates a new instance of <see cref="AbstractTextBox"/>.
        /// </summary>
        /// <param name="id"><inheritdoc cref="AbstractUIBase.Id"/></param>
        public AbstractTextBox(string id)
            : base(id)
        {
            Value = string.Empty;
            PlaceholderText = string.Empty;
        }

        /// <summary>
        /// Creates a new instance of <see cref="AbstractTextBox"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"><inheritdoc cref="Value"/></param>
        /// <param name="placeholderText"><inheritdoc cref="PlaceholderText"/></param>
        public AbstractTextBox(string id, string value, string placeholderText)
            : base(id)
        {
            Value = value;
            PlaceholderText = placeholderText;
        }

        /// <summary>
        /// Placeholder text to show when the text box is empty.
        /// </summary>
        public string PlaceholderText { get; }

        /// <summary>
        /// The initial or current value of the text box.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Called to tell the core about the new value.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        public void SaveValue(string newValue)
        {
            Value = newValue;
            ValueChanged?.Invoke(this, newValue);
        }

        /// <summary>
        /// Fires when <see cref="Value"/> is changed.
        /// </summary>
        public event EventHandler<string>? ValueChanged;
    }
}