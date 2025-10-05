namespace Test_Project
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TestRun();

        }

        static void TestRun()
        {
            string datas = "2 32 32 42 48 50 49 51 57 49 46 53 51 71 3";
            string[] dataArray = datas.Split(" ");
            byte[] byteArray = dataArray.Select(x => byte.Parse(x)).ToArray();

            // Print bytes
            Console.WriteLine(string.Join(", ", byteArray[3..10]));
        }

        public static void ReadFileLines(string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                Console.WriteLine("File contents (line by line):");
                foreach (string line in lines)
                {
                    Console.WriteLine(line);
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Error: File not found at '{filePath}'");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
            }
        }

        public static void ReadFileWithStreamReader(string filePath)
        {
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    Console.WriteLine("File contents (using StreamReader):");
                    while ((line = reader.ReadLine()) != null)
                    {
                        Console.WriteLine(line);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Error: File not found at '{filePath}'");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
            }
        }

        static string backwardDirectory(string path, int n)
        {
            if (path == null || CountCharacter(path, '\\') < n) { return "Parameter Trouble"; }
            
            if (n > 1)
            {
                
                path = backwardDirectory(path, n - 1);
            }
            path = Path.GetDirectoryName(path);
            return path;
        }

        public static int CountCharacter(string text, char characterToCount)
        {
            return text.Count(c => c == characterToCount);
        }
    }
}
