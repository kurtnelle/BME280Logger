using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;


public class WaitForDebugger {

    public WaitForDebugger() {
        DateTime _buildDate = GetBuildDate().ToLocalTime();
        Console.WriteLine("Build Date: " + _buildDate.ToLongDateString() + " : " + _buildDate.ToLongTimeString());
        int _waiting = 0;
        while (!Debugger.IsAttached) {
            string _dots = new string('.', _waiting);
            Console.Write($"\rWaiting on debugger{new string('.', _waiting)}{new string(' ', 3 - _waiting)}");
            Thread.Sleep(1000);
            _waiting = _waiting < 3 ? _waiting + 1 : 1;
        }
        Console.WriteLine();
    }
    private static DateTime GetBuildDate(Assembly assembly) {
        const string BuildVersionMetadataPrefix = "+build";

        var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attribute?.InformationalVersion != null) {
            var value = attribute.InformationalVersion;
            var index = value.IndexOf(BuildVersionMetadataPrefix);
            if (index > 0) {
                value = value.Substring(index + BuildVersionMetadataPrefix.Length);
                if (DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)) {
                    return result;
                }
            }
        }

        return default;
    }
    public static DateTime GetBuildDate() {
        return GetBuildDate(Assembly.GetExecutingAssembly());
    }
}
