using System;
using System.Collections.Generic;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает математический аппарат метода иерархий
	/// </summary>
	public class MakeDecisionMath
		{
		// Переменные
		List<List<double>> comparisonMatrix = new List<List<double>> ();

		/// <summary>
		/// Конструктор. Инициализирует математический аппарат метода
		/// </summary>
		/// <param name="ComparisonVector">Вектор приоритетов</param>
		public MakeDecisionMath (List<int> ComparisonVector)
			{
			// Контроль
			if ((ComparisonVector == null) || (ComparisonVector.Count < 2))
				throw new Exception ("Incorrect internal function call (point 1)");

			// Сборка матрицы
			for (int i = 0; i < ComparisonVector.Count; i++)
				comparisonMatrix.Add (new List<double> ());

			for (int c = 0; c < ComparisonVector.Count; c++)
				for (int r = 0; r < ComparisonVector.Count; r++)
					comparisonMatrix[c].Add (ComparisonVector[r]);

			// Нормализация матрицы
			for (int c = 0; c < ComparisonVector.Count; c++)
				{
				double divisor = comparisonMatrix[c][c];
				for (int r = 0; r < ComparisonVector.Count; r++)
					comparisonMatrix[c][r] /= divisor;
				}
			}

		/// <summary>
		/// Возвращает нормализованную матрицу сравнения
		/// </summary>
		public List<List<double>> ComparisonMatrix
			{
			get
				{
				return comparisonMatrix;
				}
			}

		/// <summary>
		/// Метод рассчитывает иерархию оценок
		/// </summary>
		/// <param name="CriteriaMath">Математика для критериев</param>
		/// <param name="ObjectsMaths">Список математик для объектов</param>
		/// <returns></returns>
		public static List<double> EvaluateHierarchy (MakeDecisionMath CriteriaMath, List<MakeDecisionMath> ObjectsMaths)
			{
			// Переменные
			List<double> criteriaVector = new List<double> ();
			List<List<double>> objectsVectors = new List<List<double>> ();
			List<double> supportVector = new List<double> ();
			List<double> resultVector = new List<double> ();

			// Сборка вектора для критериев
			double multiplier;
			for (int r = 0; r < CriteriaMath.ComparisonMatrix.Count; r++)
				{
				multiplier = 1;
				for (int c = 0; c < CriteriaMath.ComparisonMatrix.Count; c++)
					multiplier *= CriteriaMath.ComparisonMatrix[c][r];

				supportVector.Add (Math.Pow (multiplier, 1.0 / CriteriaMath.ComparisonMatrix.Count));
				}

			for (int r = 0; r < CriteriaMath.ComparisonMatrix.Count; r++)
				{
				criteriaVector.Add (0.0);
				for (int c = 0; c < CriteriaMath.ComparisonMatrix.Count; c++)
					criteriaVector[r] += CriteriaMath.ComparisonMatrix[c][r] * supportVector[c];
				}

			// Сборка векторов для объектов
			for (int obj = 0; obj < ObjectsMaths.Count; obj++)
				{
				objectsVectors.Add (new List<double> ());
				supportVector.Clear ();

				for (int r = 0; r < ObjectsMaths[obj].ComparisonMatrix.Count; r++)
					{
					multiplier = 1;
					for (int c = 0; c < ObjectsMaths[obj].ComparisonMatrix.Count; c++)
						multiplier *= ObjectsMaths[obj].ComparisonMatrix[c][r];

					supportVector.Add (Math.Pow (multiplier, 1.0 / ObjectsMaths[obj].ComparisonMatrix.Count));
					}

				for (int r = 0; r < ObjectsMaths[obj].ComparisonMatrix.Count; r++)
					{
					objectsVectors[obj].Add (0.0);
					for (int c = 0; c < ObjectsMaths[obj].ComparisonMatrix.Count; c++)
						objectsVectors[obj][r] += ObjectsMaths[obj].ComparisonMatrix[c][r] * supportVector[c];
					}
				}

			// Финализация
			for (int r = 0; r < ObjectsMaths[0].ComparisonMatrix.Count; r++)
				{
				resultVector.Add (0.0);
				for (int c = 0; c < objectsVectors.Count; c++)
					resultVector[r] += objectsVectors[c][r] * criteriaVector[c];
				}

			// Завершено
			return resultVector;
			}
		}
	}
