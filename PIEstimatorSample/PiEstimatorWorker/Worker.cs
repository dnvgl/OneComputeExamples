using System;

namespace PiEstimatorWorker
{
    using System.Collections.Generic;
    using System.Composition;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using DNVGL.One.Compute.Core.FlowModel;
    using DNVGL.One.Compute.Core.Scheduling;
    using DNVGL.One.Compute.Core.Worker;

    using PiEstimatorCommon;

    [Export(typeof(IWorker))]
    public class Worker : WorkerBase, ISupportProgress, ISupportReduction
    {
        [ImportingConstructor]
        public Worker()
        {
            
        }

        /// <inheritdoc />
        public override async Task<object> ExecuteAsync(IWorkerExecutionStatusNotificationService workerExecutionStatusNotificationService, IWorkUnit workUnit, IEnumerable<Result> dependencyResults)
        {
            await this.GetLogger().LogInformationAsync("PiWorker Execute", Assembly.GetExecutingAssembly().GetName().Version.ToString());

            var seed = workUnit.GetHashCode();
            await this.GetLogger().LogInformationAsync("PiWorker Execute", $"Random seed = {seed}");
            var random = new Random(seed);

            var inputData = workUnit.GetInput<PiEstimateInput>();
            var numberOfSamples = inputData.NumberOfSamples;

            await this.GetLogger().LogInformationAsync("PiWorker Execute", $"Input requesting NumberOfSamples={numberOfSamples}");

            var numberWithinUnitCircle = 0;
            for (var iteration = 0; iteration < numberOfSamples; iteration++)
            {
                var x = (random.NextDouble() * 2.0) - 1.0;
                var y = (random.NextDouble() * 2.0) - 1.0;
                var bucket = Math.Pow(x, 2.0) + Math.Pow(y, 2.0);
                if (bucket <= 1.0)
                {
                    numberWithinUnitCircle++;
                }

                var progress = (double)iteration / numberOfSamples;
                workerExecutionStatusNotificationService?.AddWorkItemStatus(WorkStatus.Executing, progress, $"progress update {(int)progress * 100}%");
            }

            var result = new PiEstimateIntermediateResult
                             {
                                 NumberOfSamples = numberOfSamples,
                                 NumberWithinUnitCircle = numberWithinUnitCircle
                             };

            await this.GetLogger().LogInformationAsync("PiWorker Execute", $"Intermediate result is NumberWithinUnitCircle={numberWithinUnitCircle}, NumberOfSamples={numberOfSamples}");

            return result;
        }

        public async Task<object> ReduceAsync(IWorkerExecutionStatusNotificationService workerExecutionStatusNotificationService, IWorkUnit workUnit, IEnumerable<IResult> dependencyResults)
        {
            try
            {
                await this.GetLogger().LogInformationAsync("PiWorker Reduce", Assembly.GetExecutingAssembly().GetName().Version.ToString());

                var listResults = dependencyResults
                    .Select(r => r.GetResult<PiEstimateIntermediateResult>())
                    .Where(r => r != null).ToList();
               
                var totalNumberOfSamples = listResults.Sum(r => r.NumberOfSamples);
                var totalNumberWithinUnitCircle = listResults.Sum(r => r.NumberWithinUnitCircle);
                
                var pi = 4.0 * totalNumberWithinUnitCircle / totalNumberOfSamples;

                // Estimate error
                const double Z = 1.96; // 95% confidence
                var p = pi / 4;
                var standardDeviation = Z * Math.Sqrt(p * (1 - p) / totalNumberOfSamples);

                var result = new PiEstimateFinalResult
                                 {
                                     PI = pi,
                                     TotalNumberOfSamples = totalNumberOfSamples,
                                     TotalNumberWithinUnitCircle = totalNumberWithinUnitCircle,
                                     StandardDeviation = standardDeviation
                                 };

                await this.GetLogger().LogInformationAsync("PiWorker Reduce", $"Result is Pi={pi}, Std. dev={standardDeviation:E1}, TotalNumberWithinUnitCircle={totalNumberWithinUnitCircle}, TotalNumberOfSamples={totalNumberOfSamples}");

                return result;
            }
            catch (Exception ex)
            {
                await this.GetLogger().LogErrorAsync("PiWorker Reduce", ex.Message + ex.StackTrace);
                throw;
            }
        }
    }
}
