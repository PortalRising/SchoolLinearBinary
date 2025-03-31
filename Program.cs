namespace SchoolLinearBinary
{
    struct SearchResult
    {
        int ItemIndex { get; }
        int RequiredIterations { get; }

        public SearchResult(int ItemIndex, int RequiredIterations)
        {
            this.ItemIndex = ItemIndex;
            this.RequiredIterations = RequiredIterations;
        }
    }

    abstract class Searcher
    {
        ///<summary>
        /// Attempt to find the item within the provided list, returning its index,
        /// or null if not found
        ///</summary>
        public abstract SearchResult Find<T>(ref IReadOnlyList<T> items, ref T searchItem);

        ///<summary>
        /// Return the name of this searcher
        ///</summary>
        public abstract string GetName();
    }

    class LinearSearch : Searcher
    {
        public override SearchResult Find<T>(ref IReadOnlyList<T> items, ref T searchItem)
        {
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;

            for (int i = 0; i < items.Count; i++)
            {
                T item = items[i];

                if (comparer.Equals(item, searchItem))
                {
                    // It takes at least one iteration to find an item
                    return new SearchResult(i, i + 1);
                }
            }

            return new SearchResult(-1, items.Count);
        }

        public override string GetName()
        {
            return "LinearSearch";
        }
    }

    class BinarySearch : Searcher
    {
        public override SearchResult Find<T>(ref IReadOnlyList<T> items, ref T searchItem)
        {
            int lowPointer = 0;
            int highPointer = items.Count;

            Comparer<T> comparer = Comparer<T>.Default;

            int iterations = 1;
            while (lowPointer < highPointer)
            {
                int midPoint = (lowPointer + highPointer) / 2;

                T item = items[midPoint];

                // Compare returns < 0 for less than
                // > 0 for greater than
                // and 0 for equals
                int compareResult = comparer.Compare(searchItem, item);
                if (compareResult < 0)
                {
                    // We need to check the bottom half now
                    highPointer = midPoint - 1;
                }
                else if (compareResult > 0)
                {
                    // We need to check the top half now
                    lowPointer = midPoint + 1;
                }
                else
                {
                    // We found the item, return its index
                    return new SearchResult(midPoint, iterations);
                }

                iterations += 1;
            }

            // Item not found
            return new SearchResult(-1, iterations);
        }

        public override string GetName()
        {
            return "BinarySearch";
        }
    }

    class Program
    {
        public static void Main()
        {
            Console.WriteLine("Running searching tests!");

            // Create instance of random
            Random random = new Random(0x63_23_6C_62);

            // Create a list of random unique integers
            int listSize = 10_000;
            HashSet<long> uniqueIntegers = new HashSet<long>(listSize);

            while (uniqueIntegers.Count < listSize)
            {
                uniqueIntegers.Add(random.NextInt64());
            }

            // Iterate over a list of search classes, attempting to find a random element and 
            // checking how long it takes
            Searcher[] searchers = [new LinearSearch(), new BinarySearch()];
        }
    }
}