using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models
{
    //код взят здесь: https://mvblog.ru/archives/601/
    public class Fraction
    {
        private int numerator;              // Числитель
        private int denominator;            // Знаменатель
        private int sign;					// Знак


        public Fraction(int numerator, int denominator)
        {
            if (denominator == 0)
            {
                throw new DivideByZeroException("В знаменателе не может быть нуля");
            }
            this.numerator = Math.Abs(numerator);
            this.denominator = Math.Abs(denominator);
            if (numerator * denominator < 0)
                sign = -1;
            else
                sign = 1;
        }
        public Fraction(int number) : this(number, 1) { }


        // Наибольший общий делитель (Алгоритм Евклида)
        private static int getGreatestCommonDivisor(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }
        // Наименьшее общее кратное
        private static int getLeastCommonMultiple(int a, int b) =>
            Math.Abs(a * b) / getGreatestCommonDivisor(a, b);


        private static Fraction plusMinusOperation(Fraction a, Fraction b, Func<int, int, int> operation)
        {
            int leastCommonMultiple = getLeastCommonMultiple(a.denominator, b.denominator);
            // Дополнительный множитель к первой дроби
            int additionalMultiplierA = leastCommonMultiple / a.denominator;
            // Дополнительный множитель ко второй дроби
            int additionalMultiplierB = leastCommonMultiple / b.denominator;
            int newNumerator = operation(
                a.numerator * additionalMultiplierA * a.sign,
                b.numerator * additionalMultiplierB * b.sign);
            return new Fraction(newNumerator, a.denominator * additionalMultiplierA);
        }
        private Fraction GetReverse() => new Fraction(denominator * sign, numerator);
        private Fraction GetWithChangedSign() => new Fraction(-numerator * sign, denominator);


        public static Fraction operator +(Fraction a, Fraction b) =>
            plusMinusOperation(a, b, (int x, int y) => x + y);
        public static Fraction operator +(Fraction a, int b) => a + new Fraction(b);
        public static Fraction operator +(int a, Fraction b) => b + a;

        public static Fraction operator -(Fraction a, Fraction b) => 
            plusMinusOperation(a, b, (int x, int y) => x - y);
        public static Fraction operator -(Fraction a, int b) => a - new Fraction(b);
        public static Fraction operator -(int a, Fraction b) => new Fraction(a) - b;

        public static Fraction operator *(Fraction a, Fraction b) =>
            new Fraction(a.numerator * a.sign * b.numerator * b.sign, a.denominator * b.denominator);
        public static Fraction operator *(Fraction a, int b) => a * new Fraction(b);
        public static Fraction operator *(int a, Fraction b) => b * a;

        public static Fraction operator /(Fraction a, Fraction b) => a * b.GetReverse();
        public static Fraction operator /(Fraction a, int b) => a / new Fraction(b);
        public static Fraction operator /(int a, Fraction b) => new Fraction(a) / b;
        
        public static Fraction operator -(Fraction a) => a.GetWithChangedSign();
        public static Fraction operator ++(Fraction a) => a + 1;
        public static Fraction operator --(Fraction a) => a - 1;


        // Возвращает сокращенную дробь
        public Fraction Reduce()
        {
            Fraction result = this;
            int greatestCommonDivisor = getGreatestCommonDivisor(numerator, denominator);
            result.numerator /= greatestCommonDivisor;
            result.denominator /= greatestCommonDivisor;
            return result;
        }
        public bool Equals(Fraction that)
        {
            Fraction a = Reduce();
            Fraction b = that.Reduce();
            return a.numerator == b.numerator &&
                a.denominator == b.denominator &&
                a.sign == b.sign;
        }
        public override bool Equals(object obj)
        {
            if (obj is Fraction)
                return Equals(obj as Fraction);
            return false;
        }
        public override int GetHashCode()
        {
            return sign * (numerator * numerator + denominator * denominator);
        }
        public override string ToString()
        {
            string result = "";
            if (numerator == 0)
                return "0";
            if (sign < 0)
                result = "-";
            if (numerator == denominator)
                return result + "1";
            if (denominator == 1)
                return result + numerator;
            return result + numerator + "/" + denominator;
        }
    }
}
