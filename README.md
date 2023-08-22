# Twitch Get Channel Points

The **Twitch Get Channel Points** is a console application written in C# that enables you to retrieve and export channel points data for followed Twitch channels. It uses the Twitch API to fetch user and channel information, perform OAuth authorization, and extract channel points information for the specified channels.

## Getting Started

Follow these steps to set up and use the application:

### Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) installed on your machine.
- This is designed only for windows OS.

## Download and Use

1. Download the pre-packaged application from the [Releases](https://github.com/GingerSnap96/TwitchGetChannelPoints/releases) section.

2. Extract the downloaded zip file to a directory of your choice.

3. Navigate to the extracted directory using a terminal.

4. Open `config.json` in a text editor and fill in your Twitch Developer account `ClientId` and `ClientSecret`. You will need to sign up for a twicth developer account if you do not already have one.

5. In the `config.json` for `AuthToken`, Log into twitch in a web browser, open the dev tools (F12), navigate to the console tab, enter the following command and copy the code in the alert message that pops up.
   
    ```bash
    alert( document.cookie.match( /(; )?auth-token=([^;]*);?/ )[ 2 ] )
    ```

6. Run the application executable GetChannelPoints.exe

7. Follow the prompts to perform the OAuth authorization and enter the username for which you want to extract channel points data.

8. Once the extraction process is complete, the extracted data will be saved as a CSV file in your user's "Downloads" folder.

## Run from source code

### Installation

1. Clone this repository to your local machine:

    ```bash
    git clone https://github.com/GingerSnap96/TwitchGetChannelPoints.git
    ```

2. Navigate to the project directory:

    ```bash
    cd GetChannelPoints
    ```


### Configuration

1. Open `config.json` in a text editor and fill in your Twitch Developer account `ClientId` and `ClientSecret`. You will need to sign up for a twicth developer account if you do not already have one.

2. In the `config.json` for `AuthToken`, Log into twitch in a web browser, open the dev tools (F12), navigate to the console tab, enter the following command and copy the code in the alert message that pops up.
   
    ```bash
    alert( document.cookie.match( /(; )?auth-token=([^;]*);?/ )[ 2 ] )
    ```
### Running the Application

2. Open a terminal and navigate to the project directory.

3. Build the project:

    ```bash
    dotnet build
    ```

4. Run the application:

    ```bash
    dotnet run
    ```

5. Follow the prompts to perform the OAuth authorization and enter the username for which you want to extract channel points data.

6. Once the extraction process is complete, the extracted data will be saved as a CSV file in your user's "Downloads" folder.

## Application Flow

1. The application reads configuration values from the `config.json` file.

2. The application prompts you to enter the username for which you want to extract channel points data.

3. The application fetches the followed channels for the user and iterates over them.

4. For each followed channel, it captures the console output of the channel points data and stores it in CSV rows.

5. The collected CSV rows are then saved in a CSV file named `<username>_ChannelPoints_<timestamp>.csv` in the user's "Downloads" folder.

## Contact

For any issues or questions, please open an issue in this repository or contact [banstey@technovity.solutions](mailto:banstey@technovity.solutions).
