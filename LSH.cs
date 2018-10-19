using System;
using System.Collections.Generic;
using System.Text;


namespace VRPTW
{
    class LSH
    {
        public double[][] cTocDistance { get; set; }
        public Vehicle vehicle { get; set; }

        public LSH(double[][] cTocDistance, Vehicle vehicle)
        {
            this.cTocDistance = cTocDistance;
            this.vehicle = vehicle;
        }

        //global best lambda interchange(local search heuristic) 
        //selects best(min cost)solution across all combinations of 2 routes (k*(k-1)/2), k = no of routes
        //lambda is fixed to 2
        public void LambdaInterchangeGB(ref List<List<Customer>> routeList)
        {
            int lamda = 2;
            double globalMaxDif = 0;
            int globalFirstRouteIndex = 0;
            int globalSecondRouteIndex = 0;
            List<Customer> globalMinCostFirstRoute = null;
            List<Customer> globalMinCostSecondRoute = null;

            for(int i = 0; i < routeList.Count; i++)
            {
                List<Customer> firstRoute = routeList[i];
                for(int j = i+1; j < routeList.Count; j++)
                {
                    List<Customer> secondRoute = routeList[j];
                    List<Customer> localMinCostFirstRoute = null;
                    List<Customer> localMinCostSecondRoute = null;
                    double oldDistance = Utils.CalculateTotalDistance(firstRoute, cTocDistance) +
                                            Utils.CalculateTotalDistance(secondRoute, cTocDistance);
                    double newDistance = Interchange(firstRoute, secondRoute, ref localMinCostFirstRoute, ref localMinCostSecondRoute, lamda);
                    
                    if (oldDistance - newDistance > globalMaxDif)
                    {
                        globalMaxDif = oldDistance - newDistance;
                        globalFirstRouteIndex = i;
                        globalSecondRouteIndex = j;
                        globalMinCostFirstRoute = localMinCostFirstRoute;
                        globalMinCostSecondRoute = localMinCostSecondRoute;
                    }
                }
            }
            if(globalMaxDif != 0)
            {
                routeList.RemoveAt(globalFirstRouteIndex);
                routeList.Insert(globalFirstRouteIndex, globalMinCostFirstRoute);
                routeList.RemoveAt(globalSecondRouteIndex);
                routeList.Insert(globalSecondRouteIndex, globalMinCostSecondRoute);
            }
        }

        //interchanging customer between 2 given routes using all operators (0,1) (1,0) (2,0) (0,2) (1,2) (2,1) (1,1) (2,2)  
        //accepting only mincost solution across all operators 
        public double Interchange(List<Customer> firstRoute, List<Customer> secondRoute, ref List<Customer> minCostFirstRoute, ref List<Customer> minCostSecondRoute, int lamda)
        {
            double oldDistance = Utils.CalculateTotalDistance(firstRoute, cTocDistance) +
                                 Utils.CalculateTotalDistance(secondRoute, cTocDistance);
            double minDistance = oldDistance;
            minCostFirstRoute = new List<Customer>(firstRoute);
            minCostSecondRoute = new List<Customer>(secondRoute);

            for (int i=0; i<=lamda; i++)
            {
                for(int j=0; j<=lamda; j++)
                {

                    List<Customer> firstRouteCopy = new List<Customer>(firstRoute);
                    List<Customer> secondRouteCopy = new List<Customer>(secondRoute);
                    bool feasible = PerformInterchange(firstRouteCopy, secondRouteCopy, i, j);
                    double newDistance = Utils.CalculateTotalDistance(firstRouteCopy, cTocDistance) + Utils.CalculateTotalDistance(secondRouteCopy, cTocDistance);
                    if(newDistance < minDistance)
                    {
                        minDistance = newDistance;
                        minCostFirstRoute = firstRouteCopy;
                        minCostSecondRoute = secondRouteCopy;
                    }
                    else
                    {
                        firstRouteCopy.Clear();
                        secondRouteCopy.Clear();
                    }
                }
            }
            return minDistance;
        }

        //performing interchange between 2 given routers using given operator 
        public bool PerformInterchange(List<Customer> firstRoute, List<Customer> secondRoute, int operator1, int operator2)
        {
            double minDistance = Utils.CalculateTotalDistance(firstRoute, cTocDistance) +
                                  Utils.CalculateTotalDistance(secondRoute, cTocDistance);
            bool feasible = true;
            Random random = new Random();

            while (operator1 != 0 && feasible)
            {
                if (firstRoute.Count - 2 < 1)
                {
                    feasible = false;
                    break;
                }
                int rand1 = random.Next(1, firstRoute.Count - 1);
                Customer firstRouteCustomer = firstRoute[rand1];
                firstRoute.RemoveAt(rand1);

                for (int secondRouteCustomerIndex = 1; secondRouteCustomerIndex < secondRoute.Count; secondRouteCustomerIndex++)
                {
                    secondRoute.Insert(secondRouteCustomerIndex, firstRouteCustomer);
                    feasible = Utils.CheckTimeConstraint(secondRoute, cTocDistance) &&
                            Utils.CheckCapacityConstraint(secondRoute, vehicle.capacity);
                    if (feasible)
                    {
                        break;
                    }
                    else
                    {
                        secondRoute.RemoveAt(secondRouteCustomerIndex);
                    }
                }
                if (!feasible)
                {
                    firstRoute.Insert(rand1, firstRouteCustomer);
                }
                operator1--;
            }
            

            while(operator2 != 0 && feasible)
            {
                if (secondRoute.Count - 2 < 1)
                {
                    feasible = false;
                    break;
                }
                int rand2 = random.Next(1, secondRoute.Count - 1);
                Customer secondRouteCustomer = secondRoute[rand2];
                secondRoute.RemoveAt(rand2);

                for (int firstRouteCustomerIndex = 1; firstRouteCustomerIndex < firstRoute.Count; firstRouteCustomerIndex++)
                {
                    firstRoute.Insert(firstRouteCustomerIndex, secondRouteCustomer);
                    feasible = Utils.CheckTimeConstraint(firstRoute, cTocDistance) &&
                            Utils.CheckCapacityConstraint(firstRoute, vehicle.capacity);
                    if (feasible)
                    {
                        break;
                    }
                    else
                    {
                        firstRoute.RemoveAt(firstRouteCustomerIndex);
                    }
                }
                if (!feasible)
                {
                    secondRoute.Insert(rand2, secondRouteCustomer);
                }

                operator2--;
            }
            return feasible;
        }
         
    }
}
