using System;
using System.Collections.Generic;
using System.Text;

namespace VRPTW
{
    class TabuSearch
    {
        private int noOfCustomer;
        public Depo depo { get; set; }
        public Vehicle vehicle { get; set; }
        public List<Customer> customerList { get; set; }
        private double[][] cTocDistance;

        //recording swapping move     
        public List<SwapTabuMove> swapTabuList { get; set; }

        //recording insertion move 
        public List<InsertionTabuMove> insertionTabuList { get; set; }

        public TabuSearch(Depo depo, Vehicle vehicle, List<Customer> customerList, double[][] cTocDistance)
        {
            this.depo = depo;
            this.vehicle = vehicle;
            this.customerList = customerList;
            this.cTocDistance = cTocDistance;
            this.noOfCustomer = customerList.Count;
            swapTabuList = new List<SwapTabuMove>();
            insertionTabuList = new List<InsertionTabuMove>();
        }

        public void run()
        {
            //constructing first solution using randomized function
            List<List<Customer>> routeList = Utils.GenerateRandomRouteList(depo, vehicle, customerList, cTocDistance);
            Utils.Print(routeList, cTocDistance);

            //improving solution using tabu search
            List<List<Customer>> bestRouteList = new List<List<Customer>>();
            List<List<Customer>> currRouteList = new List<List<Customer>>();
            List<List<Customer>> neighbourRouteList = new List<List<Customer>>();
            Utils.CopyListOfList(routeList, bestRouteList);
            Utils.CopyListOfList(routeList, currRouteList);
            Utils.CopyListOfList(routeList, neighbourRouteList);
            double costOfBestRouteList = Utils.CalculateTotalDistanceOfAllRoute(bestRouteList, cTocDistance);
            double costOfCurrRouteList = Utils.CalculateTotalDistanceOfAllRoute(currRouteList, cTocDistance);

            int noOfIteration = 100;
            while (noOfIteration-- > 0)
            {
                neighbourRouteList = GetNeighbour(currRouteList, costOfCurrRouteList, costOfBestRouteList);
                currRouteList.Clear();
                currRouteList = neighbourRouteList;
                costOfCurrRouteList = Utils.CalculateTotalDistanceOfAllRoute(currRouteList, cTocDistance);
                if (costOfCurrRouteList < costOfBestRouteList)
                {
                    costOfBestRouteList = costOfCurrRouteList;
                    bestRouteList.Clear();
                    Utils.CopyListOfList(currRouteList, bestRouteList);
                }
                Console.WriteLine("cost of best: " + costOfBestRouteList + "  cost of curr: " + costOfCurrRouteList);
            }

            Utils.Print(bestRouteList, cTocDistance);
        }

        //finding best neighbour solution
        public List<List<Customer>> GetNeighbour(List<List<Customer>> routeList, double costOfRouteList, double costOfBestRouteList)
        {
            Random random = new Random();
            int rand = random.Next(1, 3);
            switch (rand)
            {
                case 1:
                    return MutateSwap(routeList, costOfRouteList, costOfBestRouteList);
                case 2:
                    return MutateInsertion(routeList, costOfRouteList, costOfBestRouteList);
            }
            return null;
        }

