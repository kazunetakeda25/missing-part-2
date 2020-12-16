using System;
using System.Runtime.InteropServices;
using System.Text;

namespace wyDay.TurboActivate
{
    public static class TurboActivate
    {
        [Flags]
        public enum TA_Flags : uint
        {
            TA_SYSTEM = 1,
            TA_USER = 2,

            /// <summary>
            /// Use the TA_DISALLOW_VM in UseTrial() to disallow trials in virtual machines. 
            /// If you use this flag in UseTrial() and the customer's machine is a Virtual
            /// Machine, then UseTrial() will throw VirtualMachineException.
            /// </summary>
            TA_DISALLOW_VM = 4,
        }

        [Flags]
        public enum TA_DateCheckFlags : uint
        {
            TA_HAS_NOT_EXPIRED = 1,
        }

        static class Native
        {
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct ACTIVATE_OPTIONS
            {
                public UInt32 nLength;

                [MarshalAs(UnmanagedType.LPWStr)]
                public string sExtraData;
            }

            [Flags]
            public enum GenuineFlags : uint
            {
                TA_SKIP_OFFLINE = 1,
                TA_OFFLINE_SHOW_INET_ERR = 2
            };

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct GENUINE_OPTIONS
            {
                public UInt32 nLength;
                public GenuineFlags flags;
                public UInt32 nDaysBetweenChecks;
                public UInt32 nGraceDaysOnInetErr;
            }

#if UNITY_STANDALONE_WIN
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int Activate();

#if UNITY_STANDALONE_WIN
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int ActivateEx(ref ACTIVATE_OPTIONS options);
#if UNITY_STANDALONE_WIN
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int ActivationRequestToFile(string filename);

#if UNITY_STANDALONE_WIN
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int ActivationRequestToFileEx(string filename, ref ACTIVATE_OPTIONS options);

#if UNITY_STANDALONE_WIN
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int ActivateFromFile(string filename);

#if UNITY_STANDALONE_WIN
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int BlackListKeys([In] string[] keys, uint numKeys);

#if UNITY_STANDALONE_WIN
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int CheckAndSavePKey(string productKey, TA_Flags flags);

#if UNITY_STANDALONE_WIN
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int Deactivate(byte erasePkey);

#if UNITY_STANDALONE_WIN
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int DeactivationRequestToFile(string filename, byte erasePkey);

#if UNITY_STANDALONE_WIN
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int GetExtraData(StringBuilder lpValueStr, int cchValue);

#if UNITY_STANDALONE_WIN
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int GetFeatureValue(string featureName, StringBuilder lpValueStr, int cchValue);

#if UNITY_STANDALONE_WIN
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int GetPKey(StringBuilder lpPKeyStr, int cchPKey);

#if UNITY_STANDALONE_WIN
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int IsActivated(string versionGUID);

#if UNITY_STANDALONE_WIN
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int IsDateValid(string date_time, TA_DateCheckFlags flags);

#if UNITY_STANDALONE_WIN
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int IsGenuine(string versionGUID);

#if UNITY_STANDALONE_WIN
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int IsGenuineEx(string versionGUID, ref GENUINE_OPTIONS options);

#if UNITY_STANDALONE_WIN
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int IsProductKeyValid(string versionGUID);

#if UNITY_STANDALONE_WIN
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int SetCustomProxy(string proxy);

#if UNITY_STANDALONE_WIN
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int TrialDaysRemaining(string versionGUID, ref uint DaysRemaining);

#if UNITY_STANDALONE_WIN
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int UseTrial(TA_Flags flags);

#if UNITY_STANDALONE_WIN
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int ExtendTrial(string trialExtension);

#if UNITY_STANDALONE_WIN
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int PDetsFromPath(string filename);

#if UNITY_STANDALONE_WIN
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int SetCurrentProduct(string versionGUID);

#if UNITY_STANDALONE_WIN
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int GetCurrentProduct(StringBuilder lpValueStr, int cchValue);

#if UNITY_STANDALONE_WIN
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int SetCustomActDataPath(string directory);

            /* Obsolete functions: These will be removed in an upcoming version. */
#if UNITY_STANDALONE_WIN
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int GracePeriodDaysRemaining(string versionGUID, ref uint DaysRemaining);
        }

        /*
         To use "AnyCPU" Target CPU type, first copy the x64 TurboActivate.dll and rename to TurboActivate64.dll
         Then in your project properties go to the Build panel, and add the TA_BOTH_DLL conditional compilation symbol.
        */

#if TA_BOTH_DLL
        static class Native64
        {
            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int Activate();

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int ActivateEx(ref Native.ACTIVATE_OPTIONS options);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int ActivationRequestToFile(string filename);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int ActivationRequestToFileEx(string filename, ref Native.ACTIVATE_OPTIONS options);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int ActivateFromFile(string filename);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int BlackListKeys([In] string[] keys, uint numKeys);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int CheckAndSavePKey(string productKey, TA_Flags flags);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int Deactivate(byte erasePkey);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int DeactivationRequestToFile(string filename, byte erasePkey);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int GetExtraData(StringBuilder lpValueStr, int cchValue);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int GetFeatureValue(string featureName, StringBuilder lpValueStr, int cchValue);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int GetPKey(StringBuilder lpPKeyStr, int cchPKey);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int IsActivated(string versionGUID);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int IsDateValid(string date_time, TA_DateCheckFlags flags);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int IsGenuine(string versionGUID);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int IsGenuineEx(string versionGUID, ref Native.GENUINE_OPTIONS options);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int IsProductKeyValid(string versionGUID);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int SetCustomProxy(string proxy);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int TrialDaysRemaining(string versionGUID, ref uint DaysRemaining);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int UseTrial(TA_Flags flags);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int ExtendTrial(string trialExtension);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int PDetsFromPath(string filename);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int SetCurrentProduct(string versionGUID);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int GetCurrentProduct(StringBuilder lpValueStr, int cchValue);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int SetCustomActDataPath(string directory);

            /* Obsolete functions: These will be removed in an upcoming version. */
            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int GracePeriodDaysRemaining(string versionGUID, ref uint DaysRemaining);
        }
#endif

        const int TA_OK = 0x00;
        const int TA_FAIL = 0x01;
        const int TA_E_PKEY = 0x02;
        const int TA_E_ACTIVATE = 0x03;
        const int TA_E_INET = 0x04;
        const int TA_E_INUSE = 0x05;
        const int TA_E_REVOKED = 0x06;
        const int TA_E_GUID = 0x07;
        const int TA_E_PDETS = 0x08;
        const int TA_E_TRIAL = 0x09;
        const int TA_E_REACTIVATE = 0x0A;
        const int TA_E_COM = 0x0B;
        const int TA_E_TRIAL_EUSED = 0x0C;
        const int TA_E_TRIAL_EEXP = 0x0D;
        const int TA_E_EXPIRED = 0x0D;
        const int TA_E_INSUFFICIENT_BUFFER = 0x0E;
        const int TA_E_PERMISSION = 0x0F;
        const int TA_E_INVALID_FLAGS = 0x10;
        const int TA_E_IN_VM = 0x11;
        const int TA_E_EDATA_LONG = 0x12;
        const int TA_E_INVALID_ARGS = 0x13;
        const int TA_E_KEY_FOR_TURBOFLOAT = 0x14;
        const int TA_E_INET_DELAYED = 0x15;
        const int TA_E_FEATURES_CHANGED = 0x16;
        const int TA_E_NO_MORE_DEACTIVATIONS = 0x18;

