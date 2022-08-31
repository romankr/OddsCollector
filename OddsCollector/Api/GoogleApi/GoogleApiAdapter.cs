﻿namespace OddsCollector.Api.GoogleApi;

using Betting;
using Common;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using System.Reflection;

/// <summary>
/// Provides access to Google API-s.
/// </summary>
public class GoogleApiAdapter : IGoogleApiAdapter
{
    private const string StatisticsSheetTitle = nameof(Statistics);
    private const string SuggestionsSheetTitle = nameof(BettingSuggestion);
    private const string DefaultSheetTitle = "Sheet1";
    private const string DefaultCellRange = "A:A";
    private const string DoubleFormat = "N3";

    private readonly string _applicationName;
    private readonly List<string> _emailAddresses;
    private readonly string _credentialFile;

    /// <summary>
    /// A constructor that is suitable for the dependency injection.
    /// </summary>
    /// <param name="config">An <see cref="IConfiguration"/> created by the dependency injection container.</param>
    /// <exception cref="ArgumentNullException"><paramref name="config"/> is null.</exception>
    public GoogleApiAdapter(IConfiguration config)
    {
        if (config is null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        _applicationName = config["GoogleApi:applicationName"];

        _emailAddresses = 
            config
                .GetSection("GoogleApi:emailAddresses")
                .GetChildren()
                .Select(c => c.Value)
                .ToList();
        
        _credentialFile = config["GoogleApi:credentialFile"];
    }

    /// <summary>
    /// Creates a new Google Sheets document with provided <see cref="BettingStrategyResult"/>.
    /// </summary>
    /// <param name="bettingStrategyName">A betting strategy name to use in the file name.</param>
    /// <param name="result">A <see cref="BettingStrategyResult"/> to be saved as a Google Sheets document.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bettingStrategyName"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is null.</exception>
    /// <remarks>The document is being created in 2 steps. First in Google Drive and only then edited in Google Sheets.</remarks>
    public void CreateReport(string bettingStrategyName, BettingStrategyResult? result)
    {
        if (string.IsNullOrEmpty(bettingStrategyName))
        {
            throw new ArgumentOutOfRangeException(nameof(bettingStrategyName), "bettingStrategyName cannot be null or empty");
        }

        if (result is null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        // The document must be created in 2 steps. First in Google Drive and only then edited in Google Sheets.
        var file = CreateGoogleDriveDocument(bettingStrategyName);
        PopulateSpreadsheet(file, result);
    }

    /// <summary>
    /// Creates a new Google Sheets document in Google Drive.
    /// </summary>
    /// <param name="bettingStrategyName">A betting strategy name to use in the file name.</param>
    /// <returns>A newly created <see cref="File"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bettingStrategyName"/> is null or empty.</exception>
    private File CreateGoogleDriveDocument(string bettingStrategyName)
    {
        if (string.IsNullOrEmpty(bettingStrategyName))
        {
            throw new ArgumentOutOfRangeException(nameof(bettingStrategyName), "bettingStrategyName cannot be null or empty");
        }

        var scopes = new [] { DriveService.Scope.Drive, DriveService.Scope.DriveFile };

        var service = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = GetCredential(scopes),
            ApplicationName = _applicationName
        });

        // Create a new file with given metadata. This does not include file permissions.
        var metadata = new File
        {
            Name = $"{bettingStrategyName}_{DateUtility.GetTimestamp()}",
            MimeType = "application/vnd.google-apps.spreadsheet"
        };

        var createRequest = service.Files.Create(metadata);
        var file = createRequest.Execute();

        // The file permissions must be granted in a separate request.
        foreach (var email in _emailAddresses)
        {
            var permissionRequest = 
                service.Permissions.Create(
                new Permission
                    {
                        EmailAddress = email,
                        Type = "user",
                        Role = "writer"
                    }, file.Id);

            permissionRequest.Execute();
        }

        return file;
    }

    /// <summary>
    /// Populates an existing Google Sheets document.
    /// </summary>
    /// <param name="file">An existing <see cref="File"/> in Google Drive.</param>
    /// <param name="result">A <see cref="BettingStrategyResult"/> to be saved as a Google Sheets document.</param>
    /// <exception cref="ArgumentNullException">Either <paramref name="file"/> or <paramref name="result"/> are null.</exception>
    private void PopulateSpreadsheet(File? file, BettingStrategyResult? result)
    {
        if (file is null)
        {
            throw new ArgumentNullException(nameof(file));
        }

        if (result is null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        var scopes = new[] { SheetsService.Scope.Spreadsheets };

        var service = new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = GetCredential(scopes),
            ApplicationName = _applicationName
        });

        // Create a new sheet for each property of BettingStrategyResult class.
        var batchUpdateRequest = 
            new BatchUpdateSpreadsheetRequest { Requests = new List<Request>() };
        
        batchUpdateRequest.Requests.Add(CreateAddSheetRequest(SuggestionsSheetTitle));
        batchUpdateRequest.Requests.Add(CreateAddSheetRequest(StatisticsSheetTitle));

        var updateRequest = 
            service.Spreadsheets.BatchUpdate(batchUpdateRequest, file.Id);

        updateRequest.Execute();

        var request = service.Spreadsheets.Get(file.Id);
        var response = request.Execute();

        // Remove the default sheet that is always present in a newly created Google Sheets file.
        // Must be done when there are already some sheets in the file -
        // Google Sheets API doesn't allow to have 0 sheets in a Google Sheets file.
        var defaultSheet = 
            response.Sheets.FirstOrDefault(s => s.Properties.Title == DefaultSheetTitle);

