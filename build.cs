// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the Apache license.

#:package WeihanLi.Common

using WeihanLi.Common.Helpers;

var solutionPath = "./Spark.OpenXml.slnx";
string[] srcProjects = [ 
    "./src/Spark.OpenXml/Spark.OpenXml.csproj"
];
string[] testProjects = [ 
    "./test/Spark.OpenXml.Test/Spark.OpenXml.Test.csproj"
];
string[] runFileSamplesFolders = [
    "./samples/run-file-samples"
];

await DotNetPackageBuildProcess
    .Create(options => 
    {
        options.SolutionPath = solutionPath;
        options.SrcProjects = srcProjects;
        options.TestProjects = testProjects;
        options.RunFileSampleFolders = runFileSamplesFolders;
    })
    .ExecuteAsync(args);