        //swapping pair of every 2 customer and returning global best neighbour
        public List<List<Customer>> MutateSwap(List<List<Customer>> routeList, double costOfRouteList, double costOfBestRouteList)
        {
            List<List<Customer>> neighbourRouteList = new List<List<Customer>>();
            Utils.CopyListOfList(routeList, neighbourRouteList);
            double costOfNeighbourRouteList = costOfRouteList;
            bool feasible = false;

            SwapTabuMove swapTabuMove = new SwapTabuMove();
            double dif = 0;
            bool swapTabuMoveSelected = false;

            for (int firstRouteIndex = 0; firstRouteIndex < routeList.Count; firstRouteIndex++)
            {
                List<Customer> firstRoute = routeList[firstRouteIndex];
                for (int firstRouteCustomerIndex = 1; firstRouteCustomerIndex < firstRoute.Count - 1; firstRouteCustomerIndex++)
                {
                    for (int secondRouteIndex = firstRouteIndex; secondRouteIndex < routeList.Count; secondRouteIndex++)
                    {
                        List<Customer> secondRoute = routeList[secondRouteIndex];
                        for (int secondRouteCustomerIndex = 1; secondRouteCustomerIndex < secondRoute.Count - 1; secondRouteCustomerIndex++)
                        {
                            //Console.WriteLine("Before Swap: "+ firstRoute[firstRouteCustomerIndex].id + " "+ secondRoute[secondRouteCustomerIndex].id);
                            swap(firstRoute, firstRouteCustomerIndex, secondRoute, secondRouteCustomerIndex);
                            //Console.WriteLine("After Swap: " + firstRoute[firstRouteCustomerIndex].id + " " + secondRoute[secondRouteCustomerIndex].id);

                            feasible = Utils.CheckTimeConstraint(firstRoute, cTocDistance) && Utils.CheckCapacityConstraint(firstRoute, vehicle.capacity) &&
                                Utils.CheckTimeConstraint(secondRoute, cTocDistance) && Utils.CheckCapacityConstraint(secondRoute, vehicle.capacity);

                            if (feasible)
                            {
                                costOfRouteList = Utils.CalculateTotalDistanceOfAllRoute(routeList, cTocDistance);

                                if (IsSwapTabu(firstRouteCustomerIndex, secondRouteCustomerIndex))
                                {
                                    if (costOfRouteList < costOfBestRouteList && costOfRouteList < costOfNeighbourRouteList)
                                    {
                                        dif = costOfRouteList - costOfNeighbourRouteList;
                                        neighbourRouteList.Clear();
                                        Utils.CopyListOfList(routeList, neighbourRouteList);
                                        costOfNeighbourRouteList = costOfRouteList;

                                        swapTabuMoveSelected = true;
                                        swapTabuMove.customer1 = firstRouteCustomerIndex;
                                        swapTabuMove.customer2 = secondRouteCustomerIndex;
                                    }
                                }
                                else
                                {
                                    if (costOfRouteList < costOfNeighbourRouteList)
                                    {
                                        dif = costOfRouteList - costOfNeighbourRouteList;
                                        neighbourRouteList.Clear();
                                        Utils.CopyListOfList(routeList, neighbourRouteList);
                                        costOfNeighbourRouteList = costOfRouteList;

                                        swapTabuMoveSelected = false;
                                        swapTabuMove.customer1 = firstRouteCustomerIndex;
                                        swapTabuMove.customer2 = secondRouteCustomerIndex;
                                    }
                                    else if (costOfRouteList - costOfNeighbourRouteList < dif)
                                    {
                                        dif = costOfRouteList - costOfNeighbourRouteList;
                                        neighbourRouteList.Clear();
                                        Utils.CopyListOfList(routeList, neighbourRouteList);
                                        costOfNeighbourRouteList = costOfRouteList;

                                        swapTabuMoveSelected = false;
                                        swapTabuMove.customer1 = firstRouteCustomerIndex;
                                        swapTabuMove.customer2 = secondRouteCustomerIndex;
                                    }
                                }
                            }
                            swap(firstRoute, firstRouteCustomerIndex, secondRoute, secondRouteCustomerIndex);
                        }
                    }
                }
            }
            if (swapTabuMoveSelected)
            {
                RemoveFromSwapTabuList(swapTabuMove);
            }
            else
            {
                InsertIntoSwapTabuList(swapTabuMove);
            }

            Console.WriteLine("mutateSwap: " + Utils.CalculateTotalDistanceOfAllRoute(routeList, cTocDistance) + " " + Utils.CalculateTotalDistanceOfAllRoute(neighbourRouteList, cTocDistance));
            return neighbourRouteList;
        }

