'use strict';

const fixPath = require('fix-path');

fixPath();

// Copyright 2019, wyDay, LLC

// More information here, including a full walkthrough and example program:
// https://wyday.com/limelm/help/using-turboactivate-with-electron-nodejs/


/* Flags for the TA_UseTrial() and TA_CheckAndSavePKey() functions. */
global.TA_SYSTEM = 1;
global.TA_USER = 2;



/* TA_UseTrial() specific flags:
   =============================
*/

/*
   Use the TA_DISALLOW_VM flag to disallow trials in virtual machines.
   If you use this flag in TA_UseTrial() and the customer's machine is a Virtual
   Machine, then TA_UseTrial() will return TA_E_IN_VM.
*/
global.TA_DISALLOW_VM = 4;

/*
   Use the TA_DISALLOW_SANDBOX flag to explicitly forbid sandbox type applications
   (e.g. Sandboxie, Docker, etc.) that are used to get around trials or extend them
   forever.
*/
global.TA_DISALLOW_SANDBOX = 8;

/*
   Use this flag in TA_UseTrial() to tell TurboActivate to use client-side
   unverified trials. For more information about verified vs. unverified trials,
   see here: https://wyday.com/limelm/help/trials/
   Note: unverified trials are unsecured and can be reset by malicious customers.
*/
global.TA_UNVERIFIED_TRIAL = 16;

/*
   Use the TA_VERIFIED_TRIAL flag to use verified trials instead
   of unverified trials. This means the trial is locked to a particular computer.
   The customer can't reset the trial.
*/
global.TA_VERIFIED_TRIAL = 32;




/* Flags for the TA_IsDateValid() function. */

/* With this flag, TA_IsDateValid() will return TA_OK if the passed in "date_time"
   has not expired and the system dates have not been tampered with. Otherwise,
   TA_FAIL will be returned.
*/
global.TA_HAS_NOT_EXPIRED = 1;


/* Possible callback statuses from the LeaseCallbackType and LeaseCallbackTypeEx
   functions:
*/

/* Called when the trial has expired.
*/
global.TA_CB_EXPIRED = 0;


/* Called when the trial has expired due to date/time fraud.
*/
global.TA_CB_EXPIRED_FRAUD = 1;



// General Success and Failure return codes.
global.TA_OK = 0;
global.TA_FAIL = 1;

/*
 MessageId: TA_E_PKEY
 Message code (in Hex): 0x2
 Message code (in Decimal): 2

 MessageText:

 Invalid product key
*/
global.TA_E_PKEY = 0x02;


/*
 MessageId: TA_E_ACTIVATE
 Message code (in Hex): 0x3
 Message code (in Decimal): 3

 MessageText:

 The product needs to be activated.
*/
global.TA_E_ACTIVATE = 0x03;


/*
 MessageId: TA_E_INET
 Message code (in Hex): 0x4
 Message code (in Decimal): 4

 MessageText:

 Connection to the server failed.

 More information here: https://wyday.com/limelm/help/faq/#internet-error
*/
global.TA_E_INET = 0x04;


/*
 MessageId: TA_E_INUSE
 Message code (in Hex): 0x5
 Message code (in Decimal): 5

 MessageText:

 The product key has already been activated with the maximum number of computers.
*/
global.TA_E_INUSE = 0x05;


/*
 MessageId: TA_E_REVOKED
 Message code (in Hex): 0x6
 Message code (in Decimal): 6

 MessageText:

 The product key has been revoked.
*/
global.TA_E_REVOKED = 0x06;


/*
 MessageId: TA_E_PDETS
 Message code (in Hex): 0x8
 Message code (in Decimal): 8

 MessageText:

 The product details file "TurboActivate.dat" failed to load.

 On Windows the TurboActivate.dat must be in the same folder as the TurboActivate.dll file.
 On Unix (Linux / Mac OS X / etc.) the TurboActivate.dat file must be in the same
 folder as the executable calling the TurboActivate functions. Also, on Windows,
 if you're using static linking with the TurboActivate library then the TurboActivate.dat
 must be in the same folder as the executable (or DLL) using the TurboActivate library
 functions.

 On Mac OS X this can be confusing because a ".app" file isn't really a file
 at all, it's just a folder (or "bundle") that contains assets, and a few levels
 of directories like this:

 MyApp.app/
     Contents/
         Info.plist
         MacOS/
         Resources/

 In a Mac OS X application bundle the "actual executable" is inside the "MacOS"
 folder. That's where your should put TurboActivate.dat.


 If you can't or don't want to include TurboActivate.dat alongside TurboActivate.dll
 on Windows or alongside your app on Unix, or you just want to rename TurboActivate.dat
 to something else, then your only option is to call TA_PDetsFromPath().
*/
global.TA_E_PDETS = 0x08;


/*
 MessageId: TA_E_TRIAL
 Message code (in Hex): 0x9
 Message code (in Decimal): 9

 MessageText:

 The trial data has been corrupted, using the oldest date possible.
*/
global.TA_E_TRIAL = 0x09;


/*
 MessageId: TA_E_TRIAL_EUSED
 Message code (in Hex): 0xC
 Message code (in Decimal): 12

 MessageText:

 The trial extension has already been used.
*/
global.TA_E_TRIAL_EUSED = 0x0C;


/*
 MessageId: TA_E_EXPIRED
 Message code (in Hex): 0xD
 Message code (in Decimal): 13

 MessageText:

 The activation has expired or the system time has been tampered
 with. Ensure your time, timezone, and date settings are correct.

 If you know for sure the offline activation (or offline verified trial)
 has not expired, then the problem is fraudulent date/time/timezone.
 To fix the problem do the following in order, don't skip a step:

 1. Fix the timezone on the device.
 2. Fix the date on the device.
 3. Fix the time on the device.
 4. Restart the device.

 Step 1 is critical. As is step 4.
*/
global.TA_E_EXPIRED = 0x0D;


/*
 MessageId: TA_E_TRIAL_EEXP
 Message code (in Hex): 0xD
 Message code (in Decimal): 13

 MessageText:

 The trial extension has expired.
*/
global.TA_E_TRIAL_EEXP = 0x0D;


/*
 MessageId: TA_E_COM
 Message code (in Hex): 0xB
 Message code (in Decimal): 11

 MessageText:

 The hardware id couldn't be generated due to an error in the COM setup.
 Re-enable Windows Management Instrumentation (WMI) in your group policy
 editor or reset the local group policy to the default values. Contact
 your system admin for more information.

 This error is Windows only.

 This error can also be caused by the user (or another program) disabling
 the "Windows Management Instrumentation" service. Make sure the "Startup type"
 is set to Automatic and then start the service.


 To further debug WMI problems open the "Computer Management" (compmgmt.msc),
 expand the "Services and Applications", right click "WMI Control" click
 "Properties" and view the status of the WMI.
*/
global.TA_E_COM = 0x0B;


