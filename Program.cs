using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VRPTW
{
    class Program
    {
        static void Main(string[] args)
        {
            Depo depo = new Depo();
            Vehicle vehicle = new Vehicle();
            List<Customer> customerList = new List<Customer>();

            vehicle.id = 0;
            vehicle.capacity = 200;
            bool isDepo = true;
            int customerId = 0;

            //Getting customer data from data.txt which contains data from soloman's dataset 
            //URL for soloman's dataset http://web.cba.neu.edu/~msolomon/r101.htm (replace "  " with " " in data.txt)
            foreach (string line in File.ReadLines(@"/home/rahul/vscode-workspace/VRPTW/data.txt", Encoding.UTF8))
            {
                line.Trim();
                string[] data = line.Split(" ");
                if (isDepo)
                {
                    depo.id = customerId++;
                    depo.x_coordinate = double.Parse(data[2]);
                    depo.y_coorninate = double.Parse(data[3]);
                    depo.demand = double.Parse(data[4]);
                    depo.timeWindow_start = double.Parse(data[5]); ;
                    depo.timeWindow_end = double.Parse(data[6]);
                    depo.serviceTime = double.Parse(data[7]);
                    isDepo = false;
                }
                else
                {
                    Customer customer = new Customer();
                    customer.id = customerId++;
                    customer.x_coordinate = double.Parse(data[2]);
                    customer.y_coordinate = double.Parse(data[3]);
                    customer.demand = double.Parse(data[4]);
                    customer.timeWindow_start = double.Parse(data[5]); ;
                    customer.timeWindow_end = double.Parse(data[6]);
                    customer.serviceTime = double.Parse(data[7]);
                    customerList.Add(customer);
                    //Console.WriteLine(customer.x_coordinate+" "+ customer.y_coordinate+" "+ customer.demand+" "+ customer.timeWindow_start+" "+ customer.timeWindow_end);
                }
            }

            //inserting depo as customer at beggining
            customerList.Insert(0, Utils.DepoAsCustomer(depo));

            //calculating customer to customer distance (0 index represents depo)
            int noOfCustomer = customerList.Count;
            double[][] cTocDistance = new double[noOfCustomer][];
            for (int i = 0; i < noOfCustomer; i++)
            {
                cTocDistance[i] = new double[noOfCustomer];
                for (int j = 0; j < noOfCustomer; j++)
                {
                    cTocDistance[i][j] = Utils.CalculateDistance(customerList[i].x_coordinate,
                                                 customerList[i].y_coordinate,
                                                 customerList[j].x_coordinate,
                                                 customerList[j].y_coordinate);
                }
            }

            SimulatedAnnealing simulatedAnnealing = new SimulatedAnnealing(depo, vehicle, customerList, cTocDistance);
            simulatedAnnealing.run(100, 1, 0.98, 6000);
        }
        
    }
}
