// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace MunichCityLibraryReminder
{
    using System;
    using System.Windows;
    using System.Windows.Threading;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class ExtensionMethods
    {
        private static Action EmptyDelegate = delegate() { };



        public static void Refresh(this UIElement uiElement)
        {

            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);

        }        
    }
}