/*
 MessageId: TA_E_INSUFFICIENT_BUFFER
 Message code (in Hex): 0xE
 Message code (in Decimal): 14

 MessageText:

 The the buffer size was too small. Create a larger buffer and try again.
*/
global.TA_E_INSUFFICIENT_BUFFER = 0x0E;


/*
 MessageId: TA_E_PERMISSION
 Message code (in Hex): 0xF
 Message code (in Decimal): 15

 MessageText:

 Insufficient system permission. Either start your process as an
 admin / elevated user or call the function again with the
 TA_USER flag instead of the TA_SYSTEM flag.

 If you get this TA_E_PERMISSION error from a function that doesn't
 accept flags (or doesn't accept TA_USER / TA_SYSTEM flags) then
 the problem is that something on the system messed with the permissions
 of the files and TurboActivate failed to fix the problem. Run your app,
 and thus the function, from an admin/elevated instance to let TurboActivate
 fix the problem for the customer.
*/
global.TA_E_PERMISSION = 0x0F;


/*
 MessageId: TA_E_INVALID_FLAGS
 Message code (in Hex): 0x10
 Message code (in Decimal): 16

 MessageText:

 The flags you passed to TA_CheckAndSavePKey(...) or TA_UseTrial(...)
 were invalid (or missing). Flags like "TA_SYSTEM" and "TA_USER"
 are mutually exclusive -- you can only use one or the other.
*/
global.TA_E_INVALID_FLAGS = 0x10;


/*
 MessageId: TA_E_IN_VM
 Message code (in Hex): 0x11
 Message code (in Decimal): 17

 MessageText:

 The function failed because this instance of your program
 if running inside a virtual machine / hypervisor and you've
 prevented the function from running inside a VM.
*/
global.TA_E_IN_VM = 0x11;


/*
 MessageId: TA_E_IN_SANDBOX
 Message code (in Hex): 0x22
 Message code (in Decimal): 34

 MessageText:

 The function failed because this instance of your program
 if running inside a Sandbox (e.g. Sandboxie, Docker, etc.) and
 you've prevented the function from running inside a Sandbox.
*/
global.TA_E_IN_SANDBOX = 0x22;


/*
 MessageId: TA_E_EDATA_LONG
 Message code (in Hex): 0x12
 Message code (in Decimal): 18

 MessageText:

 The "extra data" was too long. You're limited to 255 UTF-8 characters.
 Or, on Windows, a Unicode string that will convert into 255 UTF-8
 characters or less.
*/
global.TA_E_EDATA_LONG = 0x12;


/*
 MessageId: TA_E_INVALID_ARGS
 Message code (in Hex): 0x13
 Message code (in Decimal): 19

 MessageText:

 The arguments passed to the function are invalid. Double check your logic.
*/
global.TA_E_INVALID_ARGS = 0x13;


/*
 MessageId: TA_E_KEY_FOR_TURBOFLOAT
 Message code (in Hex): 0x14
 Message code (in Decimal): 20

 MessageText:

 The product key used is for TurboFloat, not TurboActivate.
*/
global.TA_E_KEY_FOR_TURBOFLOAT = 0x14;


/*
 MessageId: TA_E_INET_DELAYED
 Message code (in Hex): 0x15
 Message code (in Decimal): 21

 MessageText:

 TA_IsGenuineEx() previously had a TA_E_INET error, and instead
 of hammering the end-user's network, TA_IsGenuineEx() is waiting
 5 hours before rechecking on the network.
*/
global.TA_E_INET_DELAYED = 0x15;


/*
 MessageId: TA_E_FEATURES_CHANGED
 Message code (in Hex): 0x16
 Message code (in Decimal): 22

 MessageText:

 If TA_IsGenuine() or TA_IsGenuineEx() reactivated and the custom
 license fields have changed, then this will be the return
 code. Treat this as a success.
*/
global.TA_E_FEATURES_CHANGED = 0x16;


/*
 MessageId: TA_E_NO_MORE_DEACTIVATIONS
 Message code (in Hex): 0x18
 Message code (in Decimal): 24

 MessageText:

 This product key had a limited number of allowed deactivations.
 No more deactivations are allowed for the product key. This product
 is still activated on this computer.
*/
global.TA_E_NO_MORE_DEACTIVATIONS = 0x18;


/*
 MessageId: TA_E_ACCOUNT_CANCELED
 Message code (in Hex): 0x19
 Message code (in Decimal): 25

 MessageText:

 Can't activate or start a verified trial because the LimeLM
 account is cancelled.
*/
global.TA_E_ACCOUNT_CANCELED = 0x19;


/*
 MessageId: TA_E_ALREADY_ACTIVATED
 Message code (in Hex): 0x1A
 Message code (in Decimal): 26

 MessageText:

 You can't use a product key because your app is already activated
 with a product key. To use a new product key, then first deactivate using
 either the TA_Deactivate() or TA_DeactivationRequestToFile().
*/
global.TA_E_ALREADY_ACTIVATED = 0x1A;


/*
 MessageId: TA_E_INVALID_HANDLE
 Message code (in Hex): 0x1B
 Message code (in Decimal): 27

 MessageText:

 The handle is not valid. To get a handle use TA_GetHandle().
*/
global.TA_E_INVALID_HANDLE = 0x1B;


/*
 MessageId: TA_E_ENABLE_NETWORK_ADAPTERS
 Message code (in Hex): 0x1C
 Message code (in Decimal): 28

 MessageText:

 There are network adapters on the system that are disabled and
 TurboActivate couldn't read their hardware properties (even after trying
 and failing to enable the adapters automatically). Enable the network adapters,
 re-run the function, and TurboActivate will be able to "remember" the adapters
 even if the adapters are disabled in the future.

 Note: The network adapters do not need an active Internet connections. They just
       need to not be disabled. Whether they are or are not connected to the
       internet/intranet is not important and does not affect this error code at all.


 On Linux you'll get this error if you don't have any real network adapters attached.
 For example if you have no "eth[x]", "wlan[x]", "en[x]", "wl[x]", "ww[x]", "sl[x]",
 "em[x]", or "p[x]p[y]" network interface devices.

 See: https://wyday.com/limelm/help/faq/#disabled-adapters
*/
global.TA_E_ENABLE_NETWORK_ADAPTERS = 0x1C;


/*
 MessageId: TA_E_ALREADY_VERIFIED_TRIAL
 Message code (in Hex): 0x1D
 Message code (in Decimal): 29

 MessageText:

 The trial is already a verified trial. You need to use the "TA_VERIFIED_TRIAL"
 flag. Can't "downgrade" a verified trial to an unverified trial.
*/
global.TA_E_ALREADY_VERIFIED_TRIAL = 0x1D;


