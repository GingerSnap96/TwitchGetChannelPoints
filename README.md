# 📺 Twitch Get Channel Points

 **Twitch Get Channel Points** is a console application written in C# that enables you to retrieve and export channel points data for followed Twitch channels. It uses the Twitch API to fetch user and channel information, perform OAuth authorization, and extract channel points information for the followed channels of a specified user.


## 🖥️ Supported Operating Systems

Twitch Get Channel Points is compatible with the following operating systems and architectures:

| Operating System | Architecture  | Compatibility |
| ---------------- | ------------- | ------------- |
| Windows 11       | x64           | ✅             |
| Windows 10       | x86, x64      | ✅             |
| Linux (Ubuntu)   | x86, x64, ARM | ✅             |
| Linux (Debian)   | x86, x64, ARM | ✅             |
| macOS            | x64, ARMx64   | ✅             |

## 💡 Getting Started

To get started with the application, follow these simple steps:

1. Download the Latest Release: 
   - Visit the [Releases](https://github.com/gingersnap96/twitchgetchannelpoints/releases) page and download the latest release of Twitch Get Channel Points. Look for the release package named something like `GetChannelPoints-vX.X.X-win-x64.zip`. Make sure to select the release package appropriate for your operating system (Windows, Linux or macOS) and architecture (x86, x64, arm).

2. Extract the Files: 
   - After downloading the zip file, extract its contents to a directory of your choice on your machine.

3. Navigate to the Directory: 
   - Open the folder where you extracted the files.

## ⚙️ Usage

1. Open `config.json` in a text editor and fill in your Twitch Developer account `ClientId` and `ClientSecret`. You will need to sign up for a twicth developer account if you do not already have one.
   - For more information on how to get these values follow this article. https://faq.demostoreprestashop.com/faq.php?fid=144&pid=41

2. In the `config.json` for `AuthToken`, Log into twitch in a web browser, open the dev tools (F12), navigate to the console tab, enter the following command and copy the code in the alert message that pops up.
   
    ```bash
    alert( document.cookie.match( /(; )?auth-token=([^;]*);?/ )[ 2 ] )
    ```
3. Run the Application:
    - For Windows:
        - Double-click the `GetChannelPoints.exe` executable to launch the application.
    - For Linux:
        - Open a terminal and navigate to the directory containing the `GetChannelPoints` executable (It will have no extension).
        - Ensure that the `config.json` file is also located in this directory.
        - Run the following command to add execute permissions to the executable: (you only need to do this once)
            ```bash
            chmod r+w+x GetChannelPoints
            ```
        - Run the following command to add execute the application:
            ```bash
            ./GetChannelPoints
            ```
    - For macOS:
        - Double-click the `GetChannelPoints` executable to launch the application(It will have no extension). 
        - You may get a security warning and your OS will prevent the app from running, if so follow the steps below.
            - Open Settings > Security and Privacy > General Tab 
            - If you just tried to run the application there should be an option to allow the application, select allow.
        - Double-click the `GetChannelPoints` executable to launch the application

4. Follow the prompts to perform the OAuth authorization and enter the username for which you want to extract channel points data.

## ✉️ Contact

For any issues or questions, please open an issue in this repository.
