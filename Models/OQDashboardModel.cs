using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DPGSalesClient.Models
{
    public class AccordionModel
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string ParentID { get; set; }
        public string BoxClass { get; set; }
        public string BadgeClass { get; set; }
        public string FontClass { get; set; }
        public List<ReportProxy.OrderDashboard_DivisionWise> Divisions { get; set; }
    }

    public class OQDashReportModel
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }

        public OrderDashboardResponseModel Response { get; set; }
        public ReportProxy.OrderDashboardResponse Response1 { get; set; }
    }

    public class OrderDashboardResponseModel
    {      
        public int OrderLostCount { get; set; }
        public double OrderLostValue { get; set; }
        public string OrderLostDivisionWise { get; set; }
        public int OrderWinCount { get; set; }
        public double OrderWinValue { get; set; }
        public string OrderWinDivisionWise { get; set; }
        public int QuotesCount { get; set; }
        public double QuotesValue { get; set; }
        public string QuotesDivisionWise { get; set; }        
        public int OrderForcast3MonthsCount { get; set; }
        public double OrderForcast3MonthsValue { get; set; }
        public string OrderForcast3MonthsDivisionWise { get; set; }
        public string OrderForcastMonth1Name { get; set; }
        public int OrderForcastMonth1Count { get; set; }
        public double OrderForcastMonth1Value { get; set; }
        public string OrderForcastMonth1DivisionWise { get; set; }
        public string OrderForcastMonth2Name { get; set; }
        public int OrderForcastMonth2Count { get; set; }
        public double OrderForcastMonth2Value { get; set; }
        public string OrderForcastMonth2DivisionWise { get; set; }
        public string OrderForcastMonth3Name { get; set; }
        public int OrderForcastMonth3Count { get; set; }
        public double OrderForcastMonth3Value { get; set; }
        public string OrderForcastMonth3DivisionWise { get; set; }


        public class ItemModel
        {
            public string Type { get; set; }
            public string Parent { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public List<ItemModel> Childs { get; set; }

        }
    }

    public class OQReportNavModel
    {
        public string Type { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string Division { get; set; }
        public string Region { get; set; }
        public string Segment { get; set; }
    }

    public class OQQuoteDataModel
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }

        public List<Item> QuoteItemList { get; set; }
        public class Item
        {
            public string Division { get; set; }
            public string Branch { get; set; }

            public string Plant { get; set; }
            public double? ContractValue { get; set; }
            public double? Tonnage { get; set; }
            public string ProductRequired { get; set; }
            public string SalesEngg { get; set; }
        }
    }

    public class OQOrderDataModel
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }

        public List<Item> OrderItemList { get; set; }

        public class Item
        {
            public string AccountName { get; set; }
            public string Branch { get; set; }
            public string BusinessSegment { get; set; }
            public string SubSegment { get; set; }
            public string Competitor { get; set; }
            public string Reason { get; set; }
            public double? Price { get; set; }
            public string OrderType { get; set; }
            public double? ContractValue { get; set; }
            public double? Tonnage { get; set; }
            public string ProductName { get; set; }
            public int? Quantity { get; set; }
            public string Status { get; set; }
        }
    }

}
