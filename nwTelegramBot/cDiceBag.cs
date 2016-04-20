/* 
 * All contents copyright 2016, Andy Dingo and Contributors
 * All rights reserved.  YOU MAY NOT REMOVE THIS NOTICE.
 * Please read docs/gpl.txt for licensing information.
 * ---------------------------------------------------------------
 * -- CREATOR INFORMATION --
 * Created by: Microsoft Visual Studio 2015.
 * User      : AndyDingoWolf
 * -- VERSION --
 * Version   : 1.0.0.33
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nwTelegramBot
{
    public class cDiceBag
    {
        /// <summary>
        /// The big phat instance. It doesn't have a boss, and it doesn't give phat lewtz.
        /// </summary>
        public static cDiceBag Instance = new cDiceBag();

        public int d4(int dice)
        {
            //if(dice==0)temp
            int temp = new Random(DateTime.Now.Millisecond).Next(dice - 1, dice * 4);
            return temp;
        }

        public int d5(int dice)
        {
            //if(dice==0)temp
            int temp = new Random(DateTime.Now.Millisecond).Next(dice - 1, dice * 5);
            return temp;
        }

        public int d6(int dice)
        {
            int temp = new Random(DateTime.Now.Millisecond).Next(dice - 1, dice * 6);
            return temp;
        }

        public int d7(int dice)
        {
            //if(dice==0)temp
            int temp = new Random(DateTime.Now.Millisecond).Next(dice - 1, dice * 7);
            return temp;
        }

        public int d8(int dice)
        {
            int temp = new Random(DateTime.Now.Millisecond).Next(dice - 1, dice * 8);
            return temp;
        }

        public int d9(int dice)
        {
            //if(dice==0)temp
            int temp = new Random(DateTime.Now.Millisecond).Next(dice - 1, dice * 9);
            return temp;
        }

        public int d10(int dice)
        {
            int temp = new Random(DateTime.Now.Millisecond).Next(dice - 1, dice * 10);
            return temp;
        }

        public int d12(int dice)
        {
            int temp = new Random(DateTime.Now.Millisecond).Next(dice - 1, dice * 12);
            return temp;
        }

        public int d14(int dice)
        {
            int temp = new Random(DateTime.Now.Millisecond).Next(dice - 1, dice * 14);
            return temp;
        }

        public int d20(int dice)
        {
            int temp = new Random(DateTime.Now.Millisecond).Next(dice - 1, dice * 20);
            return temp;
        }

        public int d100(int dice)
        {
            int temp = new Random(DateTime.Now.Millisecond).Next(dice - 1, dice * 100);
            return temp;
        }

        public int[] d4a(int dice)
        {
            
            int[] tempa = new int[dice];

            for (int i = 0; i < dice; i++)
            {
                tempa[i] = d4(1);
            }
            return tempa;
        }

        /// <summary>
        /// Rolls the specified number of die each with the specified number of
        /// sides and returns the result as a string, including the total.
        /// </summary>
        /// <param name="numberOfDice">The number of die to roll.</param>
        /// <param name="numberOfSides">The number of faces on each dice rolled.</param>
        /// <returns>A string containing the result of the roll.</returns>
        public string Roll(Int32 numberOfDice, Int32 numberOfSides)
        {

            // don't allow a Number of Dice less than or equal to zero
            if (numberOfDice <= 0)
            {
                throw new ApplicationException("Number of die must be greater than zero.");
            }

            // don't allow a Number of Sides less than or equal to zero
            if (numberOfSides <= 0)
            {
                throw new ApplicationException("Number of sides must be greater than zero.");
            }

            // Create the random class used to generate random numbers.
            // See: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpref/html/frlrfSystemRandomClassTopic.asp
            Random rnd = new Random((Int32)DateTime.Now.Ticks);

            // Create the string builder class used to build the string 
            // we return with the result of the die rolls.
            // See: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpref/html/frlrfsystemtextstringbuilderclasstopic.asp
            StringBuilder result = new StringBuilder();

            // Declare the integer in which we will keep the total of the rolls
            Int32 total = 0;

            // repeat once for each number of dice
            for (Int32 i = 0; i < numberOfDice; i++)
            {

                // Get a pseudo-random result for this roll
                Int32 roll = rnd.Next(1, numberOfSides);

                // Add the result of this roll to the total
                total += roll;

                // Add the result of this roll to the string builder
                result.AppendFormat("Dice {0:00}:\t{1}\n", i + 1, roll);

            }

            // Add a line to the result to seperate the rolls from the total
            result.Append("\t\t--\n");

            // Add the total to the result
            result.AppendFormat("TOTAL:\t\t{0}\n", total);

            // Now that we've finished building the result, get the string
            // that we've been building and return it.
            return result.ToString();

        }


        internal int d2(int dice)
        {
            int temp = new Random(DateTime.Now.Millisecond).Next(dice - 1, dice * 2);
            return temp;
        }

        internal int d3(int dice)
        {
            int temp = new Random(DateTime.Now.Millisecond).Next(dice - 1, dice * 3);
            return temp;
        }
    }
}
