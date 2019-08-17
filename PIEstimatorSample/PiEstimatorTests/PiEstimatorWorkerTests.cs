using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiEstimatorTests
{
    using DNVGL.One.Compute.Core.FlowModel;
    using DNVGL.One.Compute.Core.ServiceContracts;
    using DNVGL.One.Compute.Core.Worker;

    using NUnit.Framework;

    using PiEstimatorCommon;

    using PiEstimatorWorker;

    [TestFixture]
    public class PiEstimatorWorkerTests
    {
        [Test]
        [TestCase(10000)]
        public async Task Reduction(long numberOfSamples)
        {
            var statusNotificationService = NSubstitute.Substitute.For<IWorkerExecutionStatusNotificationService>();

            var input1 = new PiEstimateInput { NumberOfSamples = numberOfSamples };
            var input2 = new PiEstimateInput { NumberOfSamples = numberOfSamples };
            var wu1 = new WorkUnit(input1);
            var wu2 = new WorkUnit(input2);

            var worker = new Worker()
            {
                Logger = NSubstitute.Substitute.For<ILogger>()
            };

            var r1 = await worker.ExecuteAsync(statusNotificationService, wu1, null) as PiEstimateIntermediateResult;
            Assert.IsNotNull(r1);
            Console.WriteLine($"Intermediate result 1: C = {r1.NumberWithinUnitCircle}\tN={r1.NumberOfSamples}");
            Assert.AreEqual(numberOfSamples, r1.NumberOfSamples);

            var r2 = await worker.ExecuteAsync(statusNotificationService, wu2, null) as PiEstimateIntermediateResult;
            Assert.IsNotNull(r2);
            Console.WriteLine($"Intermediate result 2: C = {r1.NumberWithinUnitCircle}\tN={r2.NumberOfSamples}");
            Assert.AreEqual(numberOfSamples, r2.NumberOfSamples);
            var dependencyResults = new[]
                                        {
                                            new Result(r1),
                                            new Result(r2),
                                        };
            var finalResult = await worker.ReduceAsync(
                                  statusNotificationService,
                                  new WorkUnit(),
                                  dependencyResults) as PiEstimateFinalResult;
            Assert.IsNotNull(finalResult);
            Console.WriteLine($"Final result: PI = {finalResult.PI}");
        }
    }
}
