using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238
using Windows.UI.Xaml.Shapes;

namespace GameOfLife
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
		private ApplicationDataCompositeValue _composite = new ApplicationDataCompositeValue();

	    private const int UniverseWidth = 100;
	    private const int UniverseHeight = 100;
	    private const int RectangleWidth = 7;
	    private const int RectangleHeight = 7;
	    private const int DelayBetweenSteps = 100;

		private readonly SolidColorBrush _whiteColor = new SolidColorBrush(Colors.White);
		private readonly SolidColorBrush _blackColor = new SolidColorBrush(Colors.Black);

		private bool _isLifeCycleRun; // Гооврит нам, работает ли сейчас режим просмотра эволюции клеток (кнопка Start)

        public MainPage()
        {
            this.InitializeComponent();
	        InitializeUniverse();
        }

		/// <summary>
		/// Инициализация поля в UI
		/// </summary>
	    private void InitializeUniverse()
	    {
			for (int i = 0; i < UniverseWidth; i++)
		    {
				for (int j = 0; j < UniverseHeight; j++)
			    {
					var rectangle = new Rectangle
					{
						Fill = _whiteColor,
						Stroke = _blackColor,
						Width = RectangleWidth,
						Height = RectangleHeight,
						Name = i + "." + j
					};
					rectangle.Tapped += RectangleOnTapped;
					rectangle.SetValue(Canvas.LeftProperty, i * RectangleWidth);
					rectangle.SetValue(Canvas.TopProperty, j * RectangleHeight);
					MainCanvas.Children.Add(rectangle);
			    }
		    }
	    }
		
	    private void RectangleOnTapped(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
	    {
		    var rectangle = sender as Rectangle;
			if (rectangle != null)
		    {
				if (!App.ChangedPointsList.Contains(rectangle.Name))
				{
					rectangle.Fill = _blackColor;
					App.ChangedPointsList.Add(rectangle.Name);
				}
				else
				{
					rectangle.Fill = _whiteColor;
					App.ChangedPointsList.Remove(rectangle.Name);
				}
		    }
	    }
		/// <summary>
		/// Видоизменяем ранее инициализированное поле клеток, если произошли изменения
		/// </summary>
	    private void ViewChanges()
	    {
		    if (App.PointsToAddList.Any())
		    {
				foreach (var itemName in App.PointsToAddList)
			    {
				    try
				    {
						(MainCanvas.Children.First(x => (x as Rectangle).Name == itemName) as Rectangle).Fill = _blackColor;
				    }
				    catch (InvalidOperationException)
				    {
					    // ловим exp когда фигура выходит за пределы поля
				    }
			    }
		    }
			if (App.PointsToDeleteList.Any())
			{
				foreach (var itemName in App.PointsToDeleteList)
				{
					try
					{
						(MainCanvas.Children.First(x => (x as Rectangle).Name == itemName) as Rectangle).Fill = _whiteColor;
					}
					catch (InvalidOperationException)
					{
						// ловим exp когда фигура выходит за пределы поля
					}
					
				}
			}
		    Core.ChangeChangedPointsList();
	    }

	    private void NextStepButton_OnTapped(object sender, TappedRoutedEventArgs e)
	    {
		    MainActions();
	    }

	    private async void StartButton_OnTapped(object sender, TappedRoutedEventArgs e)
	    {
			NextStepButton.IsEnabled = _isLifeCycleRun;
			ClearButton.IsEnabled = _isLifeCycleRun;
		    SaveStateButton.IsEnabled = _isLifeCycleRun;
		    LoadStateButton.IsEnabled = _isLifeCycleRun;
		    StartButton.Content = StartButton.Content.Equals("Start") ? "Stop" : "Start";
		    _isLifeCycleRun = !_isLifeCycleRun;
		    while (_isLifeCycleRun)
		    {
				MainActions();
				await Task.Delay(TimeSpan.FromMilliseconds(DelayBetweenSteps));
		    }
	    }

		/// <summary>
		/// Метод одного цикла жизни клеток в поле
		/// </summary>
	    private void MainActions()
	    {
			if (App.ChangedPointsList.Any())
			{
				Core.AddToListDeadPoints(App.ChangedPointsList);
				Core.AddToListALivePoints(App.ChangedPointsList);
				ViewChanges();
				SoundEffectMediaElement.Play();
			}
	    }

	    private void ClearButton_OnTapped(object sender, TappedRoutedEventArgs e)
	    {
		    if (App.ChangedPointsList.Any())
		    {
				foreach (var itemName in App.ChangedPointsList)
				{
					try
					{
						(MainCanvas.Children.First(x => (x as Rectangle).Name == itemName) as Rectangle).Fill = _whiteColor;
					}
					catch (InvalidOperationException)
					{
						// ловим exp когда фигура выходит за пределы поля
					}
				}
				App.ChangedPointsList.Clear();
		    }
	    }

	    private void SaveStateButton_OnTapped(object sender, TappedRoutedEventArgs e)
	    {
		    if (App.ChangedPointsList.Any())
		    {
			    for (int i = 0; i < App.ChangedPointsList.Count; i++)
			    {
					_composite[i.ToString()] = App.ChangedPointsList[i]; //используем ApplicationDataCompositeValue для автоматической сериализации / десериализации списка
			    }
				App.LocalSettings.Values["ChangedPointsList"] = _composite;
		    }
	    }

	    private void LoadStateButton_OnTapped(object sender, TappedRoutedEventArgs e)
	    {
			if (App.LocalSettings.Values.ContainsKey("ChangedPointsList"))
			{
				_composite = (ApplicationDataCompositeValue)App.LocalSettings.Values["ChangedPointsList"];
				App.ChangedPointsList.Clear();
				foreach (var item in _composite)
				{
					App.ChangedPointsList.Add((string)item.Value);
				}
				foreach (var item in MainCanvas.Children.Where(x => (x as Rectangle).Fill == _blackColor))
				{
					(MainCanvas.Children.First(x => x == item) as Rectangle).Fill = _whiteColor;
				}
				foreach (var item in App.ChangedPointsList)
				{
					(MainCanvas.Children.First(x => (x as Rectangle).Name.Equals(item)) as Rectangle).Fill = _blackColor;
				}
			}
	    }
    }
}