        if (defaultSheet is not null)
        {
            batchUpdateRequest.Requests.Clear();

            batchUpdateRequest.Requests.Add(new Request
            {
                DeleteSheet = new DeleteSheetRequest
                {
                    SheetId = defaultSheet.Properties.SheetId
                }
            });

            updateRequest = service.Spreadsheets.BatchUpdate(batchUpdateRequest, file.Id);
            updateRequest.Execute();
        }

        // Populate BettingStrategyResult data.
        // Betting suggestions first.
        var appendSuggestionsRequest =
            service.Spreadsheets.Values.Append(
                new ValueRange { Values = MapBettingSuggestions(result.Suggestions) },
                file.Id,
                $"{SuggestionsSheetTitle}!{DefaultCellRange}");

        appendSuggestionsRequest.InsertDataOption =
            SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
        appendSuggestionsRequest.ValueInputOption =
            SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;

        appendSuggestionsRequest.Execute();

        // Statistics last.
        var appendStatisticsRequest = 
            service.Spreadsheets.Values.Append(
                new ValueRange { Values = MapStatistics(result.Statistics) }, 
                file.Id,
                $"{StatisticsSheetTitle}!{DefaultCellRange}");

        appendStatisticsRequest.InsertDataOption = 
            SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
        appendStatisticsRequest.ValueInputOption = 
            SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;

        appendStatisticsRequest.Execute();
    }

    /// <summary>
    /// Creates a new instance of <see cref="GoogleCredential"/> object from a JSON file with given scopes.
    /// </summary>
    /// <param name="scopes">A list of scopes.</param>
    /// <returns>A new instance of <see cref="GoogleCredential"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="scopes"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="scopes"/> has 0 elements.</exception>
    /// <remarks>
    /// A <see cref="GoogleCredential"/> object is required for access to Google API.
    /// Scopes express the permissions you request users to authorize for your app.
    /// The JSON file must be obtained from https://console.cloud.google.com/
    /// (a full list of steps can be found here https://medium.com/@a.marenkov/how-to-get-credentials-for-google-sheets-456b7e88c430).
    /// </remarks>
    private GoogleCredential GetCredential(string[] scopes)
    {
        if (scopes is null)
        {
            throw new ArgumentNullException(nameof(scopes));
        }

        if (scopes.Length == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(scopes), "scopes cannot be empty");
        }

        return GoogleCredential.FromFile(_credentialFile).CreateScoped(scopes);
    }

    /// <summary>
    /// Creates a new instance of <see cref="Request"/> that adds a new sheet to an existing Google Sheets document.
    /// </summary>
    /// <param name="sheetTitle">A title of the sheet.</param>
    /// <returns>A new instance of <see cref="Request"/></returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="sheetTitle"/> is null or empty.</exception>
    private static Request CreateAddSheetRequest(string sheetTitle)
    {
        if (string.IsNullOrEmpty(sheetTitle))
        {
            throw new ArgumentOutOfRangeException(nameof(sheetTitle), "sheetTitle cannot be null or empty");
        }

        return new Request
        {
            AddSheet = new AddSheetRequest
            {
                Properties = new SheetProperties
                {
                    Title = sheetTitle
                }
            }
        };
    }

    /// <summary>
    /// Creates a list of headers for given object.
    /// </summary>
    /// <typeparam name="T">An object type.</typeparam>
    /// <returns>A list of headers.</returns>
    private static List<object> GetHeaders<T>()
    {
        return typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToList<object>();
    }

    /// <summary>
    /// Maps a <see cref="Statistics"/> object to Google Sheets objects.
    /// </summary>
    /// <param name="statistics">>A <see cref="Statistics"/> instance.</param>
    /// <returns>A list of Google Sheets objects.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="statistics"/> is null.</exception>
    private static IList<IList<object>> MapStatistics(Statistics? statistics)
    {
        if (statistics is null)
        {
            throw new ArgumentNullException(nameof(statistics));
        }

        var result = new List<IList<object>>
        {
            GetHeaders<Statistics>(),
            new List<object>
            {
                statistics.Accuracy.ToString(DoubleFormat),
                statistics.EarningsInPoints.ToString(DoubleFormat),
                statistics.Earnings10Bet.ToString(DoubleFormat),
                statistics.Earnings20Bet.ToString(DoubleFormat),
                statistics.Earnings50Bet.ToString(DoubleFormat),
                statistics.TotalNumberOfGames.ToString(),
                statistics.NumberOfSuccessfulPredictions.ToString()
            }
        };

        return result;
    }

    /// <summary>
    /// Maps a <see cref="BettingSuggestion"/> object to Google Sheets objects.
    /// </summary>
    /// <param name="suggestions">A <see cref="BettingSuggestion"/> instance.</param>
    /// <returns>A list of Google Sheets objects.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="suggestions"/> is null.</exception>
    private static IList<IList<object>> MapBettingSuggestions(IEnumerable<BettingSuggestion>? suggestions)
    {
        if (suggestions is null)
        {
            throw new ArgumentNullException(nameof(suggestions));
        }

        var result = new List<IList<object>>
        {
            GetHeaders<BettingSuggestion>()
        };

        foreach (var s in suggestions.OrderBy(s => s.CommenceTime))
        {
            result.Add(new List<object>
            {
                s.SportEventId ?? string.Empty,
                s.CommenceTime.ToString("R"),
                s.AwayTeam ?? string.Empty,
                s.HomeTeam ?? string.Empty,
                s.BestBookmaker ?? string.Empty,
                s.AverageScore.ToString(DoubleFormat),
                s.BestScore.ToString(DoubleFormat),
                s.ExpectedOutcome ?? string.Empty,
                s.RealOutcome ?? string.Empty
            });
        }

        return result;
    }
}