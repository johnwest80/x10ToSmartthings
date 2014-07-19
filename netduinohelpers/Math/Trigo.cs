using System;

namespace netduino.helpers.Math {
    /// <summary>
    /// Math Library for Micro Framework
    /// Compatible with full .NET Framework System.Math
    /// (C)opyright 2009 Elze Kool, http://www.microframework.nl
    /// This Sourcecode is Public Domain. You are free to use this class Non-Commercialy and Commercialy.
    /// This sourcecode is provided AS-IS. I take no responsibility for direct or indirect damage caused by this program/class. 
    /// 
    /// Changes by Fabien Royer:
    /// 
    /// Converted all double precision operations to float operations for performance reasons on the netduino platform.
    /// Included fixes for issues discussed here: http://www.microframework.nl/2009/01/15/math-library-compatible-with-full-net/comment-page-1/
    /// 
    /// </summary>
    public static class Trigo
    {
        const float Sq2P1 = 2.414213562373095048802e0F;
        const float Sq2M1 = .414213562373095048802e0F;
        const float Pio2 = 1.570796326794896619231e0F;
        const float Pio4 = .785398163397448309615e0F;
        const float Log2E = 1.4426950408889634073599247F;
        const float Sqrt2 = 1.4142135623730950488016887F;
        const float Ln2 = 6.93147180559945286227e-01F;
        const float AtanP4 = .161536412982230228262e2F;
        const float AtanP3 = .26842548195503973794141e3F;
        const float AtanP2 = .11530293515404850115428136e4F;
        const float AtanP1 = .178040631643319697105464587e4F;
        const float AtanP0 = .89678597403663861959987488e3F;
        const float AtanQ4 = .5895697050844462222791e2F;
        const float AtanQ3 = .536265374031215315104235e3F;
        const float AtanQ2 = .16667838148816337184521798e4F;
        const float AtanQ1 = .207933497444540981287275926e4F;
        const float AtanQ0 = .89678597403663861962481162e3F;

        /// <summary>
        /// PI
        /// Redefined from the original value due to issue discussed here: http://www.microframework.nl/2009/01/15/math-library-compatible-with-full-net/comment-page-1/#comment-3790
        /// </summary>
        public static readonly float Pi = 3.1415926535897931F;

        /// <summary>
        /// Natural base E
        /// </summary>
        public static readonly float E = 2.71828182845904523536F;

        /// <summary>
        /// Epsilon definition
        /// Addresses the issue discussed here: http://forums.netduino.com/index.php?/topic/972-whats-going-on-with-the-operator/page__gopid__7052
        /// and here: http://www.microframework.nl/2009/01/15/math-library-compatible-with-full-net/comment-page-1/#comment-4713
        /// </summary>
        public static readonly float Epsilon = 2.22045e-16F;

        /// <summary>
        /// Returns the absolute value 
        /// </summary>
        /// <param name="x">A number</param>
        /// <returns>absolute value of x</returns>
        public static float Abs(float x) {
            if (x >= 0.0F) return x;
            return (-x);
        }

        /// <summary>
        /// Returns the angle whose cosine is the specified number
        /// </summary>
        /// <param name="x">A number representing a cosine</param>
        /// <returns>An angle</returns>
        public static float Acos(float x) {
            if ((x > 1.0F) || (x < -1.0F))
                throw new ArgumentOutOfRangeException("x");

            return Pio2 - Asin(x);
        }

        /// <summary>
        /// Returns the angle whose sine is the specified number
        /// </summary>
        /// <param name="x">A number representing a sine</param>
        /// <returns>An angle</returns>
        public static float Asin(float x) {
            var sign = 1.0F;

            if (x < 0.0F) {
                x = -x;
                sign = -1.0F;
            }

            if (x > 1.0F) {
                throw new ArgumentOutOfRangeException("x");
            }

            var temp = Sqrt(1.0F - (x * x));

            if (x > 0.7) {
                temp = Pio2 - Atan(temp / x);
            }
            else {
                temp = Atan(x / temp);
            }

            return (sign * temp);
        }

        /// <summary>
        /// Returns the angle whose tangent is the specified number
        /// </summary>
        /// <param name="x">A number representing a tangent</param>
        /// <returns>the arctangent of x</returns>
        public static float Atan(float x) {
            if (x > 0.0F) return (Atans(x));
            return (-Atans(-x));
        }