        /// <summary>The GUID for this product version. This is found on the LimeLM site on the version overview.</summary>
        public static string VersionGUID { get; set; }

        /// <summary>Activates the product on this computer. You must call <see cref="CheckAndSavePKey(string)"/> with a valid product key or have used the TurboActivate wizard sometime before calling this function.</summary>
        /// <exception cref="InvalidProductKeyException">The product key is invalid or there's no product key.</exception>
        /// <exception cref="InternetException">Connection to the server failed.</exception>
        /// <exception cref="PkeyMaxUsedException">The product key has already been activated with the maximum number of computers.</exception>
        /// <exception cref="PkeyRevokedException">The product key has been revoked.</exception>
        /// <exception cref="ProductDetailsException">The product details file "TurboActivate.dat" failed to load. It's either missing or corrupt.</exception>
        /// <exception cref="COMException">CoInitializeEx failed.</exception>
        /// <exception cref="TurboActivateException">Failed to activate.</exception>
        /// <exception cref="DateTimeException">Failed because your system date and time settings are incorrect. Fix your date and time settings, restart your computer, and try to activate again.</exception>
        /// <exception cref="VirtualMachineException">The function failed because this instance of your program is running inside a virtual machine / hypervisor and you've prevented the function from running inside a VM.</exception>
        /// <exception cref="TurboFloatKeyException">The product key used is for TurboFloat, not TurboActivate.</exception>
        public static void Activate()
        {
#if TA_BOTH_DLL
            switch (IntPtr.Size == 8 ? Native64.Activate() : Native.Activate())
#else
            switch (Native.Activate())
#endif
            {
                case TA_E_PKEY:
                    throw new InvalidProductKeyException();
                case TA_E_INET:
                    throw new InternetException();
                case TA_E_INUSE:
                    throw new PkeyMaxUsedException();
                case TA_E_REVOKED:
                    throw new PkeyRevokedException();
                case TA_E_PDETS:
                    throw new ProductDetailsException();
                case TA_E_COM:
                    throw new COMException();
                case TA_E_EXPIRED:
                    throw new DateTimeException(false);
                case TA_E_IN_VM:
                    throw new VirtualMachineException();
                case TA_E_KEY_FOR_TURBOFLOAT:
                    throw new TurboFloatKeyException();
                case TA_OK: // successful
                    return;
                default:
                    throw new TurboActivateException("Failed to activate.");
            }
        }

        /// <summary>Activates the product on this computer. You must call <see cref="CheckAndSavePKey(string)"/> with a valid product key or have used the TurboActivate wizard sometime before calling this function.</summary>
        /// <param name="extraData">Extra data to pass to the LimeLM servers that will be visible for you to see and use. Maximum size is 255 UTF-8 characters.</param>
        /// <exception cref="InvalidProductKeyException">The product key is invalid or there's no product key.</exception>
        /// <exception cref="InternetException">Connection to the server failed.</exception>
        /// <exception cref="PkeyMaxUsedException">The product key has already been activated with the maximum number of computers.</exception>
        /// <exception cref="PkeyRevokedException">The product key has been revoked.</exception>
        /// <exception cref="ProductDetailsException">The product details file "TurboActivate.dat" failed to load. It's either missing or corrupt.</exception>
        /// <exception cref="COMException">CoInitializeEx failed.</exception>
        /// <exception cref="TurboActivateException">Failed to activate.</exception>
        /// <exception cref="DateTimeException">Failed because your system date and time settings are incorrect. Fix your date and time settings, restart your computer, and try to activate again.</exception>
        /// <exception cref="VirtualMachineException">The function failed because this instance of your program is running inside a virtual machine / hypervisor and you've prevented the function from running inside a VM.</exception>
        /// <exception cref="ExtraDataTooLongException">The "extra data" was too long. You're limited to 255 UTF-8 characters. Or, on Windows, a Unicode string that will convert into 255 UTF-8 characters or less.</exception>
        /// <exception cref="InvalidArgsException">The arguments passed to the function are invalid. Double check your logic.</exception>
        /// <exception cref="TurboFloatKeyException">The product key used is for TurboFloat, not TurboActivate.</exception>
        public static void Activate(string extraData)
        {
            Native.ACTIVATE_OPTIONS opts = new Native.ACTIVATE_OPTIONS {sExtraData = extraData};
            opts.nLength = (uint)Marshal.SizeOf(opts);

#if TA_BOTH_DLL
            switch (IntPtr.Size == 8 ? Native64.ActivateEx(ref opts) : Native.ActivateEx(ref opts))
#else
            switch (Native.ActivateEx(ref opts))
#endif
            {
                case TA_E_PKEY:
                    throw new InvalidProductKeyException();
                case TA_E_INET:
                    throw new InternetException();
                case TA_E_INUSE:
                    throw new PkeyMaxUsedException();
                case TA_E_REVOKED:
                    throw new PkeyRevokedException();
                case TA_E_PDETS:
                    throw new ProductDetailsException();
                case TA_E_COM:
                    throw new COMException();
                case TA_E_EXPIRED:
                    throw new DateTimeException(false);
                case TA_E_IN_VM:
                    throw new VirtualMachineException();
                case TA_E_EDATA_LONG:
                    throw new ExtraDataTooLongException();
                case TA_E_INVALID_ARGS:
                    throw new InvalidArgsException();
                case TA_E_KEY_FOR_TURBOFLOAT:
                    throw new TurboFloatKeyException();
                case TA_OK: // successful
                    return;
                default:
                    throw new TurboActivateException("Failed to activate.");
            }
        }

        /// <summary>Get the "activation request" file for offline activation. You must call <see cref="CheckAndSavePKey(string)"/> with a valid product key or have used the TurboActivate wizard sometime before calling this function.</summary>
        /// <param name="filename">The location where you want to save the activation request file.</param>
        /// <exception cref="InvalidProductKeyException">The product key is invalid or there's no product key.</exception>
        /// <exception cref="ProductDetailsException">The product details file "TurboActivate.dat" failed to load. It's either missing or corrupt.</exception>
        /// <exception cref="COMException">CoInitializeEx failed.</exception>
        /// <exception cref="TurboActivateException">Failed to activate.</exception>
        public static void ActivationRequestToFile(string filename)
        {
#if TA_BOTH_DLL
            switch (IntPtr.Size == 8 ? Native64.ActivationRequestToFile(filename) : Native.ActivationRequestToFile(filename))
#else
            switch (Native.ActivationRequestToFile(filename))
#endif
            {
                case TA_E_PKEY:
                    throw new InvalidProductKeyException();
                case TA_E_PDETS:
                    throw new ProductDetailsException();
                case TA_E_COM:
                    throw new COMException();
                case TA_OK: // successful
                    return;
                default:
                    throw new TurboActivateException("Failed to save the activation request file.");
            }
        }

