using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

using Amazon.S3;

using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Commands.Commands;

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using RichardSzalay.MockHttp;

#pragma warning disable EF1001

internal class AutoAttribute : AutoDataAttribute
{
    public AutoAttribute()
        : base(Create)
    {
    }

    public static IFixture Create()
    {
        var fixture = new Fixture();
        fixture.Inject(new CancellationToken(false));
        var messageHandler = new MockHttpMessageHandler();
        fixture.Inject(messageHandler);
        fixture.Inject(new MemoryStream());
        fixture.Inject(Substitute.For<Assembly>());
        fixture.Inject<IServiceCollection>(new ServiceCollection());
        fixture.Inject<ICommandCache>(new DefaultCommandCache());
        fixture.Inject(new System.Net.Http.HttpClient(messageHandler));
        fixture.Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
        fixture.Customizations.Add(new OptionsRelay());
        fixture.Customizations.Add(new TypeOmitter<IDictionary<string, JsonElement>>());
        fixture.Customizations.Add(new TypeOmitter<JsonConverter>());
        fixture.Customizations.Add(new TypeOmitter<MemoryStream>());
        fixture.Customizations.Add(new TypeOmitter<RequestCharged>());
        fixture.Customizations.Add(new TypeOmitter<ISingletonOptionsInitializer>());
        fixture.Customizations.Insert(-1, new TargetRelay());
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}
