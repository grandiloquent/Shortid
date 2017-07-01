using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;

namespace Shortid
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var ls = new List<string>();
            var s = Shortid.GetInstace();
            for (int i = 0; i < 100; i++)
            {
                ls.Add(s.Generate());
            }
            Console.WriteLine(ls.Count + "=>" + ls.Distinct().Count());
            stopwatch.Stop();
            Console.Write(stopwatch.Elapsed.TotalMilliseconds);
        }
    }

    public class Shortid
    {
        private static Shortid sShortid;
        private const string ORIGINAL = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_-";
        private readonly DateTime REDUCE;
        private double _previousSecond;
        private int _counter = 0;
        private int _version = 6;
        private string _shuffled;
        private double _seed = 1;

        public Shortid() => REDUCE = new DateTime(1970, 1, 9, 0, 0, 00);

        public static Shortid GetInstace()
        {
            if (sShortid == null)
                return sShortid = new Shortid();
            return sShortid;
        }

        double GetNextValue()
        {
            _seed = (_seed * 9301 + 49297) % 233280;
            return _seed / (233280.0);
        }

        int RandomByte()
        {
            Random random = new Random(Guid.NewGuid().GetHashCode());
            byte[] row = new byte[1];
            random.NextBytes(row);
            return row[0] & 0x30;
        }

        string Encode(Func<int, string> lookup, int number)
        {
            var loopCounter = 0;
            var done = false;
            var str = string.Empty;
            while (!done)
            {
                str = str + lookup(((number >> (4 * loopCounter)) & 0x0f) | RandomByte());
                done = number < (Math.Pow(16, loopCounter + 1));
                loopCounter++;
            }
            return str;
        }


        public string Shuffle(string str)
        {
            char[] array = str.ToCharArray();
            int n = array.Length;
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            
            while (n > 1)
            {
                n--;
                int k = (int) Math.Floor(rand.NextDouble() * n);
                var value = array[k];
                array[k] = array[n];
                array[n] = value;
            }
            return new string(array);
        }

        string getShuffled()
        {
            if (_shuffled != null)
            {
                return _shuffled;
            }
            _shuffled = Shuffle(ORIGINAL);
            return _shuffled;
        }

        string Lookup(int index)
        {
            var alphabetShuffled = getShuffled();
            return alphabetShuffled[index].ToString();
        }

//        function encode(lookup, number) {
//            var loopCounter = 0;
//            var done;
//
//            var str = '';
//
//            while (!done) {
//                str = str + lookup( ( (number >> (4 * loopCounter)) & 0x0f ) | randomByte() );
//                done = number < (Math.pow(16, loopCounter + 1 ) );
//                loopCounter++;
//            }
//            return str;
//        }
        public string Generate()
        {
            var str = string.Empty;

            var seconds = DateTime.Now.Subtract(REDUCE).TotalSeconds;

            if (seconds == _previousSecond)
            {
                _counter++;
            }
            else
            {
                _counter = 0;
                _previousSecond = seconds;
            }
            str += Encode(Lookup, _version);
            str += Encode(Lookup, 9);
            if (_counter > 0)
            {
                str += Encode(Lookup, _counter);
            }

            str += Encode(Lookup, (int) seconds);
            return str;
        }
    }
}