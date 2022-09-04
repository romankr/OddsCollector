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
    private readonly IEnumerable<string> _emailAddresses;
    private readonly string _credentialFile;
    private readonly bool _enabled;

    /// <summary>
    /// A constructor that is suitable for the dependency injection.
    /// </summary>
    /// <param name="config">An <see cref="IConfiguration"/> instance created by the dependency injection container.</param>
    /// <exception cref="ArgumentNullException"><paramref name="config"/> is null.</exception>
    /// <exception cref="Exception">
    /// ApplicationName is null or empty or
    /// Email addresses are null or
    /// Email addresses are empty or
    /// Credential file name is null or empty.
    /// </exception>
    public GoogleApiAdapter(IConfiguration config)
    {
        if (config is null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        _enabled = ConfigurationReader.GetGenerateGoogleSheets(config);

        _applicationName = config["GoogleApi:ApplicationName"];

        _emailAddresses =
            config
                .GetSection("GoogleApi:EmailAddresses")
                .GetChildren()
                .Select(c => c.Value)
                .ToList();

        _credentialFile = config["GoogleApi:CredentialFile"];

        if (!_enabled)
        {
            return;
        }

        if (string.IsNullOrEmpty(_applicationName))
        {
            throw new Exception("ApplicationName is null or empty.");
        }
        
        if (_emailAddresses is null)
        {
            throw new Exception("Email addresses are null.");
        }

        if (!_emailAddresses.Any())
        {
            throw new Exception("Email addresses are empty.");
        }
        
        if (string.IsNullOrEmpty(_credentialFile))
        {
            throw new Exception("Credential file name is null or empty.");
        }
    }

    /// <summary>
    /// Creates a new Google Sheets document with provided <see cref="BettingStrategyResult"/>.
    /// </summary>
    /// <param name="bettingStrategyName">A betting strategy name to use in the file name.</param>
    /// <param name="result">A <see cref="BettingStrategyResult"/> to be saved as a Google Sheets document.</param>
    /// <returns>String Id of the newly created Google Sheets document.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bettingStrategyName"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is null.</exception>
    /// <exception cref="Exception">
    /// Google Document generation is disabled or
    /// No Google Drive document created.
    /// </exception>
    /// <remarks>The document is being created in 2 steps. First in Google Drive and only then edited in Google Sheets.</remarks>
    public async Task<string> CreateReportAsync(string bettingStrategyName, BettingStrategyResult? result)
    {
        if (string.IsNullOrEmpty(bettingStrategyName))
        {
            throw new ArgumentOutOfRangeException(nameof(bettingStrategyName), "bettingStrategyName cannot be null or empty");
        }

        if (result is null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        if (!_enabled)
        {
            throw new Exception("Google Document generation is disabled.");
        }

        // The document must be created in 2 steps. First in Google Drive and only then edited in Google Sheets.
        var file = await CreateGoogleDriveDocumentAsync(bettingStrategyName);

        if (file is null)
        {
            throw new Exception("No Google Drive document created.");
        }

        await PopulateSpreadsheetAsync(file, result);
        
        return file.Id;
    }

    /// <summary>
    /// Creates a new Google Sheets document in Google Drive.
    /// </summary>
    /// <param name="bettingStrategyName">A betting strategy name to use in the file name.</param>
    /// <returns>A newly created <see cref="File"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bettingStrategyName"/> is null or empty.</exception>
    /// <exception cref="Exception">
    /// Failed to create Google Drive service or
    /// Failed to create a new Google Drive document request or
    /// No Google Drive document created.
    /// </exception>
    private async Task<File?> CreateGoogleDriveDocumentAsync(string bettingStrategyName)
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

        if (service is null)
        {
            throw new Exception("Failed to create Google Drive service.");
        }

        // Create a new file with given metadata. This does not include file permissions.
        var metadata = new File
        {
            Name = $"{bettingStrategyName}_{DateUtility.GetTimestamp()}",
            MimeType = "application/vnd.google-apps.spreadsheet"
        };

        var createRequest = service.Files.Create(metadata);

        if (createRequest is null)
        {
            throw new Exception("Failed to create a new Google Drive document request.");
        }

        var file = await createRequest.ExecuteAsync();

        if (file is null)
        {
            throw new Exception("No Google Drive document created.");
        }

        // File permissions must be granted in a separate request per each user.
        var tasks = 
            _emailAddresses
                .Select(async e => await SetPermissionsAsync(service, file.Id, e))
                .ToArray();

        await Task.WhenAll(tasks);

        return file;
    }

    /// <summary>
    /// Sets permissions for a given file in Google Drive.
    /// </summary>
    /// <param name="service">An instance of <see cref="DriveService"/>.</param>
    /// <param name="fileId">An Id of a file in Google Drive.</param>
    /// <param name="email">A user e-mail.</param>
    /// <returns>A result <see cref="Task"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Either <paramref name="fileId"/> or <paramref name="email"/> are null or empty.</exception>
    /// <exception cref="Exception">Failed to create a permissions request.</exception>
    private static async Task SetPermissionsAsync(DriveService? service, string fileId, string email)
    {
        if (service is null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        if (string.IsNullOrEmpty(fileId))
        {
            throw new ArgumentOutOfRangeException(nameof(fileId), "fileId cannot be null or empty");
        }

        if (string.IsNullOrEmpty(email))
        {
            throw new ArgumentOutOfRangeException(nameof(email), "bettingStrategyName cannot be null or empty");
        }

        var permissionRequest =
            service.Permissions.Create(
                new Permission
                {
                    EmailAddress = email,
                    Type = "user",
                    Role = "writer"
                }, fileId);

        if (permissionRequest is null)
        {
            throw new Exception("Failed to create a permissions request.");
        }

        await permissionRequest.ExecuteAsync();
    }

    /// <summary>
    /// Populates an existing Google Sheets document.
    /// </summary>
    /// <param name="file">An existing <see cref="File"/> in Google Drive.</param>
    /// <param name="result">A <see cref="BettingStrategyResult"/> to be saved as a Google Sheets document.</param>
    /// <exception cref="ArgumentNullException">Either <paramref name="file"/> or <paramref name="result"/> are null.</exception>
    /// <exception cref="Exception">
    /// Failed to create Google Sheets service or
    /// Failed to create sheets or
    /// Failed to create get sheets request or
    /// Failed to get a list of sheets or
    /// Failed to create delete sheet request.
    /// </exception>
    private async Task PopulateSpreadsheetAsync(File? file, BettingStrategyResult? result)
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

        if (service is null)
        {
            throw new Exception("Failed to create Google Sheets service.");
        }

        // Create a new sheet for each property of BettingStrategyResult class.
        var batchUpdateRequest = 
            new BatchUpdateSpreadsheetRequest { Requests = new List<Request>() };
        
        batchUpdateRequest.Requests.Add(CreateAddSheetRequest(SuggestionsSheetTitle));
        batchUpdateRequest.Requests.Add(CreateAddSheetRequest(StatisticsSheetTitle));

        var updateRequest = 
            service.Spreadsheets.BatchUpdate(batchUpdateRequest, file.Id);

        if (updateRequest is null)
        {
            throw new Exception("Failed to create sheets.");
        }

        await updateRequest.ExecuteAsync();

        var request = service.Spreadsheets.Get(file.Id);

        if (request is null)
        {
            throw new Exception("Failed to create get sheets request.");
        }

        var response = await request.ExecuteAsync();

        if (request is null)
        {
            throw new Exception("Failed to get a list of sheets.");
        }

        // Remove the default sheet that is always present in a newly created Google Sheets file.
        // Must be done when there are already some sheets in the file -
        // Google Sheets API doesn't allow to have 0 sheets in a Google Sheets file.
        var defaultSheet = 
            response.Sheets.FirstOrDefault(s => s.Properties.Title == DefaultSheetTitle);

        var suggestionSheet =
            response.Sheets.First(s => s.Properties.Title == SuggestionsSheetTitle);

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

            if (updateRequest is null)
            {
                throw new Exception("Failed to create delete sheet request.");
            }

            await updateRequest.ExecuteAsync();
        }

        // Populate BettingStrategyResult data.
        // Betting suggestions first.
        await MakeAppendRequest(
            service,
            new ValueRange { Values = MapBettingSuggestions(result.Suggestions) }, 
            file.Id, 
            $"{SuggestionsSheetTitle}!{DefaultCellRange}");

        // Statistics last.
        await MakeAppendRequest(
            service,
            new ValueRange { Values = MapStatistics(result.Statistics) },
            file.Id,
            $"{StatisticsSheetTitle}!{DefaultCellRange}");

        var chartRequest = new BatchUpdateSpreadsheetRequest
        {
            Requests = new List<Request>()
        };

        chartRequest.Requests.Add(new Request
        {
            AddChart = GetPredictionsRateChartRequest(suggestionSheet)
        });

        chartRequest.Requests.Add(new Request
        {
            AddChart = GetPredictionsRateDistributionChartRequest(suggestionSheet)
        });

        var batchChartRequest = service.Spreadsheets.BatchUpdate(chartRequest, file.Id);
        await batchChartRequest.ExecuteAsync();
    }

    /// <summary>
    /// Creates a prediction rate chart from given <see cref="Sheet"/>.
    /// </summary>
    /// <param name="suggestionSheet">Data source sheet <see cref="Sheet"/>.</param>
    /// <returns>A <see cref="AddChartRequest"/> request.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="suggestionSheet"/> is null.</exception>
    private static AddChartRequest GetPredictionsRateChartRequest(Sheet? suggestionSheet)
    {
        if (suggestionSheet is null)
        {
            throw new ArgumentNullException(nameof(suggestionSheet));
        }
        
        return new AddChartRequest
        {
            Chart = new EmbeddedChart
            {
                Spec = new ChartSpec
                {
                    BasicChart = new BasicChartSpec
                    {
                        Series = new List<BasicChartSeries>(new List<BasicChartSeries>
                        {
                            new ()
                            {
                                Series = new ChartData
                                {
                                    SourceRange = new ChartSourceRange
                                    {
                                        Sources = new List<GridRange>
                                        {
                                            new ()
                                            {
                                                SheetId = suggestionSheet.Properties.SheetId,
                                                StartColumnIndex = 10,
                                                EndColumnIndex = 11,
                                                StartRowIndex = 1
                                            }
                                        }
                                    }
                                }
                            }
                        }),
                        ChartType = "LINE"
                    },
                    Title = "Successful predictions rate"
                },
                Position = new EmbeddedObjectPosition
                {
                    NewSheet = true
                }
            }
        };
    }

    /// <summary>
    /// Creates a prediction rate distribution chart from given <see cref="Sheet"/>.
    /// </summary>
    /// <param name="suggestionSheet">Data source sheet <see cref="Sheet"/>.</param>
    /// <returns>A <see cref="AddChartRequest"/> request.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="suggestionSheet"/> is null.</exception>
    private static AddChartRequest GetPredictionsRateDistributionChartRequest(Sheet? suggestionSheet)
    {
        if (suggestionSheet is null)
        {
            throw new ArgumentNullException(nameof(suggestionSheet));
        }

        return new AddChartRequest
        {
            Chart = new EmbeddedChart
            {
                Spec = new ChartSpec
                {
                    HistogramChart = new HistogramChartSpec
                    {
                        Series = new List<HistogramSeries>
                        {
                            new ()
                            {
                                Data = new ChartData
                                {
                                    SourceRange = new ChartSourceRange
                                    {
                                        Sources = new List<GridRange>
                                        {
                                            new ()
                                            {
                                                SheetId = suggestionSheet.Properties.SheetId,
                                                StartColumnIndex = 10,
                                                EndColumnIndex = 11,
                                                StartRowIndex = 1
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        BucketSize = 0.02
                    },
                    Title = "Successful predictions rate distribution"
                },
                Position = new EmbeddedObjectPosition
                {
                    NewSheet = true
                }
            }
        };
    }

    /// <summary>
    /// Makes a data append request.
    /// </summary>
    /// <param name="service">A <see cref="SheetsService"/> instance.</param>
    /// <param name="body">Data to be inserted as a <see cref="ValueRange"/> instance.</param>
    /// <param name="fileId">Google Sheet document Id.</param>
    /// <param name="range">Insertion point.</param>
    /// <exception cref="ArgumentOutOfRangeException">Either <paramref name="fileId"/> or <paramref name="range"/> are null.</exception>
    /// <exception cref="ArgumentNullException">Either <paramref name="service"/> or <paramref name="body"/> are null.</exception>
    /// <exception cref="Exception">Failed to create append values request.</exception>
    private static async Task MakeAppendRequest(SheetsService service, ValueRange body, string fileId, string range)
    {
        if (service is null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        if (body is null)
        {
            throw new ArgumentNullException(nameof(body));
        }

        if (string.IsNullOrEmpty(fileId))
        {
            throw new ArgumentOutOfRangeException(nameof(fileId), "fileId cannot be null or empty");
        }

        if (string.IsNullOrEmpty(range))
        {
            throw new ArgumentOutOfRangeException(nameof(range), "range cannot be null or empty");
        }

        var request = service.Spreadsheets.Values.Append(body, fileId, range);

        if (request is null)
        {
            throw new Exception("Failed to create append values request.");
        }

        request.InsertDataOption =
            SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
        request.ValueInputOption =
            SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;

        await request.ExecuteAsync();
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
    /// The JSON file must be obtained from https://console.cloud.google.com/.
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
    /// <returns>A new instance of <see cref="Request"/>.</returns>
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
                statistics.TotalNumberOfEvents.ToString(),
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
                .Union(new List<string> { string.Empty, "Aggregated rate of successful predictions" })
                .ToList()
        };

        var total = 1;
        var successful = 0;

        foreach (var s in suggestions.OrderBy(s => s.CommenceTime))
        {
            if (s.RealOutcome is not null && s.ExpectedOutcome == s.RealOutcome)
            {
                successful++;
            }

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
                s.RealOutcome ?? string.Empty,
                string.Empty,
                (s.RealOutcome is not null ? (double)successful / total : null)!
            });

            total++;
        }

        return result;
    }
}