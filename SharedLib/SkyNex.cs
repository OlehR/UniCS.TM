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
    public class Orders
    {
        static bool IsLoaded = false;
        static Orders()
        {
            if (!IsLoaded)
            {
                LoadOrder();
                IsLoaded = true;
            }            
        }

        public static OrdersRoot GetOrders(DateTime pDT)
        {
            OrdersRoot Res = new();
            foreach (var el in List.AsEnumerable().Where(x => x.Key >= pDT && x.Value.CodeReceipt>0).ToList())
                Res.Orders.Add(el.Value);
            return Res;
        }

        public static ConcurrentDictionary<DateTime, Order> List = new();

        public static void AddOrder(Receipt pR, string pCodeOrder = null,bool pIsAddFile = true)
        {
            Order O = new(pR, pCodeOrder);
            AddOrder(O,pIsAddFile);
        }

        public static void AddOrder(Order pO,bool pIsAddFile)
        {           
            List.TryAdd(pO.CreatedAt, pO);
            if(pIsAddFile)
                File.AppendAllText(FileName, pO.ToJson() + Environment.NewLine);
            if (pO.CodeReceipt < 0) ChangeReturnOrder(pO);
        }

        static void ChangeReturnOrder(Order pO)
        {
            var ListOrders = List.Where(el => el.Value.CodeReceipt == -pO.CodeReceipt);
            if (ListOrders?.Count() == 1)
            {
                var OR = ListOrders.FirstOrDefault();
                var FindOrder = OR.Value;
                foreach (var Pr in FindOrder.Products) //
                {
                    try
                    {
                        var w = pO.Products.FirstOrDefault(x => x.ProductId == Pr.ProductId);
                        if (w != null)
                        {
                            Pr.Quantity -= w.Quantity;
                            if (Pr.Quantity > 0)
                                //FindOrder.Products.Remove(Pr);
                            //else
                            {
                                foreach (var m in Pr.Modifications)
                                {
                                    var M = w.Modifications.FirstOrDefault(x => x.Id == m.Id);
                                    m.Quantity -= M?.Quantity??0;
                                }
                                if (Pr.Modifications?.Any(el => el.Quantity <= 0) == true)
                                    Pr.Modifications = Pr.Modifications.Where(el => el.Quantity > 0).ToList();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        File.AppendAllText(FileLogger.GetFileName, $"Error change return order: {e.Message}{Environment.NewLine}");
                    }
                }
                if (FindOrder.Products?.Any(el => el.Quantity <= 0) == true)
                {
                    FindOrder.Products = FindOrder.Products.Where(el => el.Quantity > 0).ToList();
                    FindOrder.Status = FindOrder.Products?.Any() == true ? "open" : "cancelled";
                }

                FindOrder.UpdatedAt = pO.CreatedAt.AddMilliseconds(1);
                if (List.TryRemove(OR.Key, out var value))
                {
                    List.TryAdd(FindOrder.UpdatedAt, value);
                }
            }
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
                        AddOrder(O,false);
                    }
                    catch (Exception e)
                    {
                        File.AppendAllText(FileLogger.GetFileName, $"Error load order from file: {e.Message}{Environment.NewLine}");
                    }
                }
                var xx = List;
            }
        }
    }

    public class OrdersRoot
    {
        [JsonPropertyName("orders")]
        public List<Order> Orders { get; set; } = new();
    }


    public class Order
    {
        public Order() { }
        public Order(Receipt R, string pCodeOrder = null)
        {
            foreach (var el in R.Wares.Where(x => x.ProductionLocation > 0))
            {
                bool IsLinked = false;
                foreach (var w in R.Wares.Where(x => x.ReceiptWaresLink?.Any() ?? false))
                {
                    IsLinked = w.ReceiptWaresLink.Any(x => x.CodeWares == el.CodeWares);
                    if (IsLinked) break;
                }
                if (!IsLinked)
                    Products.Add(new Product(el));
            }
            //Products = R.Wares.Where(x => x.ProductionLocation > 0).Select(x => new Product(x));
            ReceiptNumber = pCodeOrder ?? R.NumberReceipt1C;
            Id = R.NumberReceiptRRO;
            CreatedAt = R.DateReceipt.ToUniversalTime();
            UpdatedAt = CreatedAt;
            CodeReceipt = R.TypeReceipt == eTypeReceipt.Refund ? -R.CodeReceiptRefund : R.CodeReceipt;
        }

        public int CodeReceipt { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("receipt_number")]
        public string? ReceiptNumber { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "open"; // { return Products?.Any() == true ? "open" : "cancelled"; } }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }

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
            Modifications = pRW.ReceiptWaresLink.Select(x => new Modification(x)).ToList();
        }
        [JsonPropertyName("product_id")]
        public string? ProductId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("modifications")]
        public List<Modification> Modifications { get; set; }
    }

    public class Modification
    {
        public Modification() { }
        public Modification(ReceiptWaresLink pRWL)
        {
            Id = pRWL.CodeWares.ToString();
            Name = pRWL.NameWares;
            Action = "add";
            Quantity = pRWL.Quantity;
        }
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("action")]
        public string? Action { get; set; }

        public int Quantity { get; set; }
    }
}
