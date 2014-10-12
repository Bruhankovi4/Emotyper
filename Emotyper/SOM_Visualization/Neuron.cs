using System;

namespace SOM_Visualization
{
    public class Neuron
    {
        public double[] Weights;
        public int X;
        public int Y;
        private int length;
        private double nf;

        public Neuron(int x, int y, int length)
        {
            X = x;
            Y = y;
            this.length = length;
            nf = 1000/Math.Log(length);
        }

        private double Gauss(SOM.Neuron win, int it)
        {
            double distance = Math.Abs(Math.Pow(win.X - X, 2) + Math.Pow(win.Y - Y, 2));
            return Math.Exp(-distance/(Math.Pow(Strength(it), 2)));
        }

        private double LearningRate(double it)
        {
            return Math.Exp(-it/1000)*0.1;
        }

        private double Strength(int it)
        {
            return Math.Exp(-it/nf)*length;
        }

        public double UpdateWeights(double[] pattern, SOM.Neuron winner, int it)
        {
            double sum = 0;
            for (int i = 0; i < Weights.Length; i++)
            {
                double delta = LearningRate(it)*Gauss(winner, it)*(pattern[i] - Weights[i]);
                Weights[i] += delta;
                sum += delta;
            }
            return sum/Weights.Length;
        }
    }
}