        /// <summary>Get the "activation request" file for offline activation. You must call <see cref="CheckAndSavePKey(string)"/> with a valid product key or have used the TurboActivate wizard sometime before calling this function.</summary>
        /// <param name="filename">The location where you want to save the activation request file.</param>
        /// <param name="extraData">Extra data to pass to the LimeLM servers that will be visible for you to see and use. Maximum size is 255 UTF-8 characters.</param>
        /// <exception cref="InvalidProductKeyException">The product key is invalid or there's no product key.</exception>
        /// <exception cref="ProductDetailsException">The product details file "TurboActivate.dat" failed to load. It's either missing or corrupt.</exception>
        /// <exception cref="COMException">CoInitializeEx failed.</exception>
        /// <exception cref="ExtraDataTooLongException">The "extra data" was too long. You're limited to 255 UTF-8 characters. Or, on Windows, a Unicode string that will convert into 255 UTF-8 characters or less.</exception>
        /// <exception cref="InvalidArgsException">The arguments passed to the function are invalid. Double check your logic.</exception>
        /// <exception cref="TurboActivateException">Failed to activate.</exception>
        public static void ActivationRequestToFile(string filename, string extraData)
        {
            Native.ACTIVATE_OPTIONS opts = new Native.ACTIVATE_OPTIONS { sExtraData = extraData };
            opts.nLength = (uint)Marshal.SizeOf(opts);

#if TA_BOTH_DLL
            switch (IntPtr.Size == 8 ? Native64.ActivationRequestToFileEx(filename, ref opts) : Native.ActivationRequestToFileEx(filename, ref opts))
#else
            switch (Native.ActivationRequestToFileEx(filename, ref opts))
#endif
            {
                case TA_E_PKEY:
                    throw new InvalidProductKeyException();
                case TA_E_PDETS:
                    throw new ProductDetailsException();
                case TA_E_COM:
                    throw new COMException();
                case TA_E_EDATA_LONG:
                    throw new ExtraDataTooLongException();
                case TA_E_INVALID_ARGS:
                    throw new InvalidArgsException();
                case TA_OK: // successful
                    return;
                default:
                    throw new TurboActivateException("Failed to save the activation request file.");
            }
        }

        /// <summary>Activate from the "activation response" file for offline activation.</summary>
        /// <param name="filename">The location of the activation response file.</param>
        /// <exception cref="InvalidProductKeyException">The product key is invalid or there's no product key.</exception>
        /// <exception cref="ProductDetailsException">The product details file "TurboActivate.dat" failed to load. It's either missing or corrupt.</exception>
        /// <exception cref="COMException">CoInitializeEx failed.</exception>
        /// <exception cref="TurboActivateException">Failed to activate.</exception>
        /// <exception cref="DateTimeException">Either the activation response file has expired or your date and time settings are incorrect. Fix your date and time settings, restart your computer, and try to activate again.</exception>
        /// <exception cref="VirtualMachineException">The function failed because this instance of your program is running inside a virtual machine / hypervisor and you've prevented the function from running inside a VM.</exception>
        public static void ActivateFromFile(string filename)
        {
#if TA_BOTH_DLL
            switch (IntPtr.Size == 8 ? Native64.ActivateFromFile(filename) : Native.ActivateFromFile(filename))
#else
            switch (Native.ActivateFromFile(filename))
#endif
            {
                case TA_E_PKEY:
                    throw new InvalidProductKeyException();
                case TA_E_PDETS:
                    throw new ProductDetailsException();
                case TA_E_COM:
                    throw new COMException();
                case TA_E_EXPIRED:
                    throw new DateTimeException(true);
                case TA_E_IN_VM:
                    throw new VirtualMachineException();
                case TA_OK: // successful
                    return;
                default:
                    throw new TurboActivateException("Failed to activate.");
            }
        }

        /// <summary>Blacklists keys so they are no longer valid. Use "BlackListKeys" only if you're using the "Serial-only plan" in LimeLM. Otherwise revoke keys.</summary>
        /// <param name="keys">A string array of the product keys to blacklist.</param>
        /// <exception cref="TurboActivateException">Failed to add keys to the blacklist. Make sure the keys string array is not empty or null.</exception>
        public static void BlackListKeys(string[] keys)
        {
#if TA_BOTH_DLL
            if ((IntPtr.Size == 8 ? Native64.BlackListKeys(keys, (uint)keys.Length) : Native.BlackListKeys(keys, (uint)keys.Length)) != 0)
#else
            if (Native.BlackListKeys(keys, (uint)keys.Length) != TA_OK)
#endif
                throw new TurboActivateException("Failed to add keys to the blacklist. Make sure the keys string array is not empty or null.");
        }

        /// <summary>Checks and saves the product key.</summary>
        /// <param name="productKey">The product key you want to save.</param>
        /// <param name="flags">Whether to create the activation either user-wide or system-wide. Valid flags are <see cref="TA_Flags.TA_SYSTEM"/> and <see cref="TA_Flags.TA_USER"/>.</param>
        /// <returns>True if the product key is valid, false if it's not</returns>
        /// <exception cref="ProductDetailsException">The product details file "TurboActivate.dat" failed to load. It's either missing or corrupt.</exception>
        /// <exception cref="PermissionException">Insufficient system permission. Either start your process as an admin / elevated user or call the function again with the TA_USER flag.</exception>
        /// <exception cref="InvalidFlagsException">The flags you passed to the function were invalid (or missing). Flags like "TA_SYSTEM" and "TA_USER" are mutually exclusive -- you can only use one or the other.</exception>
        public static bool CheckAndSavePKey(string productKey, TA_Flags flags = TA_Flags.TA_USER)
        {
#if TA_BOTH_DLL
            switch (IntPtr.Size == 8 ? Native64.CheckAndSavePKey(productKey, flags) : Native.CheckAndSavePKey(productKey, flags))
#else
            switch (Native.CheckAndSavePKey(productKey, flags))
#endif
            {
                case TA_E_PDETS:
                    throw new ProductDetailsException();
                case TA_OK: // successful
                    return true;
                case TA_E_PERMISSION:
                    throw new PermissionException();
                case TA_E_INVALID_FLAGS:
                    throw new InvalidFlagsException();
                default:
                    return false;
            }
        }

