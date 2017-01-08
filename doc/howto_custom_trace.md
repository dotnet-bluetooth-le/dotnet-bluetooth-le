### How can I define a custom Trace method?

The trace method is used to output internaltrace messages. The default trace methods are `Console.WriteLine` for vanilla and `Mvx.Trace` for MvvMCross.
The trace method is a `Action<string, object[]>` (a function with a format string followed by a parameter array).

To override the default method, just set the property `Trace.TraceImplementation` somewhere in your App's startup code (e.g. AppDelegate, MainActivity, ...).
If you are using the MvvMCross plugin, you have to set it after MvvMCross has loaded the plugin bootstrap object.

The following example adds a custom trace method, that prefixes the message with a time stamp and writes it to the console.

```
Trace.TraceImplementation = (format, @params) =>
{
    Debug.WriteLine($"{DateTime.Now}: {format}", @params);  
};
```
