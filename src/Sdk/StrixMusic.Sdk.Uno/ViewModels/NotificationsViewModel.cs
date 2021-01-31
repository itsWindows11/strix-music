﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using OwlCore;
using StrixMusic.Sdk.Services.Notifications;
using StrixMusic.Sdk.Uno.Services.NotificationService;

namespace StrixMusic.Sdk.Uno.ViewModels
{
    /// <summary>
    /// Manages the notifications coming from the <see cref="NotificationService"/>.
    /// </summary>
    public class NotificationsViewModel : IDisposable
    {
        private readonly NotificationService _notificationService;

        /// <summary>
        /// Creates a new instance of <see cref="NotificationsViewModel"/>.
        /// </summary>
        /// <param name="notificationService"></param>
        public NotificationsViewModel(NotificationService notificationService)
        {
            _notificationService = notificationService;

            AttachEvents();
        }

        /// <summary>
        /// The currently display notifications.
        /// </summary>
        public ObservableCollection<NotificationViewModel> Notifications { get; set; } = new ObservableCollection<NotificationViewModel>();

        private void AttachEvents()
        {
            _notificationService.NotificationRaised += NotificationService_NotificationRaised;
            _notificationService.NotificationDismissed += NotificationService_NotificationDismissed;
        }

        private void DetachEvents()
        {
            _notificationService.NotificationRaised -= NotificationService_NotificationRaised;
            _notificationService.NotificationDismissed -= NotificationService_NotificationDismissed;
        }

        private void NotificationService_NotificationDismissed(object sender, Notification e)
        {
            _ = Threading.OnPrimaryThread(() =>
            {
                var relevantNotification = Notifications.FirstOrDefault(x => ReferenceEquals(x.Model, e));
                if (relevantNotification is null)
                    return;

                Notifications.Remove(relevantNotification);
            });
        }

        private void NotificationService_NotificationRaised(object sender, Notification e)
        {
            _ = Threading.OnPrimaryThread(() =>
            {
                Notifications.Add(new NotificationViewModel(e));
            });
        }

        /// <inheritdoc />
        public void Dispose()
        {
            DetachEvents();
        }
    }
}