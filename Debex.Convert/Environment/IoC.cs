using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Debex.Convert.BL;
using Debex.Convert.BL.CalculatingFields;
using Debex.Convert.BL.ExcelReading;
using Debex.Convert.BL.FieldsChecking;
using Debex.Convert.BL.FieldsMatching;
using Debex.Convert.BL.RegionsMatching;
using Debex.Convert.BL.Stages.CleanFormatsStage;
using Debex.Convert.Enviroment;
using Debex.Convert.ViewModels;
using Debex.Convert.ViewModels.Controls;
using Debex.Convert.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;
using RestSharp;
using RestSharp.Serializers.SystemTextJson;

namespace Debex.Convert.Environment
{
    public class IoC
    {
        private static readonly Lazy<IServiceProvider> instance = new Lazy<IServiceProvider>(() => new IoC().CreateDebex());
        public static IServiceProvider Instance => instance.Value;

        public static T Get<T>() => (T)Instance.GetService(typeof(T));
        public static object Get(Type type) => Instance.GetService(type);

        public IServiceProvider CreateDebex()
        {
            var collection = new ServiceCollection();

            collection
                .AddSingleton<HttpClient>()
                .AddSingleton<Updater>()
                .AddSingleton<Navigator>()
                .AddSingleton<DialogService>()
                .AddSingleton<ExcelReader>()
                .AddSingleton<Storage>()
                .AddSingleton<UiService>()
                .AddSingleton<IRestClient>((_) => new RestClient().UseSystemTextJson(new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }))

                .AddSingleton<DaDataClient>()
                .AddSingleton<GoogleDocsLoader>()
                .AddSingleton<ConfigurationLoader>()
                .AddSingleton<AutomaticFieldsMatcher>()

                .AddTransient<CalculatedFieldsFactory>()
                .AddTransient<FieldsCalculator>()
                .AddTransient<FieldsChecker>()

                .AddTransient<SidebarViewModel>()
                .AddTransient<FieldsMatchingViewModel>()
                .AddTransient<MainWindowViewModel>()
                .AddTransient<EmptyPageViewModel>()
                .AddTransient<ClearFormatsViewModel>()
                .AddTransient<RegionMatchViewModel>()
                .AddTransient<CalculatedFieldsViewModel>()
                .AddTransient<CheckFieldsViewModel>()

                .AddTransient<Logger>()
                .AddTransient<FormatCleaner>()
                .AddTransient<RegionsMatcher>()
                .AddTransient<ExcelSaver>()
                ;

            return collection.BuildServiceProvider();
        }
    }
}
