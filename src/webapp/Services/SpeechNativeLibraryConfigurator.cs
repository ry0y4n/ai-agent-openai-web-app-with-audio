using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;

namespace dotnetfashionassistant.Services
{
    public static class SpeechNativeLibraryConfigurator
    {
        private static bool _configured;
        private static readonly object _lock = new object();
        private static readonly string[] AdditionalLibraries = new[]
        {
            "libcrypto.so.1.1",
            "libssl.so.1.1"
        };

        public static void EnsureConfigured()
        {
            if (_configured)
            {
                return;
            }

            lock (_lock)
            {
                if (_configured)
                {
                    return;
                }

                try
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        ConfigureLinuxLibraryPath();
                    }
                }
                catch
                {
                    // swallow - if configuration fails we'll rely on existing environment
                }

                _configured = true;
            }
        }

        private static void ConfigureLinuxLibraryPath()
        {
            var baseDirectory = AppContext.BaseDirectory;
            var nativePath = Path.Combine(baseDirectory, "runtimes", "linux-x64", "native");
            if (!Directory.Exists(nativePath))
            {
                return;
            }

            const string variableName = "LD_LIBRARY_PATH";
            var currentValue = Environment.GetEnvironmentVariable(variableName);

            if (string.IsNullOrWhiteSpace(currentValue))
            {
                Environment.SetEnvironmentVariable(variableName, nativePath);
            }
            else
            {
                var paths = currentValue.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (!paths.Contains(nativePath, StringComparer.Ordinal))
                {
                    var newValue = string.Join(':', new[] { nativePath }.Concat(paths));
                    Environment.SetEnvironmentVariable(variableName, newValue);
                }
            }

            foreach (var libraryFile in AdditionalLibraries.Concat(new[] { "libMicrosoft.CognitiveServices.Speech.core.so" }))
            {
                var libraryPath = Path.Combine(nativePath, libraryFile);
                if (!File.Exists(libraryPath))
                {
                    continue;
                }

                try
                {
                    NativeLibrary.Load(libraryPath);
                }
                catch
                {
                    // Ignore load failures; the Speech SDK will attempt its own resolution path.
                }
            }
        }
    }
}
