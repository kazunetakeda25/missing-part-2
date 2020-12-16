const { app, remote, dialog, BrowserWindow } = require('electron');
const path = require('path');
const ipc = require('electron').ipcMain;

let mainWindow = null;

function createWindow() {
  mainWindow = new BrowserWindow({
    width: 400,
    height: 300,
    resizable: false, 
    useContentSize: true,
    movable: true, 
    center: true, 
    acceptFirstMouse: true,
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      nodeIntegration: true,
      nodeIntegrationInWorker: true,
      enableRemoteModule: true,
        devTools: false
    }
  });

  mainWindow.setMenuBarVisibility(false);
  mainWindow.loadFile('index.html');
  mainWindow.webContents.on('did-finish-load', function () {
    mainWindow.webContents.send('mainWindowLoaded');
  });
}

app.whenReady().then(() => {
  createWindow();

  app.on('activate', function () {
    if (BrowserWindow.getAllWindows().length === 0)
      createWindow();
  });
});

app.on('window-all-closed', function () {
  if (process.platform !== 'darwin')
    app.quit();
});

ipc.on('close-application', function () {
  if (process.platform !== 'darwin')
    app.quit();
});

ipc.on('save-turboactivate-dat-file', function (event, datFilePath) {

  let options = {
    title: "Please select Missing P2 App to activate the application.",
    buttonLabel: "Select",
    properties: ['openFile']
  };

  dialog.showOpenDialog(mainWindow, options)
    .then(result => {
      if (result.canceled == false) {
        let targetPath = result.filePaths[0] + "/Contents/MacOS/TurboActivate.dat";
        console.log("Source Path: " + datFilePath);
        console.log("Target Path: " + targetPath);
        let fs = require('fs');
        fs.copyFile(datFilePath, targetPath, (err) => {
          if (err) throw err;
          console.log("Success");
          fs.chmodSync(targetPath, '0755');
        });
      }
    })
    .catch(err => {
      console.log(err);
    });
});