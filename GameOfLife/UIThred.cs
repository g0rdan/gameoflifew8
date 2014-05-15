using System;
using Windows.UI.Core;

namespace GameOfLife
{
	public static class UIThred
	{
		private static readonly CoreDispatcher Dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

		public static void Invoke(Action action)
		{
			Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
		}
	}
}