        /// <summary>Deactivates the product on this computer.</summary>
        /// <param name="eraseProductKey">Erase the product key so the user will have to enter a new product key if they wish to reactivate.</param>
        /// <exception cref="InvalidProductKeyException">The product key is invalid or there's no product key.</exception>
        /// <exception cref="NotActivatedException">The product needs to be activated.</exception>
        /// <exception cref="InternetException">Connection to the server failed.</exception>
        /// <exception cref="ProductDetailsException">The product details file "TurboActivate.dat" failed to load. It's either missing or corrupt.</exception>
        /// <exception cref="COMException">CoInitializeEx failed.</exception>
        /// <exception cref="TurboActivateException">Failed to deactivate.</exception>
        public static void Deactivate(bool eraseProductKey = false)
        {
#if TA_BOTH_DLL
            switch (IntPtr.Size == 8 ? Native64.Deactivate((byte)(eraseProductKey ? 1 : 0)) : Native.Deactivate((byte)(eraseProductKey ? 1 : 0)))
#else
            switch (Native.Deactivate((byte)(eraseProductKey ? 1 : 0)))
#endif
            {
                case TA_E_PKEY:
                    throw new InvalidProductKeyException();
                case TA_E_ACTIVATE:
                    throw new NotActivatedException();
                case TA_E_INET:
                    throw new InternetException();
                case TA_E_PDETS:
                    throw new ProductDetailsException();
                case TA_E_COM:
                    throw new COMException();
                case TA_E_NO_MORE_DEACTIVATIONS:
                    throw new NoMoreDeactivationsException();
                case TA_OK: // successful
                    return;
                default:
                    throw new TurboActivateException("Failed to deactivate.");
            }
        }

        /// <summary>Get the "deactivation request" file for offline deactivation.</summary>
        /// <param name="filename">The location where you want to save the deactivation request file.</param>
        /// <param name="eraseProductKey">Erase the product key so the user will have to enter a new product key if they wish to reactivate.</param>
        /// <exception cref="InvalidProductKeyException">The product key is invalid or there's no product key.</exception>
        /// <exception cref="NotActivatedException">The product needs to be activated.</exception>
        /// <exception cref="ProductDetailsException">The product details file "TurboActivate.dat" failed to load. It's either missing or corrupt.</exception>
        /// <exception cref="COMException">CoInitializeEx failed.</exception>
        /// <exception cref="TurboActivateException">Failed to activate.</exception>
        public static void DeactivationRequestToFile(string filename, bool eraseProductKey)
        {
#if TA_BOTH_DLL
            switch (IntPtr.Size == 8 ? Native64.DeactivationRequestToFile(filename, (byte)(eraseProductKey ? 1 : 0)) : Native.DeactivationRequestToFile(filename, (byte)(eraseProductKey ? 1 : 0)))
#else
            switch (Native.DeactivationRequestToFile(filename, (byte)(eraseProductKey ? 1 : 0)))
#endif
            {
                case TA_E_PKEY:
                    throw new InvalidProductKeyException();
                case TA_E_ACTIVATE:
                    throw new NotActivatedException();
                case TA_E_PDETS:
                    throw new ProductDetailsException();
                case TA_E_COM:
                    throw new COMException();
                case TA_OK: // successful
                    return;
                default:
                    throw new TurboActivateException("Failed to deactivate.");
            }
        }

        /// <summary>Gets the extra data value you passed in when activating.</summary>
        /// <returns>Returns the extra data if it exists, otherwise it returns null.</returns>
        /// <exception cref="ProductDetailsException">The product details file "TurboActivate.dat" failed to load. It's either missing or corrupt.</exception>
        public static string GetExtraData()
        {
#if TA_BOTH_DLL
            int length = IntPtr.Size == 8 ? Native64.GetExtraData(null, 0) : Native.GetExtraData(null, 0);
#else
            int length = Native.GetExtraData(null, 0);
#endif

            StringBuilder sb = new StringBuilder(length);

#if TA_BOTH_DLL
            switch (IntPtr.Size == 8 ? Native64.GetExtraData(sb, length) : Native.GetExtraData(sb, length))
#else
            switch (Native.GetExtraData(sb, length))
#endif
            {
                case TA_E_PDETS:
                    throw new ProductDetailsException();
                case TA_OK: // success
                    return sb.ToString();
                default:
                    return null;
            }
        }

        /// <summary>Gets the value of a feature.</summary>
        /// <param name="featureName">The name of the feature to retrieve the value for.</param>
        /// <returns>Returns the feature value.</returns>
        /// <exception cref="ProductDetailsException">The product details file "TurboActivate.dat" failed to load. It's either missing or corrupt.</exception>
        /// <exception cref="TurboActivateException">Failed to get feature value. The feature doesn't exist.</exception>
        public static string GetFeatureValue(string featureName)
        {
            string value = GetFeatureValue(featureName, null);

            if (value == null)
                throw new TurboActivateException("Failed to get feature value. The feature doesn't exist.");

            return value;
        }

        /// <summary>Gets the value of a feature.</summary>
        /// <param name="featureName">The name of the feature to retrieve the value for.</param>
        /// <param name="defaultValue">The default value to return if the feature doesn't exist.</param>
        /// <returns>Returns the feature value if it exists, otherwise it returns the default value.</returns>
        /// <exception cref="ProductDetailsException">The product details file "TurboActivate.dat" failed to load. It's either missing or corrupt.</exception>
        public static string GetFeatureValue(string featureName, string defaultValue)
        {
#if TA_BOTH_DLL
            int length = IntPtr.Size == 8 ? Native64.GetFeatureValue(featureName, null, 0) : Native.GetFeatureValue(featureName, null, 0);
#else
            int length = Native.GetFeatureValue(featureName, null, 0);
#endif

            StringBuilder sb = new StringBuilder(length);

#if TA_BOTH_DLL
            switch (IntPtr.Size == 8 ? Native64.GetFeatureValue(featureName, sb, length) : Native.GetFeatureValue(featureName, sb, length))
#else
            switch (Native.GetFeatureValue(featureName, sb, length))
#endif
            {
                case TA_E_PDETS:
                    throw new ProductDetailsException();
                case TA_OK: // success
                    return sb.ToString();
                default:
                    return defaultValue;
            }
        }

        /// <summary>Gets the stored product key. NOTE: if you want to check if a product key is valid simply call <see cref="IsProductKeyValid()"/>.</summary>
        /// <returns>string Product key.</returns>
        /// <exception cref="InvalidProductKeyException">The product key is invalid or there's no product key.</exception>
        /// <exception cref="ProductDetailsException">The product details file "TurboActivate.dat" failed to load. It's either missing or corrupt.</exception>
        /// <exception cref="TurboActivateException">Failed to get the product key.</exception>
        public static string GetPKey()
        {
            // this makes the assumption that the PKey is 34+NULL characters long.
            // This may or may not be true in the future.
            StringBuilder sb = new StringBuilder(35);

#if TA_BOTH_DLL
            switch (IntPtr.Size == 8 ? Native64.GetPKey(sb, 35) : Native.GetPKey(sb, 35))
#else
            switch (Native.GetPKey(sb, 35))
#endif
            {
                case TA_E_PKEY:
                    throw new InvalidProductKeyException();
                case TA_E_PDETS:
                    throw new ProductDetailsException();
                case TA_OK: // success
                    return sb.ToString();
                default:
                    throw new TurboActivateException("Failed to get the product key.");
            }
        }

