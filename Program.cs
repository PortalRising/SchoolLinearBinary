namespace SchoolLinearBinary
{
    struct SearchResult
    {
        public int ItemIndex { get; }
        public int RequiredIterations { get; }

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
        public abstract SearchResult Find<L, T>(L items, T searchItem) where L : IReadOnlyList<T>;

        ///<summary>
        /// Return the name of this searcher
        ///</summary>
        public abstract string GetName();
    }

    class LinearSearch : Searcher
    {
        public override SearchResult Find<L, T>(L items, T searchItem)
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
        public override SearchResult Find<L, T>(L items, T searchItem)
        {
            int lowPointer = 0;
            int highPointer = items.Count;

            Comparer<T> comparer = Comparer<T>.Default;

            int iterations = 1;
            while (lowPointer <= highPointer)
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
            // Random random = Random.Shared;

            // Create a list of random unique integers
            int listSize = 100_000;
            HashSet<long> uniqueIntegers = new HashSet<long>(listSize);

            while (uniqueIntegers.Count < listSize)
            {
                uniqueIntegers.Add(random.NextInt64());
            }

            // Convert the set into an array
            long[] randomIntegers = uniqueIntegers.ToArray();
            // Sort the array for binary search
            Array.Sort(randomIntegers);

            // Create a list of random values that exist in the randomIntegers array
            int numOfTestCases = listSize / 10;
            HashSet<long> validIndexes = new HashSet<long>();
            while (validIndexes.Count < numOfTestCases)
            {
                // Collect all the random unique indexes of the items to search for
                int randomIndex = random.Next(0, randomIntegers.Length);
                validIndexes.Add(randomIndex);
            }

            // Convert the indexes into values
            List<long> validItems = validIndexes.Select(index => randomIntegers[index]).ToList();

            // Create another list of random values that do not exist in the randomIntegers array
            HashSet<long> invalidItems = new HashSet<long>();
            while (invalidItems.Count < numOfTestCases)
            {
                long randomValue = random.NextInt64();
                if (!uniqueIntegers.Contains(randomValue))
                {
                    invalidItems.Add(randomValue);
                }
            }

            // Iterate over a list of search classes, attempting to find a random element and 
            // checking how long it takes
            Searcher[] searchers = [new LinearSearch(), new BinarySearch()];
            foreach (Searcher searcher in searchers)
            {
                Console.WriteLine("--- Searching using: {0}", searcher.GetName());

                // Store how many test cases we correctly identify and incorrectly fail
                int correctResults = 0;
                int incorrectResults = 0;

                // Store how many iterations all correct results took so we can work out the average
                int totalIterations = 0;

                // Check all valid integers
                Console.WriteLine("-- Checking valid items");
                foreach (long validInteger in validItems)
                {
                    // Search for this integer
                    SearchResult result = searcher.Find(randomIntegers, validInteger);

                    if (result.ItemIndex == -1)
                    {
                        // We should have found this value
                        incorrectResults += 1;

                        Console.WriteLine("- We should have found {0} in randomIntegers", validInteger);
                    }
                    else
                    {
                        // This result is valid
                        totalIterations += result.RequiredIterations;
                        correctResults += 1;
                    }
                }

                // Check all invalid integers
                Console.WriteLine("-- Checking invalid items");
                foreach (long invalidInteger in invalidItems)
                {
                    // Search for this integer
                    SearchResult result = searcher.Find(randomIntegers, invalidInteger);

                    if (result.ItemIndex == -1)
                    {
                        // This result is valid as we did not find a value
                        totalIterations += result.RequiredIterations;
                        correctResults += 1;
                    }
                    else
                    {
                        // We should have not found this value
                        incorrectResults += 1;

                        Console.WriteLine("- We should not have found {0} in randomIntegers", invalidInteger);
                    }
                }

                int totalResults = correctResults + incorrectResults;

                double correctPercentage = (correctResults * 100.0) / totalResults;
                double incorrectPercentage = (incorrectResults * 100.0) / totalResults;
                double averageIterations = (totalIterations * 100.0) / correctResults;

                Console.WriteLine("-- Results:");
                Console.WriteLine("- Correct Results: {0}, {1:F2}%", correctResults, correctPercentage);
                Console.WriteLine("- Incorrect Results: {0}, {1:F2}%", incorrectResults, incorrectPercentage);
                Console.WriteLine("- Total iterations on correct results: {0}, average: {1:F2}", totalIterations, averageIterations);

                Console.WriteLine("---\n");
            }
        }
    }
}