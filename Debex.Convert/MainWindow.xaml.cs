using Debex.Convert.Converters;
using Debex.Convert.Enviroment;
using Debex.Convert.Environment;
using Debex.Convert.Extensions;
using Debex.Convert.ViewModels;
using Debex.Convert.Views.Pages;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using Colors = System.Windows.Media.Colors;

namespace Debex.Convert
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel vm;

        public MainWindow()
        {

            DataContext = vm = IoC.Get<MainWindowViewModel>();
            InitializeComponent();
            Loaded += (e, a) => vm.Loaded(e, a);
            Unloaded += (e, a) => vm.Unloaded(e, a);
            vm.PropertyChanged += VmOnPropertyChanged;
            vm.OnSorted += OnSorted;

        }

        private void VmOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.IsOneOf(nameof(MainWindowViewModel.Data)))
            {
                VisualData.DataContext = vm.Data;
                VisualData.ItemsSource = null;
                VisualData.ItemsSource = vm.Data.DefaultView;
                UpdateVisualData();
            }
            if (e.PropertyName.IsOneOf(nameof(MainWindowViewModel.BaseFields)))
            {
                UpdateVisualData();
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            IoC.Get<Updater>().CheckUpdates();
            IoC.Get<Navigator>().SetFrame(MainFrame);
            IoC.Get<Navigator>().Navigate<EmptyPage>();

            var ui = IoC.Get<UiService>();

            ui.OnDataHide += () =>
            {
                foreach (var row in MainGrid.RowDefinitions.Skip(1))
                {
                    row.Height = new GridLength(0);
                }
            };
            ui.OnDataShow += () =>
            {
                MainGrid.RowDefinitions[1].Height = new GridLength(15);
                MainGrid.RowDefinitions[2].Height = new GridLength(200);
            };
            ui.OnLoadingShow += () =>
            {
                MainGrid.RowDefinitions[1].Height = new GridLength(0);
                MainGrid.RowDefinitions[2].Height = new GridLength(20);
            };

            ui.OnLoadingHide += () =>
            {
                foreach (var row in MainGrid.RowDefinitions.Skip(1))
                {
                    row.Height = new GridLength(0);
                }
            };

            vm.Initialize();
        }

        public void UpdateVisualData()
        {
            if ((!vm.BaseFields?.Any() ?? true)
                || !vm.BaseFields.Any(x => x.IsMatched)
                || IoC.Get<Navigator>().CurrentPage == typeof(FieldsMatchingPage)
            )
            {
                foreach (var column in VisualData.Columns)
                    column.Visibility = Visibility.Visible;

                return;
            }

            VisualData.Visibility = Visibility.Visible;
            foreach (var column in VisualData.Columns)
            {
                var pureHeader = PrepareHeader(column.Header.ToString());
                column.Visibility = vm.BaseFields.Any(f => f.MatchedName == pureHeader || pureHeader.StartsWith("Debex"))
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        private void OnSelectFile()
        {
            vm.SelectFile();
        }


        private void OnSorted(string column)
        {
            var targetColumn = VisualData.Columns.FirstOrDefault(x => PrepareHeader(x.Header.ToString()) == column);
            if (targetColumn == null) return;

            if (VisualData.Items.Count == 0) return;

            VisualData.ScrollIntoView(VisualData.Items[VisualData.Items.Count - 1]);
            VisualData.UpdateLayout();
            VisualData.ScrollIntoView(VisualData.Items[0], targetColumn);

            UpdateVisualData();
        }

        private string PrepareHeader(string header)
        {
            return header.Replace(" ▼", string.Empty).Replace(" ▲", string.Empty);
        }

        private void OnVisualDataColumnGenerating(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName.IsNullOrEmpty()) return;

            var name = e.PropertyName.Replace("(", "^(").Replace(")", "^)").Replace(@"\", @"^\").Replace("%", "^%")
                .Replace(",", "^,").Replace(" ", "^ ");
            var propPath = $"[{name}]";

            var header = e.PropertyName;

            if (sortField?.Equals(header, StringComparison.OrdinalIgnoreCase) ?? false)
            {
                header += reverseSort ? " ▼" : " ▲";
            }

            e.Column = new DataGridTextColumn
            {
                Binding = new Binding($"{propPath}.Value")
                {
                    Converter = ExcelValueToGridViewConverter.Instance,
                    ConverterParameter = e.PropertyName
                },
                Header = header,
                HeaderStyle = new Style(typeof(DataGridColumnHeader), (Style)FindResource(typeof(DataGridColumnHeader)))
                {
                    Setters =
                    {
                        new Setter
                        {
                            Property = BackgroundProperty,
                            Value = e.PropertyName.StartsWith("Debex") ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.Transparent)
                        },
                        new Setter
                        {
                            Property = PaddingProperty,
                            Value = new Thickness(4, 4, 4, 4)
                        }
                    }
                },
                ElementStyle = new Style(typeof(TextBlock))
                {
                    Setters =
                    {
                        new Setter
                        {
                            Property = TextBlock.TextAlignmentProperty,
                            Value = TextAlignment.Right
                        },
                    }
                },
                CellStyle = new Style(typeof(DataGridCell))
                {
                    Setters =
                    {
                        new Setter
                        {
                            Property = BackgroundProperty,
                            Value = new Binding($"{propPath}.IsError")
                            {
                                Converter = BooleanToColorConverter.Instance,
                                ConverterParameter = new[] { new SolidColorBrush(Colors.LightPink), new SolidColorBrush(Colors.Transparent) }
                            }
                        },
                        new Setter
                        {
                            Property = ForegroundProperty,
                            Value = new SolidColorBrush(Colors.Black)
                        }
                    }
                }
            };

        }

        private bool reverseSort = false;
        private string sortField;

        private void OnHeaderSort(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;
            reverseSort = !reverseSort;
            var header = PrepareHeader(e.Column.Header.ToString() ?? string.Empty);

            sortField = header;

            if ((!vm.BaseFields?.Any() ?? true) || !vm.BaseFields.Any(x => x.IsMatched) || IoC.Get<Navigator>().CurrentPage == typeof(FieldsMatchingPage))
                vm.SortByNames(header, reverseSort);
            else
                vm.SortByNames(header, reverseSort);
        }
    }
}