        /// <summary>Get the number of days left in the activation grace period.</summary>
        /// <returns>The number of days remaining. 0 days if the grace period has expired. (E.g. 1 day means *at most* 1 day. That is it could be 30 seconds.)</returns>
        /// <exception cref="GUIDMismatchException">The version GUID doesn't match that of the product details file. Make sure you set the GUID using TurboActivate.VersionGUID.</exception>
        /// <exception cref="ProductDetailsException">The product details file "TurboActivate.dat" failed to load. It's either missing or corrupt.</exception>
        /// <exception cref="TurboActivateException">Failed to get the activation grace period days remaining.</exception>
        [Obsolete("This function is obsolete and will be removed in TurboActivate 4.0; use the UseTrial(), TrialDaysRemaining(), and ExtendTrial() functions instead.")]
        public static int GracePeriodDaysRemaining()
        {
            uint daysRemain = 0;

#if TA_BOTH_DLL
            switch (IntPtr.Size == 8 ? Native64.GracePeriodDaysRemaining(VersionGUID, ref daysRemain) : Native.GracePeriodDaysRemaining(VersionGUID, ref daysRemain))
#else
            switch (Native.GracePeriodDaysRemaining(VersionGUID, ref daysRemain))
#endif
            {
                case TA_E_GUID:
                    throw new GUIDMismatchException();
                case TA_E_PDETS:
                    throw new ProductDetailsException();
                case TA_OK: // successful
                    break;
                default:
                    throw new TurboActivateException("Failed to get the activation grace period days remaining.");
            }

            return (int)daysRemain;
        }

        /// <summary>Checks whether the computer has been activated.</summary>
        /// <returns>True if the computer is activated. False otherwise.</returns>
        /// <exception cref="GUIDMismatchException">The version GUID doesn't match that of the product details file. Make sure you set the GUID using TurboActivate.VersionGUID.</exception>
        /// <exception cref="ProductDetailsException">The product details file "TurboActivate.dat" failed to load. It's either missing or corrupt.</exception>
        /// <exception cref="COMException">CoInitializeEx failed.</exception>
        /// <exception cref="VirtualMachineException">The function failed because this instance of your program is running inside a virtual machine / hypervisor and you've prevented the function from running inside a VM.</exception>
        public static bool IsActivated()
        {
#if TA_BOTH_DLL
            switch (IntPtr.Size == 8 ? Native64.IsActivated(VersionGUID) : Native.IsActivated(VersionGUID))
#else
            switch (Native.IsActivated(VersionGUID))
#endif
            {
                case TA_E_GUID:
                    throw new GUIDMismatchException();
                case TA_E_PDETS:
                    throw new ProductDetailsException();
                case TA_E_COM:
                    throw new COMException();
                case TA_E_IN_VM:
                    throw new VirtualMachineException();
                case TA_OK: // is activated
                    return true;
            }

            return false;
        }

        /// <summary>Checks if the string in the form "YYYY-MM-DD HH:mm:ss" is a valid date/time. The date must be in UTC time and "24-hour" format. If your date is in some other time format first convert it to UTC time before passing it into this function.</summary>
        /// <param name="date_time">The date time string to check.</param>
        /// <param name="flags">The type of date time check. Valid flags are <see cref="TA_DateCheckFlags.TA_HAS_NOT_EXPIRED"/>.</param>
        /// <returns>True if the date is valid, false if it's not</returns>
        /// <exception cref="ProductDetailsException">The product details file "TurboActivate.dat" failed to load. It's either missing or corrupt.</exception>
        /// <exception cref="InvalidFlagsException">The flags you passed to the function were invalid (or missing). Flags like "TA_SYSTEM" and "TA_USER" are mutually exclusive -- you can only use one or the other.</exception>
        public static bool IsDateValid(string date_time, TA_DateCheckFlags flags)
        {
#if TA_BOTH_DLL
            switch (IntPtr.Size == 8 ? Native64.IsDateValid(date_time, flags) : Native.IsDateValid(date_time, flags))
#else
            switch (Native.IsDateValid(date_time, flags))
#endif
            {
                case TA_E_PDETS:
                    throw new ProductDetailsException();
                case TA_OK: // successful
                    return true;
                case TA_E_INVALID_FLAGS:
                    throw new InvalidFlagsException();
                default:
                    return false;  
            }
        }

        /// <summary>Checks whether the computer is genuinely activated by verifying with the LimeLM servers.</summary>
        /// <param name="needsReactivate">Whether this product needs to be reactivated by calling <see cref="Activate()"/></param>
        /// <returns>True if this product is genuine, false otherwise.</returns>
        /// <exception cref="NotActivatedException">The product needs to be activated.</exception>
        /// <exception cref="InternetException">Connection to the server failed.</exception>
        /// <exception cref="GUIDMismatchException">The version GUID doesn't match that of the product details file. Make sure you set the GUID using TurboActivate.VersionGUID.</exception>
        /// <exception cref="ProductDetailsException">The product details file "TurboActivate.dat" failed to load. It's either missing or corrupt.</exception>
        /// <exception cref="COMException">CoInitializeEx failed.</exception>
        /// <exception cref="DateTimeException">Failed because your system date and time settings are incorrect. Fix your date and time settings, restart your computer, and try to activate again.</exception>
        /// <exception cref="VirtualMachineException">The function failed because this instance of your program is running inside a virtual machine / hypervisor and you've prevented the function from running inside a VM.</exception>
        [Obsolete("This particular override for the IsGenuine() function is obsolete; use the IsGenuine() with no arguments or IsGenuine(uint dayBetweenChecks, uint graceDaysOnInetErr, bool skipOffline = false, bool offlineShowInetErr = false).")]
        public static bool IsGenuine(ref bool needsReactivate)
        {
#if TA_BOTH_DLL
            switch (IntPtr.Size == 8 ? Native64.IsGenuine(VersionGUID) : Native.IsGenuine(VersionGUID))
#else
            switch (Native.IsGenuine(VersionGUID))
#endif
            {
                case TA_E_ACTIVATE:
                    throw new NotActivatedException();
                case TA_E_INET:
                    throw new InternetException();
                case TA_E_GUID:
                    throw new GUIDMismatchException();
                case TA_E_PDETS:
                    throw new ProductDetailsException();
                case TA_E_COM:
                    throw new COMException();
                case TA_E_EXPIRED:
                    throw new DateTimeException(false);
                case TA_E_IN_VM:
                    throw new VirtualMachineException();
                case TA_E_FEATURES_CHANGED:
                case TA_OK: // is activated
                    needsReactivate = false;
                    return true;
            }

            // not genuine (TA_FAIL, TA_E_REVOKED)
            return false;
        }