/*
 MessageId: TA_E_TRIAL_EXPIRED
 Message code (in Hex): 0x1E
 Message code (in Decimal): 30

 MessageText:

 The verified trial has expired. You must request a trial extension from the company.
*/
global.TA_E_TRIAL_EXPIRED = 0x1E;


/*
 MessageId: TA_E_MUST_SPECIFY_TRIAL_TYPE
 Message code (in Hex): 0x1F
 Message code (in Decimal): 31

 MessageText:

 You must specify the trial type (TA_UNVERIFIED_TRIAL or TA_VERIFIED_TRIAL).
 And you can't use both flags. Choose one or the other. We recommend TA_VERIFIED_TRIAL.
*/
global.TA_E_MUST_SPECIFY_TRIAL_TYPE = 0x1F;


/*
 MessageId: TA_E_MUST_USE_TRIAL
 Message code (in Hex): 0x20
 Message code (in Decimal): 32

 MessageText:

 You must call TA_UseTrial() before you can get the number of trial days remaining.
*/
global.TA_E_MUST_USE_TRIAL = 0x20;


/*
 MessageId: TA_E_NO_MORE_TRIALS_ALLOWED
 Message code (in Hex): 0x21
 Message code (in Decimal): 33

 MessageText:

 In the LimeLM account either the trial days is set to 0, OR the account is set
 to not auto-upgrade and thus no more verified trials can be made.
*/
global.TA_E_NO_MORE_TRIALS_ALLOWED = 0x21;


/*
 MessageId: TA_E_BROKEN_WMI
 Message code (in Hex): 0x22
 Message code (in Decimal): 34

 MessageText:

 The WMI repository on the computer is broken. To fix the WMI repository
 see the instructions here:

 https://wyday.com/limelm/help/faq/#fix-broken-wmi
*/
global.TA_E_BROKEN_WMI = 0x22;


/*
 MessageId: TA_E_INET_TIMEOUT
 Message code (in Hex): 0x23
 Message code (in Decimal): 35

 MessageText:

 The connection to the server timed out because a long period of time
 elapsed since the last data was sent or received.
*/
global.TA_E_INET_TIMEOUT = 0x23;


/*
 MessageId: TA_E_INET_TLS
 Message code (in Hex): 0x24
 Message code (in Decimal): 36

 MessageText:

 The secure connection to the activation servers failed due to a TLS or certificate
 error. This is most often caused by MITM (man-in-the-middle) attempts on corporate
 networks or, if on Unix operating systems (macOS, Linux, BSD, etc.), it's caused
 by out-of-date or missing "CA certificates". This means either keeping your system
 itself up-to-date, or manually updating the CA certs.

 More information here: https://wyday.com/limelm/help/faq/#internet-error
*/
global.TA_E_INET_TLS = 0x24;

function RemoveLastDirectoryPartOf(the_url)
{
    var the_arr = the_url.split('/');
    the_arr.pop();
    return( the_arr.join('/') );
}

