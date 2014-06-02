using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfLife
{
	class Core
	{
		/// <summary>
		/// Добавляем в список клетки, которые мы в будущем добавим к живым клеткам
		/// </summary>
		/// <param name="changedPointsList">список живых клеток</param>
		public static void AddToListALivePoints(List<string> changedPointsList)
		{
			var cChangedPointsList = changedPointsList.GetRange(0, changedPointsList.Count);
			Parallel.ForEach(GetWrapPoints(cChangedPointsList), pointName =>
			{
				int countOfAlivePointsAround = GetArroundPoints(pointName).Count(cChangedPointsList.Contains);
				if (countOfAlivePointsAround == 3)
				{
					App.PointsToAddList.Add(pointName);
				}
			});
		}
		/// <summary>
		/// Добавляем в список клетки, которые мы в будущем удалим из списка живых клеток
		/// </summary>
		/// <param name="changedPointsList">список живых клеток</param>
		public static void AddToListDeadPoints(List<string> changedPointsList)
		{
			var cChangedPointsList = changedPointsList.GetRange(0, changedPointsList.Count);
			Parallel.ForEach(cChangedPointsList, pointName =>
			{
				int countOfAlivePointsAround = GetArroundPoints(pointName).Count(cChangedPointsList.Contains);
				if (countOfAlivePointsAround < 2 || countOfAlivePointsAround > 3)
				{
					App.PointsToDeleteList.Add(pointName);
				}
			});
		}
		/// <summary>
		/// Находим координаты обволакивающих клеток, вокруг фигур на поле (т.е. те клетки, которые потенциально могут измениться)
		/// </summary>
		/// <param name="changedPointsList">текущие живые клетки</param>
		/// <returns>координаты "обволакивающих" клеток</returns>
		private static IEnumerable<string> GetWrapPoints(List<string> changedPointsList)
		{
			var tempList = new List<string>();
			var cChangedPointsList = changedPointsList.GetRange(0, changedPointsList.Count);
			foreach (var points in cChangedPointsList.Select(pointName => GetArroundPoints(pointName).Except(cChangedPointsList)))
			{
				tempList.AddRange(points);
			}
			return tempList.Distinct().ToList();
		}
		/// <summary>
		/// Находим "координаты" 8ми окружающих клеток, вокруг pointName
		/// </summary>
		/// <param name="pointName">клетка, вокруг которой мы хотим узнать координаты других точек</param>
		/// <returns>Список "координат" 8 клеток</returns>
		private static IEnumerable<string> GetArroundPoints(string pointName)
		{
			string[] xj = pointName.Split(new[] { '.' });
			var x = Convert.ToUInt64(xj[0]); // т.к. возможное поле перемещения ограничивается только координатами клетки, то макимум, это UInt64 * UInt64
			var j = Convert.ToUInt64(xj[1]);
			return new List<string>()
			{
				(x - 1) + "." + (j - 1),
				(x) + "." + (j - 1),
				(x + 1) + "." + (j - 1),
				(x - 1) + "." + (j),
				(x + 1) + "." + (j),
				(x - 1) + "." + (j + 1),
				(x) + "." + (j + 1),
				(x + 1) + "." + (j + 1),
			};
		}
		/// <summary>
		/// Изменяем список клеток для отображения после нахождения клеток для добавления и удаления
		/// </summary>
		public static void ChangeChangedPointsList()
		{
			if (App.PointsToAddList.Any())
			{
				App.ChangedPointsList.AddRange(App.PointsToAddList);
			}
			if (App.PointsToDeleteList.Any())
			{
				foreach (var item in App.PointsToDeleteList)
				{
					App.ChangedPointsList.Remove(item);
				}
			}
			App.PointsToAddList.Clear();
			App.PointsToDeleteList.Clear();
		}
	}
}