        /// <summary>Checks whether the computer is genuinely activated by verifying with the LimeLM servers.</summary>
        /// <returns>IsGenuineResult</returns>
        /// <exception cref="GUIDMismatchException">The version GUID doesn't match that of the product details file. Make sure you set the GUID using TurboActivate.VersionGUID.</exception>
        /// <exception cref="ProductDetailsException">The product details file "TurboActivate.dat" failed to load. It's either missing or corrupt.</exception>
        /// <exception cref="COMException">CoInitializeEx failed.</exception>
        /// <exception cref="DateTimeException">Failed because your system date and time settings are incorrect. Fix your date and time settings, restart your computer, and try to activate again.</exception>
        public static IsGenuineResult IsGenuine()
        {
#if TA_BOTH_DLL
            switch (IntPtr.Size == 8 ? Native64.IsGenuine(VersionGUID) : Native.IsGenuine(VersionGUID))
#else
            switch (Native.IsGenuine(VersionGUID))
#endif
            {
                case TA_E_INET:
                    return IsGenuineResult.InternetError;
                case TA_E_GUID:
                    throw new GUIDMismatchException();
                case TA_E_PDETS:
                    throw new ProductDetailsException();
                case TA_E_COM:
                    throw new COMException();
                case TA_E_EXPIRED:
                    throw new DateTimeException(false);
                case TA_E_IN_VM:
                    return IsGenuineResult.NotGenuineInVM;
                case TA_E_FEATURES_CHANGED:
                    return IsGenuineResult.GenuineFeaturesChanged;
                case TA_OK: // is activated
                    return IsGenuineResult.Genuine;
            }

            // not genuine (TA_FAIL, TA_E_REVOKED, TA_E_ACTIVATE)
            return IsGenuineResult.NotGenuine;
        }

        /// <summary>Checks whether the computer is activated, and every "daysBetweenChecks" days it check if the customer is genuinely activated by verifying with the LimeLM servers.</summary>
        /// <param name="daysBetweenChecks">How often to contact the LimeLM servers for validation. 90 days recommended.</param>
        /// <param name="graceDaysOnInetErr">If the call fails because of an internet error, how long, in days, should the grace period last (before returning deactivating and returning TA_FAIL).
        /// 
        /// 14 days is recommended.</param>
        /// <param name="skipOffline">If the user activated using offline activation 
        /// (ActivateRequestToFile(), ActivateFromFile() ), then with this
        /// option IsGenuineEx() will still try to validate with the LimeLM
        /// servers, however instead of returning <see cref="IsGenuineResult.InternetError"/> (when within the
        /// grace period) or <see cref="IsGenuineResult.NotGenuine"/> (when past the grace period) it will
        /// instead only return <see cref="IsGenuineResult.Genuine"/> (if IsActivated()).
        /// 
        /// If the user activated using online activation then this option
        /// is ignored.</param>
        /// <param name="offlineShowInetErr">If the user activated using offline activation, and you're
        /// using this option in tandem with skipOffline, then IsGenuineEx()
        /// will return <see cref="IsGenuineResult.InternetError"/> on internet failure instead of <see cref="IsGenuineResult.Genuine"/>.
        ///
        /// If the user activated using online activation then this flag
        /// is ignored.</param>
        /// <returns>IsGenuineResult</returns>
        /// <exception cref="GUIDMismatchException">The version GUID doesn't match that of the product details file. Make sure you set the GUID using TurboActivate.VersionGUID.</exception>
        /// <exception cref="ProductDetailsException">The product details file "TurboActivate.dat" failed to load. It's either missing or corrupt.</exception>
        /// <exception cref="COMException">CoInitializeEx failed.</exception>
        /// <exception cref="DateTimeException">Failed because your system date and time settings are incorrect. Fix your date and time settings, restart your computer, and try to activate again.</exception>
        public static IsGenuineResult IsGenuine(uint daysBetweenChecks, uint graceDaysOnInetErr, bool skipOffline = false, bool offlineShowInetErr = false)
        {
            Native.GENUINE_OPTIONS opts = new Native.GENUINE_OPTIONS { nDaysBetweenChecks = daysBetweenChecks, nGraceDaysOnInetErr = graceDaysOnInetErr, flags = 0 };
            opts.nLength = (uint)Marshal.SizeOf(opts);

            if (skipOffline)
            {
                opts.flags |= Native.GenuineFlags.TA_SKIP_OFFLINE;

                if (offlineShowInetErr)
                    opts.flags |= Native.GenuineFlags.TA_OFFLINE_SHOW_INET_ERR;
            }

#if TA_BOTH_DLL
            switch (IntPtr.Size == 8 ? Native64.IsGenuineEx(VersionGUID, ref opts) : Native.IsGenuineEx(VersionGUID, ref opts))
#else
            switch (Native.IsGenuineEx(VersionGUID, ref opts))
#endif
            {
                case TA_E_INET:
                case TA_E_INET_DELAYED:
                    return IsGenuineResult.InternetError;
                case TA_E_GUID:
                    throw new GUIDMismatchException();
                case TA_E_PDETS:
                    throw new ProductDetailsException();
                case TA_E_COM:
                    throw new COMException();
                case TA_E_EXPIRED:
                    throw new DateTimeException(false);
                case TA_E_IN_VM:
                    return IsGenuineResult.NotGenuineInVM;
                case TA_E_INVALID_ARGS:
                    throw new InvalidArgsException();
                case TA_E_FEATURES_CHANGED:
                    return IsGenuineResult.GenuineFeaturesChanged;
                case TA_OK: // is activated and/or Genuine
                    return IsGenuineResult.Genuine;
            }

            // not genuine (TA_FAIL, TA_E_REVOKED, TA_E_ACTIVATE)
            return IsGenuineResult.NotGenuine;
        }

        /// <summary>Checks if the product key installed for this product is valid. This does NOT check if the product key is activated or genuine. Use <see cref="IsActivated()"/> and <see cref="IsGenuine(ref bool)"/> instead.</summary>
        /// <returns>True if the product key is valid.</returns>
        /// <exception cref="GUIDMismatchException">The version GUID doesn't match that of the product details file. Make sure you set the GUID using TurboActivate.VersionGUID.</exception>
        /// <exception cref="ProductDetailsException">The product details file "TurboActivate.dat" failed to load. It's either missing or corrupt.</exception>
        public static bool IsProductKeyValid()
        {
#if TA_BOTH_DLL
            switch (IntPtr.Size == 8 ? Native64.IsProductKeyValid(VersionGUID) : Native.IsProductKeyValid(VersionGUID))
#else
            switch (Native.IsProductKeyValid(VersionGUID))
#endif
            {
                case TA_E_GUID:
                    throw new GUIDMismatchException();
                case TA_E_PDETS:
                    throw new ProductDetailsException();
                case TA_OK: // is valid
                    return true;
            }

            // not valid
            return false;
        }

        /// <summary>Sets the custom proxy to be used by functions that connect to the internet.</summary>
        /// <param name="proxy">The proxy to use. Proxy must be in the form "http://username:password@host:port/".</param>
        /// <exception cref="TurboActivateException">Failed to set the custom proxy.</exception>
        public static void SetCustomProxy(string proxy)
        {
#if TA_BOTH_DLL
            if ((IntPtr.Size == 8 ? Native64.SetCustomProxy(proxy) : Native.SetCustomProxy(proxy)) != 0)
#else
            if (Native.SetCustomProxy(proxy) != TA_OK)
#endif
                throw new TurboActivateException("Failed to set the custom proxy.");
        }

