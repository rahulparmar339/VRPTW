using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VRPTW
{
    class SimulatedAnnealing
    {
        private int noOfCustomer;
        public Depo depo { get; set; }
        public Vehicle vehicle { get; set; }
        public List<Customer> customerList { get; set; }
        private double[][] cTocDistance;

        public SimulatedAnnealing(Depo depo, Vehicle vehicle, List<Customer> customerList, double[][] cTocDistance)
        {
            this.noOfCustomer = customerList.Count;
            this.depo = depo;
            this.vehicle = vehicle;
            this.customerList = customerList; //depo is included as first customer
            this.cTocDistance = cTocDistance; //depo is include as First customer
        }

        public void run(double initialTemp, double finalTemp, double coolingFactor, int noOfIteration)
        {
            //constructing first solution using randomized function
            List<List<Customer>> routeList = Utils.GenerateRandomRouteList(depo, vehicle, customerList, cTocDistance);
            Utils.Print(routeList, cTocDistance);


            //construct first solution using push first insertion Heuristic
            //PFIH pfih = new PFIH(depo, vehicle, customerList, cTocDistance);
            //List<List<Customer>> routeList = pfih.Construct();
            //Program.print(routeList, cTocDistance);


            //improve solution using lambda-interchange Heuristic
            //LSH lsh = new LSH(cTocDistance, vehicle);
            //for (int i = 0; i < 50; i++)
            //{
            //    lsh.LambdaInterchangeGB(ref routeList);
            //    Utils.printTD(routeList, cTocDistance);
            //}
            //Utils.print(routeList, cTocDistance);


            //improving solution using simulated annealing
            List<List<Customer>> bestRouteList = new List<List<Customer>>();
            List<List<Customer>> currRouteList = new List<List<Customer>>();
            List<List<Customer>> neighbourRouteList = new List<List<Customer>>();
            Utils.CopyListOfList(routeList, bestRouteList);
            Utils.CopyListOfList(routeList, currRouteList);
            Utils.CopyListOfList(routeList, neighbourRouteList);
            double costOfBestRouteList = Utils.CalculateTotalDistanceOfAllRoute(bestRouteList, cTocDistance);
            double costOfCurrRouteList = Utils.CalculateTotalDistanceOfAllRoute(currRouteList, cTocDistance);
            double currTemp = initialTemp;
            Random random = new Random();

            while (currTemp > finalTemp)
            {
                int iteration = 0;
                while (iteration++ < noOfIteration)
                {
                    neighbourRouteList = GetNeighbour(currRouteList);
                    double costOfNeighbourRouteList = Utils.CalculateTotalDistanceOfAllRoute(neighbourRouteList, cTocDistance);

                    Console.WriteLine("BestRouteDis: "+Utils.CalculateTotalDistanceOfAllRoute(bestRouteList,cTocDistance)+
                                      " currRouteDis: "+Utils.CalculateTotalDistanceOfAllRoute(currRouteList, cTocDistance)+
                                      " temp: "+currTemp);
                    
                    double costDiff = costOfNeighbourRouteList - costOfCurrRouteList;
                    if (costDiff < 0 || Math.Exp(-costDiff / currTemp) > random.Next(0, 1))
                    {
                        currRouteList.Clear();
                        currRouteList.AddRange(neighbourRouteList);
                        costOfCurrRouteList = costOfNeighbourRouteList;

                        if (costOfCurrRouteList < costOfBestRouteList)
                        {
                            bestRouteList.Clear();
                            bestRouteList.AddRange(currRouteList);
                            costOfBestRouteList = costOfCurrRouteList;
                        }
                    }
                    neighbourRouteList.Clear();
                }
                currTemp = currTemp * coolingFactor;
            }
            Utils.Print(bestRouteList, cTocDistance);
        }

        //finding neighbour solution using 3 mutation methodes
        public List<List<Customer>> GetNeighbour(List<List<Customer>> routeList)
        {
            Random random = new Random();
            int rand = random.Next(1, 4);
            switch (rand)
            {
                case 1:
                    return mutateInsertion(routeList);
                case 2:
                    return mutateSwap(routeList);
                case 3:
                    return mutateInversion(routeList);
            }
            return null;
        }

        //mutateInsertion removes random customer and try to insert it in same or different vehicle route if possible
        public List<List<Customer>> mutateInsertion(List<List<Customer>> routeList)
        {
            List<List<Customer>> neighbourRouteList = new List<List<Customer>>();
            Utils.CopyListOfList(routeList, neighbourRouteList);

            Random random = new Random();
            bool feasible = false;
            int firstRouteIndex = random.Next(0, neighbourRouteList.Count);
            List<Customer> firstRoute = neighbourRouteList[firstRouteIndex];
            
            if(firstRoute.Count >= 3)
            {
                int customerIndex = random.Next(1, firstRoute.Count - 1);
                Customer customer = firstRoute[customerIndex];

                firstRoute.RemoveAt(customerIndex);
                foreach (List<Customer> secondRoute in neighbourRouteList)
                {
                    for (int index = 1; index < secondRoute.Count; index++)
                    {
                        secondRoute.Insert(index, customer);
                        feasible = Utils.CheckTimeConstraint(secondRoute, cTocDistance) && Utils.CheckCapacityConstraint(secondRoute, vehicle.capacity);
                        if (feasible)
                            break;
                        else
                            secondRoute.RemoveAt(index);

                    }
                    if (feasible)
                        break;
                }
                if (!feasible)
                    firstRoute.Insert(customerIndex, customer);
            }
            Console.WriteLine("mutateInsertion " + feasible);
            return neighbourRouteList;
        }

        //mutateSwap swaps two random customer across all vehicle routes if possible
        public List<List<Customer>> mutateSwap(List<List<Customer>> routeList)
        {
            List<List<Customer>> neighbourRouteList = new List<List<Customer>>();
            Utils.CopyListOfList(routeList, neighbourRouteList);

            Random random = new Random();
            bool feasible = false;

            int firstRouteIndex = random.Next(0, neighbourRouteList.Count);
            int secondRouteIndex = random.Next(0, neighbourRouteList.Count);
            List<Customer> firstRoute = neighbourRouteList[firstRouteIndex];
            List<Customer> secondRoute = neighbourRouteList[secondRouteIndex];

            if (firstRouteIndex == secondRouteIndex || firstRoute.Count <=2 || secondRoute.Count <= 2)
            {
                Console.WriteLine("mutateSwap " + feasible);
                return neighbourRouteList;
            }

            int firstRouteCustomerIndex = random.Next(1, firstRoute.Count-1);
            int secondRouteCustomerIndex = random.Next(1, secondRoute.Count-1);
            Customer firstRouteCustomer = firstRoute[firstRouteCustomerIndex];
            Customer secondRouteCustomer = secondRoute[secondRouteCustomerIndex];

            firstRoute.RemoveAt(firstRouteCustomerIndex);
            secondRoute.RemoveAt(secondRouteCustomerIndex);
            firstRoute.Insert(firstRouteCustomerIndex, secondRouteCustomer);
            secondRoute.Insert(secondRouteCustomerIndex, firstRouteCustomer);

            feasible = Utils.CheckTimeConstraint(firstRoute, cTocDistance) && Utils.CheckCapacityConstraint(firstRoute, vehicle.capacity) &&
                Utils.CheckTimeConstraint(secondRoute, cTocDistance) && Utils.CheckCapacityConstraint(secondRoute, vehicle.capacity);

            if (!feasible)
            {
                firstRoute.RemoveAt(firstRouteCustomerIndex);
                secondRoute.RemoveAt(secondRouteCustomerIndex);
                firstRoute.Insert(firstRouteCustomerIndex, firstRouteCustomer);
                secondRoute.Insert(secondRouteCustomerIndex, secondRouteCustomer);
            }
            return neighbourRouteList;
        }

        //mutateInvesion reverses random vehicle route's substring if possible
        public List<List<Customer>> mutateInversion(List<List<Customer>> routeList)
        {
            List<List<Customer>> neighbourRouteList = new List<List<Customer>>();
            Utils.CopyListOfList(routeList, neighbourRouteList);

            Random random = new Random();
            bool feasible = false;
            int routeIndex = random.Next(0, neighbourRouteList.Count);
            List<Customer> route = neighbourRouteList[routeIndex];

            if(route.Count <= 3)
            {
                return neighbourRouteList;
            }

            int customerStart = random.Next(1, route.Count - 2);
            int count = random.Next(2, route.Count - customerStart);

            route.Reverse(customerStart, count);
            feasible = Utils.CheckCapacityConstraint(route, vehicle.capacity) && Utils.CheckTimeConstraint(route, cTocDistance);
            if (!feasible)
            {
                route.Reverse(customerStart, count);
            }
            
            Console.WriteLine("mutateInversion " + feasible);
            return neighbourRouteList;
        }

        

        //public List<List<Customer>> GetNeighbour(List<List<Customer>> routeList, LSH lsh)
        //{
        //    List<List<Customer>> neighbourRouteList = new List<List<Customer>>();
        //    copyListOfList(routeList, neighbourRouteList);

        //    Random random = new Random();
        //    int firstRouteIndex = random.Next(0, neighbourRouteList.Count);
        //    int secondRouteIndex = random.Next(0, neighbourRouteList.Count);
        //    int operator1 = 1;
        //    int operator2 = 0;

        //    bool feasible = lsh.PerformInterchange(neighbourRouteList[firstRouteIndex], neighbourRouteList[secondRouteIndex], operator1, operator2);
        //    return neighbourRouteList;
        //}

        //mutate swap with randomization
        //public List<List<Customer>> mutateSwap(List<List<Customer>> routeList, LSH lsh)
        //{
        //    List<List<Customer>> neighbourRouteList = new List<List<Customer>>();
        //    copyListOfList(routeList, neighbourRouteList);

        //    Random random = new Random();
        //    bool feasible = false;

        //    for (int firstRouteIndex = 0; firstRouteIndex < routeList.Count && feasible; firstRouteIndex++)
        //    {
        //        List<Customer> firstRoute = routeList[firstRouteIndex];
        //        for (int firstRouteCustomerIndex = 0; firstRouteCustomerIndex < firstRoute.Count && feasible; firstRouteCustomerIndex++)
        //        {
        //            Customer firstRouteCustomer = firstRoute[firstRouteCustomerIndex];
        //            firstRoute.RemoveAt(firstRouteCustomerIndex);
        //            for (int secondRouteIndex = firstRouteIndex; secondRouteIndex < routeList.Count && feasible; secondRouteIndex++)
        //            {
        //                List<Customer> secondRoute = routeList[secondRouteIndex];
        //                for (int secondRouteCustomerIndex = 0; secondRouteCustomerIndex < secondRoute.Count && feasible; secondRouteCustomerIndex++)
        //                {
        //                    Customer secondRouteCustomer = secondRoute[secondRouteCustomerIndex];
        //                    secondRoute.RemoveAt(secondRouteCustomerIndex);

        //                    firstRoute.Insert(firstRouteCustomerIndex, secondRouteCustomer);
        //                    secondRoute.Insert(secondRouteCustomerIndex, firstRouteCustomer);

        //                    feasible = Utils.CheckTimeConstraint(firstRoute, cTocDistance) && Utils.CheckCapacityConstraint(firstRoute, vehicle.capacity) &&
        //                        Utils.CheckTimeConstraint(secondRoute, cTocDistance) && Utils.CheckCapacityConstraint(secondRoute, vehicle.capacity);

        //                    if (!feasible)
        //                        firstRoute.RemoveAt(firstRouteCustomerIndex);
        //                    secondRoute.RemoveAt(secondRouteCustomerIndex);
        //                    secondRoute.Insert(secondRouteCustomerIndex, secondRouteCustomer);
        //                }
        //            }
        //            if (!feasible)
        //                firstRoute.Insert(firstRouteCustomerIndex, firstRouteCustomer);
        //        }
        //    }
        //    Console.WriteLine("mutateSwap " + feasible);
        //    return neighbourRouteList;
        //}

        //public bool Check(List<List<Customer>> list)
        //{
        //    foreach (List<Customer> route in list)
        //    {
        //        if (route[0].id != 0 || route[route.Count - 1].id != 0)
        //        {
        //            Console.WriteLine(route[0].id + " first and last " + route[route.Count - 1].id);
        //            return true;
        //        }
        //    }
        //    return false;
        //}
    }
}

