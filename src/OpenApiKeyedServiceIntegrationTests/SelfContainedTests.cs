using Microsoft.Extensions.Hosting;
using System;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace OpenApiKeyedServiceIntegrationTests
{
    public class SelfContainedTests
    {
        [Theory]
        [InlineData(OpenApiTestConstants.TestServiceKey1, typeof(MyTest))]
        [InlineData(OpenApiTestConstants.TestServiceKey2, typeof(MyOtherTest))]
        [InlineData(OpenApiTestConstants.TestServiceKey3, typeof(MyTypedTest))]
        public void inject_keyed_services(string key, Type type)
        {
            IHostBuilder? builder = new HostBuilder()
                .UseLamar((_, registry) =>
                {
                    registry.AddSingleton<ITest, UnkeyedTest>();
                    registry.AddKeyedSingleton<ITest, MyTest>(OpenApiTestConstants.TestServiceKey1);
                    registry.AddKeyedSingleton<ITest, MyOtherTest>(OpenApiTestConstants.TestServiceKey2);
                    registry.AddKeyedSingleton<ITest, MyTypedTest>(OpenApiTestConstants.TestServiceKey3);
                });

            using (IHost host = builder.Start())
            {
                var requiredService = host.Services.GetKeyedService<ITest>(key);

                requiredService.ShouldNotBeNull();

                requiredService.ShouldBeOfType(type);
            }
        }

        [Fact]
        public void inject_keyed_services_in_type()
        {
            IHostBuilder? builder = new HostBuilder()
                .UseLamar((_, registry) =>
                {
                    registry.AddSingleton<ITest, UnkeyedTest>();
                    registry.AddKeyedSingleton<ITest, MyTest>(OpenApiTestConstants.TestServiceKey1);
                    registry.AddKeyedSingleton<ITest, MyOtherTest>(OpenApiTestConstants.TestServiceKey2);
                    registry.AddKeyedSingleton<ITest, MyTypedTest>(OpenApiTestConstants.TestServiceKey3);
                });

            using (IHost host = builder.Start())
            {
                var requiredService = host.Services.GetService<ExampleTypeUsingTypedITest>();

                requiredService.ShouldNotBeNull();
            }
        }

        [Fact]
        public void fail_on_inject_generic_services()
        {
            IHostBuilder? builder = new HostBuilder()
                .UseLamar((_, registry) =>
                {
                    registry.AddKeyedSingleton<ITest, MyTest>(OpenApiTestConstants.TestServiceKey1);
                    registry.AddKeyedSingleton<ITest, MyOtherTest>(OpenApiTestConstants.TestServiceKey2);
                    registry.AddKeyedSingleton<ITest, MyTypedTest>(OpenApiTestConstants.TestServiceKey3);
                });

            using (IHost host = builder.Start())
            {
                var requiredService = host.Services.GetService<ITest>();

                requiredService.ShouldBeNull();
            }
        }

        [Fact]
        public void fail_on_inject_generic_services_in_type()
        {
            IHostBuilder? builder = new HostBuilder()
                .UseLamar((_, registry) =>
                {
                    registry.AddKeyedSingleton<ITest, MyTest>(OpenApiTestConstants.TestServiceKey1);
                    registry.AddKeyedSingleton<ITest, MyOtherTest>(OpenApiTestConstants.TestServiceKey2);
                    registry.AddKeyedSingleton<ITest, MyTypedTest>(OpenApiTestConstants.TestServiceKey3);
                });

            using (IHost host = builder.Start())
            {
                var requiredService = host.Services.GetService<ExampleTypeUsingGenericITest>();

                requiredService.ShouldBeNull();
            }
        }

        [Fact]
        public void allow_unkeyed_service()
        {
            IHostBuilder? builder = new HostBuilder()
                .UseLamar((_, registry) =>
                {
                    registry.AddSingleton<ITest, UnkeyedTest>();
                    registry.AddKeyedSingleton<ITest, MyTest>(OpenApiTestConstants.TestServiceKey1);
                    registry.AddKeyedSingleton<ITest, MyOtherTest>(OpenApiTestConstants.TestServiceKey2);
                    registry.AddKeyedSingleton<ITest, MyTypedTest>(OpenApiTestConstants.TestServiceKey3);
                });

            using (IHost host = builder.Start())
            {
                // Not requesting a keyed service should get right type even though it is not the last registration
                host.Services.GetService<ITest>().ShouldNotBeNull().ShouldBeOfType<UnkeyedTest>();
            }
        }

        [Fact]
        public void allow_fallback_to_single_keyed_service()
        {
            IHostBuilder? builder = new HostBuilder()
                .UseLamar((_, registry) =>
                {
                    registry.AddKeyedSingleton<ITest, MyTest>(OpenApiTestConstants.TestServiceKey1);
                });

            using (IHost host = builder.Start())
            {
                // Not requesting a keyed service but since there is only one it is safe to return
                host.Services.GetService<ITest>().ShouldNotBeNull().ShouldBeOfType<MyTest>();
            }
        }

        [Fact]
        public void get_last_unkeyed_service()
        {
            IHostBuilder? builder = new HostBuilder()
                .UseLamar((_, registry) =>
                {
                    registry.AddSingleton<ITest, UnkeyedTest>();
                    registry.AddSingleton<ITest, MyTest>();
                });

            using (IHost host = builder.Start())
            {
                // When more than one unkeyed service has been injected, get the last one. Preserves existing behavior.
                host.Services.GetService<ITest>().ShouldNotBeNull().ShouldBeOfType<MyTest>();
            }
        }
    }
}
