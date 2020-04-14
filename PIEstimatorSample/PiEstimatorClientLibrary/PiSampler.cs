namespace PiEstimatorClientLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using DNVGL.One.Compute.Core.FlowModel;
    using DNVGL.One.Compute.Core.Scheduling;
    using DNVGL.One.Compute.Platform.Client;

    using PiEstimatorCommon;

    public static class PiSampler
    {
        const long NumSamplesPerWorkUnit = 10_000_000;
        const long NumWorkUnits = 100;
        const string ApiUrl = "https://develop.onecompute.dnvgl.com/westeurope/api/api";

        public static async Task Run(string accessToken)
        {
            var oneComputePlatformClient = new OneComputePlatformClient(ApiUrl, accessToken);

            // 2. Create Job
            Console.Write("Creating the job....");
            var workItems = new List<WorkItem>();
            for (var parallelTask = 0; parallelTask < NumWorkUnits; parallelTask++)
            {
                workItems.Add(new WorkUnit(new PiEstimateInput()
                {
                    NumberOfSamples = NumSamplesPerWorkUnit
                }));
            }

            var reductionWorkUnit = new WorkUnit();

            var work = new ParallelWork
            {
                WorkItems = workItems,
                ReductionTask = reductionWorkUnit
            };

            var job = new Job
            {
                ServiceName = "PiCalc",
                Work = work,
                PoolId = "OneComputePlatformDemoPool"
            };
            Console.WriteLine("Done");

            // When running on a pool that does not have PiCalc pre-deployed, a DeploymentModel must be set that specifies that the PiCalc application
            // must be deployed for the job. In that case, uncomment the next 3 lines.
            var deploymentModel = new DeploymentModel();
            deploymentModel.AddApplicationPackage("PiCalc");
            job.DeploymentModel = deploymentModel;

            // 3. Submit job
            // Submit the job and return the monitor
            Console.Write("Submitting the job....");
            var monitor = await oneComputePlatformClient.SubmitJobAsync(job);
            Console.WriteLine("Done");

            // 4. Monitor Job
            // Set up callbacks on the job monitor to handle status and progress events.
            monitor.JobStatusChanged += async (s, e) => await JobStatusChanged(s, e);
            monitor.JobProgressChanged += JobProgressChanged;

            Console.WriteLine("Monitoring the job.....");
            await monitor.AwaitTerminationAsync(job.JobId);

            // Local callback handler for job status changed events
            // ReSharper disable StyleCop.SA1126
            async Task JobStatusChanged(object sender, JobEventArgs jobEvent)
            {
                try
                {
                    var jobStatusFlag = jobEvent.WorkStatus;
                    var message = jobEvent.Message;
                    switch (jobStatusFlag)
                    {
                        case WorkStatus.Faulted:
                            message = $"Cloud job {job.JobId} faulted. Details: {message}";
                            Console.WriteLine($"{message}{Environment.NewLine}");
                            break;
                        case WorkStatus.Aborted:
                            Console.WriteLine($"Aborted{Environment.NewLine}");
                            break;
                        case WorkStatus.Completed:
                            Console.WriteLine($"JobStatusChanged - Completed!{Environment.NewLine}");
                            Console.WriteLine($"Retrieving results...{Environment.NewLine}");

                            // 5. Results retrieval
                            var finalResultItem = await oneComputePlatformClient.GetWorkItemResultAsync(job.JobId, reductionWorkUnit.Id);
                            if (finalResultItem != null)
                            {
                                var finalResult = finalResultItem.GetResult<PiEstimateFinalResult>();
                                Console.WriteLine("FINAL RESULT");
                                Console.WriteLine($"PI = {finalResult.PI}");
                                Console.WriteLine($"Std. dev. = {finalResult.StandardDeviation:E1}");
                                Console.WriteLine($"Number of samples = {finalResult.TotalNumberOfSamples}");
                                Console.WriteLine($"Number within unit circle = {finalResult.TotalNumberWithinUnitCircle}");
                            }

                            break;
                        default:
                            return;
                    }

                    // The job has now terminated - successfully or otherwise.  Obtain the status record and write out the compute duration
                    var jobStatusInfo = oneComputePlatformClient.GetJobStatusAsync(job.JobId).GetAwaiter().GetResult();
                    if (jobStatusInfo != null)
                    {
                        Console.WriteLine($"Job completed at {jobStatusInfo.CompletionTime}{Environment.NewLine}");
                        Console.WriteLine($"Job total compute time {jobStatusInfo.TotalComputeSeconds} seconds{Environment.NewLine}");
                        Console.WriteLine("Press any key to exit...");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + ex.StackTrace);
                }
            }

            // Local callback handler for job progress update events
            void JobProgressChanged(object sender, JobEventArgs jobEvent)
            {
                var currentProgress = jobEvent.Progress * 100;
                var message = jobEvent.Message;

                Console.WriteLine($"JobProgressChanged => currentProgress={currentProgress}   {message}");
            }

            // And wait for response
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
