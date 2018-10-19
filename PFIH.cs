using System;
using System.Collections.Generic;
using System.Text;

namespace VRPTW
{
    class PFIH
    {
        private int noOfCustomer;
        public Depo depo { get; set; }
        public Vehicle vehicle { get; set; }
        public List<Customer> customerList { get; set; }
        private double[][] cTocDistance;

        public PFIH(Depo depo, Vehicle vehicle, List<Customer> customerList, double[][] cTocDistance)
        {
            this.depo = depo;
            this.vehicle = vehicle;
            this.customerList = customerList;
            this.noOfCustomer = customerList.Count;
            this.cTocDistance = cTocDistance;
        }

        //constructing solution(List<List<Customer>>)  using push forward insertion hueristic(PFIH)
        public List<List<Customer>> Construct()
        {
            //removing depo 
            customerList.RemoveAt(0);

            List<List<Customer>> routeList = new List<List<Customer>>();
            List<Customer> unroutedCustomerList = new List<Customer>();
            unroutedCustomerList.AddRange(customerList);

            while (unroutedCustomerList.Count > 0)
            {
                List<Customer> currentRouteCustomerList = new List<Customer>();
                Customer seedCustomer = FindSeedCustomer(unroutedCustomerList);

                //adding depo as first customer
                currentRouteCustomerList.Add(Utils.DepoAsCustomer(depo));
                currentRouteCustomerList.Add(seedCustomer);
                //adding depo as last customer
                currentRouteCustomerList.Add(Utils.DepoAsCustomer(depo));

                unroutedCustomerList.Remove(seedCustomer);

                bool placementPossible = true;
                while (placementPossible)
                {
                    double minCost = Double.MaxValue;
                    int insertionIndex = 0;
                    Customer selectedCustomer = null;

                    foreach (Customer unroutedCustomer in unroutedCustomerList)
                    {
                        for (int currentRouteCustomerIndex = 1; currentRouteCustomerIndex < currentRouteCustomerList.Count; currentRouteCustomerIndex++)
                        {
                            double cost = CostOfInsertionInRoute(currentRouteCustomerList, currentRouteCustomerIndex, unroutedCustomer);
                            if (cost < minCost)
                            {
                                minCost = cost;
                                insertionIndex = currentRouteCustomerIndex;
                                selectedCustomer = unroutedCustomer;
                            }
                        }
                    }
                    if (minCost != Double.MaxValue)
                    {
                        currentRouteCustomerList.Insert(insertionIndex, selectedCustomer);
                        unroutedCustomerList.Remove(selectedCustomer);
                    }
                    else
                    {
                        placementPossible = false;
                    }
                }
                routeList.Add(currentRouteCustomerList);
            }
            return routeList;
        }

        //finds seed customer for new route from given unrouted customers
        public Customer FindSeedCustomer(List<Customer> unroutedCustomerList)
        {
            double minCost = Double.MaxValue;
            Customer seedCustomer = null;

            foreach (Customer customer in unroutedCustomerList)
            {
                double distanceFromDepo = Utils.CalculateDistance(depo.x_coordinate, depo.y_coorninate,
                                                                customer.x_coordinate, customer.y_coordinate);
                double cost = CostOfSeedCustomer(distanceFromDepo, customer.timeWindow_start, 0);
                if (cost < minCost)
                {
                    minCost = cost;
                    seedCustomer = customer;
                }
            }
            return seedCustomer;
        }

        //calculates cost of customer for selectioning it as seed customer
        public double CostOfSeedCustomer(double disFromDepo, double latestDeadline, double angleFromLastInsertedCustomer)
        {
            double alpha = 0.7;
            double beta = 0.2;
            double gema = 0.1;
            return -alpha * disFromDepo + beta * latestDeadline + gema * angleFromLastInsertedCustomer;
        }

        //calculates cost of insertion for given customer in given vehicle route at specific index
        public double CostOfInsertionInRoute(List<Customer> customerList, int index, Customer customer)
        {
            customerList.Insert(index, customer);
            bool feasible = Utils.CheckTimeConstraint(customerList, cTocDistance) &&
                            Utils.CheckCapacityConstraint(customerList, vehicle.capacity);
            if (feasible)
            {
                double newTotalDistance = Utils.CalculateTotalDistance(customerList, cTocDistance);
                double theta = 0.01 * newTotalDistance;

                customerList.RemoveAt(index);
                return newTotalDistance + theta * Utils.CalculateTotalRouteTime(customerList, cTocDistance);
            }

            customerList.RemoveAt(index);
            return Double.MaxValue;
        }

        // public bool CheckTimeConstraint(ArrayList customerList)
        // {
        //     double arrivalTime = 0;
        //     for(int customerIndex=0; customerIndex<customerList.Count; customerIndex++)
        //     {
        //         int customer = (int)customerList[customerIndex];

        //         if (customerIndex == 0)
        //         {
        //             arrivalTime += (vTocDis[0][customer]);
        //         }
        //         else
        //         {
        //             int previousCustomer = (int)customerList[customerIndex - 1];
        //             arrivalTime += 0.10 + (cTocDistance[previousCustomer][customer]);
        //         }

        //         if(arrivalTime > timeWindow[customer][1])
        //         {
        //             return false;
        //         }
        //         customerIndex++;
        //     }
        //     return true;
        // }

        // public bool CheckCapacityConstraint(ArrayList customerList)
        // {
        //     double currCapacity = 0;
        //     foreach (int customer in customerList)
        //     {
        //         currCapacity += demand[customer];
        //         if(currCapacity > capacity[0])
        //         {
        //             return false;
        //         }
        //     }
        //     return true;
        // }

        // public double TotalDistance(ArrayList customerList)
        // {
        //     double totalDistance = 0;
        //     for(int customerIndex = 0; customerIndex < customerList.Count; customerIndex++)
        //     {
        //         int customer = (int)customerList[customerIndex];
        //         if (customerIndex == 0)
        //         {
        //             totalDistance += vTocDis[0][customer];
        //         }
        //         else
        //         {
        //             int previousCustomer = (int)customerList[customerIndex - 1];
        //             totalDistance += cTocDistance[previousCustomer][customer];
        //         }
        //     }
        //     return totalDistance;
        // }

        // public double TotalRouteTime(ArrayList customerList)
        // {
        //     double currTime = 0;
        //     for (int customerIndex = 0; customerIndex < customerList.Count; customerIndex++)
        //     {
        //         int customer = (int)customerList[customerIndex];
        //         if (customerIndex == 0)
        //         {
        //             currTime += (vTocDis[0][customer]);
        //         }
        //         else
        //         {
        //             int previousCustomer = (int)customerList[customerIndex - 1];
        //             currTime += (cTocDistance[previousCustomer][customer]);
        //         }
        //         if(currTime < timeWindow[customer][0])
        //         {
        //             currTime = timeWindow[customer][0];
        //         }
        //         currTime += 0.10;
        //         customerIndex++;
        //     }
        //     return currTime;
        // }
    }
}

