namespace OddsCollector.Jobs
{
    using Csv;
    using DAL;
    using Prediction;
    using Quartz;

    [DisallowConcurrentExecution]
    public class CsvGeneratorJob : IJob
    {
        private readonly ILogger<CsvGeneratorJob> _logger;
        private readonly IConfiguration _config;
        private readonly IDatabaseAdapter _databaseAdapter;
        private readonly IPredictor _predictor;
        private readonly ICsvSaver _saver;

        public CsvGeneratorJob(
            ILogger<CsvGeneratorJob> logger, IConfiguration config, IDatabaseAdapter databaseAdapter, IPredictor predictor, ICsvSaver saver)
        {
            _logger = logger;
            _config = config;
            _databaseAdapter = databaseAdapter;
            _predictor = predictor;
            _saver = saver;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Generating CSV-s.");

            try
            {
                var events = _databaseAdapter.GetEventsWithLatestOdds();
                var predictions = _predictor.GetPredictions(events).ToList();
                var statistics = _predictor.GetStatistics(predictions);

                var csvPath = _config.GetValue<string>("CsvOutputPath");

                _saver.WritePredictions(csvPath, predictions);
                _saver.WriteStatistics(csvPath, statistics);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to generate CSV-s.");
            }

            return Task.CompletedTask;
        }
    }
}
