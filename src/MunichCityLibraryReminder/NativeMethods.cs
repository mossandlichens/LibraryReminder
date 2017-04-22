// -----------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace MunichCityLibraryReminder
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class NativeMethods
    {
        /// <summary>
        ///     Get the Internet Connection State
        /// </summary>
        /// <param name="description">Internet Connection State Description</param>
        /// <param name="reservedValue">Internet Connection State Reserved Value</param>
        /// <returns>true if the state is connected</returns>
        [DllImport("wininet.dll")]
        public static extern bool InternetGetConnectedState(out int description, int reservedValue);
    }
}
