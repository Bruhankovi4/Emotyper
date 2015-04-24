using System;
using System.Text;

namespace ParticleSwarm
{
	public static class Utilites
	{
		public static string ResultToString (Swarm swarm)
		{
			StringBuilder builder = new StringBuilder ();

			builder.AppendFormat ("Iteration: {0}\n\n", swarm.Iteration);

			if (swarm.BestPosition != null)
			{
				for (int i = 0; i < swarm.Dimension; i++)
				{
					builder.AppendFormat ("X[{0}] = {1}\n", i, swarm.BestPosition[i]);
				}

				builder.AppendFormat ("\nFinal function: {0}\n", swarm.BestFinalFunc);
			}
			builder.Append ("-----------\n\n");

			return builder.ToString ();
		}
	}
}
