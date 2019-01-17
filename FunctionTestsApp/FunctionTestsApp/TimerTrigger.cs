using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;


namespace FunctionTestsApp
{

    public static class TimerTrigger
    {
        [FunctionName("TimerTrigger")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {SystemTime.Now()}");
        }
    }
}
