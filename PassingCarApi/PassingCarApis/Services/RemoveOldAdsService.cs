using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PassingCarApis.Configuration;
using PassingCarApis.Extensions;

namespace PassingCarApis.Services
{
    public class RemoveOldAdsService : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private readonly ILogger<RemoveOldAdsService> _logger;
        private Timer? _timer = null;

        public RemoveOldAdsService(ILogger<RemoveOldAdsService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            // Delay first execution by 5 minutes to avoid startup delays
            _timer = new Timer(DoWork, null, TimeSpan.FromMinutes(5),
                TimeSpan.FromDays(1));

            return Task.CompletedTask;
        }

        private async void DoWork(object? state)
        {
            int count = Interlocked.Increment(ref executionCount);
            try
            {
                //$"START AGENT {DateTime.Now:HH:mm dd.MM.yyyy}".AddToLog();
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    //Remove photos after 1 week
                    string query = $@" Update Ads Set
                                Photo1 = NULL,
                                Photo2 = NULL,
                                Photo3 = NULL,
                                Photo4 = NULL,
                                ModifiedAt = GETDATE()
                                WHERE DATEDIFF(week, ModifiedAt, GetDate()) >= 1
                                AND State >= 5";
                    _ = await connection.QueryAsync(query);

                    //Remove ads after 5 years
                    query = $@" DECLARE @AdsIdents table (Id int);
                                INSERT INTO @AdsIdents
	                                Select Id from Ads
							    WHERE DATEDIFF(YEAR, CreatedAt, GetDate()) >= 5;

                                DECLARE @ChatIdents table (Id int)
                                INSERT INTO @ChatIdents
	                                Select Id from Chat 
	                                where AdId in (Select * from @AdsIdents);
	
                                Delete from ChatMessage
                                where ChatId in (Select * from @ChatIdents);

                                Delete from Chat
                                where Id in (Select * from @ChatIdents);

                                Delete from FavoriteAds
                                where AdId in (Select * from @AdsIdents);


                                DECLARE @OfferIdents table (Id int);
                                INSERT INTO @OfferIdents
	                                Select Id from Offer 
	                                where AdId in (Select * from @AdsIdents);


                                Delete from Payment
                                where OfferId in (Select * from @OfferIdents);

                                Delete from Shipping
                                where OfferId in (Select * from @OfferIdents);

                                Delete from Offer
                                where Id in (Select * from @OfferIdents);


                                Delete from Ads
                                where Id in (Select * from @AdsIdents);";
                    _ = await connection.QueryAsync(query);
                }
                catch (Exception ex)
                {
                    $"AGENT EXCEPTION {JsonConvert.SerializeObject(ex)} {DateTime.Now:HH:mm dd.MM.yyyy}".AddToLog();
                }
                finally
                {
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                $"AGENT EXCEPTION {JsonConvert.SerializeObject(ex)} {DateTime.Now:HH:mm dd.MM.yyyy}".AddToLog();
            }

            _logger.LogInformation(
                "Timed Hosted Service is working. Count: {Count}", count);
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _ = (_timer?.Change(Timeout.Infinite, 0));

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