        /// <summary>Get the number of trial days remaining. You must call <see cref="UseTrial()"/> at least once in the past before calling this function.</summary>
        /// <returns>The number of days remaining. 0 days if the trial has expired. (E.g. 1 day means *at most* 1 day. That is it could be 30 seconds.)</returns>
        /// <exception cref="GUIDMismatchException">The version GUID doesn't match that of the product details file. Make sure you set the GUID using TurboActivate.VersionGUID.</exception>
        /// <exception cref="ProductDetailsException">The product details file "TurboActivate.dat" failed to load. It's either missing or corrupt.</exception>
        /// <exception cref="TurboActivateException">Failed to get the trial data.</exception>
        public static int TrialDaysRemaining()
        {
            uint daysRemain = 0;

#if TA_BOTH_DLL
            switch (IntPtr.Size == 8 ? Native64.TrialDaysRemaining(VersionGUID, ref daysRemain) : Native.TrialDaysRemaining(VersionGUID, ref daysRemain))
#else
            switch (Native.TrialDaysRemaining(VersionGUID, ref daysRemain))
#endif
            {
                case TA_E_GUID:
                    throw new GUIDMismatchException();
                case TA_E_PDETS:
                    throw new ProductDetailsException();
                case TA_OK: // successful
                    break;
                default:
                    throw new TurboActivateException("Failed to get the trial data.");
            }

            return (int)daysRemain;
        }

        /// <summary>Begins the trial the first time it's called. Calling it again will validate the trial data hasn't been tampered with.</summary>
        /// <param name="flags">Whether to create the trial either user-wide or system-wide and whether to allow trials in virtual machines. Valid flags are <see cref="TA_Flags.TA_SYSTEM"/>, <see cref="TA_Flags.TA_USER"/>, and <see cref="TA_Flags.TA_DISALLOW_VM"/>.</param>
        /// <exception cref="ProductDetailsException">The product details file "TurboActivate.dat" failed to load. It's either missing or corrupt.</exception>
        /// <exception cref="PermissionException">Insufficient system permission. Either start your process as an admin / elevated user or call the function again with the TA_USER flag.</exception>
        /// <exception cref="InvalidFlagsException">The flags you passed to the function were invalid (or missing). Flags like "TA_SYSTEM" and "TA_USER" are mutually exclusive -- you can only use one or the other.</exception>
        /// <exception cref="VirtualMachineException">The function failed because this instance of your program is running inside a virtual machine / hypervisor and you've prevented the function from running inside a VM.</exception>
        /// <exception cref="TurboActivateException">Failed to save the trial data.</exception>
        public static void UseTrial(TA_Flags flags = TA_Flags.TA_USER)
        {
#if TA_BOTH_DLL
            switch (IntPtr.Size == 8 ? Native64.UseTrial(flags) : Native.UseTrial(flags))
#else
            switch (Native.UseTrial(flags))
#endif
            {
                case TA_E_PDETS:
                    throw new ProductDetailsException();
                case TA_OK: // successful
                    return;
                case TA_E_PERMISSION:
                    throw new PermissionException();
                case TA_E_INVALID_FLAGS:
                    throw new InvalidFlagsException();
                case TA_E_IN_VM:
                    throw new VirtualMachineException();
                default:
                    throw new TurboActivateException("Failed to save the trial data.");
            }
        }

        /// <summary>Extends the trial using a trial extension created in LimeLM.</summary>
        /// <param name="trialExtension">The trial extension generated from LimeLM.</param>
        /// <exception cref="InternetException">Connection to the server failed.</exception>
        /// <exception cref="ProductDetailsException">The product details file "TurboActivate.dat" failed to load. It's either missing or corrupt.</exception>
        /// <exception cref="TrialExtUsedException">The trial extension has already been used.</exception>
        /// <exception cref="TrialExtExpiredException">The trial extension has expired.</exception>
        /// <exception cref="TurboActivateException">Failed to extend trial.</exception>
        public static void ExtendTrial(string trialExtension)
        {
#if TA_BOTH_DLL
            switch (IntPtr.Size == 8 ? Native64.ExtendTrial(trialExtension) : Native.ExtendTrial(trialExtension))
#else
            switch (Native.ExtendTrial(trialExtension))
#endif
            {
                case TA_E_INET:
                    throw new InternetException();
                case TA_E_PDETS:
                    throw new ProductDetailsException();
                case TA_E_TRIAL_EUSED:
                    throw new TrialExtUsedException();
                case TA_E_TRIAL_EEXP:
                    throw new TrialExtExpiredException();
                case TA_OK: // successful
                    return;
                default:
                    throw new TurboActivateException("Failed to extend trial.");
            }
        }

        /// <summary>Loads the "TurboActivate.dat" file from a path rather than loading it from the same dir as TurboActivate.dll on Windows or the app that uses libTurboActivate.dylib / libTurboActivate.so on Mac / Linux.</summary>
        /// <param name="filename">The full path to the TurboActivate.dat file.</param>
        /// <exception cref="ProductDetailsException">The product details file "TurboActivate.dat" failed to load. It's either missing or corrupt.</exception>
        /// <exception cref="TurboActivateException">The TurboActivate.dat file has already been loaded. You must call this function only once and before any other TurboActivate function.</exception>
        public static void PDetsFromPath(string filename)
        {
#if TA_BOTH_DLL
            switch (IntPtr.Size == 8 ? Native64.PDetsFromPath(filename) : Native.PDetsFromPath(filename))
#else
            switch (Native.PDetsFromPath(filename))
#endif
            {
                case TA_OK: // successful
                    return;
                case TA_E_PDETS:
                    throw new ProductDetailsException();
                default:
                    throw new TurboActivateException("The TurboActivate.dat file has already been loaded. You must call this function only once and before any other TurboActivate function.");
            }
        }

        /// <summary>Gets the "current product" previously set by <see cref="SetCurrentProduct(string)"/>.</summary>
        /// <returns>string version GUID.</returns>
        /// <exception cref="TurboActivateException">Failed to get the current product. Make sure you've loaded the product details file using <see cref="PDetsFromPath(string)"/>.</exception>
        public static string GetCurrentProduct()
        {
#if TA_BOTH_DLL
            int length = IntPtr.Size == 8 ? Native64.GetCurrentProduct(null, 0) : Native.GetCurrentProduct(null, 0);
#else
            int length = Native.GetCurrentProduct(null, 0);
#endif

            StringBuilder sb = new StringBuilder(length);

#if TA_BOTH_DLL
            switch (IntPtr.Size == 8 ? Native64.GetCurrentProduct(sb, length) : Native.GetCurrentProduct(sb, length))
#else
            switch (Native.GetCurrentProduct(sb, length))
#endif
            {
                case TA_OK: // success
                    return sb.ToString();
                default:
                    throw new TurboActivateException("Failed to get the current product. Make sure you've loaded the product details file using PDetsFromPath().");
            }
        }