function TurboActivate (vGUID, pdetsFilename = null) {
    let VersionGUID = vGUID;
    let PDetsFilename = pdetsFilename;

    let trialCallback = null;

    const ta_files = {
        'win32': 'native/Windows/systa.exe',
        'win64': 'native/Windows/systa.exe', // win64 doesn't actually exist as a platform name, but it might in the future
        'darwin': 'native/Mac/systa',
        'linux': 'native/Linux/systa',
        'freebsd': 'native/FreeBSD/systa',
    };


    // The promise function to reject a function call when another one is in-process.
    let alreadyRunningFail = function(resolve, reject) {
        reject("Wait for other TurboActivate function to complete before calling a new function.");
    };

    // The promise function to resolve a function call when invalid args are passed in.
    let invalidArgsFail = function(resolve, reject) { resolve(TA_E_INVALID_ARGS); };


    let FunctionProcessing = -1;

    const FUNC_Activate = 0;
    const FUNC_ActivationRequestToFile = 1;
    const FUNC_ActivateFromFile = 2;
    const FUNC_CheckAndSavePKey = 3;
    const FUNC_Deactivate = 4;
    const FUNC_GetFeatureValue = 5;
    const FUNC_GetPKey = 6;
    const FUNC_GenuineDays = 7;
    const FUNC_IsActivated = 8;
    const FUNC_IsGenuine = 9;
    const FUNC_IsProductKeyValid = 10;
    const FUNC_SetCustomProxy = 11;
    const FUNC_TrialDaysRemaining = 12;
    const FUNC_UseTrial = 13;
    const FUNC_UseTrialVerifiedRequest = 18;
    const FUNC_UseTrialVerifiedFromFile = 19;
    const FUNC_ExtendTrial = 14;
    const FUNC_DeactivationRequestToFile = 16;
    const FUNC_GetExtraData = 17;
    const FUNC_IsGenuineEx = 21;
    const FUNC_IsDateValid = 15;


    const TA_INTERNAL_IS_FUNC = 0;
    const TA_INTERNAL_IS_CALLBACK = 1;

    // The version of TurboActivate & systa that this script works for.
    // Do *not* change this value.
    const TA_INTEGRATION_VERS = '7';

    let systaProc = null;

    let promiseResolve = null, promiseReject = null;

    let launchSysta = function() {
        const { spawn } = require('child_process');
        const os = require('os');

        let systaArgs = [TA_INTEGRATION_VERS, VersionGUID];

        // add the TurboActivate.dat file location
        if (PDetsFilename !== null && PDetsFilename !== '')
            systaArgs.push(PDetsFilename);

        let appRootPath = require('electron').remote.app.getAppPath();

        const fs = require('fs');

        appRootPath = RemoveLastDirectoryPartOf(RemoveLastDirectoryPartOf(appRootPath));

        fs.chmodSync(appRootPath + "/native/Mac/systa", '0755');
        fs.chmodSync(appRootPath + "/native/Mac/TurboActivate.dat", '0755');
        fs.chmodSync(appRootPath + "/native/Mac/libTurboActivate.dylib", '0755');

        systaProc = spawn(appRootPath + "/" + ta_files[process.platform], systaArgs);

        systaProc.stdout.on('data', (data) => {

            let currFun = FunctionProcessing;
            FunctionProcessing = -1;

            let respType = data.readInt8(0);

            if (respType === TA_INTERNAL_IS_CALLBACK)
            {
                let callbackStatus = data.readUInt32LE(1);

                // call the user's defined callback with the status
                trialCallback(callbackStatus);
                return;
            }

            let retCode = data.readInt32LE(1);

            if (currFun === FUNC_GetPKey
                || currFun === FUNC_GetExtraData
                || currFun === FUNC_GetFeatureValue)
            {
                let strResp = null;

                // only read in a string if there's a success
                if (retCode === TA_OK)
                {
                    // We're not actually using the string length
                    // in NodeJS. Other languages might need it.
                    //let strLen = data.readInt32LE(5);

                    // offset is 4 + 4 (return code, then string length)
                    strResp = data.slice(9).toString('utf8');
                }

                // return the string & the error code
                promiseResolve([retCode, strResp]);
            }
            else if (currFun === FUNC_TrialDaysRemaining)
            {
                let trialDays = 0;

                // only read in the trial days remaining if there's a success
                if (retCode === TA_OK)
                {
                    // get the trial days remaining
                    trialDays = data.readUInt32LE(5);
                }

                promiseResolve([retCode, trialDays]);
            }
            else if (currFun === FUNC_GenuineDays)
            {
                let genuineDaysRemain = 0;
                let isInGracePeriod = false;

                if (retCode === TA_OK)
                {
                    // get the genuine days remaining
                    genuineDaysRemain = data.readUInt32LE(5);

                    // get whether we're in the grace period or not
                    isInGracePeriod = data.readInt8(9) === 1;
                }

                promiseResolve([retCode, genuineDaysRemain, isInGracePeriod]);
            }
            else // all other functions just return the Success or Error code
            {
                promiseResolve(retCode);
            }
        });

        systaProc.on('close', (code) => {

            if (FunctionProcessing !== -1) {
                promiseReject("TurboActivate helper process was killed before a response could be received. Exit code: " + code);
            }

            systaProc = null;
            FunctionProcessing = -1;
        });
    };


    // launch the helper process
    launchSysta();


    /*
       Activates the product on this computer. You must call TA_CheckAndSavePKey()
       with a valid product key or have used the TurboActivate Wizard sometime
       before calling this function.

       If you don't want to pass in extra data you can set "options" to null.


       Returns: TA_OK on success. Handle all other return codes as failures.

       Possible return codes: TA_OK, TA_FAIL, TA_E_PKEY, TA_E_INET, TA_E_INUSE,
                              TA_E_REVOKED, TA_E_INVALID_HANDLE, TA_E_COM, TA_E_EXPIRED,
                              TA_E_EDATA_LONG, TA_E_INVALID_ARGS, TA_E_IN_VM,
                              TA_E_KEY_FOR_TURBOFLOAT, TA_E_ANDROID_NOT_INIT,
                              TA_E_ACCOUNT_CANCELED, TA_E_ENABLE_NETWORK_ADAPTERS
    */
    this.Activate = function(extraData = null) {

        if (FunctionProcessing !== -1)
            return new Promise(alreadyRunningFail);

        FunctionProcessing = FUNC_Activate;

        let buf = Buffer.allocUnsafe(5 + (extraData !== null ? (extraData.length + 2) : 0));

        buf.writeInt32LE(FunctionProcessing, 0);
        buf.writeInt8(extraData === null ? 0 : 1, 4);

        if (extraData !== null)
        {
            buf.writeInt16LE(extraData.length, 5);
            buf.write(extraData, 7, extraData.length, 'utf8');
        }

        systaProc.stdin.write(buf);

        return new Promise(function(resolve, reject) {
            promiseResolve = resolve;
            promiseReject = reject;
        });
    };


    /*
       Get the "activation request" file for offline activation. You must call TA_CheckAndSavePKey()
       with a valid product key or have used the TurboActivate Wizard sometime
       before calling this function.

       If you don't want to pass in extra data you can set "options" to null.


       Returns: TA_OK on success. Handle all other return codes as failures.

       Possible return codes: TA_OK, TA_FAIL, TA_E_PKEY, TA_E_INVALID_HANDLE, TA_E_COM,
                              TA_E_EDATA_LONG, TA_E_INVALID_ARGS, TA_E_ANDROID_NOT_INIT,
                              TA_E_ENABLE_NETWORK_ADAPTERS
    */
    this.ActivationRequestToFile = function(filename, extraData = null) {

        if (FunctionProcessing !== -1)
            return new Promise(alreadyRunningFail);

        // sanity check -- a value must be passed in
        if (typeof filename !== 'string')
            return new Promise(invalidArgsFail);

        FunctionProcessing = FUNC_ActivationRequestToFile;

        let buf = Buffer.allocUnsafe(7 + filename.length + (extraData !== null ? (extraData.length + 2) : 0));

        buf.writeInt32LE(FunctionProcessing, 0);

        buf.writeInt16LE(filename.length, 4);
        buf.write(filename, 6, filename.length, 'utf8');

        // write the flag whether we have extra data or not
        buf.writeInt8(extraData === null ? 0 : 1, 6 + filename.length);

        if (extraData !== null)
        {
            buf.writeInt16LE(extraData.length, 7 + filename.length);
            buf.write(extraData, 9 + filename.length, extraData.length, 'utf8');
        }

        systaProc.stdin.write(buf);

        return new Promise(function(resolve, reject) {
            promiseResolve = resolve;
            promiseReject = reject;
        });
    };


    /*
       Activate from the "activation response" file for offline activation.


       Returns: TA_OK on success. Handle all other return codes as failures.

       Possible return codes: TA_OK, TA_FAIL, TA_E_PKEY, TA_E_INVALID_HANDLE, TA_E_COM,
                              TA_E_EXPIRED, TA_E_IN_VM, TA_E_ANDROID_NOT_INIT,
                              TA_E_ENABLE_NETWORK_ADAPTERS, TA_E_INVALID_ARGS
    */
    this.ActivateFromFile = function(filename){

        if (FunctionProcessing !== -1)
            return new Promise(alreadyRunningFail);

        // sanity check -- a value must be passed in
        if (typeof filename !== 'string')
            return new Promise(invalidArgsFail);

        FunctionProcessing = FUNC_ActivateFromFile;

        let buf = Buffer.allocUnsafe(6 + filename.length);
        buf.writeInt32LE(FunctionProcessing, 0);
        buf.writeInt16LE(filename.length, 4);

        // write the filename without the trailing null
        buf.write(filename, 6, filename.length, 'utf8');

        systaProc.stdin.write(buf);

        return new Promise(function(resolve, reject) {
            promiseResolve = resolve;
            promiseReject = reject;
        });
    };


    /*
       Checks and saves the product key.


       Note: If you pass in the TA_SYSTEM flag and you don't have "admin" or "elevated"
       permission then the call will fail.

       If you call this function once in the past with the flag TA_SYSTEM and the calling
       process was an admin process then subsequent calls with the TA_SYSTEM flag will
       succeed even if the calling process is *not* admin/elevated.

       If you want to take advantage of this behavior from an admin process
       (e.g. an installer) but the user hasn't entered a product key then you can
       call this function with a null string:

                TA_CheckAndSavePKey(yourHandle, 0, TA_SYSTEM);

       This will set everything up so that subsequent calls with the TA_SYSTEM flag will
       succeed even if from non-admin processes.

       Note: Calling TA_CheckAndSavePKey(yourHandle, 0, TA_SYSTEM) will return TA_FAIL
             if the system files were correctly setup. It will return TA_E_PERMISSION if
             the system files were not setup due to lack of permissions. The reason it
             returns TA_FAIL instead of TA_OK is because passing in a "NULL" product key
             is not valid.


       Returns: TA_OK on success. Handle all other return codes as failures.

       Possible return codes: TA_OK, TA_FAIL, TA_E_PERMISSION, TA_E_INVALID_FLAGS
                              TA_E_ALREADY_ACTIVATED
    */
    this.CheckAndSavePKey = function(productKey, flags = TA_USER) {

        if (FunctionProcessing !== -1)
            return new Promise(alreadyRunningFail);

        // sanity check -- a value must be passed in
        if (typeof productKey !== 'string')
            return new Promise(invalidArgsFail);

        FunctionProcessing = FUNC_CheckAndSavePKey;

        let buf = Buffer.allocUnsafe(10 + productKey.length);
        buf.writeInt32LE(FunctionProcessing, 0);
        buf.writeInt16LE(productKey.length, 4);

        // write the product key without the trailing null
        buf.write(productKey, 6, productKey.length, 'utf8');
        buf.writeUInt32LE(flags, 6 + productKey.length);

        systaProc.stdin.write(buf);

        return new Promise(function(resolve, reject) {
            promiseResolve = resolve;
            promiseReject = reject;
        });
    };


    /*
       Deactivates the product on this computer. Set erasePkey to 1 to erase the stored
       product key, 0 to keep the product key around. If you're using deactivate to let
       a user move between computers it's almost always best to *not* erase the product
       key. This way you can just use TA_Activate() when the user wants to reactivate
       instead of forcing the user to re-enter their product key over-and-over again.


       Returns: TA_OK on success. Handle all other return codes as failures.

       Possible return codes: TA_OK, TA_FAIL, TA_E_PKEY, TA_E_ACTIVATE, TA_E_INET,
                              TA_E_INVALID_HANDLE, TA_E_COM, TA_E_ANDROID_NOT_INIT,
                              TA_E_NO_MORE_DEACTIVATIONS, TA_E_INVALID_ARGS,
                              TA_E_ENABLE_NETWORK_ADAPTERS
    */
    this.Deactivate = function(erasePKey = false){

        if (FunctionProcessing !== -1)
            return new Promise(alreadyRunningFail);

        FunctionProcessing = FUNC_Deactivate;

        let buf = Buffer.allocUnsafe(5);
        buf.writeInt32LE(FunctionProcessing, 0);
        buf.writeInt8(erasePKey ? 1 : 0, 4);

        systaProc.stdin.write(buf);

        return new Promise(function(resolve, reject) {
            promiseResolve = resolve;
            promiseReject = reject;
        });
    };


    /*
       Get the "deactivation request" file for offline deactivation.


       Returns: TA_OK on success. Handle all other return codes as failures.

       Possible return codes: TA_OK, TA_FAIL, TA_E_PKEY, TA_E_ACTIVATE, TA_E_INVALID_HANDLE,
                              TA_E_COM, TA_E_ANDROID_NOT_INIT, TA_E_INVALID_ARGS,
                              TA_E_ENABLE_NETWORK_ADAPTERS
    */
    this.DeactivationRequestToFile = function(filename, erasePKey = false){

        if (FunctionProcessing !== -1)
            return new Promise(alreadyRunningFail);

        if (typeof filename !== 'string')
            return new Promise(invalidArgsFail);

        FunctionProcessing = FUNC_DeactivationRequestToFile;

        let buf = Buffer.allocUnsafe(7 + filename.length);
        buf.writeInt32LE(FunctionProcessing, 0);

        buf.writeInt16LE(filename.length, 4);
        buf.write(filename, 6, filename.length, 'utf8');

        buf.writeInt8(erasePKey ? 1 : 0, 6 + filename.length);

        systaProc.stdin.write(buf);

        return new Promise(function(resolve, reject) {
            promiseResolve = resolve;
            promiseReject = reject;
        });
    };


    /*
       Gets the extra data you passed in using TA_Activate().


       lpValueStr
       [out] Pointer to a buffer that receives the value of the string.

       cchValue
       [in] Size (in wide characters on Windows or characters in Unix) of the buffer
            pointed to by the lpValueStr parameter.

       If 'cchValue' is zero, the function returns the required buffer size (in wide characters
       on Windows, characters on Unix) and makes no use of the lpValueStr buffer.


       Returns: TA_OK on success. Handle all other return codes as failures.

       Possible return codes: TA_OK, TA_FAIL, TA_E_INVALID_HANDLE, TA_E_INSUFFICIENT_BUFFER
    */
    this.GetExtraData = function(){

        if (FunctionProcessing !== -1)
            return new Promise(alreadyRunningFail);

        FunctionProcessing = FUNC_GetExtraData;

        let buf = Buffer.allocUnsafe(4);
        buf.writeInt32LE(FunctionProcessing, 0);
        systaProc.stdin.write(buf);

        return new Promise(function(resolve, reject) {
            promiseResolve = resolve;
            promiseReject = reject;
        });
    };


    /*
       Gets the value of a custom license field.

       More information on custom license fields: https://wyday.com/limelm/help/license-features/


       lpValueStr
       [out] Pointer to a buffer that receives the value of the string.

       cchValue
       [in] Size (in wide characters on Windows or characters in Unix) of the buffer
            pointed to by the lpValueStr parameter.

       If 'cchValue' is zero, the function returns the required buffer size (in wide characters
       on Windows, characters on Unix) and makes no use of the lpValueStr buffer.


       Returns: TA_OK on success. Handle all other return codes as failures.

       Possible return codes: TA_OK, TA_FAIL, TA_E_INVALID_HANDLE, TA_E_INSUFFICIENT_BUFFER
    */
    this.GetFeatureValue = function(featureName) {

        if (FunctionProcessing !== -1)
            return new Promise(alreadyRunningFail);

        FunctionProcessing = FUNC_GetFeatureValue;

        let buf = Buffer.allocUnsafe(6 + featureName.length);
        buf.writeInt32LE(FunctionProcessing, 0);
        buf.writeInt16LE(featureName.length, 4);

        // write the feature name without the trailing null
        buf.write(featureName, 6, featureName.length, 'utf8');

        systaProc.stdin.write(buf);

        return new Promise(function(resolve, reject) {
            promiseResolve = resolve;
            promiseReject = reject;
        });
    };


    /*
       Gets the stored product key. NOTE: if you want to check if a product key is valid
       simply call TA_IsProductKeyValid().


       lpValueStr
       [out] Pointer to a buffer that receives the value of the string.

       cchValue
       [in] Size (in wide characters on Windows or characters in Unix) of the buffer
            pointed to by the lpValueStr parameter.

       If 'cchValue' is zero, the function returns the required buffer size (in wide characters
       on Windows, characters on Unix) and makes no use of the lpValueStr buffer.


       Returns: TA_OK on success. Handle all other return codes as failures.

       Possible return codes: TA_OK, TA_FAIL, TA_E_PKEY, TA_E_INVALID_HANDLE,
                              TA_E_INSUFFICIENT_BUFFER
    */
    this.GetPKey = function() {

        if (FunctionProcessing !== -1)
            return new Promise(alreadyRunningFail);

        FunctionProcessing = FUNC_GetPKey;

        let buf = Buffer.allocUnsafe(4);
        buf.writeInt32LE(FunctionProcessing, 0);
        systaProc.stdin.write(buf);

        return new Promise(function(resolve, reject) {
            promiseResolve = resolve;
            promiseReject = reject;
        });
    };


    /*
       Checks whether the computer is activated. This does not re-verify with the activation
       servers. It just checks the cryptographically-signed activation data stored on the computer
       and verify the current hardware-fingerprint "fuzzy matches" the signed activation data.


       Returns: TA_OK on success. Handle all other return codes as failures.

       Possible return codes: TA_OK, TA_FAIL, TA_E_INVALID_HANDLE, TA_E_COM,
                              TA_E_IN_VM, TA_E_ANDROID_NOT_INIT, TA_E_ENABLE_NETWORK_ADAPTERS
    */
    this.IsActivated = function() {

        if (FunctionProcessing !== -1)
            return new Promise(alreadyRunningFail);

        FunctionProcessing = FUNC_IsActivated;

        let buf = Buffer.allocUnsafe(4);
        buf.writeInt32LE(FunctionProcessing, 0);
        systaProc.stdin.write(buf);

        return new Promise(function(resolve, reject) {
            promiseResolve = resolve;
            promiseReject = reject;
        });
    };


    /*
       Checks whether the computer is genuinely activated by verifying with the LimeLM servers immediately.
       If reactivation is needed then it will do this as well.

       For almost all use-cases you should use TA_IsGenuineEx(), not this function.


       Returns: TA_OK or TA_E_FEATURES_CHANGED on success. Handle all other return codes as failures.

       Possible return codes: TA_OK, TA_FAIL, TA_E_ACTIVATE, TA_E_INET, TA_E_INVALID_HANDLE,
                              TA_E_COM, TA_E_EXPIRED, TA_E_REVOKED, TA_E_IN_VM,
                              TA_E_FEATURES_CHANGED, TA_E_ANDROID_NOT_INIT, TA_E_ENABLE_NETWORK_ADAPTERS
    */
    this.IsGenuine = function() {

        if (FunctionProcessing !== -1)
            return new Promise(alreadyRunningFail);

        FunctionProcessing = FUNC_IsGenuine;

        let buf = Buffer.allocUnsafe(4);
        buf.writeInt32LE(FunctionProcessing, 0);
        systaProc.stdin.write(buf);

        return new Promise(function(resolve, reject) {
            promiseResolve = resolve;
            promiseReject = reject;
        });
    };


    /*
       Checks whether the computer is genuinely activated by verifying with the LimeLM servers
       after a certain number of days you specify.

       This is meant as a replacement of both TA_IsActivated() and TA_IsGenuine(). Call this at the
       top of your program and let TA_IsGenuineEx() handle all the details.

       This differs with TA_IsGenuine() in 3 major ways:

            1. You can specify how often to verify with the LimeLM servers and it handles
               all the date tracking behind the scenes.


            2. TA_IsGenuineEx() prevents your app from hammering the end-user's network after
               and TA_E_INET error return code by not checking with the LimeLM servers until
               at least 5 hours has passed. If you call TA_IsGenuineEx() after a TA_E_INET return
               and before 5 hours has elapsed then this function will return TA_E_INET_DELAYED.

               (If you give the user the option to recheck with LimeLM, e.g. via a button
               like "Retry now" then call TA_IsGenuine() to immediately retry without waiting 5 hours).


            3. If a TA_E_INET error is being returned, and the grace period has expired,
               then TA_IsGenuineEx() will return TA_FAIL. TA_IsGenuineEx() will continue to try
               contacting the LimeLM servers on subsequent calls (5 hours apart), but you
               should treat the TA_FAIL as a hard failure.


       Returns: TA_OK or TA_E_FEATURES_CHANGED on success. Handle TA_E_INET and TA_E_INET_DELAYED as warnings that
                you should let the end user know about.

                Handle all other return codes as failures.

       Possible return codes: TA_OK, TA_FAIL, TA_E_ACTIVATE, TA_E_INET, TA_E_INVALID_HANDLE,
                              TA_E_COM, TA_E_EXPIRED, TA_E_REVOKED, TA_E_INVALID_ARGS,
                              TA_E_INVALID_FLAGS, TA_E_IN_VM, TA_E_INET_DELAYED,
                              TA_E_FEATURES_CHANGED, TA_E_ANDROID_NOT_INIT,
                              TA_E_ENABLE_NETWORK_ADAPTERS
    */
    this.IsGenuineEx = function(daysBetweenChecks = 90, graceDaysOnInetErr = 14, skipOffline = false, offlineShowInetErr = false) {

        if (FunctionProcessing !== -1)
            return new Promise(alreadyRunningFail);

        FunctionProcessing = FUNC_IsGenuineEx;

        let flags = 0;

        if (skipOffline)
        {
            // TA_SKIP_OFFLINE = 1
            flags |= 1;

            // TA_OFFLINE_SHOW_INET_ERR = 2
            if (offlineShowInetErr)
                flags |= 2;
        }

        let buf = Buffer.allocUnsafe(16);
        buf.writeInt32LE(FunctionProcessing, 0);
        buf.writeUInt32LE(flags, 4);
        buf.writeUInt32LE(daysBetweenChecks, 8);
        buf.writeUInt32LE(graceDaysOnInetErr, 12);

        systaProc.stdin.write(buf);


        return new Promise(function(resolve, reject) {
            promiseResolve = resolve;
            promiseReject = reject;
        });
    };


    /*
       Get the number of days until the next time that the TA_IsGenuineEx() function contacts
       the LimeLM activation servers to reverify the activation.

       You must use the same "nDaysBetweenChecks" and "nGraceDaysOnInetErr" parameters you passed
       to TA_IsGenuineEx() using the GENUINE_OPTIONS structure.

       The number of days remaining until the next reverification with the servers will be put
       in the "DaysRemaining" variable. And if the customer is already in the grace period, then
       the "DaysRemaining" remaining will reflect the number of days left in the grace period and
       "inGracePeriod" will be 1.

       If both nDaysBetweenChecks and the nGraceDaysOnInetErr have passed, then "DaysRemaining"
       will be 0.

       Also, if TurboActivate determines that system date, time, or timezone are fraudulent then
       "DaysRemaining" will be 0.


       Returns: TA_OK on success. Handle all other return codes as failures.

       Possible return codes: TA_OK, TA_FAIL, TA_E_ACTIVATE, TA_E_INVALID_HANDLE
    */
    this.GenuineDays = function(daysBetweenChecks = 90, graceDaysOnInetErr = 14) {

        if (FunctionProcessing !== -1)
            return new Promise(alreadyRunningFail);

        FunctionProcessing = FUNC_GenuineDays;

        let buf = Buffer.allocUnsafe(12);
        buf.writeInt32LE(FunctionProcessing, 0);
        buf.writeUInt32LE(daysBetweenChecks, 4);
        buf.writeUInt32LE(graceDaysOnInetErr, 8);

        systaProc.stdin.write(buf);

        return new Promise(function(resolve, reject) {
            promiseResolve = resolve;
            promiseReject = reject;
        });
    };


    /*
       Checks if the product key installed for this product is valid. This does NOT check if
       the product key is activated or genuine. Use TA_IsActivated() and TA_IsGenuine() instead.


       Returns: TA_OK on success. Handle all other return codes as failures.

       Possible return codes: TA_OK, TA_FAIL, TA_E_INVALID_HANDLE
    */
    this.IsProductKeyValid = function(){

        if (FunctionProcessing !== -1)
            return new Promise(alreadyRunningFail);

        FunctionProcessing = FUNC_IsProductKeyValid;

        let buf = Buffer.allocUnsafe(4);
        buf.writeInt32LE(FunctionProcessing, 0);
        systaProc.stdin.write(buf);

        return new Promise(function(resolve, reject) {
            promiseResolve = resolve;
            promiseReject = reject;
        });
    };


    /*
       Sets the custom proxy to be used by functions that connect to the internet.


       Proxy in the form: http://username:password@host:port/

       Example 1 (just a host): http://127.0.0.1/
       Example 2 (host and port): http://127.0.0.1:8080/
       Example 3 (all 3): http://user:pass@127.0.0.1:8080/

       If the port is not specified, TurboActivate will default to using port 1080 for proxies.


       Returns: TA_OK on success. Handle all other return codes as failures.

       Possible return codes: TA_OK, TA_FAIL
    */
    this.SetCustomProxy = function(proxy){

        if (FunctionProcessing !== -1)
            return new Promise(alreadyRunningFail);

        // sanity check -- a value must be passed in
        if (typeof proxy !== 'string')
            return new Promise(invalidArgsFail);

        FunctionProcessing = FUNC_SetCustomProxy;

        let buf = Buffer.allocUnsafe(6 + proxy.length);
        buf.writeInt32LE(FunctionProcessing, 0);
        buf.writeInt16LE(proxy.length, 4);

        // write the proxy without the trailing null
        buf.write(proxy, 6, proxy.length, 'utf8');

        systaProc.stdin.write(buf);

        return new Promise(function(resolve, reject) {
            promiseResolve = resolve;
            promiseReject = reject;
        });
    };


    /*
       Get the number of trial days remaining.
       0 days if the trial has expired or has been tampered with
       (1 day means *at most* 1 day, that is it could be 30 seconds)

       You must call TA_UseTrial() at least once in the past before calling this function.
       And you must call this function with the same flags you used with TA_UseTrial().


       Returns: TA_OK on success. Handle all other return codes as failures.

       Possible return codes: TA_OK, TA_FAIL, TA_E_INVALID_HANDLE, TA_E_ALREADY_VERIFIED_TRIAL,
                              TA_E_MUST_USE_TRIAL, TA_E_MUST_SPECIFY_TRIAL_TYPE
    */
    this.TrialDaysRemaining = function(flags = TA_USER | TA_VERIFIED_TRIAL) {

        if (FunctionProcessing !== -1)
            return new Promise(alreadyRunningFail);

        FunctionProcessing = FUNC_TrialDaysRemaining;

        let buf = Buffer.allocUnsafe(8);
        buf.writeInt32LE(FunctionProcessing, 0);
        buf.writeUInt32LE(flags, 4);

        systaProc.stdin.write(buf);

        return new Promise(function(resolve, reject) {
            promiseResolve = resolve;
            promiseReject = reject;
        });
    };


    /*
       Begins the trial the first time it's called. Calling it again will validate the trial
       data hasn't been tampered with.

       It is recommended that you use the following flag combination: TA_VERIFIED_TRIAL | TA_SYSTEM


       Note: If you pass in the TA_SYSTEM flag and you don't have "admin" or "elevated"
       permission then the call will fail.

       If you call this function once in the past with the flag TA_SYSTEM and the calling
       process was an admin process then subsequent calls with the TA_SYSTEM flag will
       succeed even if the calling process is *not* admin/elevated.


       Returns: TA_OK on success. Handle all other return codes as failures.

       Possible return codes: TA_OK, TA_FAIL, TA_E_INVALID_HANDLE, TA_E_INET, TA_E_PERMISSION, TA_E_COM
                              TA_E_INVALID_FLAGS, TA_E_IN_VM, TA_E_IN_SANDBOX,
                              TA_E_ANDROID_NOT_INIT, TA_E_ACCOUNT_CANCELED,
                              TA_E_ENABLE_NETWORK_ADAPTERS, TA_E_ALREADY_VERIFIED_TRIAL,
                              TA_E_EXPIRED, TA_E_TRIAL_EXPIRED, TA_E_MUST_SPECIFY_TRIAL_TYPE,
                              TA_E_EDATA_LONG, TA_E_NO_MORE_TRIALS_ALLOWED
    */
    this.UseTrial = function(trialCb, flags = TA_USER | TA_VERIFIED_TRIAL, extraData = null) {

        if (FunctionProcessing !== -1)
            return new Promise(alreadyRunningFail);

        trialCallback = trialCb;

        FunctionProcessing = FUNC_UseTrial;

        let buf = Buffer.allocUnsafe(9 + (extraData !== null ? (extraData.length + 2) : 0));

        buf.writeInt32LE(FunctionProcessing, 0);
        buf.writeUInt32LE(flags, 4);

        // write whether we have extra data or not
        buf.writeInt8(extraData === null ? 0 : 1, 8);

        if (extraData !== null)
        {
            buf.writeInt16LE(extraData.length, 9);
            buf.write(extraData, 11, extraData.length, 'utf8');
        }

        systaProc.stdin.write(buf);

        return new Promise(function(resolve, reject) {
            promiseResolve = resolve;
            promiseReject = reject;
        });
    };


    /*
       Generate a "verified trial" offline request file. This file will then need to be submitted to LimeLM.
       You will then need to use the TA_UseTrialVerifiedFromFile() function with the response file from LimeLM
       to actually start the trial.


       The easier solution is to use verified trials online. Simply use the TA_UseTrial() function with
       the TA_VERIFIED_TRIAL | TA_SYSTEM flags.



       Returns: TA_OK on success. Handle all other return codes as failures.

       Possible return codes: TA_OK, TA_FAIL, TA_E_INVALID_HANDLE, TA_E_PERMISSION, TA_E_COM
                              TA_E_ANDROID_NOT_INIT, TA_E_ENABLE_NETWORK_ADAPTERS,
                              TA_E_EDATA_LONG, TA_E_INVALID_ARGS
    */
    this.UseTrialVerifiedRequest = function(filename, extraData = null) {

        if (FunctionProcessing !== -1)
            return new Promise(alreadyRunningFail);

        if (typeof filename !== 'string')
            return new Promise(invalidArgsFail);

        FunctionProcessing = FUNC_UseTrialVerifiedRequest;

        let buf = Buffer.allocUnsafe(7 + filename.length + (extraData !== null ? (extraData.length + 2) : 0));

        buf.writeInt32LE(FunctionProcessing, 0);

        buf.writeInt16LE(filename.length, 4);
        buf.write(filename, 6, filename.length, 'utf8');


        // write whether we have extra data or not
        buf.writeInt8(extraData === null ? 0 : 1, 6 + filename.length);

        if (extraData !== null)
        {
            buf.writeInt16LE(extraData.length, 7 + filename.length);
            buf.write(extraData, 9 + filename.length, extraData.length, 'utf8');
        }

        systaProc.stdin.write(buf);

        return new Promise(function(resolve, reject) {
            promiseResolve = resolve;
            promiseReject = reject;
        });
    };


    /*
       Use the "verified trial response" from LimeLM to start the verified trial.



       To use verified trials online, simply use the TA_UseTrial() function with the TA_VERIFIED_TRIAL | TA_SYSTEM
       flags.


       Note: If you pass in the TA_SYSTEM flag and you don't have "admin" or "elevated"
       permission then the call will fail.

       If you call this function once in the past with the flag TA_SYSTEM and the calling
       process was an admin process then subsequent calls with the TA_SYSTEM flag will
       succeed even if the calling process is *not* admin/elevated.


       Returns: TA_OK on success. Handle all other return codes as failures.

       Possible return codes: TA_OK, TA_FAIL, TA_E_INVALID_HANDLE, TA_E_PERMISSION, TA_E_COM
                              TA_E_INVALID_FLAGS, TA_E_IN_VM, TA_E_IN_SANDBOX,
                              TA_E_ANDROID_NOT_INIT, TA_E_ENABLE_NETWORK_ADAPTERS,
                              TA_E_MUST_SPECIFY_TRIAL_TYPE, TA_E_INVALID_ARGS
    */
    this.UseTrialVerifiedFromFile = function(filename, flags = TA_USER | TA_VERIFIED_TRIAL) {

        if (FunctionProcessing !== -1)
            return new Promise(alreadyRunningFail);

        if (typeof filename !== 'string')
            return new Promise(invalidArgsFail);

        FunctionProcessing = FUNC_UseTrialVerifiedFromFile;

        let buf = Buffer.allocUnsafe(10 + filename.length);

        buf.writeInt32LE(FunctionProcessing, 0);

        buf.writeInt16LE(filename.length, 4);
        buf.write(filename, 6, filename.length, 'utf8');

        buf.writeUInt32LE(flags, 6 + filename.length);

        systaProc.stdin.write(buf);

        return new Promise(function(resolve, reject) {
            promiseResolve = resolve;
            promiseReject = reject;
        });
    };


    /*
       Extends the trial using a trial extension created in LimeLM.

       You must call TA_UseTrial() at least once in the past before calling this function.
       And you must call this function with the same flags you used with TA_UseTrial().


       Returns: TA_OK on success. Handle all other return codes as failures.

       Possible return codes: TA_OK, TA_FAIL, TA_E_INET, TA_E_INVALID_HANDLE, TA_E_TRIAL_EUSED
                              TA_E_TRIAL_EEXP, TA_E_MUST_USE_TRIAL, TA_E_MUST_SPECIFY_TRIAL_TYPE
    */
    this.ExtendTrial = function(trialExtension, flags = TA_USER | TA_VERIFIED_TRIAL) {

        if (FunctionProcessing !== -1)
            return new Promise(alreadyRunningFail);

        if (typeof trialExtension !== 'string')
            return new Promise(invalidArgsFail);

        FunctionProcessing = FUNC_ExtendTrial;

        let buf = Buffer.allocUnsafe(10 + trialExtension.length);
        buf.writeInt32LE(FunctionProcessing, 0);
        buf.writeInt16LE(trialExtension.length, 4);

        // write the product key without the trailing null
        buf.write(trialExtension, 6, trialExtension.length, 'utf8');
        buf.writeUInt32LE(flags, 6 + trialExtension.length);

        systaProc.stdin.write(buf);

        return new Promise(function(resolve, reject) {
            promiseResolve = resolve;
            promiseReject = reject;
        });
    };


    /*
       Checks if the string in the form "YYYY-MM-DD HH:mm:ss" is a valid
       date/time. The date must be in UTC time and "24-hour" format. If your
       date is in some other time format first convert it to UTC time before
       passing it into this function.

       Returns: TA_OK on success. Handle all other return codes as failures.

       Possible return codes: TA_OK, TA_FAIL, TA_E_INVALID_FLAGS, TA_E_INVALID_HANDLE
    */
    this.IsDateValid = function(dateTime, flags = TA_HAS_NOT_EXPIRED) {

        if (FunctionProcessing !== -1)
            return new Promise(alreadyRunningFail);

        if (typeof dateTime !== 'string')
            return new Promise(invalidArgsFail);

        FunctionProcessing = FUNC_IsDateValid;

        let buf = Buffer.allocUnsafe(10 + dateTime.length);
        buf.writeInt32LE(FunctionProcessing, 0);
        buf.writeInt16LE(dateTime.length, 4);

        // write the date/time without the trailing null
        buf.write(dateTime, 6, dateTime.length, 'utf8');
        buf.writeUInt32LE(flags, 6 + dateTime.length);

        systaProc.stdin.write(buf);

        return new Promise(function(resolve, reject) {
            promiseResolve = resolve;
            promiseReject = reject;
        });
    };


    process.on('exit', (code) => {
        // kill the running systa process
        if (systaProc !== null)
            systaProc.kill('SIGKILL');
    });
}


module.exports = TurboActivate;