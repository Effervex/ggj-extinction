using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Extinction
{
    class ProbabilityDistribution<T> : Collection<T>
    {
        Dictionary<T, double> elements;

        public ProbabilityDistribution()
        {
            elements = new Dictionary<T, double>();
        }

        public void addItem(T element, double prob)
        {
            elements.Add(element, prob);
        }

        public void normalise()
        {
            double sum = 0;
            foreach (double value in elements.Values)
                sum += value;

            foreach (T element in elements.Keys)
                elements[element] /= sum;
        }

        public T sample()
        {
            double randVal = 0;
            double sumVal = 0;
            foreach (T element in elements.Keys)
            {
                sumVal += elements[element];
                if (randVal < sumVal)
                    return element;
            }

            return default(T);
        }
    }
}