        /// <summary>
        /// Returns the angle whose tangent is the quotient of two specified numbers.
        /// </summary>
        /// <param name="y">The y coordinate of a point</param>
        /// <param name="x">The x coordinate of a point</param>
        /// <returns>the arctangent of x/y</returns>
        public static float Atan2(float y, float x) {
            if ((x + y) == x) {
                if ((x == 0F) & (y == 0F)) return 0F;
                if (x >= 0.0F) return Pio2;
                return (-Pio2);
            }
            if (y < 0.0F) {
                if (x >= 0.0F) return ((Pio2 * 2) - Atans((-x) / y));
                return (((-Pio2) * 2) + Atans(x / y));
            }
            if (x > 0.0F) {
                return (Atans(x / y));
            }
            return (-Atans((-x) / y));
        }

        /// <summary>
        /// Returns the smallest integer greater than or equal to the specified number
        /// </summary>
        /// <param name="x">a Number</param>
        /// <returns>the smallest integer greater than or equal to x</returns>
        public static float Ceiling(float x) {
            return (float) System.Math.Ceiling(x);
        }


        /// <summary>
        /// Calculate Cosinus
        /// </summary>
        /// <param name="x">Value</param>
        /// <returns>Cosinus of Value</returns>
        public static float Cos(float x)
        {
            // This function is based on the work described in http://www.ganssle.com/approx/approx.pdf

            // Make X positive if negative
            if (x < 0) { x = 0.0F - x; }

            // Get quadrand

            // Quadrand 0,  >-- Pi/2
            byte quadrand = 0;

            // Quadrand 1, Pi/2 -- Pi
            if ((x > (Pi / 2F)) & (x < (Pi))) {
                quadrand = 1;
                x = Pi - x;
            }

            // Quadrand 2, Pi -- 3Pi/2
            if ((x > (Pi)) & (x < ((3F * Pi) / 2))) {
                quadrand = 2;
                x = Pi - x;
            }

            // Quadrand 3 - 3Pi/2 -->
            if ((x > ((3F * Pi) / 2))) {
                quadrand = 3;
                x = 2F * Pi - x;
            }

            // Constants used for approximation
            const float c1 = 0.99999999999925182f;
            const float c2 = -0.49999999997024012f;
            const float c3 = 0.041666666473384543f;
            const float c4 = -0.001388888418000423f;
            const float c5 = 0.0000248010406484558f;
            const float c6 = -0.0000002752469638432f;
            const float c7 = 0.0000000019907856854f;

            // X squared
            var x2 = x * x;

            // Check quadrand
            if ((quadrand == 0) | (quadrand == 3)) {
                // Return positive for quadrand 0, 3
                return (c1 + x2 * (c2 + x2 * (c3 + x2 * (c4 + x2 * (c5 + x2 * (c6 + c7 * x2))))));
            }
            // Return negative for quadrand 1, 2
            return 0.0F - (c1 + x2 * (c2 + x2 * (c3 + x2 * (c4 + x2 * (c5 + x2 * (c6 + c7 * x2))))));
        }


        /// <summary>
        /// Returns the hyperbolic cosine of the specified angle
        /// </summary>
        /// <param name="x">An angle, measured in radians</param>
        /// <returns>hyperbolic cosine of x</returns>
        public static float Cosh(float x)
        {
            if (x < 0.0F) x = -x;

            if (x == 0F) {
                return 1F;
            }
            if (x <= (Ln2 / 2)) {
                return (1 + (Power((Exp(x) - 1), 2) / (2 * Exp(x))));
            }
            if (x <= 22F) {
                return ((Exp(x) + (1 / Exp(x))) / 2);
            }
            return (0.5F * (Exp(x) + Exp(-x)));
        }