        /// <summary>This functions allows you to use licensing for multiple products within the same running process. First load all the TurboActivate.dat files for all your products using the <see cref="PDetsFromPath(string)"/> function. Then, to use any of the licensing functions for a product you need to for any particular product, you must first call <see cref="SetCurrentProduct(string)"/> to "switch" to the product.</summary>
        /// <param name="vGuid">The version GUID of the product to switch to.</param>
        /// <exception cref="TurboActivateException">Failed to set the current product. Make sure you've loaded the product details file using PDetsFromPath().</exception>
        public static void SetCurrentProduct(string vGuid)
        {
#if TA_BOTH_DLL
            switch (IntPtr.Size == 8 ? Native64.SetCurrentProduct(vGuid) : Native.SetCurrentProduct(vGuid))
#else
            switch (Native.SetCurrentProduct(vGuid))
#endif
            {
                case TA_OK: // successful
                    VersionGUID = vGuid;
                    return;
                default:
                    throw new TurboActivateException("Failed to set the current product. Make sure you've loaded the product details file using PDetsFromPath().");
            }
        }

        /// <summary>This function allows you to set a custom folder to store the activation
        ///data files. For normal use we do not recommend you use this function.
        ///
        ///Only use this function if you absolutely must store data into a separate
        ///folder. For example if your application runs on a USB drive and can't write
        ///any files to the main disk, then you can use this function to save the activation
        ///data files to a directory on the USB disk.
        ///
        ///If you are using this function (which we only recommend for very special use-cases)
        ///then you must call this function on every start of your program at the very top of
        ///your app before any other functions are called.
        ///
        ///The directory you pass in must already exist. And the process using TurboActivate
        ///must have permission to create, write, and delete files in that directory.</summary>
        /// <param name="directory">The full directory to store the activation files.</param>
        /// <exception cref="ProductDetailsException">The product details file "TurboActivate.dat" failed to load. It's either missing or corrupt.</exception>
        /// <exception cref="TurboActivateException">The directory must exist and you must have access to it.</exception>
        public static void SetCustomActDataPath(string directory)
        {
#if TA_BOTH_DLL
            switch (IntPtr.Size == 8 ? Native64.SetCustomActDataPath(directory) : Native.SetCustomActDataPath(directory))
#else
            switch (Native.SetCustomActDataPath(directory))
#endif
            {
                case TA_OK: // successful
                    return;
                case TA_E_PDETS:
                    throw new ProductDetailsException();
                default:
                    throw new TurboActivateException("The directory must exist and you must have access to it.");
            }
        }
    }

    public class COMException : TurboActivateException
    {
        public COMException()
            : base("CoInitializeEx failed. Re-enable Windows Management Instrumentation (WMI) service. Contact your system admin for more information.")
        {
        }
    }

    public class PkeyRevokedException : TurboActivateException
    {
        public PkeyRevokedException()
            : base("The product key has been revoked.")
        {
        }
    }

    public class PkeyMaxUsedException : TurboActivateException
    {
        public PkeyMaxUsedException()
            : base("The product key has already been activated with the maximum number of computers.")
        {
        }
    }

    public class InternetException : TurboActivateException
    {
        public InternetException()
            : base("Connection to the server failed.")
        {
        }
    }

    public class InvalidProductKeyException : TurboActivateException
    {
        public InvalidProductKeyException()
            : base("The product key is invalid or there's no product key.")
        {
        }
    }

    public class NotActivatedException : TurboActivateException
    {
        public NotActivatedException()
            : base("The product needs to be activated.")
        {
        }
    }

    public class GUIDMismatchException : TurboActivateException
    {
        public GUIDMismatchException()
#if DEBUG
            : base("The version GUID \"" + TurboActivate.VersionGUID + "\" doesn't match that of the product details file. Make sure you set the GUID using TurboActivate.VersionGUID.")
#else
            : base("The product details file \"TurboActivate.dat\" is corrupt.")
#endif
        {
        }
    }

    public class ProductDetailsException : TurboActivateException
    {
        public ProductDetailsException()
            : base("The product details file \"TurboActivate.dat\" failed to load. It's either missing or corrupt.")
        {
        }
    }

    public class TrialExtUsedException : TurboActivateException
    {
        public TrialExtUsedException()
            : base("The trial extension has already been used.")
        {
        }
    }

    public class TrialExtExpiredException : TurboActivateException
    {
        public TrialExtExpiredException()
            : base("The trial extension has expired.")
        {
        }
    }

    public class DateTimeException : TurboActivateException
    {
        public DateTimeException(bool either)
            : base(either ? "Either the activation response file has expired or your date and time settings are incorrect. Fix your date and time settings, restart your computer, and try to activate again."
                    : "Failed because your system date and time settings are incorrect. Fix your date and time settings, restart your computer, and try to activate again.")
        {
        }
    }

    public class PermissionException : TurboActivateException
    {
        public PermissionException()
            : base("Insufficient system permission. Either start your process as an admin / elevated user or call the function again with the TA_USER flag.")
        {
        }
    }

    public class InvalidFlagsException : TurboActivateException
    {
        public InvalidFlagsException()
            : base("The flags you passed to the function were invalid (or missing). Flags like \"TA_SYSTEM\" and \"TA_USER\" are mutually exclusive -- you can only use one or the other.")
        {
        }
    }

    public class VirtualMachineException : TurboActivateException
    {
        public VirtualMachineException()
            : base("The function failed because this instance of your program is running inside a virtual machine / hypervisor and you've prevented the function from running inside a VM.")
        {
        }
    }

    public class ExtraDataTooLongException : TurboActivateException
    {
        public ExtraDataTooLongException()
            : base("The \"extra data\" was too long. You're limited to 255 UTF-8 characters. Or, on Windows, a Unicode string that will convert into 255 UTF-8 characters or less.")
        {
        }
    }

    public class InvalidArgsException : TurboActivateException
    {
        public InvalidArgsException()
            : base("The arguments passed to the function are invalid. Double check your logic.")
        {
        }
    }

    public class TurboFloatKeyException : TurboActivateException
    {
        public TurboFloatKeyException()
            : base("The product key used is for TurboFloat, not TurboActivate.")
        {
        }
    }

    public class NoMoreDeactivationsException : TurboActivateException
    {
        public NoMoreDeactivationsException()
            : base("No more deactivations are allowed for the product key. This product is still activated on this computer.")
        {
        }
    }

    public class TurboActivateException : Exception
    {
        public TurboActivateException(string message) : base(message) { }
    }

    public enum IsGenuineResult
    {
        /// <summary>Is activated and genuine.</summary>
        Genuine = 0,

        /// <summary>Is activated and genuine and the features changed.</summary>
        GenuineFeaturesChanged = 1,

        /// <summary>Not genuine (note: use this in tandem with NotGenuineInVM).</summary>
        NotGenuine = 2,

        /// <summary>Not genuine because you're in a Virtual Machine.</summary>
        NotGenuineInVM = 3,

        /// <summary>Treat this error as a warning. That is, tell the user that the activation couldn't be validated with the servers and that they can manually recheck with the servers immediately.</summary>
        InternetError = 4
    }   
}