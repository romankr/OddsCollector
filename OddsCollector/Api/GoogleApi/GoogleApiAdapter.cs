namespace OddsCollector.Api.GoogleApi;

using Betting;
using Common;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using System.Reflection;

public class GoogleApiAdapter : IGoogleApiAdapter
{
    private const string StatisticsSheetTitle = nameof(Statistics);
    private const string SuggestionsSheetTitle = nameof(BettingSuggestion);
    private const string DefaultSheetTitle = "Sheet1";
    private const string DefaultCellRange = "A:A";
    private const string DoubleFormat = "N3";

    private readonly string _applicationName;
    private readonly string _emailAddress;
    private readonly string _credentialFile;

    public GoogleApiAdapter(IConfiguration config)
    {
        _applicationName = config["GoogleApi:applicationName"];
        _emailAddress = config["GoogleApi:emailAddress"];
        _credentialFile = config["GoogleApi:credentialFile"];
    }

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

        var file = CreateGoogleDriveDocument(bettingStrategyName);
        PopulateSpreadsheet(file, result);
    }

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

        var metadata = new File
        {
            Name = $"{bettingStrategyName}_{DateUtility.GetTimestamp()}",
            MimeType = "application/vnd.google-apps.spreadsheet"
        };

        var createRequest = service.Files.Create(metadata);
        var file = createRequest.Execute();

        var permission = new Permission
        {
            EmailAddress = _emailAddress,
            Type = "user",
            Role = "writer"
        };

        var permissionRequest = service.Permissions.Create(permission, file.Id);
        permissionRequest.Execute();

        return file;
    }

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

        var batchUpdateRequest = 
            new BatchUpdateSpreadsheetRequest { Requests = new List<Request>() };
        
        batchUpdateRequest.Requests.Add(CreateAddSheetRequest(SuggestionsSheetTitle));
        batchUpdateRequest.Requests.Add(CreateAddSheetRequest(StatisticsSheetTitle));

        var updateRequest = 
            service.Spreadsheets.BatchUpdate(batchUpdateRequest, file.Id);

        updateRequest.Execute();

        var request = service.Spreadsheets.Get(file.Id);
        var response = request.Execute();

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

    private static List<object> GetHeaders<T>()
    {
        return typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToList<object>();
    }

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