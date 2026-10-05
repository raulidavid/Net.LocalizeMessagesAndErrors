// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using LocalizeMessagesAndErrors;
using Microsoft.Data.Sqlite;
using System;
using System.IO;
using System.Linq;
using Test.StubClasses;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests;

[Collection(LocalizationDatabaseTestCollection.Name)]
public class TestStubLocalizeDefaultWithLogging
{
    private readonly ITestOutputHelper _output;

    public TestStubLocalizeDefaultWithLogging(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void TestStubLocalizeDefaultWithLogging_Logs()
    {
        //SETUP
        var stubLocalizer = new StubDefaultLocalizerWithLogging
            ("en", GetType());

        //ATTEMPT
        stubLocalizer.LocalizeStringMessage(
            "Test1".MethodLocalizeKey(this),
            "Hello Earth");
        stubLocalizer.LocalizeStringMessage(
            "Test2".MethodLocalizeKey(this),
            "Hello Mars");

        //VERIFY
        stubLocalizer.Logs.Count.ShouldEqual(2);
        stubLocalizer.Logs[0].ActualMessage.ShouldEqual("Hello Earth");
        stubLocalizer.Logs[0].LocalizeKey.ShouldEqual(
            "TestStubLocalizeDefaultWithLogging_Logs_Test1");
        stubLocalizer.Logs[1].ActualMessage.ShouldEqual("Hello Mars");
        stubLocalizer.Logs[1].LocalizeKey.ShouldEqual(
            "TestStubLocalizeDefaultWithLogging_Logs_Test2");
    }

    [Fact]
    public void TestAddErrorString()
    {
        //SETUP
        var stubLocalizer = new StubDefaultLocalizerWithLogging<TestStubLocalizeDefaultWithLogging>("en");

        //ATTEMPT
        var status = new StatusGenericLocalizer(stubLocalizer);
        status.AddErrorString("test".MethodLocalizeKey(this), "An Error");

        //VERIFY
        status.Errors.Single().ToString().ShouldEqual("An Error");
        stubLocalizer.PossibleError.ShouldBeNull();
    }

    [Fact]
    public void TestSetMessageString()
    {
        //SETUP
        var stubLocalizer = new StubDefaultLocalizerWithLogging<TestStubLocalizeDefaultWithLogging>("en");

        //ATTEMPT
        var status = new StatusGenericLocalizer(stubLocalizer);
        status.SetMessageString("test".MethodLocalizeKey(this), "Status Message1");

        //VERIFY
        status.Message.ShouldEqual("Status Message1");
        stubLocalizer.PossibleError.ShouldBeNull();
    }


    [Fact]
    public void TestSetMessageFormatted()
    {
        //SETUP
        var stubLocalizer = new StubDefaultLocalizerWithLogging<TestStubLocalizeDefaultWithLogging>("en");

        //ATTEMPT
        var status = new StatusGenericLocalizer(stubLocalizer);
        status.SetMessageFormatted("test".MethodLocalizeKey(this), $"Status Message{2}");

        //VERIFY
        status.Message.ShouldEqual("Status Message2");
        stubLocalizer.PossibleError.ShouldBeNull();
    }

    [Fact]
    public void TestSetMessage_SameKeyButDiffFormat()
    {
        //SETUP
        var stubLocalizer = new StubDefaultLocalizerWithLogging<TestStubLocalizeDefaultWithLogging>("en");

        //ATTEMPT
        var status = new StatusGenericLocalizer(stubLocalizer);
        status.AddErrorString("test".MethodLocalizeKey(this), "First Error message");
        status.AddErrorString("test".MethodLocalizeKey(this), "Second Error message");

        //VERIFY
        _output.WriteLine(stubLocalizer.PossibleError ?? "- no error -");
        stubLocalizer.PossibleError.ShouldNotBeNull();
    }

    [Fact]
    public void LocalizationCaptureDatabase_UsesPortableSqlite()
    {
        var previousConnectionString = Environment.GetEnvironmentVariable("LOCALIZATION_CAPTURE_CONNECTION_STRING");
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), $"localization-capture-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temporaryDirectory);
        var databasePath = Path.Combine(temporaryDirectory, "capture.db");
        Environment.SetEnvironmentVariable("LOCALIZATION_CAPTURE_CONNECTION_STRING", $"Data Source={databasePath}");

        try
        {
            var stubLocalizer = new StubDefaultLocalizerWithLogging("en", GetType());
            using (var context = stubLocalizer.GetLocalizationCaptureDbInstance(true)!)
            {
                context.LocalizedData!.Add(new LocalizedLog(
                    GetType(), "portable-capture", "en", "Captured message", null,
                    nameof(TestStubLocalizeDefaultWithLogging), nameof(LocalizationCaptureDatabase_UsesPortableSqlite), 1));
                context.SaveChanges();
            }

            var capturedLogs = stubLocalizer.ListLocalizationCaptureDb();
            capturedLogs.Single().LocalizeKey.ShouldEqual("portable-capture");
        }
        finally
        {
            Environment.SetEnvironmentVariable("LOCALIZATION_CAPTURE_CONNECTION_STRING", previousConnectionString);
            SqliteConnection.ClearAllPools();
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }
}
