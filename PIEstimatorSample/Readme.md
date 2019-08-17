# PiEstimator Sample

This is a complete end-to-end sample using the OneCompute Platform.

# What it does

It estimates Pi by randomly picking numbers (x,y) within a square with -1 <= x,y <= 1 and estimating Pi from the fraction of points that are within the circle inscribed int the square.

# How to build and run

Clone the OneComputeExamples repository.
Open the PiEstimatorSample.sln solution in Visual Studio 2017 or 2019 and build it.
Run one of the client console applications.

# Client code

## PiSampler

Shared client code that creates and submits the job for execution and monitors it during execution.

## PiEstimatorClient

A .NET Framework console application that authenticates with Veracity and runs the PiSampler.

## PiEstimatorClient.ClientCredentials

A .NET Core console application that authenticates with the OneCompute Service Registry using client credentials and runs the PiSampler.
To run this application, a client secret must be obtained from the OneCompute Service Registry.

# Cloud Worker

## PiEstimatorWorker

Contains the code that samples Pi and returns the results.

# Common code

## PiEstiatorCommon

Common code shared between clients and cloud worker, in particular the data types representing input to and output from the calculations.
