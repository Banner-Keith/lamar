using System;
using Lamar;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

#region sample_using_lamar_with_open_api

var builder = WebApplication.CreateBuilder(args);

// use Lamar as DI.
builder.Host.UseLamar((context, registry) =>
{
    // register services using Lamar. Only inject ITest as a keyed service

    registry.AddKeyedSingleton<ITest, MyTest>(OpenApiTestConstants.TestServiceKey1);
    registry.AddKeyedSingleton<ITest, MyOtherTest>(OpenApiTestConstants.TestServiceKey2);
    registry.AddKeyedSingleton<ITest, MyTypedTest>(OpenApiTestConstants.TestServiceKey3);

    // Add your own Lamar ServiceRegistry collections
    // of registrations
    registry.IncludeRegistry<MyRegistry>();

    registry.AddControllers();

    registry.AddOpenApi();
});


var app = builder.Build();

// Did not work in Lamar 14
app.MapOpenApi();

app.MapControllers();

// Add Minimal API routes
app.MapGet("/", ([FromKeyedServices(OpenApiTestConstants.TestServiceKey1)]ITest service) => service.SayHello());

app.Run();

#endregion

public static class OpenApiTestConstants
{
    public const string TestServiceKey1 = "ExampleMyTest";
    public const string TestServiceKey2 = "ExampleOtherTest";
    public const string TestServiceKey3 = "TypedTest";
}

[Route("api/[controller]")]
[ApiController]
public class HelloController : ControllerBase
{
    private readonly ITest _test;
    private readonly ITestTime _testTime;

    public HelloController([FromKeyedServices(OpenApiTestConstants.TestServiceKey1)]ITest test, ITestTime testTime)
    {
        _test = test;
        _testTime = testTime;
    }

    [HttpGet]
    public IActionResult Get()
    => this.Ok($"{_test.SayHello()}@{_testTime.GetTime()}");
}

public interface ITest
{
    string SayHello();
}

public interface ITestTime
{
    DateTime GetTime();
}

public class TestTime : ITestTime
{
    public DateTime GetTime() => DateTime.Now;
}

public class UnkeyedTest : ITest
{
    public string SayHello() => "Hi there";
}

public class MyTest : ITest
{
    public string SayHello() => "Hi there";
}

public class MyOtherTest : ITest
{
    public string SayHello() => "Hi there again";
}

public class MyTypedTest : ITest
{
    private readonly string _serviceKey;

    public MyTypedTest([ServiceKey]string serviceKey)
    {
        _serviceKey = serviceKey;
    }

    public string SayHello() => $"Hi there from typed service {_serviceKey}";
}

public class ExampleTypeUsingGenericITest
{
    private readonly ITest _testType;

    public ExampleTypeUsingGenericITest(ITest testType)
    {
        _testType = testType;
    }
}

public class ExampleTypeUsingTypedITest
{
    private readonly ITest _testType;

    public ExampleTypeUsingTypedITest([FromKeyedServices(OpenApiTestConstants.TestServiceKey1)] ITest testType)
    {
        _testType = testType;
    }
}

public class MyRegistry : ServiceRegistry
{
    public MyRegistry()
    {
        Scan(s =>
        {
            s.TheCallingAssembly();
            s.WithDefaultConventions();
        });
    }
}