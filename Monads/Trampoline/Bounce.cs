using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExperimentalMonads.Monads.Trampoline {
    /// <summary>
    ///     A marker to distinguish between continuing or finishing a trampoline
    /// </summary>
    /// <typeparam name="A">The value returned by the result</typeparam>
    public interface Bounce<A> {
    }

    /// <summary>
    ///    Represents the result of a trampolining function
    /// </summary>
    /// <typeparam name="A">The result type of the value returned</typeparam>
    public class Done<A> : Bounce<A> {
        public readonly A result;

        public Done(A result) {
            this.result = result;
        }
    }

    /// <summary>
    ///     A continue method which contains the thunking 
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public class Call<A> : Bounce<A> {
        public readonly Func<Bounce<A>> thunk;

        public Call(Func<Bounce<A>> thunk) {
            this.thunk = thunk;
        }
    }

    /// <summary>
    ///     A Trampoline is a data structure that looks like a function
    ///     Useful for simulating tail recursion
    /// </summary>
    public class Trampoline { 

        public static A run<A>(Bounce<A> initialBounce) {
            var bounce = initialBounce;
            var done = bounce as Done<A>;
            
            while (done == null) {
                var call = (Call<A>)bounce;
                bounce = call.thunk();
                done = bounce as Done<A>;
            }


            return done.result;
        }
    }

    public class Factorial {
        public static Bounce<Decimal> factorial(int n, Decimal sum) {
            if (n <= 1) {
                return new Done<Decimal>(sum);
            } else {
                return new Call<Decimal>(() => factorial(n - 1, n * sum));
            }
        }
    }

    public class Fibonacci {
        public static Bounce<double> fibonacci(Decimal term, double val,
            double prev) {
            if (term == 0) {
                return new Done<double>(prev);
            } else if (term == 1) {
                return new Done<double>(val);
            } else {
                return new Call<double>(() => fibonacci(term - 1, val + prev, val));
            }
        }
    }
}
