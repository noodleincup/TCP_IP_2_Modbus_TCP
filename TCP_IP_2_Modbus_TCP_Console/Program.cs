using EasyModbus;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TCP_IP_2_Modbus_TCP_Console
{
    internal class Program
    {
        private static ModbusServer ModbusServer;
        
        const int START_READ_SETTING_SERVER = 2;
        const int ADDRESS_PER_CONNECTION = 10;

        static string settingFilePath = AppDomain.CurrentDomain.BaseDirectory.Trim('\\') + @"\NetworkServer.txt";
        static byte connection_num = 0;
        static int modbusPort = 8080;

        //private static bool shouldStop { get; set; } = false;
        private static volatile bool _shouldStop = false;


        static void Main(string[] args)
        {

            // Create a Modbus server instance
            InitializeModbusServer();

            // Read Network settings from file
            if (!File.Exists(settingFilePath))
            {
                Console.WriteLine($"Setting file not found at '{settingFilePath}'");
            }
            var networkObjectArray = ReadNetworkObjectArray(settingFilePath);

            if (networkObjectArray.Length == 0 )
            {
                Console.WriteLine("No valid server connections found. Exiting.");
            }
            else
            {
                // Get connection number from users
                Console.WriteLine($"Number of connection: {connection_num}");

                // Wait for user to press Enter
                //Console.WriteLine("Press Enter to Start");
                //Console.ReadLine();

                // Create TCP clients and connect to servers
                TcpClient[] clients = new TcpClient[connection_num];

                // Register shutdown handlers
                RegisterShutdownHandlers(clients);

                // Create threads array
                Thread[] threads = new Thread[connection_num];

                // Start threads for each connection
                for (int i = 0; i < connection_num; i++)
                {
                    int index = i; // Capture for lambda
                    clients[index] = new TcpClient();
                    threads[index] = new Thread(() => ThreadMethod(ModbusServer, clients[index], networkObjectArray[index], index + 1));
                    threads[index].Start();
                    Console.WriteLine($"Thread {index} starting");
                }

                // Join threads (wait for them to finish)
                for (int i = 0; i < connection_num; i++)
                {
                    threads[i].Join();
                    Console.WriteLine($"Thread {i + 1} joined.");
                }
            }

            Console.WriteLine("All threads stopped.");

            // Wait for user to press a key
            Console.WriteLine("Press any key to close program");
            Console.ReadKey();

            
        }

        private static void InitializeModbusServer()
        {
            modbusPort = ReadModbusPort(settingFilePath);
            ModbusServer = new ModbusServer
            {
                Port = modbusPort
            };
            ModbusServer.Listen();
            Console.WriteLine($"Modbus TCP Server is listening on modbusPort {modbusPort}");
        }

        private static void RegisterShutdownHandlers(TcpClient[] clients)
        {
            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("Console is closing (CancelKeyPress event)!");
                ModbusServer.StopListening();
                Console.WriteLine("Modbus server stopped.");
                _shouldStop = true;
                Console.WriteLine("Thread status change");

                for (int i = 0; i < clients.Length; i++)
                {
                    if (clients[i] != null && clients[i].Connected)
                    {
                        clients[i].Close();
                        Console.WriteLine($"Client {i + 1} connection closed.");
                    }
                }

                Console.WriteLine("Program Ended.");

                //e.Cancel = true; // Prevent immediate termination
            };

            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                Console.WriteLine("Process is exiting!");
                ModbusServer.StopListening();

                for (int i = 0; i < clients.Length; i++)
                {
                    if (clients[i] != null && clients[i].Connected)
                    {
                        clients[i].Close();
                        Console.WriteLine($"Client {i + 1} connection closed.");
                    }
                }
            };
        }

        #region Backup Function
        private static NetworkObject[] ReadConnectionsFromUser()
        {
            // Get number of connections TCP/IP Servers
            Console.WriteLine("Connection number: ");
            if (!int.TryParse(Console.ReadLine(), out int number) || number < 0)
            {
                Console.WriteLine("Invalid number, defaulting to 0.");
                number = 0;
            }


            var networkObjectArray = new NetworkObject[number];

            // Get IP and Port for each connection
            for (int i = 0; i < number; i++)
            {
                while (true)
                {
                    Console.WriteLine($"Input {i + 1} IP and Port (space separated): ");
                    string input = Console.ReadLine();
                    var parts = input?.Split(' ');

                    if (parts?.Length == 2 && int.TryParse(parts[1], out int port))
                    {
                        networkObjectArray[i] = new NetworkObject { IPAddress = parts[0], Port = port };
                        break;
                    }

                    Console.WriteLine("Invalid input. Please enter 'IP PORT'.");
                }
            }

            Console.WriteLine($"There are {networkObjectArray.Length} server connections");

            return networkObjectArray;
        }

        private static Thread[] StartWorkerThreads(TcpClient client, NetworkObject[] networkObjectArray, int number)
        {
            var threads = new Thread[networkObjectArray.Length];
            // Start threads for each connection
            for (int i = 0; i < number; i++)
            {
                int index = i; // Capture for lambda
                Thread thread = new Thread(() => ThreadMethod(ModbusServer, client, networkObjectArray[index], index));
                threads[index] = thread;
                thread.Start();
                Console.WriteLine($"Thread {i} started for {networkObjectArray[i].IPAddress}:{networkObjectArray[i].Port}");
            }

            // Join threads (wait for them to finish)
            for (int i = 0; i < number; i++)
            {
                threads[i].Join();
                Console.WriteLine($"Thread {i} joined.");
            }

            return threads;
        }
        #endregion

        #region Thread Method
        static void ThreadMethod(ModbusServer modbusServer, TcpClient client, NetworkObject networkObject, int connection_num)
        {
            string serverIP = networkObject.IPAddress;
            int port = networkObject.Port;
            while (!_shouldStop)
            {
                client = new TcpClient();
                try
                {
                    Console.WriteLine("Connecting to server...");
                    client.Connect(serverIP, port);
                    Console.WriteLine($"Connected to server at {serverIP}:{port}");

                    // Get network stream for communication
                    using NetworkStream stream = client.GetStream();

                    while (!_shouldStop)
                    {

                        // Update connection status to Modbus Server
                        RegisterConnectedToModbusServer(modbusServer, 1, connection_num);

                        // Send data to server
                        byte[] messageBytes = Encoding.UTF8.GetBytes(""); // Send empty message to trigger response
                        stream.Write(messageBytes, 0, messageBytes.Length);

                        // Receive response from server
                        byte[] buffer = new byte[1024];
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);

                        // Show raw buffer as hex (for debugging)
                        string bytesAsString = BitConverter.ToString(buffer, 0, bytesRead).Replace("-", " ");
                        Console.WriteLine($"Buffer as hex: {bytesAsString}");
                        Console.WriteLine($"Received {bytesRead} bytes from server.");

                        // Check for disconnection
                        if (bytesRead == 0)
                        {
                            Console.WriteLine("No data received, closing connection.");
                            break;
                        }

                        // Trim STX (0x02) and ETX (0x03) and get clean data bytes
                        byte[] cleanData = getCleanDataBytes(buffer, bytesRead);

                        // Convert clean data to string
                        string response = Encoding.UTF8.GetString(cleanData);
                        Console.WriteLine($"{serverIP} response: " + response);

                        // Register weight data to Modbus Server
                        string weightData = getWeightData(response);

                        // Register weight data to Modbus Server
                        if (weightData != "")
                        {
                            RegisterWeightToModbusServer(modbusServer, weightData, connection_num);
                        }
                        else
                        {
                            Console.WriteLine("Receive data incorrect");
                        }
                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Socket error: {ex.Message}");
                }
                catch (ObjectDisposedException)
                {
                    Console.WriteLine("Stream was disposed, will reconnect...");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                }
                finally
                {
                    if (client.Connected)
                    {
                        client.Close();
                        Console.WriteLine("Connection closed.");
                    }
                }

                Console.WriteLine("Retrying connection in 5 seconds...");
                RegisterConnectedToModbusServer(modbusServer, 0, connection_num);
                Thread.Sleep(5000);
            }

            Console.WriteLine($"Thread {connection_num} exiting gracefully.");

        }

        static void ThreadClient(ModbusServer modbusServer, TcpClient client, int connection_num)
        {

            try
            {
                // Get network stream for communication
                using NetworkStream stream = client.GetStream();

                while (!_shouldStop)
                {
                    //Console.Write("Enter message to send (or 'exit' to quit): ");
                    string message = "";

                    if (message == "exit")
                    {
                        Console.WriteLine("Closing connection...");
                        break;
                    }

                    // Send data to server
                    byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                    stream.Write(messageBytes, 0, messageBytes.Length);

                    // Receive response from server
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);

                    // Show raw buffer as hex (for debugging)
                    string bytesAsString = BitConverter.ToString(buffer, 0, bytesRead).Replace("-", " ");
                    Console.WriteLine($"Buffer as hex: {bytesAsString}");
                    Console.WriteLine($"Received {bytesRead} bytes from server.");

                    if (bytesRead == 0)
                    {
                        Console.WriteLine("No data received, closing connection.");
                        break;
                    }

                    // Trim STX (0x02) and ETX (0x03)
                    int startIndex = 0;
                    int endIndex = bytesRead;

                    // Check first byte
                    if (bytesRead > 0 && buffer[0] == 0x02)
                        startIndex++;

                    // Check last byte
                    if (bytesRead > 1 && buffer[bytesRead - 1] == 0x03)
                        endIndex--;

                    // Create clean data array
                    byte[] cleanData = new byte[endIndex - startIndex];
                    Array.Copy(buffer, startIndex, cleanData, 0, cleanData.Length);


                    string response = Encoding.UTF8.GetString(cleanData);


                    Console.WriteLine($"{connection_num} response: " + response);

                    // Register weight data to Modbus Server
                    string weightData = getWeightData(response);
                    if (weightData != "")
                    {
                        RegisterWeightToModbusServer(modbusServer, weightData, connection_num);
                    }
                    else
                    {
                        Console.WriteLine("Receive data incorrect");
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
        #endregion

        #region Data Processing

        static byte[] getSpecificData(byte[] rawData, int startByte, int byteAmount)
        {
            return rawData[startByte..(startByte + byteAmount)];
        }

        static string getWeightData(byte[] rawData)
        {
            byte[] weightByte = getSpecificData(rawData, 153, 12);
            string weightString = Encoding.UTF8.GetString(weightByte).Trim();
            return weightString;
        }

        static string getCountData(byte[] rawData)
        {
            byte[] countByte = getSpecificData(rawData, 100, 6);
            string countString = Encoding.UTF8.GetString(countByte).Trim();
            return countString;
        }

        static string getWeightData(string res)
        {
            res = res.Trim();
            return (res.Substring(0, 3) == "*02") ? res.Substring(4).Replace("G", "") : "";
        }

        static byte[] getCleanDataBytes(byte[] data, int bytesRead)
        {
            // Trim STX (0x02) and ETX (0x03)
            int startIndex = 0;
            int endIndex = bytesRead;

            // Check first byte
            if (bytesRead > 0 && data[0] == 0x02)
                startIndex++;

            // Check last byte
            if (bytesRead > 1 && data[bytesRead - 1] == 0x03)
                endIndex--;

            // Create clean data array
            byte[] cleanData = new byte[endIndex - startIndex];
            Array.Copy(data, startIndex, cleanData, 0, cleanData.Length);
            return cleanData;
        }

        static void RegisterWeightToModbusServer(ModbusServer modbusServer, string weightData, int connection_num)
        {
            // Convert weight data to integer (assuming it's a valid number)
            var parts = weightData.Split(".");
            int offsetAddress = connection_num - 1;
            if (short.TryParse(parts[0], out short weightInt) && short.TryParse(parts[1], out short weightFloat))
            {
                // Register the weight data to Modbus server at the specified address
                ModbusServer.HoldingRegisters reg = modbusServer.holdingRegisters; // Uncomment and use actual Modbus server instance
                reg[ADDRESS_PER_CONNECTION * offsetAddress + 1] = weightInt; // Store weight at address corresponding to connection number
                reg[ADDRESS_PER_CONNECTION * offsetAddress + 2] = weightFloat;
                Console.WriteLine($"Registered weight {weightInt} at Modbus address {ADDRESS_PER_CONNECTION * offsetAddress + 1}");
                Console.WriteLine($"Registered weight {weightFloat} at Modbus address {ADDRESS_PER_CONNECTION * offsetAddress + 2}");
            }
            else
            {
                Console.WriteLine("Invalid weight data: " + weightData);
            }
        }

        static void RegisterWeightToModbusServer_2(ModbusServer modbusServer, string weightData, int connection_num)
        {
            int offsetAddress = connection_num - 1;
            //RegisterDataToHoldingModbusServer(modbusServer, (short)weightData, ADDRESS_PER_CONNECTION * offsetAddress + 1);
        }

        static void RegisterConnectedToModbusServer(ModbusServer modbusServer, int status, int connection_num)
        {
            int offsetAddress = connection_num - 1;
            RegisterDataToHoldingModbusServer(modbusServer, (short)status, ADDRESS_PER_CONNECTION * offsetAddress + 3);
        }

        static void RegisterDataToHoldingModbusServer(ModbusServer modbusServer, short data, int register)
        {
            ModbusServer.HoldingRegisters reg = modbusServer.holdingRegisters;
            reg[register] = data;
        }

        #endregion


        #region Network Settings
        static NetworkObject[] ReadNetworkObjectArray(string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);

                connection_num = (byte.TryParse(lines[1], out byte num)) ? num : (byte)0;

                NetworkObject[] networkObjects = new NetworkObject[lines.Length];
                for (int i = START_READ_SETTING_SERVER; i < lines.Length; i++)
                {
                    var parts = lines[i].Split(' ');
                    if (parts.Length == 3 && int.TryParse(parts[2], out int port))
                    {
                        networkObjects[i] = new NetworkObject { IPAddress = parts[1], Port = port };
                        Console.WriteLine($"Read connection : IP={parts[1]}, Port={port}");
                    }
                    else
                    {
                        Console.WriteLine($"Invalid line format: '{lines[i]}'. Expected 'IP PORT'.");
                    }
                }

                networkObjects = networkObjects.Where(n => n != null).ToArray();

                return networkObjects;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Error: File not found at '{filePath}'");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
            }
            return Array.Empty<NetworkObject>();

        }

        static int ReadModbusPort(string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                if (lines.Length > 0)
                {
                    string modbusLines = lines[0];
                    if (!int.TryParse(modbusLines.Split(" ")[1], out int port) || port < 0 || port > 65535)
                    {
                        Console.WriteLine($"Invalid Modbus port in file. Using default port {modbusPort}.");
                        return modbusPort;
                    }
                    return port;
                }
                else
                {
                    Console.WriteLine($"Invalid Modbus port in file. Using default port {modbusPort}.");
                    return modbusPort;
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
            return modbusPort;
        }


        class NetworkObject
        {
            public string IPAddress { get; set; }
            public int Port { get; set; }
        }

        #endregion
    }
}
