using System;

namespace VRPTW
{
    class Depo
    {
        public int id { get; set; }
        public double x_coordinate { get; set; }
        public double y_coorninate { get; set; }
        public double demand { get; set; }
        public double timeWindow_start { get; set; }
        public double timeWindow_end { get; set; }
        public double serviceTime { get; set; }
    }
}