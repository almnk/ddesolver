using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace ddesolver
{
    public class DDESolver
    {
        private long time;

        Stopwatch timer = new Stopwatch();

        public long getTime() { return time; }
         
        public enum interval : int
        {
            closed = 1,       //[.,.]
            opened = 2,       //(.,.)
            half_opened = 3,  //[.,.)
            half_closed = 4   //(.,.]
        }

        public enum order : int
        {
            first = 1,
            second = 2,
            third = 3,
            fourth = 4
        }

        public struct Point
        {
            public double x;
            public double y;
        }

        public struct BoundaryValues
        {
            public double a;
            public double b;
            public interval type;
        }

        public DDESolver() { }

        // get y from y=f(x), where f=_in - array of (x0,y0),...,(xn,yn)
        //simply get y by x
        /*
        //for many y by one x
        public IEnumerable<double> f(double x, List<Point> _in)
        {
            Func<Point, bool>   _filter  = point => point.x == x;
            return _in.Where(_filter).Select<Point, double>(point => point.y);
        }
        */
        //for one y by one x
        public double f(double x, List<Point> _in)
        {
            Func<Point, bool> _filter = point => point.x == x;
            return _in.Where(_filter).Single().y;
        }

        // linear interpolation
        public double LinIn(double value, Point x, Point y)
        {
            return Math.Abs((((value-x.x)*(y.y-x.y))/(y.x-x.x))-y.y);
        }

        public double round(double value) { return Math.Round(value, 12); }

        public List<Point> StepMethod(
                            double step,                       //just step
                            BoundaryValues of_initial,         //[a,b]
                            BoundaryValues of_solution,        //[a,b]
                            Func<double/* x */, double/* y(x) */, double/* y(x-tao(x)) */, double/* result */> f_main,     // y'(x) = f(x,y(x),y(x-tao(x))) derivative
                            Func<double/* x */, double/* result */> f_initial,                                             // y(x) = g(x) on x: [t,T] initial function on initial boundary values
                            Func<double/* x */, double/* result */> f_tao                                                  // tao(x) = w(x)
                         )
        {
            List<Point> solution = new List<Point>();
            List<Point> f_tmp = new List<Point>();

            timer.Reset();
            timer.Start();
            
            // how much iteration will be
            int iter = 1;
            double a=of_initial.b;
            double b=of_solution.b;
            while (a < b) 
            {
                a = a + f_tao(a);
                iter += 1;
            }


            // its for calculating without aprox
            double h = round(step / Math.Pow(2,iter));

            double from=(of_initial.type==interval.half_opened ||  of_initial.type==interval.closed)? of_initial.a : of_initial.a+step/2; // if [*,.] or [*,.) begin from * else from *+step 
            double to=(of_initial.type==interval.half_closed ||  of_initial.type==interval.closed)? of_initial.b : of_initial.b-step/2;
            double to_init = to;
            
            while (from <= to)
            {
                f_tmp.Add(new Point { x = from, y = f_initial(from) });
                from = round(from + h);
            }

            //from -=h ; // not a, but from last of initial function argument
            from = to_init;
            to = round(from + f_tao(from));
            iter -= 1;
            h = round(step / Math.Pow(2, iter)); //resize step

            while (from <= b)//prew ver @to<=b@
            {
                double y = 0, k = 0, ksum = 0, x = round(from);
                while (x < to) 
                {
                    k = f_main(x, f(x, f_tmp), f(round(x - f_tao(x)), f_tmp));
                    ksum += k;

                    k = f_main(round(x + h / 2), f(x, f_tmp) + h * k / 2, f(round(x + h / 2 - f_tao(x + h / 2)), f_tmp));
                    ksum += 2 * k;

                    k = f_main(round(x + h / 2), f(x, f_tmp) + h * k / 2, f(round(x + h / 2 - f_tao(x + h / 2)), f_tmp));
                    ksum += 2 * k;

                    k = f_main(x + h, f(x, f_tmp) + h * k, f(round(x + h - f_tao(x + h)), f_tmp));
                    ksum += k;

                    ksum *= h/6;

                    y = f(x, f_tmp) + ksum;
                    x = round(x+h);
                    f_tmp.Add(new Point {x=x, y=y} );
                }

                //next interval x,x+tao
                from = to;
                to = round(from + f_tao(from));

                iter -= 1;
                h = round(step / Math.Pow(2, iter));
            }
            
            timer.Stop(); time = timer.ElapsedMilliseconds;
            
            // return solution
            from = (of_solution.type == interval.half_opened || of_solution.type == interval.closed) ? of_solution.a : of_solution.a + step;
            to = (of_solution.type == interval.half_closed || of_solution.type == interval.closed) ? of_solution.b : of_solution.b - step / 2;
            while (from <= to)
            {
                solution.Add(new Point {x=from,y=f(from,f_tmp)});
                from = round(from + step);
            }

            return solution;
        }

        public List<Point> EulerMethod(
                            double step,                       //just step
                            BoundaryValues of_initial,         //[a,b]
                            BoundaryValues of_solution,        //[a,b]
                            Func<double/* x */, double/* y(x) */, double/* y(x-tao(x)) */, double/* result */> f_main,     // y'(x) = f(x,y(x),y(x-tao(x))) derivative
                            Func<double/* x */, double/* result */> f_initial,                                             // y(x) = g(x) on x: [t,T] initial function on initial boundary values
                            Func<double/* x */, double/* result */> f_tao)
        {
            List<Point> solution = new List<Point>();
            List<Point> f_tmp = new List<Point>();

            timer.Reset();
            timer.Start();

            // how much iteration will be
            int iter = 1;
            double a = of_initial.b;
            double b = of_solution.b;
            while (a < b && step <= (b - a) / iter)
            {
                a = a + f_tao(a);
                iter += 1;
            }
            iter -= 1;
            a = of_initial.b;
            b = of_solution.b;

            double h = ( step <= (b - a) / iter) ? step : (b - a) / iter;
            double from=(of_initial.type==interval.half_opened ||  of_initial.type==interval.closed)? of_initial.a : of_initial.a+step/2; // if [*,.] or [*,.) begin from * else from *+step 
            double to=(of_initial.type==interval.half_closed ||  of_initial.type==interval.closed)? of_initial.b : of_initial.b-step/2;
            double to_init = to;
            
            while (from <= to)
            {
                f_tmp.Add(new Point { x = from, y = f_initial(from) });
                from = round(from + h);
            }

            //from -=h ; // not a, but from last of initial function argument
            from = to_init;
            to = round(from + f_tao(from));

            while (from <= b)
            {
                double y = 0, x = round(from);
                while (x < to)
                {

                    y = f(x, f_tmp) + h * f_main(x, f(x, f_tmp), f(round(x-f_tao(x)), f_tmp));
                    x = round(x + h);
                    f_tmp.Add(new Point { x = x, y = y });
                }

                //next interval x,x+tao
                from = to;
                to = round(from + f_tao(from));

            }
            
            timer.Stop(); time = timer.ElapsedMilliseconds;
            
            // return solution
            from = (of_solution.type == interval.half_opened || of_solution.type == interval.closed) ? of_solution.a : of_solution.a + step;
            to = (of_solution.type == interval.half_closed || of_solution.type == interval.closed) ? of_solution.b : of_solution.b - step / 2;
            while (from <= to)
            {
                solution.Add(new Point { x = from, y = f(from, f_tmp) });
                from = round(from + step);
            }

            return solution;
        }

        public List<Point> RKMethod(
                            double step,                       //just step
                            BoundaryValues of_initial,         //[a,b]
                            BoundaryValues of_solution,        //[a,b]
                            Func<double/* x */, double/* y(x) */, double/* y(x-tao(x)) */, double/* result */> f_main,     // y'(x) = f(x,y(x),y(x-tao(x))) derivative
                            Func<double/* x */, double/* result */> f_initial,                                             // y(x) = g(x) on x: [t,T] initial function on initial boundary values
                            Func<double/* x */, double/* result */> f_tao)
        {
            List<Point> solution = new List<Point>();
            List<Point> f_tmp = new List<Point>();

            timer.Reset();
            timer.Start();

            // how much iteration will be
            int iter = 1;
            double a = of_initial.b;
            double b = of_solution.b;
            while (a < b && step <= (b - a) / iter)
            {
                a = a + f_tao(a);
                iter += 1;
            }
            iter -= 1;
            a = of_initial.b;
            b = of_solution.b;

            double h = (step <= (b - a) / iter) ? step : (b - a) / iter;
            double from = (of_initial.type == interval.half_opened || of_initial.type == interval.closed) ? of_initial.a : of_initial.a + step / 2; // if [*,.] or [*,.) begin from * else from *+step 
            double to = (of_initial.type == interval.half_closed || of_initial.type == interval.closed) ? of_initial.b : of_initial.b - step / 2;
            double to_init = to;

            while (from <= to)
            {
                f_tmp.Add(new Point { x = from, y = f_initial(from) });
                from = round(from + h);
            }

            //from -=h ; // not a, but from last of initial function argument
            from = to_init;
            to = round(from + f_tao(from));

            while (from <= b)
            {
                double y = 0, k = 0, ksum = 0, x = round(from); double ft;
                while (x < to)
                {
                    k = f_main(x, f(x, f_tmp), f(round(x - f_tao(x)), f_tmp));
                    ksum += k;

                    
                    ft = LinIn(x + h / 2, new Point { x = x, y = f(round(x - f_tao(x)), f_tmp) }, new Point { x = x + h, y = f(round(x + h - f_tao(x + h)), f_tmp) });
                    
                    k = f_main(round(x + h / 2), f(x, f_tmp) + h * k / 2, ft);
                    ksum += 2 * k;

                    ft = LinIn(x + h / 2, new Point { x = x, y = f(round(x - f_tao(x)), f_tmp) }, new Point { x = x + h, y = f(round(x + h - f_tao(x + h)), f_tmp) });
                    
                    k = f_main(round(x + h / 2), f(x, f_tmp) + h * k / 2, ft);
                    ksum += 2 * k;

                    k = f_main(x + h, f(x, f_tmp) + h * k, f(round(x + h - f_tao(x + h)), f_tmp));
                    ksum += k;

                    ksum *= h / 6;

                    y = f(x, f_tmp) + ksum;
                    x = round(x + h);
                    f_tmp.Add(new Point { x = x, y = y });
                }

                //next interval x,x+tao
                from = to;
                to = round(from + f_tao(from));
            }
            
            timer.Stop(); time = timer.ElapsedMilliseconds;

            // return solution
            from = (of_solution.type == interval.half_opened || of_solution.type == interval.closed) ? of_solution.a : of_solution.a + step;
            to = (of_solution.type == interval.half_closed || of_solution.type == interval.closed) ? of_solution.b : of_solution.b - step / 2;
            while (from <= to)
            {
                solution.Add(new Point { x = from, y = f(from, f_tmp) });
                from = round(from + step);
            }

            return solution;
        }

        public List<Point> IMTMethod(
                            double step,                       //just step
                            BoundaryValues of_initial,         //[a,b]
                            BoundaryValues of_solution,        //[a,b]
                            Func<double/* x */, double/* y(x) */, double/* y(x-tao(x)) */, double/* result */> f_main,     // y'(x) = f(x,y(x),y(x-tao(x))) derivative
                            Func<double/* x */, double/* result */> f_initial,                                             // y(x) = g(x) on x: [t,T] initial function on initial boundary values
                            Func<double/* x */, double/* result */> f_tao)
        {
            List<Point> solution = new List<Point>();
            List<Point> f_tmp = new List<Point>();

            timer.Reset();
            timer.Start();

            // how much iteration will be
            int iter = 1;
            double a = of_initial.b;
            double b = of_solution.b;
            while (a < b && step <= (b - a) / iter)
            {
                a = a + f_tao(a);
                iter += 1;
            }
            iter -= 1;
            a = of_initial.b;
            b = of_solution.b;

            double h = (step <= (b - a) / iter) ? step : (b - a) / iter;
            double from = (of_initial.type == interval.half_opened || of_initial.type == interval.closed) ? of_initial.a : of_initial.a + step / 2; // if [*,.] or [*,.) begin from * else from *+step 
            double to = (of_initial.type == interval.half_closed || of_initial.type == interval.closed) ? of_initial.b : of_initial.b - step / 2;
            double to_init = to;

            while (from <= to)
            {
                f_tmp.Add(new Point { x = from, y = f_initial(from) });
                from = round(from + h);
            }

            //from -=h ; // not a, but from last of initial function argument
            from = to_init;
            to = round(from + f_tao(from));

            while (from <= b)
            {
                double x = round(from), yi = 0, yii = 0; double y = 0;
                while (x < to)
                {
                    y = f(x, f_tmp) + h * f_main(x, f(x, f_tmp), f(round(x - f_tao(x)), f_tmp));
                    yi = f_main(x, f(x, f_tmp), f(round(x - f_tao(x)), f_tmp));
                    yii = f_main(x+h, y, f(round(x+h - f_tao(x+h)), f_tmp));
                    if(yii-yi!=0 && ((yii/yi)>=0))
                    y = f(x, f_tmp) + h * ((yii-yi)/Math.Log(yii/yi, Math.E));
                    
                    x = round(x + h);
                    f_tmp.Add(new Point { x = x, y = y });
                }

                //next interval x,x+tao
                from = to;
                to = round(from + f_tao(from));

            }
            
            timer.Stop(); time = timer.ElapsedMilliseconds;

            // return solution
            from = (of_solution.type == interval.half_opened || of_solution.type == interval.closed) ? of_solution.a : of_solution.a + step;
            to = (of_solution.type == interval.half_closed || of_solution.type == interval.closed) ? of_solution.b : of_solution.b - step / 2;
            while (from <= to)
            {
                solution.Add(new Point { x = from, y = f(from, f_tmp) });
                from = round(from + step);
            }

            return solution;
        }

        public List<Point> EMTMethod(
                            double step,                       //just step
                            BoundaryValues of_initial,         //[a,b]
                            BoundaryValues of_solution,        //[a,b]
                            Func<double/* x */, double/* y(x) */, double/* y(x-tao(x)) */, double/* result */> f_main,     // y'(x) = f(x,y(x),y(x-tao(x))) derivative
                            Func<double/* x */, double/* result */> f_initial,                                             // y(x) = g(x) on x: [t,T] initial function on initial boundary values
                            Func<double/* x */, double/* result */> f_tao)
        {
            List<Point> solution = new List<Point>();
            List<Point> f_tmp = new List<Point>();

            timer.Reset();
            timer.Start();

            // how much iteration will be
            int iter = 1;
            double a = of_initial.b;
            double b = of_solution.b;
            while (a < b && step <= (b - a) / iter)
            {
                a = a + f_tao(a);
                iter += 1;
            }
            iter -= 1;
            a = of_initial.b;
            b = of_solution.b;

            double h = (step <= (b - a) / iter) ? step : (b - a) / iter;
            double from = (of_initial.type == interval.half_opened || of_initial.type == interval.closed) ? of_initial.a : of_initial.a + step / 2; // if [*,.] or [*,.) begin from * else from *+step 
            double to = (of_initial.type == interval.half_closed || of_initial.type == interval.closed) ? of_initial.b : of_initial.b - step / 2;
            double to_init = to;
            from -= h;
            while (from <= to)
            {
                f_tmp.Add(new Point { x = from, y = f_initial(from) });
                from = round(from + h);
            }

            //from -=h ; // not a, but from last of initial function argument
            from = to_init;
            to = round(from + f_tao(from));

            while (from <= b)
            {
                double x = round(from), yi = 0, yii = 0; double y = 0;
                while (x < to)
                {
                    
                    yi = f_main(x, f(x, f_tmp), f(round(x - f_tao(x)), f_tmp));
                    y = f(x, f_tmp) + h * yi;
                    yii = f_main(x - h, f(round(x-h), f_tmp), f(round(x - h - f_tao(x - h)), f_tmp));
                    if (yii - yi != 0 && ((yi / yii) >= 0))
                    y = f(x, f_tmp) + h * ((yi-yii)/Math.Log(yi/yii, Math.E))*(yi/yii);
                    
                    x = round(x + h);
                    f_tmp.Add(new Point { x = x, y = y });
                }

                //next interval x,x+tao
                from = to;
                to = round(from + f_tao(from));

            }
            timer.Stop(); time = timer.ElapsedMilliseconds;

            // return solution
            from = (of_solution.type == interval.half_opened || of_solution.type == interval.closed) ? of_solution.a : of_solution.a + step;
            to = (of_solution.type == interval.half_closed || of_solution.type == interval.closed) ? of_solution.b : of_solution.b - step / 2;
            while (from <= to)
            {
                solution.Add(new Point { x = from, y = f(from, f_tmp) });
                from = round(from + step);
            }

            return solution;
        }

        public List<Point> HalfStepMethod(
                    double step,                       //just step
                    BoundaryValues of_initial,         //[a,b]
                    BoundaryValues of_solution,        //[a,b]
                    Func<double/* x */, double/* y(x) */, double/* y(x-tao(x)) */, double/* result */> f_main,     // y'(x) = f(x,y(x),y(x-tao(x))) derivative
                    Func<double/* x */, double/* result */> f_initial,                                             // y(x) = g(x) on x: [t,T] initial function on initial boundary values
                    Func<double/* x */, double/* result */> f_tao)
        {
            List<Point> solution = new List<Point>();
            List<Point> f_tmp = new List<Point>();

            timer.Reset();
            timer.Start();

            // how much iteration will be
            int iter = 1;
            double a = of_initial.b;
            double b = of_solution.b;
            while (a < b)
            {
                a = a + f_tao(a);
                iter += 1;
            }
            iter -= 1;
            a = of_initial.b;
            b = of_solution.b;

            double h = (step <= (b - a) / iter) ? step : (b - a) / iter;
            double from = (of_initial.type == interval.half_opened || of_initial.type == interval.closed) ? of_initial.a : of_initial.a + step / 2; // if [*,.] or [*,.) begin from * else from *+step 
            double to = (of_initial.type == interval.half_closed || of_initial.type == interval.closed) ? of_initial.b : of_initial.b - step / 2;
            double to_init = to;

            while (from <= to)
            {
                f_tmp.Add(new Point { x = from, y = f_initial(from) });
                from = round(from + h);
            }

            //from -=h ; // not a, but from last of initial function argument
            from = to_init;
            to = round(from + f_tao(from));

            double y;// = f(from, f_tmp) + h * f_main(from, f(from, f_tmp), f(round(from - f_tao(from)), f_tmp));
            //f_tmp.Add(new Point { x = round(from + h), y = y });
            //from = round(from + h);
            while (to <= b)
            {
                double x = round(from), yi = 0, yii = 0, yc=0; y = 0;
                while (x < to)
                {
                    //y = f(x, f_tmp) + h * f_main(x, f(x, f_tmp), f(round(x - f_tao(x)), f_tmp));
                    yi = f_main(x, f(x, f_tmp), f(round(x - f_tao(x)), f_tmp));
                    //yii = f_main(x + h, y, f(round(x + h - f_tao(x + h)), f_tmp));
                    //yc = ((yii - yi) / Math.Log(yii / yi, Math.E));
                    //yc = yii / Math.Log((yii/yi)+Math.E, Math.E) ;
                    if (yii - yi != 0)
                        y = f(x, f_tmp) + yi;//(h/   4)*(((yc + yi) * (yc + yc)) / (yi/2+yc/2));

                    x = round(x + h);
                    f_tmp.Add(new Point { x = x, y = y });
                }

                //next interval x,x+tao
                from = to;
                to = round(from + f_tao(from));

            }
            timer.Stop(); time = timer.ElapsedMilliseconds;

            // return solution
            from = (of_solution.type == interval.half_opened || of_solution.type == interval.closed) ? of_solution.a : of_solution.a + step;
            to = (of_solution.type == interval.half_closed || of_solution.type == interval.closed) ? of_solution.b : of_solution.b - step / 2;
            while (from <= to)
            {
                solution.Add(new Point { x = from, y = f(from, f_tmp) });
                from = round(from + step);
            }

            return solution;
        }
    }

}
