using System;
using System.Linq;
using System.Globalization;
using System.Data.SqlClient;
using DashboardWinApp.DBase;
using System.Collections.Generic;

namespace DashboardWinApp.Models
{
    public struct RevenueByDate
    {
        public string Date { get; set; }
        public decimal TotalAmount { get; set; }
    }
    public class Dashboard : DBaseCon
    {
        //Fields & Properties
        private int NumDays;
        private DateTime startDate, endDate;
        public int NumOrders { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal TotalRevenue { get; set; }
        public int NumProducts { get; private set; }
        public int NumCustomers { get; private set; }
        public int NumSuppliers { get; private set; }
        public List<RevenueByDate> GrossRevenueList { get; private set; }
        public List<KeyValuePair<string, int>> UnderstockList { get; private set; }
        public List<KeyValuePair<string, int>> TopProductsList { get; private set; }

        //Constructor
        public Dashboard()
        {

        }

        //Private methods
        private void GetNumberItems()
        {
            using (var oCon = GetConnection())
            {
                oCon.Open();
                using (var oCmd = new SqlCommand())
                {
                    oCmd.Connection = oCon;
                    //Get Total Number of Customers
                    oCmd.CommandText = "select count(id) from Customer";
                    NumCustomers = (int)oCmd.ExecuteScalar();
                    //Get Total Number of Suppliers
                    oCmd.CommandText = "select count(id) from Supplier";
                    NumSuppliers = (int)oCmd.ExecuteScalar();
                    //Get Total Number of Products
                    oCmd.CommandText = "select count(id) from Product";
                    NumProducts = (int)oCmd.ExecuteScalar();
                    //Get Total Number of Orders
                    oCmd.CommandText = @"select count(id) from [Order]" +
                                            "where OrderDate between  @fromDate and @toDate";
                    oCmd.Parameters.Add("@fromDate", System.Data.SqlDbType.DateTime).Value = startDate;
                    oCmd.Parameters.Add("@toDate", System.Data.SqlDbType.DateTime).Value = endDate;
                    NumOrders = (int)oCmd.ExecuteScalar();
                }
            }
        }
        private void GetProductAnalisys()
        {
            TopProductsList = new List<KeyValuePair<string, int>>();
            UnderstockList = new List<KeyValuePair<string, int>>();
            using (var oCon = GetConnection())
            {
                oCon.Open();
                using (var oCmd = new SqlCommand())
                {
                    SqlDataReader reader;
                    oCmd.Connection = oCon;
                    //Get Top 5 products
                    oCmd.CommandText = @"select top 10 P.ProductName, sum(OrderItem.Quantity) as Q
                                            from OrderItem
                                            inner join Product P on P.Id = OrderItem.ProductId
                                            inner join [Order] O on O.Id = OrderItem.OrderId
                                            where OrderDate between @fromDate and @toDate
                                            group by P.ProductName order by Q desc";
                    oCmd.Parameters.Add("@fromDate", System.Data.SqlDbType.DateTime).Value = startDate;
                    oCmd.Parameters.Add("@toDate", System.Data.SqlDbType.DateTime).Value = endDate;
                    reader = oCmd.ExecuteReader();
                    while (reader.Read())
                    {
                        TopProductsList.Add(
                            new KeyValuePair<string, int>(reader[0].ToString(), (int)reader[1]));
                    }
                    reader.Close();
                    //Get Understock
                    oCmd.CommandText = @"select ProductName, Stock from Product
                                            where Stock <= 6 and IsDiscontinued = 0";
                    reader = oCmd.ExecuteReader();
                    while (reader.Read())
                    {
                        UnderstockList.Add(
                            new KeyValuePair<string, int>(reader[0].ToString(), (int)reader[1]));
                    }
                    reader.Close();
                }
            }
        }
        private void GetOrderAnalisys()
        {
            TotalProfit = TotalRevenue = 0;
            GrossRevenueList = new List<RevenueByDate>();
            using (var oCon = GetConnection())
            {
                oCon.Open();
                using (var oCmd = new SqlCommand())
                {
                    oCmd.Connection = oCon;
                    oCmd.CommandText = @"select OrderDate, sum(TotalAmount)
                                        from[Order]
                                        where OrderDate between @fromDate and @toDate
                                        group by OrderDate";
                    oCmd.Parameters.Add("@fromDate", System.Data.SqlDbType.DateTime).Value = startDate;
                    oCmd.Parameters.Add("@toDate", System.Data.SqlDbType.DateTime).Value = endDate;
                    var reader = oCmd.ExecuteReader();
                    var resultTable = new List<KeyValuePair<DateTime, decimal>>();
                    while (reader.Read())
                    {
                        resultTable.Add(
                            new KeyValuePair<DateTime, decimal>((DateTime)reader[0], (decimal)reader[1])
                        );
                        TotalRevenue += (decimal)reader[1];
                    }
                    TotalProfit = TotalRevenue * 0.2m; //20%
                    reader.Close();
                    //Group by Hours
                    if (NumDays <= 1)
                    {
                        GrossRevenueList = (from orderList in resultTable
                                            group orderList by orderList.Key.ToString("hh tt")
                                            into order
                                            select new RevenueByDate
                                            {
                                                Date = order.Key,
                                                TotalAmount = order.Sum(amount => amount.Value)
                                            }).ToList();
                    }
                    //Group by Days
                    else if (NumDays <= 30)
                    {
                        GrossRevenueList = (from orderList in resultTable
                                            group orderList by orderList.Key.ToString("dd MMM")
                                            into order
                                            select new RevenueByDate
                                            {
                                                Date = order.Key,
                                                TotalAmount = order.Sum(amount => amount.Value)
                                            }).ToList();
                    }
                    //Group by Weeks
                    else if (NumDays <= 92)
                    {
                        GrossRevenueList = (from orderList in resultTable
                                            group orderList by CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                                                orderList.Key, CalendarWeekRule.FirstDay, DayOfWeek.Monday)
                                            into order
                                            select new RevenueByDate
                                            {
                                                Date = "Week " + order.Key.ToString(),
                                                TotalAmount = order.Sum(amount => amount.Value)
                                            }).ToList();
                    }
                    //Group by Months
                    else if (NumDays <= (365 * 2))
                    {
                        bool isYear = NumDays <= 365 ? true : false;
                        GrossRevenueList = (from orderList in resultTable
                                            group orderList by orderList.Key.ToString("MMM yyyy")
                                            into order
                                            select new RevenueByDate
                                            {
                                                Date = isYear ? order.Key.Substring(0, order.Key.IndexOf(" ")) : order.Key,
                                                TotalAmount = order.Sum(amount => amount.Value)
                                            }).ToList();
                    }
                    //Group by Years
                    else
                    {
                        GrossRevenueList = (from orderList in resultTable
                                            group orderList by orderList.Key.ToString("yyyy")
                                            into order
                                            select new RevenueByDate
                                            {
                                                Date = order.Key,
                                                TotalAmount = order.Sum(amount => amount.Value)
                                            }).ToList();
                    }
                }
            }
        }
        //Public methods
        public bool LoadData(DateTime startDate, DateTime endDate)
        {
            endDate = new DateTime(endDate.Year, endDate.Month, 
                endDate.Day, endDate.Hour, endDate.Minute, 59);
            if (startDate != this.startDate || endDate != this.endDate)
            {
                this.startDate = startDate;
                this.endDate = endDate;
                this.NumDays = (endDate - startDate).Days;
                GetNumberItems();
                GetProductAnalisys();
                GetOrderAnalisys();
                Console.WriteLine("Refreshed data: {0} - {1}", startDate.ToString(), endDate.ToString());
                return true;
            }
            else
            {
                Console.WriteLine("Data not refreshed, same query: {0} - {1}", startDate.ToString(), endDate.ToString());
                return false;
            }
        }
    }
}
