using System.Text;

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
            //string datas = "2 32 32 42 48 50 49 51 57 49 46 53 51 71 3";
            string datas = "02 20 2C 31 30 31 2C 20 20 20 20 31 37 2C 32 30 32 35 2F 31 30 2F 30 37 20 31 30 3A 33 38 3A 30 37 2C 20 2C 32 2C 20 35 35 34 2E 32 36 2C 20 20 20 20 20 20 20 2C 20 20 20 20 20 20 20 2C 20 2C 31 2C 36 31 37 36 36 2C 20 20 31 31 2C 33 31 34 30 2C 20 20 20 30 2C 20 20 20 30 0D 0A 03";
            // Split the string by spaces
            string[] dataArray = datas.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Convert each hex string to a byte
            byte[] byteArray = dataArray.Select(hex => Convert.ToByte(hex, 16)).ToArray();


            byte[] readData = getSpecificData(byteArray, 0, byteArray.Length);

            // Print bytes
            Console.WriteLine($"Byte amount: {byteArray.Length}");

            Console.WriteLine(string.Join(" ", readData));

            // Convert clean data to string
            string response = Encoding.UTF8.GetString(readData);
            string[] responseArray = response.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            //Console.WriteLine($"Response: " + string.Join(" ", responseArray));
            for (int i = 0; i < responseArray.Length; i++)
            {
                Console.WriteLine($"Index {i}: {responseArray[i]}");
            }
        }

        static byte[] getSpecificData(byte[] rawData, int startByte, int byteAmount)
        {
            return rawData[startByte..(startByte + byteAmount)];
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
