using Amazon.CDK;

using Brighid.Commands.Artifacts;

#pragma warning disable SA1516

var app = new App();
_ = new ArtifactsStack(app, "brighid-commands-cicd", new StackProps
{
    Synthesizer = new BootstraplessSynthesizer(new BootstraplessSynthesizerProps()),
});

app.Synth();
