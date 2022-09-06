# OddsCollector
Collects the most recent odds from https://the-odds-api.com/ and makes predictions in CSV format.

To run/build add these parameters to appsettings.json and appsettings.Development.json:
1. Odds API key. You can request it from here https://the-odds-api.com/#get-access
2. MS SQL connection string.
3. [Optional] Path on the file system where you expect to store generated CSV files.
4. [Optional] Google API details. Can be found here: https://medium.com/@a.marenkov/how-to-get-credentials-for-google-sheets-456b7e88c430
