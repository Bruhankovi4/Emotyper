using System;
using System.Collections.Generic;
using System.Text;

namespace ParticleSwarm
{
	abstract public class Task
	{
		public Task (double[] minvalues, double[] maxvalues)
		{
			_minvalues = minvalues;
			_maxvalues = maxvalues;
		}

		/// <summary>
		/// Абстрактный метод, который представляет собой целевую функцию.
		/// Чем значуние функции меньше, тем лучше
		/// </summary>
		/// <param name="position">"Координаты" точки, где рассчитываем значение целевой функции</param>
		/// <returns></returns>
		public abstract double FinalFunction (double[] position);

		double[] _minvalues;

		/// <summary>
		/// Минимальные значения для интервала, где нужно искать минимум
		/// </summary>
		public double[] MinValues
		{
			get { return _minvalues; }
		}

		double[] _maxvalues;

		/// <summary>
		/// Максимальные значения для интервала, где нужно искать минимум
		/// </summary>
		public double[] MaxValues
		{
			get { return _maxvalues; }
		}


		/// <summary>
		/// Рассчитать штраф за то, что координаты частицы вышли за разрешенные границы
		/// </summary>
		/// <param name="ratio">Коэффициент влияния штрафа</param>
		/// <returns></returns>
		protected virtual double GetPenalty (double[] position, double ratio)
		{
			double penalty = 0.0;

			for (int i = 0; i < position.Length; i++)
			{
				if (position[i] < _minvalues[i])
				{
					penalty += Math.Abs (position[i] - _minvalues[i]);
				}

				if (position[i] > _maxvalues[i])
				{
					penalty += Math.Abs (position[i] - _maxvalues[i]);
				}
			}

			return penalty * ratio;
		}
	}
}