        //inserting every customer at every possible place and returning global best neighbour
        public List<List<Customer>> MutateInsertion(List<List<Customer>> routeList, double costOfRouteList, double costOfBestRouteList)
        {
            List<List<Customer>> neighbourRouteList = new List<List<Customer>>();
            Utils.CopyListOfList(routeList, neighbourRouteList);
            double costOfNeighbourRouteList = costOfRouteList;
            bool feasible = false;

            InsertionTabuMove insertionTabuMove = new InsertionTabuMove();
            double dif = 0;
            bool insertionTabuMoveSelected = false;

            for (int firstRouteIndex = 0; firstRouteIndex < routeList.Count; firstRouteIndex++)
            {
                List<Customer> firstRoute = routeList[firstRouteIndex];
                for (int firstRouteCustomerIndex = 1; firstRouteCustomerIndex < firstRoute.Count - 1; firstRouteCustomerIndex++)
                {
                    Customer firstRouteCustomer = firstRoute[firstRouteCustomerIndex];
                    firstRoute.RemoveAt(firstRouteCustomerIndex);

                    for (int secondRouteIndex = firstRouteIndex; secondRouteIndex < routeList.Count; secondRouteIndex++)
                    {
                        List<Customer> secondRoute = routeList[secondRouteIndex];
                        for (int secondRouteCustomerIndex = 1; secondRouteCustomerIndex < secondRoute.Count; secondRouteCustomerIndex++)
                        {
                            secondRoute.Insert(secondRouteCustomerIndex, firstRouteCustomer);
                            feasible = Utils.CheckTimeConstraint(firstRoute, cTocDistance) && Utils.CheckCapacityConstraint(firstRoute, vehicle.capacity) &&
                                Utils.CheckTimeConstraint(secondRoute, cTocDistance) && Utils.CheckCapacityConstraint(secondRoute, vehicle.capacity);

                            if (feasible)
                            {
                                costOfRouteList = Utils.CalculateTotalDistanceOfAllRoute(routeList, cTocDistance);

                                if (IsInsertionTabu(firstRouteIndex, firstRouteCustomerIndex ,secondRouteIndex))
                                {
                                    if (costOfRouteList < costOfBestRouteList && costOfRouteList < costOfNeighbourRouteList)
                                    {
                                        dif = costOfRouteList - costOfNeighbourRouteList;
                                        neighbourRouteList.Clear();
                                        Utils.CopyListOfList(routeList, neighbourRouteList);
                                        costOfNeighbourRouteList = costOfRouteList;

                                        insertionTabuMoveSelected = true;
                                        insertionTabuMove.routeList1 = firstRouteIndex;
                                        insertionTabuMove.customer = firstRouteCustomerIndex;
                                        insertionTabuMove.routeList2 = secondRouteIndex;
                                    }
                                }
                                else
                                {
                                    if (costOfRouteList < costOfNeighbourRouteList)
                                    {
                                        dif = costOfRouteList - costOfNeighbourRouteList;
                                        neighbourRouteList.Clear();
                                        Utils.CopyListOfList(routeList, neighbourRouteList);
                                        costOfNeighbourRouteList = costOfRouteList;

                                        insertionTabuMoveSelected = true;
                                        insertionTabuMove.routeList1 = firstRouteIndex;
                                        insertionTabuMove.customer = firstRouteCustomerIndex;
                                        insertionTabuMove.routeList2 = secondRouteIndex;
                                    }
                                    else if (costOfRouteList - costOfNeighbourRouteList < dif)
                                    {
                                        dif = costOfRouteList - costOfNeighbourRouteList;
                                        neighbourRouteList.Clear();
                                        Utils.CopyListOfList(routeList, neighbourRouteList);
                                        costOfNeighbourRouteList = costOfRouteList;

                                        insertionTabuMoveSelected = true;
                                        insertionTabuMove.routeList1 = firstRouteIndex;
                                        insertionTabuMove.customer = firstRouteCustomerIndex;
                                        insertionTabuMove.routeList2 = secondRouteIndex;
                                    }
                                }
                            }
                            secondRoute.RemoveAt(secondRouteCustomerIndex);
                        }
                    }

                    firstRoute.Insert(firstRouteCustomerIndex, firstRouteCustomer);
                }
            }

            if (insertionTabuMoveSelected)
            {
                RemoveFromInsertionTabuList(insertionTabuMove);
            }
            else
            {
                InsertIntoInsertionTabuList(insertionTabuMove);
            }
            Console.WriteLine("mutateInsertion: " + Utils.CalculateTotalDistanceOfAllRoute(routeList, cTocDistance) + " " + Utils.CalculateTotalDistanceOfAllRoute(neighbourRouteList, cTocDistance));
            return neighbourRouteList;
        }