        /// <summary>
        /// Returns e raised to the specified power
        /// </summary>
        /// <param name="x">A number specifying a power</param>
        /// <returns>e raised to x</returns>
        public static float Exp(float x) {
            var n = 1;
            var ex = 1F;
            var m = 1F;

            // exp(x+y) = exp(x) * exp(y)
            // http://www.quinapalus.com/efunc.html
            while (x > 10.000F) { m *= 22026.4657948067f; x -= 10F; }
            while (x > 01.000F) { m *= E; x -= 1F; }
            while (x > 00.100F) { m *= 1.10517091807565f; x -= 0.1F; }
            while (x > 00.010F) { m *= 1.01005016708417f; x -= 0.01F; }

            // if (Abs(x) < (double.Epsilon * 2)) return m;

            // Uses Taylor series 
            // http://www.mathreference.com/ca,tfn.html
            for (var y = 1; y <= 4; y++) {
                var c = Power(x, y);
                ex += c / n;
                n *= (y + 1);
            }

            return ex * m;
        }

        /// <summary>
        /// Returns a specified number raised to the specified power
        /// </summary>
        /// <param name="x">number to be raised to a power</param>
        /// <param name="y">number that specifies a power</param>
        /// <returns>x raised to the power y</returns>
        public static float Pow(float x, float y) {
            var temp = 0F;

            if (x <= 0.0F) {
                if (x == 0.0F) {
                    if (y <= 0.0F) {
                        throw new ArgumentException();
                    }
                }

                var l = (long)Floor(y);
                if (l != y) {
                    temp = Exp(y * Log(-x));
                }

                if ((l % 2) == 1) {
                    temp = -temp;
                }

                return (temp);
            }

            return (Exp(y * Log(x)));
        }

        /// <summary>
        /// Returns the largest integer less than or equal to the specified number.
        /// </summary>
        /// <param name="x">a Number</param>
        /// <returns>the largest integer less than or equal to x</returns>
        public static float Floor(float x) {
            return (float) System.Math.Floor(x);
        }

        /// <summary>
        /// Returns the natural (base e) logarithm of a specified number
        /// </summary>
        /// <param name="x">a Number</param>
        /// <returns>Logaritmic of x</returns>
        public static float Log(float x) {
            return Log(x, E);
        }

        /// <summary>
        /// Calculate logaritmic value from value with given base
        /// </summary>
        /// <param name="x">a Number</param>
        /// <param name="newBase">Base to use</param>
        /// <returns>Logaritmic of x</returns>
        public static float Log(float x, float newBase)
        {
            // Based on Python sourcecode from: http://en.literateprograms.org/Logarithm_Function_%28Python%29

            var partial = 0.5F;
            var integer = 0F;
            var fractional = 0.0F;

            if (x == 0.0F) return (float) double.NegativeInfinity;
            if ((x < 1.0F) & (newBase < 1.0F)) throw new ArgumentOutOfRangeException("x");

            while (x < 1.0F) {
                integer -= 1F;
                x *= newBase;
            }

            while (x >= newBase) {
                integer += 1F;
                x /= newBase;
            }

            x *= x;

            while (partial >= Epsilon) {
                if (x >= newBase) {
                    fractional += partial;
                    x = x / newBase;
                }
                partial *= 0.5F;
                x *= x;
            }

            return integer + fractional;
        }

        /// <summary>
        /// Returns the base 10 logarithm of a specified number. 
        /// </summary>
        /// <param name="x">a Number </param>
        /// <returns>Logaritmic of x</returns>
        public static float Log10(float x) {
            return Log(x, 10F);
        }

        /// <summary>
        /// Returns the larger of two specified numbers
        /// </summary>
        /// <param name="x">a Number</param>
        /// <param name="y">a Number</param>
        /// <returns>The larger of two specified numbers</returns>
        public static float Max(float x, float y) {
            return x >= y ? x : y;
        }

        /// <summary>
        /// Returns the smaller of two specified numbers
        /// </summary>
        /// <param name="x">a Number</param>
        /// <param name="y">a Number</param>
        /// <returns>The smaller of two specified numbers</returns>
        public static float Min(float x, float y) {
            return x <= y ? x : y;
        }

        /// <summary>
        /// Returns the hyperbolic sine of the specified angle.
        /// </summary>
        /// <param name="x">An angle, measured in radians</param>
        /// <returns>The hyperbolic sine of x</returns>
        public static float Sinh(float x) {
            if (x < 0F) x = -x;

            if (x <= 22F) {
                var ex1 = Tanh(x / 2) * (Exp(x) + 1);
                return ((ex1 + (ex1 / (ex1 - 1))) / 2);
            }
            return (Exp(x) / 2);
        }

