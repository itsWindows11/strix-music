﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace StrixMusic.Shell.Default.Controls
{
    /// <summary>
    /// A slider that can automatically update the tick position.
    /// </summary>
    public partial class ProgressSliderControl : SliderEx
    {
        /// <summary>
        /// <see cref="DependencyProperty"/> for the <see cref="UpdateFrequency"/> property.
        /// </summary>
        public static readonly DependencyProperty UpdateFrequencyProperty =
            DependencyProperty.Register(
                nameof(UpdateFrequency),
                typeof(long),
                typeof(ProgressSliderControl),
                new PropertyMetadata(50L)); // 100 milliseconds

        /// <summary>
        /// <see cref="DependencyProperty"/> for the <see cref="UpdateFrequency"/> property.
        /// </summary>
        public static readonly DependencyProperty IsAdvancingProperty =
            DependencyProperty.Register(
                nameof(IsAdvancing),
                typeof(bool),
                typeof(ProgressSliderControl),
                new PropertyMetadata(true));

        private DispatcherTimer _updateIntervalTimer = new DispatcherTimer();
        private DateTime _startTime = DateTime.Now;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressSliderControl"/> class.
        /// </summary>
        public ProgressSliderControl()
            : base()
        {
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;

            SliderManipulationStarted += (s, e) => PauseTimer();
            SliderManipulationCompleted += (s, e) => ResumeTimer();
            ValueChanged += (s, e) => _startTime = DateTime.Now - TimeSpan.FromMilliseconds(Value);
            _updateIntervalTimer.Tick += (s, e) => UpdateSliderValue();
            UpdateTimer();
        }

        /// <summary>
        /// Gets or sets the whether or not the slider is self advancing.
        /// </summary>
        public bool IsAdvancing
        {
            get => (bool)GetValue(IsAdvancingProperty);
            set
            {
                SetValue(IsAdvancingProperty, value);
                if (value)
                {
                    ResumeTimer();
                }
                else
                {
                    PauseTimer();
                }
            }
        }

        /// <summary>
        /// Gets or sets the frequency with which the slider should move forward.
        /// </summary>
        /// <remarks>
        /// Milliseconds
        /// </remarks>
        public long UpdateFrequency
        {
            get => (long)GetValue(UpdateFrequencyProperty);
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "UpdateFrequency must be greater than 0");
                }

                SetValue(UpdateFrequencyProperty, value);
                UpdateTimer();
            }
        }

        private void UpdateTimer()
        {
            _updateIntervalTimer.Stop();
            _updateIntervalTimer.Interval = TimeSpan.FromMilliseconds(UpdateFrequency);
            ResumeTimer();
        }

        private void ResumeTimer()
        {
            _startTime = DateTime.Now - TimeSpan.FromMilliseconds(Value);
            _updateIntervalTimer.Start();
        }

        private void PauseTimer()
        {
            _updateIntervalTimer.Stop();
        }

        private void UpdateSliderValue()
        {
            Value = (DateTime.Now - _startTime).TotalMilliseconds;
        }
    }
}