        //checking swap move is tabu or not
        public bool IsSwapTabu(int from, int to)
        {
            foreach (SwapTabuMove swapTabuMove in swapTabuList)
            {
                if((swapTabuMove.customer1 == from && swapTabuMove.customer2 == to) || 
                    (swapTabuMove.customer1 == to && swapTabuMove.customer2 == from))
                {
                    return true;
                }
            }
            return false;
        }

        //inserting swap move into swap tabu list
        public void InsertIntoSwapTabuList(SwapTabuMove swapTabuMove)
        {
            if(swapTabuList.Count >= 10)
            {
                swapTabuList.RemoveAt(swapTabuList.Count - 1);
            }
            swapTabuList.Insert(0, swapTabuMove);
        }

        //removing swap move from swap tabu list
        public void RemoveFromSwapTabuList(SwapTabuMove move)
        {
            foreach (SwapTabuMove swapTabuMove in swapTabuList)
            {
                if((swapTabuMove.customer1 == move.customer1 && swapTabuMove.customer2 == move.customer2) || 
                    swapTabuMove.customer1 == move.customer2 && swapTabuMove.customer2 == move.customer1)
                {
                    swapTabuList.Remove(swapTabuMove);
                    return;
                }
            }
        }

        //checking insertion move is tabu ot not
        public bool IsInsertionTabu(int routeList1, int customer, int routeList2)
        {
            foreach (InsertionTabuMove insertionTabuMove in insertionTabuList)
            {
                if(insertionTabuMove.customer == customer &&  
                    (insertionTabuMove.routeList1==routeList1 && insertionTabuMove.routeList2 == routeList2) ||
                    (insertionTabuMove.routeList1 == routeList2 && insertionTabuMove.routeList2 == routeList1))
                {
                    return true;
                }
            }
            return false;
        }

        //insering insertion move into insertion tabu list
        public void InsertIntoInsertionTabuList(InsertionTabuMove insertionTabuMove)
        {
            if (insertionTabuList.Count >= 10)
            {
                insertionTabuList.RemoveAt(insertionTabuList.Count - 1);
            }
            insertionTabuList.Insert(0, insertionTabuMove);
        }

        //removing insertion move from insertion tabu list
        public void RemoveFromInsertionTabuList(InsertionTabuMove insertionTabuMove)
        {
            foreach (InsertionTabuMove tabuMove in insertionTabuList)
            {
                if (tabuMove.customer == insertionTabuMove.customer &&
                    (tabuMove.routeList1 == insertionTabuMove.routeList1 && tabuMove.routeList2 == insertionTabuMove.routeList2) ||
                    (tabuMove.routeList1 == insertionTabuMove.routeList2 && tabuMove.routeList2 == insertionTabuMove.routeList1))
                {
                    insertionTabuList.Remove(tabuMove);
                }
            }
        }

        public void swap(List<Customer> listFrom, int from, List<Customer> listTo, int to)
        {
            Customer temp = listFrom[from];
            listFrom[from] = listTo[to];
            listTo[to] = temp;
        }
    }
}
