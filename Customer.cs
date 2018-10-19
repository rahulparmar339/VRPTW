using System;

namespace VRPTW
{
    class Customer : ICloneable
    {
        public int id { get; set; }
        public double x_coordinate { get; set; }
        public double y_coordinate { get; set; }
        public double demand { get; set; }
        public double timeWindow_start { get; set; }
        public double timeWindow_end { get; set; }
        public double serviceTime { get; set; }

        public Customer(int id, double x_coordinate, double y_coordinate, double demand, double timeWindow_start, double timeWindow_end, double serviceTime)
        {
            this.id = id;
            this.x_coordinate = x_coordinate;
            this.y_coordinate = y_coordinate;
            this.demand = demand;
            this.timeWindow_start = timeWindow_start;
            this.timeWindow_end = timeWindow_end;
            this.serviceTime = serviceTime;
        }
        public Customer()
        {

        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}