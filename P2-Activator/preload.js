window.addEventListener('DOMContentLoaded', () => {
    const replaceText = (selector, text) => {
        const element = document.getElementById(selector);
        if (element) element.innerText = text;
    }

    'use strict';

    function RemoveLastDirectoryPartOf(the_url)
    {
        var the_arr = the_url.split('/');
        the_arr.pop();
        return( the_arr.join('/') );
    }

    var TurboActivate = require('./turboactivate');
    let ipcRenderer = require('electron').ipcRenderer;

    var ta = new TurboActivate("49b9b11f567afe186ce228.81210550");

    let resultText = "Activation Failed. ";

    function ActivationCompleted() {
        resultText = "Activation Completed. You have to choose Application to apply activation.";
        replaceText("result", resultText);
        document.getElementById("key").style.display = "none";
        document.getElementById("activate").style.display = "none";
        document.getElementById("save").style.display = "block";

        SaveActivation();
    }

    function SaveActivation() {
        let datFilePath = require('electron').remote.app.getAppPath();

        datFilePath = RemoveLastDirectoryPartOf(RemoveLastDirectoryPartOf(datFilePath));

        datFilePath = datFilePath + "/native/Mac/TurboActivate.dat";

        ipcRenderer.send('save-turboactivate-dat-file', datFilePath);
    }

    function HardFailure(context, errCode) {
        resultText = context;
        replaceText("result", resultText);
        if (errCode == 0) {
            document.getElementById("key").style.display = "none";
            document.getElementById("activate").style.display = "none";
            document.getElementById("save").style.display = "none";
        } else {
            document.getElementById("key").style.display = "block";
            document.getElementById("activate").style.display = "block";
            document.getElementById("save").style.display = "none";
        }
    }

    document.getElementById("activate").addEventListener('click', CheckActivation);
    document.getElementById("save").addEventListener('click', SaveActivation);

    function PromptUserToReverify() {
        var userResp = 'n';

        if (userResp === 'n') {
            replaceText("result", resultText);
            document.getElementById("key").style.display = "block";
            document.getElementById("activate").style.display = "block";
            document.getElementById("save").style.display = "none";
        }
        else {
            ta.IsGenuine()
                .then((retObj) => {
                    if (retObj === TA_E_INET) {
                        PromptUserToReverify();
                        return null;
                    }
                    else if (retObj === TA_OK || retObj === TA_E_FEATURES_CHANGED) {
                        ActivationCompleted();
                        return null;
                    }
                    else {
                        HardFailure("Not activated.", retObj);
                    }
                })
                .catch((retObj) => {
                    HardFailure("Something failed!", retObj);
                });
        }
    }

    function PromptForProductKey() {
        var userEnteredPkey = document.getElementById("key").value;

        ta.CheckAndSavePKey(userEnteredPkey, TA_USER)
            .then((retObj) => {
                if (retObj === TA_OK)
                    return ta.Activate();
                else {
                    console.log(retObj);
                    HardFailure("Product key is not correct.", 1);
                    return null;
                }
            })
            .then((retObj) => {
                if (retObj === null)
                    return null;

                if (retObj === TA_OK) {
                    ActivationCompleted();
                }
                else {
                    HardFailure("Activation failed.", retObj);
                }
            })
            .catch((retObj) => {
                HardFailure("Something failed!", retObj);
            });
    }

    function CheckActivation() {
        ta.IsGenuine()
            .then((retObj) => {
                if (retObj === TA_OK || retObj === TA_E_FEATURES_CHANGED) {
                    ActivationCompleted();
                    return null;
                }
                else if (retObj === TA_E_INET || retObj === TA_E_INET_DELAYED) {
                    HardFailure("Activator was unable to activate due to network problem.", retObj);
                }
                else
                    return ta.IsActivated();
            })
            .then((retObj) => {
                if (retObj === null)
                    return null;

                if (retObj === TA_OK) {
                    PromptUserToReverify();
                    return null;
                }
                else {
                    if (document.getElementById("key").style.display == "block"
                        && document.getElementById("key").value.length > 0) {
                        PromptForProductKey();
                    } else {
                        HardFailure("Please input product key below.", 1);
                    }
                    return null;
                }
            })
            .catch((retObj) => {
                console.log(retObj);
                HardFailure("TurboActivate.dat is missing or corrupt. Please check and restart this application.", 0);
            });
    }

    CheckActivation();
});
