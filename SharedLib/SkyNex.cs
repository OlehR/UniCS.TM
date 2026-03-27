using ModelMID;
using ModelMID.DB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Utils;

namespace SharedLib.SkyNex
{
    public class OrdersRoot
    {
        static bool IsLoaded = false;
        public OrdersRoot(DateTime pDT)
        {
            if(!IsLoaded)
            {
                LoadOrder();
                IsLoaded = true;
            }
            Orders.Clear();
            //List.AsEnumerable().Where(x => x.Key >= pDT).ToList().ForEach(x => Orders.Append(x.Value));
            foreach (var el in List.AsEnumerable().Where(x => x.Key >= pDT).ToList())
                Orders.Add(el.Value);
                
            foreach (var el in  List.AsEnumerable().Where(x => x.Key < pDT))
             List.TryRemove(el.Key, out _);             
        }
        public static ConcurrentDictionary<DateTime, Order> List = new();
        [JsonPropertyName("orders")]
        public List<Order> Orders { get; set; } = new();

        public static void AddOrder(Receipt pR, string pCodeOrder=null)
        {
            Order O = new Order(pR, pCodeOrder);
            List.TryAdd(pR.DateReceipt, O);
            File.AppendAllText(FileName, O.ToJson() + Environment.NewLine);
        }

        static string FileName { get { return $"{Path.Combine(FileLogger.PathLog, $"Orders_{DateTime.Now:yyyyMMdd}.json")}"; } }

        public static void LoadOrder()
        {
            if (File.Exists(FileName))
            {
                foreach (string Line in File.ReadAllLines(FileName))
                {
                    try
                    {
                        Order O = JsonSerializer.Deserialize<Order>(Line);
                        List.TryAdd(O.CreatedAt, O);
                    }
                    catch (Exception e)
                    {
                        File.AppendAllText(FileLogger.GetFileName, $"Error load order from file: {e.Message}{Environment.NewLine}");
                    }
                }
            }
        }
    }

    public class Order
    {
        public Order() { }
        public Order(Receipt R, string pCodeOrder=null)
        {
            foreach(var el in R.Wares.Where(x => x.ProductionLocation > 0))
            {
                bool IsLinked = false;
                foreach(var w in R.Wares.Where(x=>x.ReceiptWaresLink?.Any()??false))
                {
                    IsLinked = w.ReceiptWaresLink.Any(x => x.CodeWares == el.CodeWares);
                    if (IsLinked) break;
                }
                if (!IsLinked)
                    Products.Add(new Product(el));
            }
            //Products = R.Wares.Where(x => x.ProductionLocation > 0).Select(x => new Product(x));
            ReceiptNumber = pCodeOrder??R.NumberReceipt1C;
            CreatedAt = R.DateReceipt;
            
        }
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("receipt_number")]
        public string? ReceiptNumber { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; } = "open";

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("scheduled_for")]
        public DateTime? ScheduledFor { get; set; }

        [JsonPropertyName("table")]
        public int Table { get; set; }

        [JsonPropertyName("products")]
        public List<Product> Products { get; set; } = new();
    }

    public class Product
    {
        public Product() { }
        public Product(ReceiptWares pRW)
        {
            ProductId = pRW.CodeWares.ToString();
            Name = pRW.NameWares;
            Quantity = (int)pRW.Quantity;
            Modifications = pRW.ReceiptWaresLink.Select(x => new Modification(x));
        }
        [JsonPropertyName("product_id")]
        public string? ProductId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("modifications")]
        public IEnumerable<Modification> Modifications { get; set; }
    }

    public class Modification
    {
        public Modification() { }
        public Modification(ReceiptWaresLink pRWL)
        {
            Id = pRWL.CodeWares.ToString();
            Name = pRWL.NameWares;
            //Action = pRWL.Action;
        }
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("action")]
        public string? Action { get; set; }
    }
}
