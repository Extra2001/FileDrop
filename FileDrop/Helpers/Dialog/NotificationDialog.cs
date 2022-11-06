using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Windows.AppNotifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDrop.Helpers.Dialog
{
    internal class NotificationDialog
    {
        private static Action<int> recievedCallback = null;

        public static AppNotification SendRecieveToast(string message, Action<int> action)
        {
            recievedCallback = action;
            var xmlPayload = new string($@"
                <toast scenario=""incomingCall""
                        useButtonStyle=""true"">    
                    <visual>
                        <binding template=""ToastGeneric"">
                            <text>{message}</text>
                            <progress status=""等待接受...""
                                      value=""indeterminate""/>
                        </binding>
                    </visual>
                    <actions>
                        <action content=""接受""
                                hint-buttonStyle=""Success""
                                activationType=""foreground""
                                arguments=""accepted"" />
                        <action content=""拒绝""
                                hint-buttonStyle=""Critical""
                                activationType=""foreground""
                                arguments=""declined"" />
                    </actions>
                </toast>");

            var toast = new AppNotification(xmlPayload);
            toast.Expiration = DateTimeOffset.Now + TimeSpan.FromSeconds(30);
            AppNotificationManager.Default.Show(toast);
            Task.Delay(30000).ContinueWith(x =>
            {
                recievedCallback?.Invoke(4);
            });
            return toast;
        }

        public static void NotificationClicked(string argument)
        {
            if (string.IsNullOrEmpty(argument))
            {
                recievedCallback?.Invoke(3);
            }
            else if (argument == "accepted")
            {
                recievedCallback?.Invoke(1);
            }
            else
            {
                recievedCallback?.Invoke(2);
            }
            recievedCallback = null;
        }
    }
}
