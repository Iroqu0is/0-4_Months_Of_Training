using System.IO.Compression;

namespace ConsoleApp3
{
    internal class MyException : Exception
    {
        public MyException(string str, Exception inner) : base(str, inner) { }
        public MyException(string str) : base(str) { }
        public MyException() : base() { }
        public override string ToString()
        {
            return $"Error massege : \"{Message}.\"";
        }
    }

    internal class Program
    {
        static int Main(string[] args)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            try
            {
                string file = @"C:\Games\Temp\logfile.txt";
                string text = @"test !!!!  test !!!!  test !!!!  test !!!!";

                Write(file, text);
                Console.WriteLine(Read(file));
            }
            catch (MyException ex)
            {
                Console.WriteLine(ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error message : \"{ex.Message}.\"");
            }
            timer.Stop();
            Console.WriteLine($"Worktime : {timer.ElapsedMilliseconds} ms.");

            return 0;
        }

        public static string Read(string FilePath)
        {
            if ((FilePath == null) || (!File.Exists(FilePath))) throw new MyException("File not created.");
            using (var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
            {
                using (var sr = new StreamReader(fs))
                {
                    return sr.ReadToEnd();
                }
                /*int count = (int)fs.Length;
                byte[] buffer = new byte[count];
                for (int tmp = 0, offset = 0; offset < count; offset += tmp)
                {
                    tmp = fs.Read(buffer, offset, count - offset);
                    if (tmp == 0) break;
                }
                return UTF8.GetString(buffer);*/
            }

        }
        public static void Write(string FilePath, string TextInFile)
        {
            string? directory = Path.GetDirectoryName(FilePath) ?? @"C:\Games\Temp";
            if (!Path.Exists(directory)) Directory.CreateDirectory(directory);
            using (var fs = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (var ds = new DeflateStream(fs, CompressionLevel.NoCompression))
                {
                    using (var sw = new StreamWriter(ds, UTF8))
                    {
                        sw.Write(TextInFile);
                    }
                }
                /* {byte[] buffer = UTF8.GetBytes(TextInFile);
                 int count = buffer.Length;
                 int offset = 0;
                     fs.Write(buffer, offset, count);
                 }*/
            }
        }
    }
}