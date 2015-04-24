using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace ParticleSwarm
{
	public class Swarm
	{
		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="task">Экземпляр класса Task, описывающий задачу (целевую фукнцию)</param>
		/// <param name="swarmsize">Размер роя (количество частиц)</param>
		/// <param name="currentVelocityRatio">Коэффициент для скорости в сторону лучшей точки, найденной самой частицей</param>
		/// <param name="localVelocityRatio">Коэффициент для скорости в сторону лучшей точки, найденной самой частицей</param>
		/// <param name="globalVelocityRatio">Коэффициент для скорости в сторону наилучшей точки, найденной всеми частицами</param>
		public Swarm (
			Task task,
			int swarmsize,
			double currentVelocityRatio,
			double localVelocityRatio,
			double globalVelocityRatio)
		{
			Debug.Assert ((localVelocityRatio + globalVelocityRatio) > 4);

			_task = task;
			_currentVelocityRatio = currentVelocityRatio;
			_localVelocityRatio = localVelocityRatio;
			_globalVelocityRatio = globalVelocityRatio;
			_iteration = 0;

			_particles = CreateParticles (swarmsize);
		}

		public void NextIteration ()
		{
			foreach (Particle particle in _particles)
			{
				particle.NextIteration ();
			}

			_iteration++;
		}

		public int Size
		{
			get { return _particles.Length; }
		}

		public int Dimension
		{
			get { return _task.MinValues.Length; }
		}


		public double[] MinValues
		{
			get { return _task.MinValues; }
		}


		public double[] MaxValues
		{
			get { return _task.MaxValues; }
		}


		Particle[] CreateParticles (int swarmSize)
		{
			Particle[] swarm = new Particle[swarmSize];
			for (int i = 0; i < swarmSize; i++)
			{
				swarm[i] = new Particle (this);
			}

			return swarm;
		}

		/// <summary>
		/// Решаемая задача (целевая функция)
		/// </summary>
		Task _task;

		/// <summary>
		/// Все частицы в рое
		/// </summary>
		Particle[] _particles;

		public Particle[] Particles
		{
			get { return _particles; }
		}

		double _bestFinalFunc = double.MaxValue;

		/// <summary>
		/// Лучшее значение целевой функции на данный момент
		/// </summary>
		public double BestFinalFunc
		{
			get { return _bestFinalFunc; }
		}

		double[] _bestPosition = null;

		/// <summary>
		/// Наилучшее решение, найденное в данный момент
		/// </summary>
		public double[] BestPosition
		{
			get { return _bestPosition; }
		}

		double _currentVelocityRatio;

		/// <summary>
		/// Коэффициент текущей скорости
		/// </summary>
		public double CurrentVelocityRatio
		{
			get { return _currentVelocityRatio; }
		}


		double _localVelocityRatio;

		/// <summary>
		/// Коэффициент для скорости в сторону лучшей точки, найденной самой частицей
		/// </summary>
		public double LocalVelocityRatio
		{
			get { return _localVelocityRatio; }
		}


		double _globalVelocityRatio;

		/// <summary>
		/// Коэффициент для скорости в сторону наилучшей точки, найденной всеми частицами
		/// </summary>
		public double GlobalVelocityRatio
		{
			get { return _globalVelocityRatio; }
		}


		int _iteration;

		public int Iteration
		{
			get { return _iteration; }
		}


		internal double FinalFunction (double[] position)
		{
			double finalfunc = _task.FinalFunction (position);

			if (finalfunc < _bestFinalFunc)
			{
				_bestFinalFunc = finalfunc;
				_bestPosition = (double[])position.Clone ();
			}

			return finalfunc;
		}
	}
}
