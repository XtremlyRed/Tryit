using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;
using Tryit;

namespace TryitWpf.Test;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        ssss ssss = new ssss();

        ssss.Run().GetAwaiter().GetResult();

        base.OnStartup(e);
    }
}

public class ssss
{
    public async Task Run()
    {
        var executor = new ActivityExecutor(new IActivityInterceptor[] { new LoggingInterceptor() });

        var engine = new ActivityEngine(executor);

        var ccc = new CompositeActivity("123") { };

        ccc.Add(new SetNameActivity());
        ccc.Add(new PrintNameActivity());

        var context = new Context();

        await engine.RunAsync(ccc, context);
    }

    public sealed class SetNameActivity : IActivity
    {
        public string Name => nameof(SetNameActivity);

        public Task<ActivityResult> RunAsync(IContext context)
        {
            context.SetValue("Name", "Tom");

            return Task.FromResult(ActivityResult.Success);
        }
    }

    public sealed class PrintNameActivity : IActivity
    {
        public string Name => nameof(PrintNameActivity);

        public Task<ActivityResult> RunAsync(IContext context)
        {
            Debug.WriteLine(context.GetValue<string>("Name"));

            return Task.FromResult(ActivityResult.Success);
        }
    }

    public sealed class LoggingInterceptor : IActivityInterceptor
    {
        async Task<ActivityResult> IActivityInterceptor.InvokeAsync(ActivityExecutionContext context, ActivityDelegate next)
        {
            var indent = "";

            var sw = Stopwatch.StartNew();

            Debug.WriteLine($"{indent}Start {context.Activity.GetType().Name}");

            try
            {
                var result = await next(context);

                Debug.WriteLine($"{indent}Finish {context.Activity.GetType().Name}");

                return result;
            }
            finally
            {
                sw.Stop();

                Debug.WriteLine($"{indent}Elapsed {sw.ElapsedMilliseconds}ms");
            }
        }
    }
}
