using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Extinction
{
    class ProbabilityDistribution<T>
    {
        private List<T> elements;
        private List<double> probs;

        public ProbabilityDistribution()
        {
            elements = new List<T>();
            probs = new List<double>();
        }

        public void addItem(T element, double prob)
        {
            elements.Add(element);
            probs.Add(prob);
        }

        public void normalise()
        {
            double sum = probs.Sum();
            List<double> newProbs = new List<double>();
            foreach (double prob in probs)
                newProbs.Add(prob / sum);
            probs = newProbs;
        }

        public T sample()
        {
            double randVal = ExtinctionGame.random.NextDouble();
            double sumVal = 0;
            List<T>.Enumerator eEnum = elements.GetEnumerator();
            List<double>.Enumerator pEnum = probs.GetEnumerator();
            for (int i = 0; i < elements.Count; i++)
            {
                eEnum.MoveNext();
                pEnum.MoveNext();
                sumVal += pEnum.Current;
                if (randVal < sumVal)
                    return eEnum.Current;
            }

            return default(T);
        }
    }
}
