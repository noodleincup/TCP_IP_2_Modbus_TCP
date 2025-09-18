namespace Test_Project
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string appPath = AppDomain.CurrentDomain.BaseDirectory.Trim('\\');

            //string settingPath = @"D:\Project\Ajinomoto\Box_Project\Middleware_ModbusTCP_App\NetworkServer.txt";
            //string settingPath = backwardDirectory(appPath, 3) + @"\NetworkServer.txt";


            Console.WriteLine($"App Path: {appPath}");
            //ReadFileLines(settingPath);

            Console.WriteLine("Press some key to end precess");
            Console.ReadLine();


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