        /// <summary>
        /// Returns a value indicating the sign 
        /// </summary>
        /// <param name="x">A signed number.</param>
        /// <returns>A number indicating the sign of x</returns>
        public static float Sign(float x) {
            if (x < 0F) return -1;
            return x == 0F ? 0 : 1;
        }

        /// <summary>
        /// Calculate Sinus
        /// </summary>
        /// <param name="x">Value</param>
        /// <returns>Sinus of Value</returns>
        public static float Sin(float x) {
            return Cos((Pi / 2.0F) - x);
        }

        /// (C)opyright 2011 Mario Vernari, http://highfieldtales.wordpress.com/
        /// http://highfieldtales.wordpress.com/2011/03/26/fast-calculation-of-the-square-root/
        /// This Source code is Public Domain. You are free to use this class Non-Commercially and Commercially.
        /// This sourcecode is provided AS-IS. I take no responsibility for direct or indirect damage coused by this program/class.
        /// <summary>
        /// Returns the square root of a specified number
        /// </summary>
        /// <param name="x">A positive real number</param>
        /// <returns>The square root of x</returns>
        public static float Sqrt(float x) {
            //cut off any special case
            if (x <= 0.0f) {
                return 0.0f;
            }

            // here is a kind of base-10 logarithm so that the argument will fall between 1 and 100, where the convergence is fast
            var exp = 1.0f;

            while (x < 1.0f) {
                 x *= 100.0f;
                 exp *= 0.1f;
            }

            while (x > 100.0f) {
                x *= 0.01f;
                exp *= 10.0f;
            }

            //choose the best starting point upon the actual argument value
            float prev;

            if (x > 10f) {
                //decade (10..100)
                prev = 5.51f;
            } else if (x == 1.0f) {
                //avoid useless iterations
                return x * exp;
            } else {
                //decade (1..10)
                prev = 1.741f;
            }

            //apply the Newton-Rhapson method just for three times
            prev = 0.5f * (prev + x / prev);
            prev = 0.5f * (prev + x / prev);
            prev = 0.5f * (prev + x / prev);

            //adjust the result multiplying for
            //the base being cut off before
            return prev * exp;
        }

        /// <summary>
        /// Calculate Tangens
        /// </summary>
        /// <param name="x">Value</param>
        /// <returns>Tangens of Value</returns>
        public static float Tan(float x) {
            return (Sin(x) / Cos(x));
        }

        /// <summary>
        /// Returns the hyperbolic tangent of the specified angle
        /// </summary>
        /// <param name="x">An angle, measured in radians</param>
        /// <returns>The hyperbolic tangent of x</returns>
        public static float Tanh(float x) {
            return (Expm1(2F * x) / (Expm1(2F * x) + 2F));
        }

        /// <summary>
        /// Calculates the integral part of x to the nearest integer towards zero. 
        /// </summary>
        /// <param name="x">A number to truncate</param>
        /// <returns>integral part of x</returns>
        public static float Truncate(float x) {
            if (x == 0F) return 0F;
            return x > 0F ? Floor(x) : Ceiling(x);
        }

        private static float Expm1(float x) {
            var u = Exp(x);

            if (u == 1.0F) {
                return x;
            }

            if (u - 1.0F == -1.0F) {
                return -1.0F;
            }

            return (u - 1.0F) * x / Log(u);
        }

        private static float Power(float x, int c)
        {
            if (c == 0) return 1.0F;

            var ret = x;

            if (c >= 0f) {
                for (var d = 1; d < c; d++)
                    ret *= ret;
            } else {
                for (var e = 1; e < c; e++) {
                    ret /= ret;
                }
            }

            return ret;
        }

        private static float Atans(float x) {
            if (x < Sq2M1) return (Atanx(x));
            return (x > Sq2P1 ? Pio2 - Atanx(1.0F/x) : Pio4 + Atanx((x - 1.0F)/(x + 1.0F)));
        }

        private static float Atanx(float x) {
            var argsq = x * x;
            var value = ((((AtanP4 * argsq + AtanP3) * argsq + AtanP2) * argsq + AtanP1) * argsq + AtanP0);
            value = value / (((((argsq + AtanQ4) * argsq + AtanQ3) * argsq + AtanQ2) * argsq + AtanQ1) * argsq + AtanQ0);
            return (value * x);
        }
    }
}
