using System;
using System.Collections.Generic;

namespace VRPTW
{
    class Utils
    {
        //generating random solution(List<List<Customer>>) 
        public static List<List<Customer>> GenerateRandomRouteList(Depo depo, Vehicle vehicle, List<Customer> customerList, double[][] cTocDistance)
        {
            //removing depo 
            customerList.RemoveAt(0);

            List<List<Customer>> routeList = new List<List<Customer>>();
            List<Customer> unroutedCustomerList = new List<Customer>();
            unroutedCustomerList.AddRange(customerList);

            while(unroutedCustomerList.Count > 0)
            {
                List<Customer> currentRouteCustomerList = new List<Customer>();
                //adding depo as first customer
                currentRouteCustomerList.Add(DepoAsCustomer(depo));
                //adding depo as last customer
                currentRouteCustomerList.Add(DepoAsCustomer(depo));

                int currentRouteCustomerIndex = 1;
                bool feasible = false;

                for (int unroutedCustomerIndex = 0; unroutedCustomerIndex < unroutedCustomerList.Count; unroutedCustomerIndex++)
                {
                    Customer unroutedCustomer = unroutedCustomerList[unroutedCustomerIndex];
                    currentRouteCustomerList.Insert(currentRouteCustomerIndex, unroutedCustomer);
                    feasible = CheckCapacityConstraint(currentRouteCustomerList, vehicle.capacity) &&
                                CheckTimeConstraint(currentRouteCustomerList, cTocDistance);
                    if (feasible)
                    {
                        unroutedCustomerList.RemoveAt(unroutedCustomerIndex);
                        currentRouteCustomerIndex++;
                    }
                    else
                    {
                        currentRouteCustomerList.RemoveAt(currentRouteCustomerIndex);
                    }
                }
                routeList.Add(currentRouteCustomerList);
            }
            return routeList;
        }

        //calculating distance between two points
        public static double CalculateDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }

        //checking time constraint of given vehicel route(brute force which can be improved) 
        public static bool CheckTimeConstraint(List<Customer> customerList, double[][] cTocDistance)
        {
            double arrivalTime = 0;
            Customer previousCustomer = null;

            foreach (Customer customer in customerList)
            {
                if (previousCustomer != null)
                {
                    arrivalTime += previousCustomer.serviceTime;
                    arrivalTime += cTocDistance[previousCustomer.id][customer.id];
                }
                if (arrivalTime < customer.timeWindow_start)
                {
                    arrivalTime = customer.timeWindow_start;
                }
                if (arrivalTime > customer.timeWindow_end)
                {
                    return false;
                }
                previousCustomer = customer;
            }
            return true;
        }

        //checking capacity constraint of given vehicle route all vehicel capacity is taken as 200
        public static bool CheckCapacityConstraint(List<Customer> customerList, double capacity)
        {
            double currCapacity = 0;
            foreach (Customer customer in customerList)
            {
                currCapacity += customer.demand;
                if (currCapacity > capacity)
                {
                    return false;
                }
            }
            return true;
        }

        //calculating total distance of given vehicle route
        public static double CalculateTotalDistance(List<Customer> customerList, double[][] cTocDistance)
        {
            double totalDistance = 0;
            Customer previousCustomer = null;
            foreach (Customer customer in customerList)
            {
                if (previousCustomer != null)
                {
                    totalDistance += cTocDistance[previousCustomer.id][customer.id];
                }
                previousCustomer = customer;
            }
            return totalDistance;
        }

        //calculating total distance of all vehicle routes
        public static double CalculateTotalDistanceOfAllRoute(List<List<Customer>> routeList, double[][] cTocDistance)
        {
            double totalDistance = 0;
            foreach (List<Customer> route in routeList)
            {
                totalDistance += CalculateTotalDistance(route, cTocDistance);
            }
            return totalDistance;
        }

        //calculating total route time of all vehicle routes
        public static double CalculateTotalRouteTime(List<Customer> customerList, double[][] cTocDistance)
        {
            double currTime = 0;
            Customer previousCustomer = null;
            foreach (Customer customer in customerList)
            {
                if (previousCustomer != null)
                {
                    currTime += cTocDistance[previousCustomer.id][customer.id];
                }
                if (currTime < customer.timeWindow_start)
                {
                    currTime = customer.timeWindow_start;
                }
                currTime += customer.serviceTime;
                previousCustomer = customer;
            }
            return currTime;
        }

        //converting depo into customer
        public static Customer DepoAsCustomer(Depo depo)
        {
            Customer customer = new Customer();
            customer.id = depo.id;
            customer.x_coordinate = depo.x_coordinate;
            customer.y_coordinate = depo.y_coorninate;
            customer.demand = depo.demand;
            customer.timeWindow_start = depo.timeWindow_start;
            customer.timeWindow_end = depo.timeWindow_end;
            customer.serviceTime = depo.serviceTime;
            return customer;
        }

        //printing totaldistance of all routes and routes information
        public static void Print(List<List<Customer>> routeList, double[][] cTocDistance)
        {
            int usedVehicleCount = 0;
            for(int i = 0; i < routeList.Count; i++){
                if(routeList[i].Count >=3 )
                    usedVehicleCount++;
            }
            Console.WriteLine("Total Vehicle used "+usedVehicleCount+" out of "+routeList.Count);
            
            double totalDis = 0;
            for (int i = 0; i < routeList.Count; i++)
            {
                List<Customer> route = routeList[i];
                Console.WriteLine("Total Distance of Route" + i + " :" + Utils.CalculateTotalDistance(route, cTocDistance));
                totalDis += Utils.CalculateTotalDistance(route, cTocDistance);
            }
            Console.WriteLine("Total Distance Of All Routes: " + totalDis);

            for (int i = 0; i < routeList.Count; i++)
            {
                List<Customer> route = routeList[i];
                for (int j = 0; j < route.Count; j++)
                {
                    Console.Write(route[j].id + " ");
                    //Console.WriteLine(route[j].x_coordinate+" "+route[j].y_coordinate+" "+route[j].timeWindow_start+" "+route[j].timeWindow_end);
                }
                Console.WriteLine();
            }
        }

        //printing total distance of all routes
        public static void PrintTD(List<List<Customer>> routeList, double[][] cTocDistance)
        {
            double totalDis = 0;
            for (int i = 0; i < routeList.Count; i++)
            {
                List<Customer> route = routeList[i];
                totalDis += Utils.CalculateTotalDistance(route, cTocDistance);
            }
            Console.WriteLine("Total Distance Of All Routes: " + totalDis);
        }

        //copying solution(List<List<Customer>>) 
        public static void CopyListOfList(List<List<Customer>> listFrom, List<List<Customer>> listTo)
        {
            foreach (List<Customer> route in listFrom)
            {
                List<Customer> newCustomerList = new List<Customer>();
                foreach (Customer customer in route)
                {
                    newCustomerList.Add((Customer)customer.Clone());
                }
                listTo.Add(newCustomerList);
            }
        }
    }
}