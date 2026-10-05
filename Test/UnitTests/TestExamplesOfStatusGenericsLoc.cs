// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using LocalizedWebApp.Controllers;
using ServiceLayer;
using Test.StubClasses;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests;

[Collection(LocalizationDatabaseTestCollection.Name)]
public class TestExamplesOfStatusGenericsLoc : IDisposable
{
    private readonly CultureInfo _originalCulture = Thread.CurrentThread.CurrentCulture;
    private readonly CultureInfo _originalUiCulture = Thread.CurrentThread.CurrentUICulture;

    public TestExamplesOfStatusGenericsLoc()
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-GB");
    }

    public void Dispose()
    {
        Thread.CurrentThread.CurrentCulture = _originalCulture;
        Thread.CurrentThread.CurrentUICulture = _originalUiCulture;
    }

    [Fact]
    public void TestCheckNull_Success()
    {
        //SETUP
        var stubDefaultLoc = new StubDefaultLocalizerWithLogging<HomeController>("en");

        var service = new ExamplesOfStatusGenericsLoc<HomeController>(stubDefaultLoc);

        //ATTEMPT
        var status = service.CheckNull("123");

        //VERIFY
        status.IsValid.ShouldBeTrue(status.GetAllErrors());
        status.Message.ShouldEqual("Successful completion.");
    }

    [Fact]
    public void TestCheckNull_Errors()
    {
        //SETUP
        var stubDefaultLoc = new StubDefaultLocalizerWithLogging<HomeController>("en");

        var service = new ExamplesOfStatusGenericsLoc<HomeController>(stubDefaultLoc);

        //ATTEMPT
        var status = service.CheckNull(null);

        //VERIFY
        status.IsValid.ShouldBeFalse();
        status.Errors.Single().ErrorResult.ErrorMessage.ShouldEqual("The input must not be null.");
        status.Errors.Single().ErrorResult.MemberNames.Single().ShouldEqual("Month");
        status.Message.ShouldEqual("Failed with 1 error.");
    }

    [Fact]
    public void TestStatusGenericWithResult_Success()
    {
        //SETUP
        var stubDefaultLoc = new StubDefaultLocalizerWithLogging<HomeController>("en");

        var service = new ExamplesOfStatusGenericsLoc<HomeController>(stubDefaultLoc);

        //ATTEMPT
        var status = service.StatusGenericWithResult(123);

        //VERIFY
        status.IsValid.ShouldBeTrue(status.GetAllErrors());
        status.Result.ShouldEqual("123");
        status.Message.ShouldEqual("Success");
    }

    [Fact]
    public void TestStatusGenericWithResult_Errors()
    {
        //SETUP
        var stubDefaultLoc = new StubDefaultLocalizerWithLogging<HomeController>("en");

        var service = new ExamplesOfStatusGenericsLoc<HomeController>(stubDefaultLoc);

        //ATTEMPT
        var status = service.StatusGenericWithResult(-1);

        //VERIFY
        status.IsValid.ShouldBeFalse();
        status.Errors.Single().ErrorResult.ErrorMessage.ShouldEqual("The property should not be negative.");
        status.Errors.Single().ErrorResult.MemberNames.Single().ShouldEqual("Year");
        status.Result.ShouldEqual(default);
    }

    [Fact]
    public void TestStatusGenericCombineStatuses_Success()
    {
        //SETUP
        var stubDefaultLoc = new StubDefaultLocalizerWithLogging<HomeController>("en");

        var service = new ExamplesOfStatusGenericsLoc<HomeController>(stubDefaultLoc);

        //ATTEMPT
        var status = service.CreateDate(1, "april", 2000);

        //VERIFY
        status.IsValid.ShouldBeTrue(status.GetAllErrors());
        status.Result.ShouldEqual(new DateTime(2000,4,1));
        var expectedDateMessage = new DateTime(2000, 4, 1).ToString("D", CultureInfo.CurrentCulture);
        status.Message.ShouldEqual($"Successfully created the date {expectedDateMessage}.");
    }

    [Theory]
    [InlineData(null, 1, 2000, "The input must not be null.")]
    [InlineData("april", 1, -1, "The property should not be negative.")]
    [InlineData("april", 99, 2000, "The day 99, month april, year 2000 doesn't turn into a valid date.")]
    public void TestStatusGenericCombineStatuses_Errors(string? month, int date, int year, string expectedErrorMessages)
    {
        //SETUP
        var stubDefaultLoc = new StubDefaultLocalizerWithLogging<HomeController>("en");

        var service = new ExamplesOfStatusGenericsLoc<HomeController>(stubDefaultLoc);

        //ATTEMPT
        var status = service.CreateDate(date, month, year);

        //VERIFY
        status.IsValid.ShouldBeFalse();
        status.Errors.Single().ErrorResult.ErrorMessage.ShouldEqual(expectedErrorMessages);
        status.Result.ShouldEqual(default);
    }
}
